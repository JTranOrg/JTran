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
using System.Dynamic;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class InnerOuterJoin : IEnumerable<object>
    {
        private readonly object _left;
        private readonly object _right;
        private readonly IExpression _expression;
        private readonly ExpressionContext _context;
        private readonly bool _inner;

        /****************************************************************************/
        internal InnerOuterJoin(object left, object right, IExpression expression, ExpressionContext context, bool inner) 
        {
            _left       = left;
            _right      = right;
            _expression = expression;
            _context    = context;
            _inner      = inner;
        }

        #region IEnumerable

        /****************************************************************************/
        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(_left, _right, _expression, _context, _inner);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_left, _right, _expression, _context, _inner);
        }

        #endregion

        /****************************************************************************/
        /****************************************************************************/
        private class Enumerator : IEnumerator<object>
        {
            private readonly IExpression _expression;
            private readonly ExpressionContext _context;

            private readonly IEnumerator<object> _leftEnum;
            private readonly IEnumerator<object> _rightEnum;

            private object? _current;
            private readonly bool _inner;

            /****************************************************************************/
            internal Enumerator(object left, object right, IExpression expression, ExpressionContext context, bool inner) 
            {
                _expression = expression;
                _context    = context;
                _inner      = inner;

                if(left is IEnumerable<object> leftEnum)
                    _leftEnum = leftEnum.GetEnumerator();
                else
                    _leftEnum = new List<object> { left }.GetEnumerator();

                if(right is IEnumerable<object> rightEnum)
                    _rightEnum = rightEnum.GetEnumerator();
                else
                    _rightEnum = new List<object> { right }.GetEnumerator();
            }

            #region IEnumerator

            public object Current => _current;
            object IEnumerator.Current => _current;

            /****************************************************************************/
            public void Dispose()
            {
                _leftEnum.Dispose();
                _rightEnum.Dispose();
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                var eval = new ExpandoObject();

                while(_leftEnum!.MoveNext())
                {
                    var left = _leftEnum.Current;

                    _rightEnum.Reset();

                    while(_rightEnum!.MoveNext()) 
                    { 
                        IDictionary<string, object> dict = eval;

                        if(!eval.TryAdd("left", _leftEnum.Current))
                            dict["left"] = _leftEnum.Current;

                        if(!eval.TryAdd("right", _rightEnum.Current))
                            dict["right"] = _rightEnum.Current;

                        _context.Data = eval;

                        if(_expression.EvaluateToBool(_context))
                        {
                            _current = eval;
                            return true;
                        }
                    }

                    // If it's an outer join add the left without a right
                    if(!_inner)
                    {
                        IDictionary<string, object> dict = eval;

                        if(!eval.TryAdd("left", _leftEnum.Current))
                            dict["left"] = _leftEnum.Current;

                        if(!eval.TryAdd("right", null))
                            dict["right"] = null;

                        _current = eval;
                        return true;
                    }
                }

                return false;
            }

            /****************************************************************************/
            public void Reset()
            {
                _leftEnum.Reset();
                _rightEnum.Reset();
            }
        
            #endregion
        }

    }
}
