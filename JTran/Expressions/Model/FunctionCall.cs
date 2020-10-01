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
            if(context.ExtensionFunctions?.ContainsKey(_functionName) ?? false)
            { 
                var func = context.ExtensionFunctions[_functionName];

                return func.Evaluate(_parameters, context);
            }

            var template = context.GetTemplate(_functionName);

            if(template == null)
                throw new Transformer.SyntaxException($"A function or template with that name was not found: {_functionName}");

            var result     = JObject.Parse("{}");
            var numParams  = _parameters.Count;
            var newContext = numParams == 0 ? context : new ExpressionContext(context.Data, context);

            for(var i = 0; i < numParams; ++i)
            {
                var name = template.Parameters[i].Trim();

                newContext.SetVariable(name, _parameters[i].Evaluate(context));
            }

            template.Evaluate(result, newContext);

            var firstChild = result.Children().First();
            var val = (firstChild as JProperty).Value;

            return val;
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
