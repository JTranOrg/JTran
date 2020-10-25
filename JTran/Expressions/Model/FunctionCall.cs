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

using JTran.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var func = context.ExtensionFunctions.GetFunction(_functionName, _parameters.Count());

            if(func != null)
                return func.Evaluate(_parameters, context);

            var tfunc = context.GetFunction(_functionName);
            
            if(tfunc != null)
            {
                var output      = JObject.Parse("{}");
                var funcContext = new ExpressionContext(context.Data, context, context.Templates, context.Functions);
                var index       = 0;
                
                foreach(var argName in tfunc.Parameters)
                {
                    if(index >= _parameters.Count)
                        break;

                    funcContext.SetVariable(argName, _parameters[index++].Evaluate(context));
                }

                tfunc.Evaluate(output, funcContext);

                if(output["return"] != null)
                    return output["return"];

                return output.ToString().JsonToExpando();
            }

            throw new Transformer.SyntaxException($"A function with that name and number of parameters was not found : {_functionName}");
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
    }
}
