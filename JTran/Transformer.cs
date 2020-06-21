/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Transformer.cs					    		        
 *        Class(es): Transformer				         		            
 *          Purpose: Main class for doing tranformations                   
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
using System.Collections;
using System.Collections.Generic;
using JTran.Expressions;
using Newtonsoft.Json.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class Transformer
    {
        private readonly CompiledTransform _transform;
        private readonly IDictionary<string, Function> _extensionFunctions;

        /****************************************************************************/
        /// <summary>
        /// Construct a new Transformer
        /// </summary>
        /// <param name="transform">The JSON that defines the transform</param>
        public Transformer(string transform, IEnumerable extensionFunctions)
        {
            _transform = CompiledTransform.Compile(transform);
            _extensionFunctions = CompileFunctions(extensionFunctions);
        }

        /****************************************************************************/
        public string Transform(string data, TransformerContext context = null)
        {
            return _transform.Transform(data, context, _extensionFunctions);
        }

        /****************************************************************************/
        public class SyntaxException : Exception
        {
            public SyntaxException(string error) : base(error)
            {
            }
        }

        #region Private

        /****************************************************************************/
        internal static IDictionary<string, Function> CompileFunctions(IEnumerable extensionFunctions)
        {
            var result = new Dictionary<string, Function>();
            var containers = new List<object>();

            containers.Add(new BuiltinFunctions());

            if(extensionFunctions != null)
                foreach(var container in extensionFunctions)
                    containers.Add(container);

            foreach(var container in containers)
            {
                var list = Function.Extract(container);

                foreach(var func in list)
                {
                    if(!result.ContainsKey(func.Name))
                        result.Add(func.Name, func);
                }
            }

            return result;
        }

        #endregion
    }
}
