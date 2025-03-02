﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: IJsonWriter.cs					    		        
 *        Class(es): IJsonWriter				         		            
 *          Purpose: Interface for writing to a json document                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 19 Dec 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;

using JTran.Common;
using JTran.Extensions;
using JTran.Json;

namespace JTran
{
    /// <summary>
    /// Interface for writing to a json document 
    /// </summary>
    internal interface IJsonWriter
    {
        void StartObject();
        void EndObject();
        void StartArray();
        void EndArray();
        void StartChild();
        void EndChild();
     
        void WriteContainerName(ICharacterSpan name);
        void WriteSimpleArrayItem(object item);
        void WriteItem(object item, bool newContainer = true);
        void WriteProperties(object item);
        void WriteProperty(ICharacterSpan? name, object val, bool forceString = false);
        void WriteList(IEnumerable<object> list);

        bool InObject { get; }
        bool InArray  { get; }

        bool HasWritten { get; }
    }    
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Class for writing to a json document 
    /// </summary>
    internal abstract class JsonWriter : IJsonWriter
    {
        private readonly int              _indent;
        private readonly Stack<Container> _stack = new Stack<Container>();

        /****************************************************************************/
        internal JsonWriter(int indent = 4)
        {
            _indent = indent;
        }    

        /****************************************************************************/
        private class Container
        {
            internal bool   IsArray           { get; set; }
            internal int    Level             { get; set; } = 0;
            internal int    NumChildren       { get; set; } = 0;
            internal bool   ChildFinished     { get; set; } = false;
            internal bool   PreviousFinished  { get; set; } = false;
        }

        protected abstract void AppendSpaces(int numSpaces);
        protected abstract void AppendNewline();
        protected abstract void AppendBoolean(bool val);
        protected abstract void AppendNull();
        protected abstract void AppendNumber(decimal val);

        /****************************************************************************/
        public bool InObject => !(_stack.Count == 0 ? false : _stack.Peek()?.IsArray ?? false);
        public bool InArray  => _stack.Count == 0 ? false : _stack.Peek()?.IsArray ?? false;

        public bool HasWritten => false;

        #region Write

        /****************************************************************************/
        public void WriteContainerName(ICharacterSpan name)
        {
            StartChild();

            if(_stack.Any())
                _stack.Peek().PreviousFinished = true;

            WritePropertyName(name, true);
        }

        /****************************************************************************/
        private void WritePropertyName(ICharacterSpan name, bool newline)
        {
            WriteIndentation();
            Append('"');
            Append(name.FormatForJsonOutput());
            Append('"');

            if(newline)
            {  
                AppendLine(':');
            }
            else
            { 
                Append(':');
                Append(' ');
            }
        }

        /****************************************************************************/
        public void WriteSimpleArrayItem(object item)
        {
            StartChild();
            WriteIndentation();
            WriteValue(item);
            EndChild();
        }

        /****************************************************************************/
        private void WriteValue(object item)
        {
            if(item is null)
            { 
                AppendNull();
                return;
            }

            if(item is ICharacterSpan)
                goto Text;

            if(item is string)
                goto Text;

            if(item is bool bval)
            { 
                AppendBoolean(bval);
                return;
            }

            if(item is decimal dval)
            { 
                AppendNumber(dval);
                return;
            }

            if(item is double dblVal)
            { 
                AppendNumber((decimal)dblVal);
                return;
            }

            if(item is float fVal)
            { 
                AppendNumber((decimal)fVal);
                return;
            }

            if(item is long lval)
            { 
                AppendNumber(lval);
                return;
            }

            if(item is int ival)
            { 
                AppendNumber(ival);
                return;
            }

            if(item is short sval)
            { 
                AppendNumber(sval);
                return;
            }

           Text:
            Append('"');
            Append(item.AsCharacterSpan().FormatForJsonOutput());
            Append('"');
        }

        /****************************************************************************/
        public void WriteItem(object item, bool newContainer = true)
        {
            if(item is IEnumerable<object> list)
                WriteList(list);
            else if(item is ICharacterSpan || item is string || !item.GetType().IsClass)
                WriteSimpleArrayItem(item);
            else 
            { 
                if(newContainer)
                    StartContainer();
                else
                    StartChild();

                item.ToJson(this);

                EndChild();
            }
        }

        /****************************************************************************/
        public void WriteProperties(object item)
        {
            item.ChildrenToJson(this);
        }

        /****************************************************************************/
        public void WriteList(IEnumerable<object> list)
        {
            this.StartArray();

            foreach(var child in list)
            {
                this.WriteItem(child, false);
            }

            this.EndArray();
        }

