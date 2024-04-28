/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: JsonStreamWriter.cs					    		        
 *        Class(es): JsonStreamWriter				         		            
 *          Purpose: Write json to to a stream                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 27 Dec 2020                                             
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
using System.Threading.Tasks;

using JTran.Common;
using JTran.Streams;

namespace JTran
{
    /// <summary>
    /// Class for writing json to a stream 
    /// </summary>
    internal class JsonMultiStreamWriter : IJsonWriter, IDisposable
    {
        private readonly IStreamFactory _factory;
        private JsonStreamWriter? _current;
        private Stream? _currentStream;
        private int _index = 0;
        private readonly Stack<int> _stack = new();

       /****************************************************************************/
        public JsonMultiStreamWriter(IStreamFactory factory) 
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        #region IJsonWriter
        
        public bool InObject => _current!.InObject;
        public bool InArray  => _current!.InArray;

        /****************************************************************************/
        public void StartObject()
        {
            if(_stack.Count == 0)
                _stack.Push(1);
            else
                _stack.Push(_stack.Peek() + 1);

            if(_stack.Count == 1)
            { 
                _currentStream = _factory!.BeginStream(_index);

                _current = new JsonStreamWriter(_currentStream);
            }

            _current!.StartObject();       
        }

        /****************************************************************************/
        public void EndObject()
        {
            _current!.EndObject();

            var level = _stack.Pop();

            if(level == 1)
            { 
                _current.Dispose();

                _factory.EndStream(_currentStream!, _index++);

                _current = null;
                _currentStream = null;
            }        
        }

        /****************************************************************************/
        public void StartArray()
        {
            _current?.StartArray();
        }

        /****************************************************************************/
        public void EndArray()
        {
            _current?.EndArray();
        }

        /****************************************************************************/
        public void StartChild()
        {
            _current!.StartChild();
        }

        /****************************************************************************/
        public void EndChild()
        {
            _current!.EndChild();
        }

        /****************************************************************************/
        public void WriteContainerName(ICharacterSpan name)
        {
            _current!.WriteContainerName(name);
        }

        /****************************************************************************/
        public void WriteSimpleArrayItem(object item)
        {
            _current!.WriteSimpleArrayItem(item);
        }

        /****************************************************************************/
        public void WriteItem(object item, bool newContainer = true)
        {
            _current!.WriteItem(item, newContainer);
        }

        /****************************************************************************/
        public void WriteProperties(object item)
        {
            _current!.WriteProperties(item);
        }

        /****************************************************************************/
        public void WriteProperty(ICharacterSpan? name, object val, bool forceString = false)
        {
            _current!.WriteProperty(name, val, forceString);
        }

        /****************************************************************************/
        public void WriteList(IEnumerable<object> list)
        {
            _current!.WriteList(list);
        }

        #endregion

        /****************************************************************************/
        public void Dispose()
        {
            _current?.Dispose();
            _currentStream?.Dispose();
        }
    }
}
