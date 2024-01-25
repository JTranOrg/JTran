/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: FunctionCall.cs					    		        
 *        Class(es): FunctionCall, Assertions
 *          Purpose: Function call expression                   
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JTran.Json;

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
            { 
                try
                { 
                    return func.Evaluate(_parameters, context);
                }
                catch(Transformer.UserError) 
                {
                    throw;
                }
                catch(TargetInvocationException tex) 
                {
                    if(tex.InnerException is Transformer.UserError)
                        throw tex.InnerException;

                    throw new TargetInvocationException($"Extension function \"{_functionName}\" threw an exception: {tex.InnerException.Message}", tex.InnerException);
                }
                catch(Exception ex) 
                {
                    throw new TargetInvocationException($"Extension function \"{_functionName}\" threw an exception: {ex.InnerException.Message}", ex);
                }
            }

            var tfunc = context.GetFunction(_functionName);
            
            if(tfunc != null)
            {
                var output      = new JsonStringWriter();
                var funcContext = new ExpressionContext(context.Data, context, context.Templates, context.Functions);
                var index       = 0;
                
                foreach(var argName in tfunc.Parameters)
                {
                    if(index >= _parameters.Count)
                        break;

                    funcContext.SetVariable(argName, _parameters[index++].Evaluate(context));
                }

                output.StartObject();
                tfunc.Evaluate(output, funcContext, (f)=> f());
                output.EndObject();

                var exp = output.ToString().JsonToExpando();
                var dict = exp as IDictionary<string, object>;

                if(dict.ContainsKey("return"))
                    return dict["return"];

                return exp;
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