        /****************************************************************************/
        public void WriteProperty(ICharacterSpan? name, object val, bool forceString = false)
        {
            if(val is IObject obj)
            { 
                StartChild();

                if(name != null)
                { 
                    WriteContainerName(name);
                    obj.ToJson(this);
                }
                else
                    WriteProperties(obj);

                EndChild();
            }
            else if(name is not null)
            { 
                StartChild();

                if(val is IEnumerable<object> list)
                { 
                    if(name != null)
                        WriteContainerName(name);
                
                    WriteList(list);
                }
                else
                { 
                    WritePropertyName(name, false);
                    WriteValue(val);
                }

                EndChild();
            }
        }        

        #endregion

        /****************************************************************************/
        public int CurrentLevel 
        {
            get
            {
                if(_stack.Empty())
                    return 0;

                return _stack.Peek().Level;
            }
        }

        /****************************************************************************/
        public int IndentLength => this.CurrentLevel * _indent;

        /****************************************************************************/
        public void StartChild()
        {
            if(!_stack.Empty() && _stack.Peek().NumChildren > 0 && !_stack.Peek().ChildFinished)
            {
                 AppendLine(',');
                _stack.Peek().ChildFinished = true;
            }
        }

        /****************************************************************************/
        public void EndChild()
        {
            if(!_stack.Empty())
            { 
                _stack.Peek().NumChildren++;
                _stack.Peek().ChildFinished = false;
            }
        }

        /****************************************************************************/
        public virtual void StartObject()
        {
            StartChild();
            WriteLine('{');

            _stack.Push(new Container { IsArray = false, Level = CurrentLevel + 1 });
        }

        /****************************************************************************/
        public virtual void EndObject()
        {
            EndContainer();
            _stack.Pop();
            WriteLine('}', false);
            EndChild();
        }

        /****************************************************************************/
        public void StartArray()
        {
            StartChild();
            WriteLine('[');

            _stack.Push(new Container { IsArray = true, Level = CurrentLevel + 1 });
        }

        /****************************************************************************/
        public void EndArray()
        {
            EndContainer();
            _stack.Pop();
            WriteLine(']', false);
            EndChild();
        }

        /****************************************************************************/
        private void EndContainer()
        {
            if(!_stack.Empty())
                if(_stack.Peek().NumChildren > 0 && !_stack.Peek().ChildFinished)
                    AppendLine(CharacterSpan.Empty);
        }        

        /****************************************************************************/
        protected abstract void AppendLine(char ch);
        
        /****************************************************************************/
        protected abstract void AppendLine(ICharacterSpan line);

        /****************************************************************************/
        protected abstract void Append(ICharacterSpan text);

        /****************************************************************************/
        protected abstract void Append(char ch);

        #region Private

        /****************************************************************************/
        private void StartContainer()
        {
            if(_stack.Count > 0 && !_stack.Peek().PreviousFinished)
            { 
                AppendNewline();
                _stack.Peek().PreviousFinished = true;
            }
        }             

        /****************************************************************************/
        private void WriteIndentation()
        {
            if(IndentLength > 0)
                AppendSpaces(IndentLength);
        }

        /****************************************************************************/
        private void WriteLine(ICharacterSpan line, bool newline = true)
        {
            WriteIndentation();

            if(newline)
                AppendLine(line);
            else
                Append(line);
        }

        /****************************************************************************/
        private void WriteLine(char ch, bool newline = true)
        {
            WriteIndentation();

            if(newline)
                AppendLine(ch);
            else
                Append(ch);
        }

        /****************************************************************************/
        private void WriteLine(string before, ICharacterSpan line, string after, bool newline = true)
        {
            WriteIndentation();

            if(newline)
                AppendLine(line);
            else
                Append(line);
        }

        #endregion
    }
        
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Tests whether any output will ever be written
    /// </summary>
    internal class JsonTestWriter : IJsonWriter
    {
        internal long NumWrites   { get; private set; }

        /****************************************************************************/
        internal JsonTestWriter()
        {
        }

        /****************************************************************************/
        public bool InObject => false;
        public bool InArray  => false;
        public bool HasWritten => this.NumWrites > 0;

        #region Write

        /****************************************************************************/
        public void WriteContainerName(ICharacterSpan name)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WriteSimpleArrayItem(object item)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WriteItem(object item, bool newContainer = true)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WriteProperties(object item)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WriteList(IEnumerable<object> list)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WriteProperty(ICharacterSpan name, object val, bool forceString = false)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WritePropertyValue(object val, bool forceString = false)
        {
        }

        #endregion

        /****************************************************************************/
        public void StartChild()
        {
        }

        /****************************************************************************/
        public void EndChild()
        {
        }

        /****************************************************************************/
        public void StartObject()
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void EndObject()
        {
        }

        /****************************************************************************/
        public void StartArray()
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void EndArray()
        {
        }
    }

}
