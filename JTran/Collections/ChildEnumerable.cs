/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ChildEnumerable.cs					    		        
 *        Class(es): ChildEnumerable				         		            
 *          Purpose: Enumerates over the a list and it's children                  
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

using JTran.Extensions;
using System.Collections;
using System.Collections.Generic;

using JTran.Common;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class ChildEnumerable<TPARENT, TCHILD> : IEnumerable<TCHILD>
    {
        private readonly IEnumerable<TPARENT> _list;
        private readonly ICharacterSpan        _field;

        /****************************************************************************/
        internal ChildEnumerable(IEnumerable<TPARENT> list, ICharacterSpan field) 
        {
            _list  = list;
            _field = field;
        }

        /****************************************************************************/
        public IEnumerator<TCHILD> GetEnumerator()
        {
            return new Enumerator(_list, _field);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_list, _field);
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<TCHILD>
        {
            private readonly IEnumerator<TPARENT> _parentEnumerator;
            private IEnumerator<TCHILD>?          _childEnumerator;
            private readonly ICharacterSpan        _field;
            private bool                          _childStatus = false;
            private bool                          _isChildValue = false;
            private TCHILD?                       _currentValue;

            /****************************************************************************/
            internal Enumerator(IEnumerable<TPARENT> list, ICharacterSpan field) 
            {
                _field            = field;
                _parentEnumerator = list.GetEnumerator();
            }

            public TCHILD Current => _isChildValue ? _currentValue : _childEnumerator!.Current;
            object IEnumerator.Current => _isChildValue ? _currentValue : _childEnumerator!.Current;

            /****************************************************************************/
            public void Dispose()
            {
                _parentEnumerator.Dispose();
                _childEnumerator?.Dispose();
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                if(_childStatus)
                {
                    _childStatus = _childEnumerator!.MoveNext();

                    if(_childStatus)    
                        return true;
                }
                
                _childEnumerator?.Dispose();
                _childEnumerator = null;

                if(!_parentEnumerator!.MoveNext())
                    return false;

                var val = _parentEnumerator!.Current!.GetPropertyValue(_field); // ??? if list is poco then we can optimize this

                if(val is IEnumerable<TCHILD> childList)
                { 
                    _childEnumerator = childList.GetEnumerator();

                    _childStatus = _childEnumerator.MoveNext();

                    if(_childStatus)    
                        return true;
                }
                else if(val is TCHILD)
                { 
                    _currentValue = (TCHILD)val;
                    _isChildValue = true;
                    return true;
                }

                return MoveNext();
            }

            /****************************************************************************/
            public void Reset()
            {
                _parentEnumerator.Reset();

                _childEnumerator?.Dispose();
                _childEnumerator = null;
            }
        }
    }
}
