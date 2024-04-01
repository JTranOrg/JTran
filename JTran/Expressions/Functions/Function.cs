/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Function.cs					    		        
 *        Class(es): Function
 *          Purpose: A single function call                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 18 Jun 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JTran.Common;
using JTran.Json;

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
            if(method == null)
                throw new ArgumentNullException(nameof(method), "A method with that name does not exist");

            var name = method.Name.ToLower();

            if(name.StartsWith("___"))  
                name = name.Substring(3);

            this.Name = name;

            var customAttributes = method.CustomAttributes?.ToList();
            var ignoreType       = typeof(IgnoreParameterCount);
            var literalType      = typeof(LiteralParameters);

            this.IgnoreParams = customAttributes == null ? false : customAttributes.Any(a=> a.AttributeType.Equals(ignoreType));

            if(this.IgnoreParams)
                this.NumParams = 0;
            else
                this.NumParams = method.GetParameters().Count( p=> !p.HasDefaultValue );

           _method    = method;
           _container = container;
           _literals  = customAttributes == null ? false : customAttributes.Any( a=> a.AttributeType.Equals(literalType));
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

        internal string Name         { get; }
        internal int    NumParams    { get; }
        internal bool   IgnoreParams { get; }

        /*****************************************************************************/
        private IList<(ParameterInfo, Type)> GetMethodParameters(int numPassedIn, out int indexOfParams, out Type paramsType)
        {
            var methodParams = _method.GetParameters();
            var numParams    = methodParams.Length;
            var result       = new List<(ParameterInfo, Type)>();
            var soFar        = 0;

            indexOfParams = -1;
            paramsType = null;

            foreach(var parm in methodParams)
            {
                if(Attribute.IsDefined(parm, typeof(ParamArrayAttribute)))
                {
                    var type = parm.ParameterType.GetElementType();

                    paramsType = type;
                    indexOfParams = soFar;

                    for(var i = soFar; i < numPassedIn; ++i)
                        result.Add((parm, type));
                }
                else
                { 
                    result.Add((parm, parm.ParameterType));
                    ++soFar;
                }
            }

            return result;
        }

        /*****************************************************************************/
        public object Evaluate(List<IExpression> inputParameters, ExpressionContext context)
        {
            var parameters   = EvaluateParameters(inputParameters, context, _literals).AssertNumParams(this.NumParams, this.Name);
            var methodParams = GetMethodParameters(inputParameters.Count, out int indexOfParams, out Type paramsType);
            var numParams    = methodParams.Count;

            if(numParams > 0)
            { 
                // Iterate thru all the method parameters 
                for(int i = 0; i < numParams; ++i)
                {
                    var parm     = methodParams[i].Item1;
                    var parmType = methodParams[i].Item2;

                    if(parmType.Equals(typeof(ExpressionContext)))
                        parameters.Add(context);
                    else if(i < parameters.Count)
                    { 
                        var currentParam = parameters[i];

                        if(currentParam != null)
                            parameters[i] = ConvertParameter(currentParam, parmType);
                    }
                    // If no value provided but the parameter has a default value then use that
                    else if(parm.HasDefaultValue)
                        parameters.Add(parm.DefaultValue);

                    // else let the invoke throw it's own exception
                }
            }

            if(indexOfParams != -1)
            {
                var paramsParms = new List<object>();

                paramsParms.AddRange(parameters.GetRange(indexOfParams, parameters.Count - indexOfParams));
                parameters.RemoveRange(indexOfParams, parameters.Count - indexOfParams);

                parameters.Add(CreateTypedArray(paramsType, paramsParms));
            }

            try
            { 
                return _method.Invoke(_container, parameters.ToArray());
            }
            catch(TargetParameterCountException ex) 
            {
                throw new Transformer.SyntaxException($"Function called with incorrect number of parameters: {this.Name}", ex);
            }
        }

        #region Private

        /*****************************************************************************/
        private object CreateTypedArray(Type elementType, IList<object> values)
        {
            if(elementType.Name == "String")
                return (new List<string>(values.Select( v=> v?.ToString()))).ToArray();

            if(elementType.Name == "Decimal")
                return (new List<decimal>(values.Select( v=> decimal.Parse(v?.ToString() ?? "0")))).ToArray();

            if(elementType.Name == "Long")
                return (new List<long>(values.Select( v=> long.Parse(v?.ToString() ?? "0")))).ToArray();

            if(elementType.Name == "Integer")
                return (new List<int>(values.Select( v=> int.Parse(v?.ToString() ?? "0")))).ToArray();

            return values.ToArray();
        }

        /*****************************************************************************/
        private object ConvertParameter(object currentParam, Type parmType)
        {
            // If the provided parameter value does not match the method parameter type then convert it
            if(!parmType.Equals(currentParam.GetType()) && parmType.Name != "Object")
            { 
                // If it's a nullable use the generic parameter type, e.g. int? ==> Nullable<int> ==> int
                if(parmType.Name.StartsWith("Nullable"))
                    parmType = parmType.GenericTypeArguments[0];
                else if(parmType.IsClass || (parmType.IsValueType && !parmType.IsEnum))
                {
                    if(currentParam is JsonObject exParam)
                        return exParam.ToObject(parmType);
                }

                if(parmType == typeof(IEnumerable<object>) && currentParam is IList<object> list)
                    return list;

                if(parmType == typeof(string) && currentParam is ICharacterSpan cspan)
                    return cspan.ToString();

                return Convert.ChangeType(currentParam, parmType);
            }

            return currentParam;
        }

        /*****************************************************************************/
        private List<object> EvaluateParameters(List<IExpression> inputParameters, ExpressionContext context, bool literals = false)
        {
            var parameters = new List<object>();

            foreach(var parameter in inputParameters)
            { 
                object? name = null;

                if(literals)
                {
                    if(parameter is MultiPartDataValue multiPart)
                        name = multiPart.JoinLiteral(context);
                    else if(parameter is DataValue val)
                        name = val.Name;
                    else
                        throw new Transformer.SyntaxException("Unknown value type");
                }
                else
                    name = parameter.Evaluate(context);

                parameters.Add(name);
            }

            return parameters;
        }

        #endregion
    }

    /*****************************************************************************/
    /*****************************************************************************/
    internal static class Assertions
    { 
        /*****************************************************************************/
        internal static List<object> AssertNumParams(this List<object> parameters, int min, string functionName)
        {
            if(parameters.Count < min)
                throw new Transformer.SyntaxException($"{functionName} has incorrect number of parameters");

            return parameters;
        }
    }
}
