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

using JTran.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class WhereClause<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _list;
        private readonly IExpression _expression;
        private readonly ExpressionContext _context;

        /****************************************************************************/
        internal WhereClause(IEnumerable<T> list, IExpression expression, ExpressionContext context) 
        {
            _list       = list;
            _expression = expression;
            _context    = context;
        }

        /****************************************************************************/
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_list, _expression, _context);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_list, _expression, _context);
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<T>
        {
            private readonly IEnumerable<T> _list;
            private readonly IExpression _expression;
            private readonly IEnumerator<T> _enumerator;
            private readonly ExpressionContext _context;

            /****************************************************************************/
            internal Enumerator(IEnumerable<T> list, IExpression expression, ExpressionContext context) 
            {
                _list       = list;
                _expression = expression;
                _enumerator = list.GetEnumerator();
                _context    = context;
            }

            public T Current => _enumerator!.Current;
            object IEnumerator.Current => _enumerator!.Current;

            /****************************************************************************/
            public void Dispose()
            {
                _enumerator.Dispose();
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                while(_enumerator!.MoveNext())
                {
                    _context.Data = _enumerator!.Current;

                    if(_expression.EvaluateToBool(_context))
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
