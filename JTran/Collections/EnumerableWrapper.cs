/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Union.cs					    		        
 *        Class(es): Union				         		            
 *          Purpose: A collection of other collections enumerated in serial order                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 26 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using JTran.Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class EnumerableWrapper : IEnumerable<object>
    {
        private readonly IEnumerable _list;

        /****************************************************************************/
        internal EnumerableWrapper(IEnumerable list) 
        {
            _list = list;
        }

        /****************************************************************************/
        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(_list);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_list);
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<object>
        {
            private readonly IEnumerator _enumerator;

            /****************************************************************************/
            internal Enumerator(IEnumerable list) 
            {
                _enumerator = list.GetEnumerator();
            }

            public object Current => _enumerator!.Current;
            object IEnumerator.Current => _enumerator!.Current;

            /****************************************************************************/
            public void Dispose()
            {
                // Nothing to do
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                return _enumerator!.MoveNext();
            }

            /****************************************************************************/
            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class PocoEnumerableWrapper : IEnumerable<object>
    {
        private readonly IEnumerable _list;
        private readonly Poco       _poco;

        /****************************************************************************/
        internal PocoEnumerableWrapper(Type type, IEnumerable list) 
        {
            _list = list;
            _poco = Poco.FromType(type);
        }

        /****************************************************************************/
        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(_poco, _list);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_poco, _list);
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<object>
        {
            private readonly IEnumerator _enumerator;
            private readonly PocoObject  _pocoObject;

            /****************************************************************************/
            internal Enumerator(Poco poco,IEnumerable list) 
            {
                _enumerator = list.GetEnumerator();
                _pocoObject = new PocoObject(poco);
            }

            public object Current => _pocoObject;
            object IEnumerator.Current => _pocoObject;

            /****************************************************************************/
            public void Dispose()
            {
                // Nothing to do
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                if(_enumerator!.MoveNext())
                { 
                    _pocoObject.Data = _enumerator!.Current;
                    return true;
                }

                return false;
            }

            /****************************************************************************/
            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
