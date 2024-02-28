/***************************************************************************
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

using System;
using System.Collections.Generic;

using System.IO;
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
        void EndContainer();
         
        void WriteContainerName(CharacterSpan name);
        void WriteSimpleArrayItem(object item);
        void WriteRaw(CharacterSpan json);
        void WriteRaw(Stream  stream);
        void WriteItem(object item, bool newContainer = true);
        void WriteProperties(object item);
        void WriteProperty(CharacterSpan name, object val, bool forceString = false);
        void WriteList(IEnumerable<object> list);

        bool InObject { get; }
        bool InArray  { get; }
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

        protected abstract CharacterSpan FormatForJsonOutput(CharacterSpan s);
        protected abstract CharacterSpan FormatForOutput(object s, bool forceString = false);
        protected abstract void AppendSpaces(int numSpaces);

        /****************************************************************************/
        public bool InObject => !(_stack.Count == 0 ? false : _stack.Peek()?.IsArray ?? false);
        public bool InArray  => _stack.Count == 0 ? false : _stack.Peek()?.IsArray ?? false;

        #region Write

        /****************************************************************************/
        public void WriteContainerName(CharacterSpan name)
        {
            StartChild();
            _stack.Peek().PreviousFinished = true;
            WriteLine(CharacterSpan.FromString($"\"{FormatForJsonOutput(name)}\":")); // Inefficient
        }

        /****************************************************************************/
        public void WriteSimpleArrayItem(object item)
        {
            StartChild();

            var sitem = FormatForOutput(item);

            WriteLine(sitem, false);
            EndChild();
        }

        /****************************************************************************/
        public void WriteItem(object item, bool newContainer = true)
        {
            if(item is IEnumerable<object> list)
                WriteList(list);
            else if(item is CharacterSpan || item is string || !item.GetType().IsClass)
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
        public void WriteProperty(CharacterSpan name, object val, bool forceString = false)
        {
            StartChild();

            if(val is JsonObject expObject)
            { 
                this.WriteContainerName(name);
                
                expObject.ToJson(this);
            }
            else if(val is IEnumerable<object> list)
            { 
                this.WriteContainerName(name);
                
                WriteList(list);
            }
            else
            { 
                WriteLine(CharacterSpan.FromString($"\"{FormatForJsonOutput(name)}\":  {FormatForOutput(val, forceString)}"), false); // ???
            }

            EndChild();
        }        
        
        /****************************************************************************/
        public void WriteRaw(CharacterSpan json)
        {
             Append(json);
        }

        /****************************************************************************/
        public void WriteRaw(Stream  stream)
        {
             Append(stream);
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
        public void EndContainer()
        {
            if(!_stack.Empty())
                if(_stack.Peek().NumChildren > 0 && !_stack.Peek().ChildFinished)
                    AppendLine(CharacterSpan.Empty);
        }        

        /****************************************************************************/
        protected abstract void AppendLine(char ch);
        
        /****************************************************************************/
        [Obsolete]
        protected abstract void AppendLine(string line);

        /****************************************************************************/
        [Obsolete]
        protected abstract void Append(string text);

        /****************************************************************************/
        protected abstract void AppendLine(CharacterSpan line);

        /****************************************************************************/
        protected abstract void Append(CharacterSpan text);

        /****************************************************************************/
        protected abstract void Append(char ch);

        /****************************************************************************/
        protected abstract void Append(Stream strm);

        #region Private

        /****************************************************************************/
        private void StartContainer()
        {
            if(_stack.Count > 0 && !_stack.Peek().PreviousFinished)
            { 
                AppendLine("");
                _stack.Peek().PreviousFinished = true;
            }
        }             

        /****************************************************************************/
        private void WriteLine(CharacterSpan line, bool newline = true)
        {
            if(IndentLength > 0)
                AppendSpaces(IndentLength);

            if(newline)
                AppendLine(line);
            else
                Append(line);
        }

        /****************************************************************************/
        private void WriteLine(char ch, bool newline = true)
        {
            if(IndentLength > 0)
                AppendSpaces(IndentLength);

            if(newline)
                AppendLine(ch);
            else
                Append(ch);
        }

        /****************************************************************************/
        private void WriteLine(string before, CharacterSpan line, string after, bool newline = true)
        {
            if(IndentLength > 0)
                AppendSpaces(IndentLength);

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

        #region Write

        /****************************************************************************/
        public void WriteContainerName(CharacterSpan name)
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
        public void WriteProperty(CharacterSpan name, object val, bool forceString = false)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WritePropertyValue(object val, bool forceString = false)
        {
        }

        /****************************************************************************/
        public void WriteRaw(CharacterSpan json)
        {
            ++NumWrites;
        }

        /****************************************************************************/
        public void WriteRaw(Stream strm)
        {
            if(strm.Length > 0)
                ++NumWrites;
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

        /****************************************************************************/
        public void EndContainer()
        {
        }
    }

}
