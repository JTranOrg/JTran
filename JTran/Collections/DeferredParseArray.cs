/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: DeferredParseArray.cs					    		        
 *        Class(es): DeferredParseArray				         		            
 *          Purpose: An array that represents a top level unparsed json array                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 6 Mar 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using JTran.Common;
using JTran.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class DeferredParseArray : IEnumerable<object>
    {
        private readonly Json.Parser _parser;

        /****************************************************************************/
        internal DeferredParseArray(Json.Parser parser) 
        {
            _parser = parser;
        }

        /****************************************************************************/
        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(this, _parser);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, _parser);
        }

        internal object? FirstObject { get; set; }

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<object>
        {
            private readonly Json.Parser        _parser;
            private readonly DeferredParseArray _deferredArray;
            private readonly JsonArray          _array = new JsonArray(null);
            private object?                     _current;
            private int                         _index = 0;

            /****************************************************************************/
            internal Enumerator(DeferredParseArray deferredArray, Json.Parser parser) 
            {
                _parser = parser;
                _deferredArray = deferredArray;
            }

            public object Current => _current;
            object IEnumerator.Current => _current;

            /****************************************************************************/
            public void Dispose()
            {
                // Nothing to do
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                if(_index++ == 0 && _deferredArray.FirstObject != null)
                { 
                    _current = _deferredArray.FirstObject;
                    return true;
                }

                _array.Clear(); // Don't want to keep all the items in the array
                _current = _parser.ReadArrayItem(_array);

                if(_index == 1)
                    _deferredArray.FirstObject = _current;

                return _current != null;
            }

            /****************************************************************************/
            public void Reset()
            {
            }
        }
    }
}
