/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: IExpression.cs					    		        
 *        Class(es): IExpression, Value, NumberValue, DataValue, MultiPartDataValue, Indexer, VariableValue,
 *                      IOperator, ComplexExpression
 *          Purpose: Expression interface                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using JTran.Extensions;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    public interface IExpression
    {
        object Evaluate(ExpressionContext context);
        bool   EvaluateToBool(ExpressionContext context);
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class ArrayExpression : IExpression
    {
        /*****************************************************************************/
        public ArrayExpression()
        {
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            return this.SubExpressions.Select( x=> x.Evaluate(context) );
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return false;
        }

        internal IList<IExpression> SubExpressions { get; set; } = new List<IExpression>();
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class Value : IExpression
    {
        private object _value;

        public Value(object val)
        {
            _value = val;
        }

        public object Evaluate(ExpressionContext context)
        {
            return _value;
        }

        public override string ToString()
        {
            return _value?.ToString();
        }

        public bool EvaluateToBool(ExpressionContext context)
        {
            return EvaluateToBool(_value, context);
        }

        internal static bool EvaluateToBool(object value, ExpressionContext context)
        {
            if(value is bool bval)
                return bval;  

            if(value is string sval)
                return !string.IsNullOrWhiteSpace(sval);  

            if(double.TryParse(value.ToString(), out double dval))
                return dval != 0d;

            return !string.IsNullOrWhiteSpace(value.ToString());
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class NumberValue : IExpression
    {
        private double _value;

        public NumberValue(double val)
        {
            _value = val;
        }

        public object Evaluate(ExpressionContext context)
        {
            if(Math.Floor(_value) == _value)
                return Convert.ToInt64(_value);

            return _value;
        }

        public bool EvaluateToBool(ExpressionContext context)
        {
            return _value > 0d;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class DataValue : IExpression
    {
        public DataValue(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            return context.GetDataValue(this.Name);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            object val = this.Evaluate(context);

            return Value.EvaluateToBool(val, context);
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class MultiPartDataValue : IExpression
    {
        private readonly IList<IExpression> _parts = new List<IExpression>();

        public MultiPartDataValue(IExpression? initial)
        {
            if(initial != null)
                _parts.Add(initial);
        }

        /*****************************************************************************/
        public void AddPart(IExpression expr)
        {
            _parts.Add(expr);
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            var numParts = _parts.Count;
            var data     = context.Data;
            var result   = new List<object>();
            
            for(var i = 0; i < numParts; ++i)
            {
                var expr = _parts[i].Evaluate(new ExpressionContext(data, context));

                if(expr == null)
                {
                    return null;
                }

                result.Clear();

                if(expr is IEnumerable<object> outList2)
                { 
                    result.AddRange(outList2);
                }
                else if(expr is string || expr is ExpandoObject)
                {
                    result.Add(expr);
                }
                else if(expr.IsDictionary())
                {
                    data = expr;
                    result.Add(expr);
                    continue;
                }
                else if(expr is IEnumerable outEnumerable)
                { 
                    foreach(var item in outEnumerable)
                        result.Add(item);
                }
                else
                    result.Add(expr);

                data = result;
            }

            if(result is IList<object> outList3)
            { 
                if(outList3.Count == 1)
                    return outList3[0];

                return outList3;
            }

            return result;
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class Indexer : IExpression
    {
        private readonly IExpression _expr;

        /*****************************************************************************/
        public Indexer(IExpression indexExpr)
        {
            _expr = indexExpr;
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            if(context.Data == null)
                return null;

            var result = new List<object>();

            if(context.Data.IsDictionary())
            {
                var indexVal = _expr.Evaluate(context).ToString();
                var rtnVal   = context.Data.GetPropertyValue(indexVal);

                return rtnVal;
            }
            else if(context.Data is IList<object> list)
            { 
                // If expression result is integer then return nth value of array
                try
                { 
                    if(int.TryParse(_expr.Evaluate(context).ToString(), out int index))
                        return list[index];
                }
                catch
                {

                }

                // Evaluate each item in list against expression
                foreach(var child in list)
                { 
                    if(_expr.EvaluateToBool(new ExpressionContext(child, context)))
                        result.Add(child);
                }
            }

            return result;
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }
    }

     /*****************************************************************************/
     /*****************************************************************************/
     internal class VariableValue : IExpression
     {
        private string _name;

        public VariableValue(string name)
        {
            _name = name;
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            return context.GetVariable(_name, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            object val = context.GetVariable(_name, context);

            return Value.EvaluateToBool(val, context);
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    public interface IOperator
    {
        object Evaluate(IExpression left, IExpression right, ExpressionContext context);
        bool   EvaluateToBool(IExpression left, IExpression right, ExpressionContext context);
        int    Precedence { get; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class ComplexExpression : IExpression
    {
        public IExpression Left         { get; set; }
        public IOperator   Operator     { get; set; }
        public IExpression Right        { get; set; }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            if(this.Operator == null)
                return this.Left.Evaluate(context);

            return this.Operator.Evaluate(this.Left, this.Right, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            if(this.Operator == null)
                return this.Left.EvaluateToBool(context);

            return this.Operator.EvaluateToBool(this.Left, this.Right, context);
        }
    }
}
