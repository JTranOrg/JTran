/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
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
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using JTran.Collections;
using JTran.Common;
using JTran.Extensions;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    public interface IExpression
    {
        object Evaluate(ExpressionContext? context);
        bool   EvaluateToBool(ExpressionContext? context);
        bool   IsConditional(ExpressionContext context);
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
        public bool EvaluateToBool(ExpressionContext? context)
        {
            return false;
        }

        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return false;
        }

        internal IList<IExpression> SubExpressions { get; set; } = new List<IExpression>();
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class Value : IExpression
    {
        private object? _value;

        public Value(object? val)
        {
            _value = val;
        }

        public object Evaluate(ExpressionContext? context)
        {
            return _value;
        }

        public override string ToString()
        {
            return _value?.ToString();
        }

        public bool EvaluateToBool(ExpressionContext? context)
        {
            return EvaluateToBool(_value, context);
        }

        internal static bool EvaluateToBool(object? value, ExpressionContext? context)
        {
            if(value is bool bval)
                return bval;  

            if(value is string sval)
                return !string.IsNullOrWhiteSpace(sval);  

            if(decimal.TryParse(value?.ToString(), out decimal dval))
                return dval != 0m;

            return !string.IsNullOrWhiteSpace(value?.ToString());
        }
        
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return _value is bool;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class NumberValue : IExpression
    {
        private decimal _value;

        public NumberValue(decimal val)
        {
            _value = val;
        }

        public object Evaluate(ExpressionContext? context)
        {
            if(Math.Floor(_value) == _value)
                return Convert.ToInt64(_value);

            return _value;
        }

        public bool EvaluateToBool(ExpressionContext context)
        {
            return _value > 0m;
        }
                
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return false;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class DataValue : IExpression
    {
        public DataValue(string name)
        {
            this.Name = CharacterSpan.FromString(name);
        }

        public ICharacterSpan Name { get; }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext? context)
        {
            return context.GetDataValue(this.Name);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext? context)
        {
            object val = this.Evaluate(context);

            return Value.EvaluateToBool(val, context);
        }
                
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return context.Data is bool;
        }

        /*****************************************************************************/
        public override string ToString()
        {
            return this.Name.ToString();
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class MultiPartDataValue : IExpression
    {
        private readonly IList<IExpression> _parts = new List<IExpression>();

        /*****************************************************************************/
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
        public override string ToString()
        {
            return JoinLiteral(null).ToString();
        }

        /*****************************************************************************/
        public ICharacterSpan JoinLiteral(ExpressionContext context)
        {   
            var parts      = new List<ICharacterSpan>();
            var numParts   = _parts.Count;
            var newContext = new ExpressionContext("", context);

            foreach(var part in _parts)
            {
                if(part is MultiPartDataValue multiPart)
                    parts.Add(multiPart.JoinLiteral(context));
                else if(part is DataValue val)
                    parts.Add(val.Name);
                else
                    parts.Add(part.Evaluate(newContext).AsCharacterSpan());
            }

            return CharacterSpan.Join(parts, '.');
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext? context)
        {
            var     numParts = _parts.Count;
            var     data     = context.Data;
            object? result   = null;
            
            if(data is IEnumerable<object> enm && enm.IsPocoList(out Type? t))
                data = new PocoEnumerableWrapper(t!, enm);

            for(var i = 0; i < numParts; ++i)
            {
                var expr = _parts[i].Evaluate(new ExpressionContext(data!, context));

                if(expr == null)
                {
                    return null;
                }

                if(expr is IEnumerable<object> outList2)
                { 
                    result = outList2;
                }
                else if(expr is ICharacterSpan || expr is string || expr is JsonObject)
                {
                    result = expr;
                }
                else if(expr.IsDictionary())
                {
                    data = expr;
                    result = expr;
                    continue;
                }
                else if(expr is IEnumerable outEnumerable)
                { 
                    result = new EnumerableWrapper(outEnumerable); 
                }
                else
                    result = expr;

                data = result;
            }

            if(result is IEnumerable<object> outList3)
            { 
                if(outList3.IsSingle())
                    return outList3.First();

                return outList3;
            }

            return result;
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext? context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }
                        
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return false;
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
        public object Evaluate(ExpressionContext? context)
        {
            if(context.Data == null)
                return null;

            if(context.Data is not JsonObject && context.Data.IsDictionary())
            {
                var indexVal = _expr.Evaluate(context).AsCharacterSpan();
                var rtnVal   = context.Data.GetPropertyValue(indexVal);

                return rtnVal;
            }

            IEnumerable<object>? enm = null;
            Type? type = null;

            if(context.Data is IEnumerable<object> enm2)
            { 
                if(!enm2.Any())
                    return null;

                if(enm2.IsPocoList(out Type? type2))
                    type = type2;

                enm = enm2;
            }
            else
                enm = new [] {context.Data};

            // If expression result is integer then return nth value of array
            if(!_expr.IsConditional(context))
            { 
                var result = _expr.Evaluate(context);

                if(!(result is bool) && result.TryParseInt(out int index))
                    return enm.GetNthItem(index);
            }   
            
            if(type != null) 
                return new PocoWhereClause(type, enm, _expr, new ExpressionContext(enm, context));

            var expr = _expr.Evaluate(context);

            if(expr is ICharacterSpan name)
            { 
                if(context.Data is IObject jobj) 
                    return jobj.GetPropertyValue(name);

                if(context.Data.IsPoco()) 
                {
                    var poco = Poco.FromObject(context.Data);
                    var pobj = new PocoObject(poco) { Data = context.Data };

                    return pobj.GetPropertyValue(name);
                }
            }

            return new WhereClause<object>(enm, _expr, new ExpressionContext(enm, context));
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext? context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }
                        
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return false;
        }
    }

     /*****************************************************************************/
     /*****************************************************************************/
     internal class VariableValue : IExpression
     {
        private ICharacterSpan _name;

        /*****************************************************************************/
        public VariableValue(ICharacterSpan name)
        {
            _name = name;
        }

        /*****************************************************************************/
        public VariableValue(string name)
        {
            _name = CharacterSpan.FromString(name);
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext? context)
        {
            return context.GetVariable(_name, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext? context)
        {
            object val = context.GetVariable(_name, context);

            return Value.EvaluateToBool(val, context);
        }
                        
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return false;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    public interface IOperator
    {
        object Evaluate(IExpression left, IExpression right, ExpressionContext? context);
        bool   EvaluateToBool(IExpression left, IExpression right, ExpressionContext? context);
        int    Precedence { get; }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal class ComplexExpression : IExpression
    {
        public IExpression? Left         { get; set; }
        public IOperator?   Operator     { get; set; }
        public IExpression? Right        { get; set; }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext? context)
        {
            if(this.Operator == null)
                return this.Left!.Evaluate(context);

            return this.Operator.Evaluate(this.Left!, this.Right!, context);
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext? context)
        {
            if(this.Operator == null)
                return this.Left.EvaluateToBool(context);

            return this.Operator.EvaluateToBool(this.Left!, this.Right!, context);
        }
                                
        /*****************************************************************************/
        public bool IsConditional(ExpressionContext context)
        {
            return this.Operator is ComparisonOperator;
        }

    }
}
