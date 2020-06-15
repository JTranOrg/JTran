/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: FunctionCall.cs					    		        
 *        Class(es): FunctionCall, Assertions
 *          Purpose: Function call expression                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class FunctionCall : IExpression
    {
        private readonly string _functionName;
        private readonly List<IExpression> _parameters = new List<IExpression>();

        /*****************************************************************************/
        public FunctionCall(string functionName)
        {
            _functionName = functionName.ToLower();
        }

        /*****************************************************************************/
        public object Evaluate(ExpressionContext context)
        {
            switch(_functionName)
            {
                case "floor":          
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return decimal.Floor(decimal.Parse(parameters[0].ToString()));
                }

                case "ceiling":        
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return decimal.Ceiling(decimal.Parse(parameters[0].ToString()));
                }

                case "round":           
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return decimal.Round(decimal.Parse(parameters[0].ToString()));
                }

                case "number":          
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return decimal.Parse(parameters[0].ToString());
                }

                case "string":          
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return new StringValue(parameters[0].ToString());
                }

                case "not":            
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return !Convert.ToBoolean(parameters[0]);
                }

                case "normalizespace": 
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return parameters[0].ToString().Trim().Replace("  ", " ");
                }

                case "stringlength":  
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    return parameters[0].ToString().Length;
                }

                case "substring":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
                    var str        = parameters[0].ToString();
                    var start      = (int)(long)parameters[1];

                    if(parameters.Count > 2)
                        return str.Substring(start, (int)(long)parameters[2]);

                    return str.Substring(start);
                }

                case "indexof":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
                    var str        = parameters[0].ToString();
                    var substr     = parameters[1].ToString();

                    return str.IndexOf(substr);
                }

                case "substringafter":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
                    var str        = parameters[0].ToString();
                    var substr     = parameters[1].ToString();
                    var index      = str.IndexOf(substr);

                    if(index == -1)
                        return "";
 
                    return str.Substring(index + substr.Length);
                }

                case "substringbefore":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
                    var str        = parameters[0].ToString();
                    var substr     = parameters[1].ToString();
                    var index      = str.IndexOf(substr);

                    if(index == -1)
                        return "";
 
                    return str.Substring(0, index);
                }

                case "startswith":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
 
                    return parameters[0].ToString().StartsWith(parameters[1].ToString());
                }

                case "endswith":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
 
                    return parameters[0].ToString().EndsWith(parameters[1].ToString());
                }

                case "contains":       
                { 
                    var parameters = EvaluateParameters(context).AssertNumParams(2, _functionName);
 
                    return parameters[0].ToString().Contains(parameters[1].ToString());
                }

                case "position":        
                {
                    try
                    { 
                        dynamic dyn      = context.Data;
                        long    position = dyn._jtran_position;

                        return position;
                    }
                    catch
                    {
                        return 0;
                    }
                }

                case "document":       
                { 
                    var parameters = EvaluateParameters(context, true).AssertNumParams(2, _functionName);
                    var repoName   = parameters[0].ToString();
                    var docName    = parameters[1].ToString();
                    
                    return context.GetDocument(repoName, docName);
                }

                case "count":
                {
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);

                    if(parameters[0] is null)
                        return 0;

                    if(parameters[0] is IList<object> list)
                        return list.Count;

                    return 1;
                }

                case "sum":
                {
                    var parameters = EvaluateParameters(context).AssertNumParams(1, _functionName);
                    var parameter = parameters[0];

                    if(parameter is null)
                        return 0M;

                    if(parameter is IList<object> list)
                    { 
                        decimal sum = 0M;

                        foreach(var item in list)
                            if(decimal.TryParse(item.ToString(), out decimal dval))
                                sum += dval;

                        return sum;
                    }

                    if(decimal.TryParse(parameter.ToString(), out decimal val))
                        return val;

                    return 0M;
                }

                default:
                    throw new Transformer.SyntaxException($"'{_functionName}' is an unknown function");
            }
        }

        /*****************************************************************************/
        public bool EvaluateToBool(ExpressionContext context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }

        /*****************************************************************************/
        public void AddParameter(IExpression expr)
        {
            _parameters.Add(expr);
        }

        /*****************************************************************************/
        private IList<object> EvaluateParameters(ExpressionContext context, bool literals = false)
        {
            var parameters = new List<object>();

            foreach(var parameter in _parameters)
                parameters.Add(literals ? (parameter as DataValue).Name : parameter.Evaluate(context));

            return parameters;
        }
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal static class Assertions
    { 
        /*****************************************************************************/
        internal static IList<object> AssertNumParams(this IList<object> parameters, int min, string functionName)
        {
            if(parameters.Count < min)
                throw new Transformer.SyntaxException($"{functionName} has incorrect number of parameters");

            return parameters;
        }
    }
}
