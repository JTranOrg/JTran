/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Function.cs					    		        
 *        Class(es): Function
 *          Purpose: A single function call                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 18 Jun 2020                                             
 *                                                                          
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using JTran.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class Function
    {
        private readonly MethodInfo _method;
        private readonly object     _container;
        private readonly bool       _literals;

        /*****************************************************************************/
        internal Function(object container, string name) : this(container.GetType(), name, container)
        {
        }

        /*****************************************************************************/
        internal Function(Type type, string name, object container = null) : this(container, type.GetMethods().Where( m=> m.Name.ToLower() == name).FirstOrDefault())
        {
        }

        /*****************************************************************************/
        internal Function(object container, MethodInfo method)
        {
            var name = method.Name.ToLower();

            if(name.StartsWith("___"))
                name = name.Substring(3);

            this.Name = name;

            if(method.CustomAttributes?.Where( a=> a.AttributeType.Equals(typeof(IgnoreParameterCount)))?.Any() ?? false)
                this.NumParams = 0;
            else
                this.NumParams = method.GetParameters().Where( p=> !p.HasDefaultValue ).Count();

           _method    = method;
           _container = container;
           _literals  = _method.CustomAttributes?.Where( a=> a.AttributeType.Equals(typeof(LiteralParameters)))?.Any() ?? false;
        }

        private static IDictionary<String, bool> _builtInFunctions = new Dictionary<string, bool> 
        { 
            { "GetType", true },
            { "ToString", true },
            { "Equals", true },
            { "GetHashCode", true }
        };

        /*****************************************************************************/
        internal static IList<Function> Extract(object container)
        {
            if(container == null)
                return null;

            var list = new List<Function>();
            var type = container.GetType();

            if(container is System.Type ctype)
            { 
                type = ctype;
                container = null;
            }

            var methods = type.GetMethods().Where( m=> !_builtInFunctions.ContainsKey(m.Name ) );

            foreach(var method in methods)
                list.Add(new Function(container, method));

            return list;
        }

        internal string Name      { get; }
        internal int    NumParams { get; }

        /*****************************************************************************/
        public object Evaluate(List<IExpression> inputParameters, ExpressionContext context)
        {
            var parameters   = EvaluateParameters(inputParameters, context, _literals).AssertNumParams(this.NumParams, this.Name);
            var methodParams = _method.GetParameters();
            var numParams    = methodParams.Length;

            if(numParams > 0)
            { 
                // Iterate thru all the method parameters 
                for(int i = 0; i < numParams; ++i)
                {
                    var parm = methodParams[i];
                    var parmType = parm.ParameterType;

                    if(i < parameters.Count)
                    { 
                        var currentParam = parameters[i];

                        if(currentParam != null)
                        { 
                            // If the provided parameter value does not match the method parameter type then convert it
                            if(!parmType.Equals(currentParam.GetType()) && parmType.Name != "Object")
                            { 
                                // If it's a nullable use the generic parameter type, e.g. int? ==> Nullable<int> ==> int
                                if(parmType.Name.StartsWith("Nullable"))
                                    parmType = parmType.GenericTypeArguments[0];
                                else if(parmType.IsClass || (parmType.IsValueType && !parmType.IsEnum))
                                {
                                    if(currentParam is ExpandoObject exParam)
                                    {
                                        var json = exParam.ToJson();
                                        var typedParam = JsonConvert.DeserializeObject(json, parmType);

                                        parameters[i] = typedParam;
                                        continue;
                                    }
                                }

                                parameters[i] = Convert.ChangeType(parameters[i], parmType);
                            }
                        }
                    }
                    // If no value provided but the parameter has a default value then use that
                    else if(parm.HasDefaultValue)
                        parameters.Add(parm.DefaultValue);
                    else if(parmType.Equals(typeof(ExpressionContext)))
                        parameters.Add(context);

                    // else let the invoke throw it's own exception
                }
            }

            return _method.Invoke(_container, parameters.ToArray());
        }

        #region Private

        /*****************************************************************************/
        private IList<object> EvaluateParameters(List<IExpression> inputParameters, ExpressionContext context, bool literals = false)
        {
            var parameters = new List<object>();

            foreach(var parameter in inputParameters)
                parameters.Add(literals ? (parameter as DataValue).Name : parameter.Evaluate(context));

            return parameters;
        }

        #endregion
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
