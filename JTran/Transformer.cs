﻿/***************************************************************************
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
        private readonly ExtensionFunctions _extensionFunctions;

        /****************************************************************************/
        /// <summary>
        /// Construct a new Transformer
        /// </summary>
        /// <param name="transform">The JSON that defines the transform</param>
        public Transformer(string transform, IEnumerable extensionFunctions = null, IDictionary<string, string> includeSource = null)
        {
            _transform = CompiledTransform.Compile(transform, includeSource);
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

        /****************************************************************************/
        /****************************************************************************/
        public class UserError : Exception
        {
            /****************************************************************************/
            public UserError(string error) : base(error)
            {
            }

            /****************************************************************************/
            public UserError(string errCode, string error) : base(error)
            {
                this.ErrorCode = errCode;
            }

            /****************************************************************************/
            public string ErrorCode { get; }
        }

        #region Private

        /****************************************************************************/
        internal static ExtensionFunctions CompileFunctions(IEnumerable extensionFunctions)
        {
            var result = new Dictionary<string, Function>();
            var containers = new List<object>();

            containers.Add(new BuiltinFunctions());
            containers.Add(new DateTimeFunctions());
            containers.Add(new AggregateFunctions());

            if(extensionFunctions != null)
                foreach(var container in extensionFunctions)
                    containers.Add(container);

            return new ExtensionFunctions(containers);
        }

        #endregion
    }
}
