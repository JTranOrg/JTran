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

using System;
using System.Collections;
using System.Collections.Generic;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class Union<T> : IEnumerable<T>
    {
        private readonly List<IEnumerable<T>> _lists = new List<IEnumerable<T>>();

        /****************************************************************************/
        internal Union(IEnumerable<T>? list1 = null, IEnumerable<T>? list2 = null, IEnumerable<T>? list3 = null) 
        {
            if(list1 != null)
                _lists!.Add(list1);

            if(list2 != null)
                _lists!.Add(list2);

            if(list3 != null)
                _lists!.Add(list3);
        }

        /****************************************************************************/
        internal void Add(IEnumerable<T>? list)
        {
            if(list != null)
                _lists.Add(list);
        }

        /****************************************************************************/
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_lists);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_lists);
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<T>
        {
            private readonly List<IEnumerable<T>> _lists;
            private int             _listIndex = 0;
            private IEnumerator<T>? _currentEnumerator;

            /****************************************************************************/
            internal Enumerator(List<IEnumerable<T>> lists) 
            {
                _lists = lists;
                _currentEnumerator = _lists[0].GetEnumerator();
            }

            public T Current => _currentEnumerator!.Current;
            object IEnumerator.Current => _currentEnumerator!.Current;

            /****************************************************************************/
            public void Dispose()
            {
                if(_currentEnumerator != null)
                    _currentEnumerator.Dispose();
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                while(!_currentEnumerator!.MoveNext())
                {
                    if(++_listIndex >= _lists.Count)
                        return false;

                    Dispose();
                    _currentEnumerator = _lists[_listIndex].GetEnumerator();
                }

                return true;
            }

            /****************************************************************************/
            public void Reset()
            {
                _listIndex = 0;
                _currentEnumerator = _lists[0].GetEnumerator();
            }
        }
    }
}
