/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
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
using System.Dynamic;
using System.IO;

using JTran.Extensions;
using JTran.Json;

namespace JTran
{
    /// <summary>
    /// Interface for writing to a json document 
    /// </summary>
    public interface IJsonWriter
    {
        void StartObject();
        void EndObject();
        void StartArray();
        void EndArray();
        void StartChild();
        void EndChild();
        void EndContainer();
         
        void WriteContainerName(string name);
        void WriteSimpleArrayItem(object item);
        void WriteRaw(string json);
        void WriteRaw(Stream  stream);
        void WriteItem(object item, bool newContainer = true);
        void WriteProperties(object item);
        void WriteProperty(string name, object val, bool forceString = false);
        void WriteList(IEnumerable<object> list);

        bool InObject { get; }
        bool InArray  { get; }
    }    
    
    /// <summary>
    /// Class for writing to a json document 
    /// </summary>
    public abstract class JsonWriter : IJsonWriter
    {
        private readonly int              _indent;
        private readonly Stack<Container> _stack = new Stack<Container>();

        /****************************************************************************/
        public JsonWriter(int indent = 4)
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

        protected abstract string FormatForJsonOutput(string s);
        protected abstract string FormatForOutput(object s, bool forceString = false);

        /****************************************************************************/
        public bool InObject => !(_stack.Count == 0 ? false : _stack.Peek()?.IsArray ?? false);
        public bool InArray  => _stack.Count == 0 ? false : _stack.Peek()?.IsArray ?? false;

        #region Write

        /****************************************************************************/
        public void WriteContainerName(string name)
        {
            StartChild();
            _stack.Peek().PreviousFinished = true;
            WriteLine($"\"{FormatForJsonOutput(name)}\":");
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
            if(item is ExpandoObject expObject)
            { 
                if(newContainer)
                    StartContainer();
                else
                    StartChild();

                expObject.ToJson(this);

               EndChild();
            }
            else if(item is IEnumerable<object> list)
                WriteList(list);
            else 
                WriteSimpleArrayItem(item);
        }

        /****************************************************************************/
        public void WriteProperties(object item)
        {
            if(item is ExpandoObject expObject)
            { 
               expObject.ChildrenToJson(this);
            }
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
        public void WriteProperty(string name, object val, bool forceString = false)
        {
            StartChild();

            if(val is ExpandoObject expObject)
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
                WriteLine($"\"{FormatForJsonOutput(name)}\":  {FormatForOutput(val, forceString)}", false);
            }

            EndChild();
        }

        /****************************************************************************/
        public void WriteRaw(string json)
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
                 AppendLine(",");
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
        public void StartObject()
        {
            StartChild();
            WriteLine("{");

            _stack.Push(new Container { IsArray = false, Level = CurrentLevel + 1 });
        }

        /****************************************************************************/
        public void EndObject()
        {
            EndContainer();
            _stack.Pop();
            WriteLine("}", false);
            EndChild();
        }

        /****************************************************************************/
        public void StartArray()
        {
            StartChild();
            WriteLine("[");

            _stack.Push(new Container { IsArray = true, Level = CurrentLevel + 1 });
        }

        /****************************************************************************/
        public void EndArray()
        {
            EndContainer();
            _stack.Pop();
            WriteLine("]", false);
            EndChild();
        }

        /****************************************************************************/
        public void EndContainer()
        {
            if(!_stack.Empty())
                if(_stack.Peek().NumChildren > 0 && !_stack.Peek().ChildFinished)
                    AppendLine("");
        }        
        
        /****************************************************************************/
        protected abstract void AppendLine(string line);

        /****************************************************************************/
        protected abstract void Append(string text);

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
        private void WriteLine(string line, bool newline = true)
        {
            if(IndentLength > 0)
                Append("".PadLeft(IndentLength));

            if(newline)
                AppendLine(line);
            else
                Append(line);
        }

        #endregion
    }
        
    /// <summary>
    /// Tests whether any output will ever be written
    /// </summary>
    internal class JsonTestWriter : IJsonWriter
    {
        /****************************************************************************/
        internal JsonTestWriter()
        {
        }

        internal class HaveOutput : Exception {}

        /****************************************************************************/
        public bool InObject => false;
        public bool InArray  => false;

        #region Write

        /****************************************************************************/
        public void WriteContainerName(string name)
        {
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteSimpleArrayItem(object item)
        {
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteItem(object item, bool newContainer = true)
        {
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteProperties(object item)
        {
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteList(IEnumerable<object> list)
        {
            // ??? Maybe not if empty
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteProperty(string name, object val, bool forceString = false)
        {
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteRaw(string json)
        {
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void WriteRaw(Stream strm)
        {
            if(strm.Length > 0)
                throw new HaveOutput();
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
            throw new HaveOutput();
        }

        /****************************************************************************/
        public void EndObject()
        {
        }

        /****************************************************************************/
        public void StartArray()
        {
            throw new HaveOutput();
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
