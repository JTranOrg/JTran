/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Transformer.cs					    		        
 *        Class(es): Transformer				         		            
 *          Purpose: Main class for doing tranformations                   
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
using System.Threading.Tasks;

using JTran.Collections;
using JTran.Expressions;
using JTran.Extensions;
using JTran.Streams;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class Transformer : Transformer<string>
    {
        /****************************************************************************/
        public Transformer(string transform, IEnumerable? extensionFunctions = null, IDictionary<string, string>? includeSource = null)
            : base(transform, extensionFunctions, includeSource)
        {
        }       
        
        /****************************************************************************/
        public Transformer(Stream transform, IEnumerable? extensionFunctions = null, IDictionary<string, string>? includeSource = null)
            : base(transform, extensionFunctions, includeSource)
        { 
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class Transformer<T> : ITransformer<T>
    {
        private readonly CompiledTransform _transform;
        private readonly ExtensionFunctions? _extensionFunctions;

        /****************************************************************************/
        /// <summary>
        /// Construct a new Transformer
        /// </summary>
        /// <param name="transform">The JSON that defines the transform</param>
        /// <param name="extensionFunctions">Extension functions</param>
        /// <param name="includeSource">Source for include files</param>
        public Transformer(string transform, IEnumerable? extensionFunctions = null, IDictionary<string, string>? includeSource = null)
        {
            _transform = CompiledTransform.Compile(transform, includeSource);
            _extensionFunctions = CompileFunctions(extensionFunctions);
        }       
        
        /****************************************************************************/
        /// <summary>
        /// Construct a new Transformer
        /// </summary>
        /// <param name="transform">A stream containing the JTran source</param>
        /// <param name="extensionFunctions">Extension functions</param>
        /// <param name="includeSource">Source for include files</param>
        public Transformer(Stream transform, IEnumerable? extensionFunctions = null, IDictionary<string, string>? includeSource = null)
        {
            _transform = CompiledTransform.Compile(transform, includeSource);
            _extensionFunctions = CompileFunctions(extensionFunctions);
        }

        #region ITransformer

        /****************************************************************************/
        /// <summary>
        /// Transforms the input json and returns the output as a string
        /// </summary>
        /// <param name="input">Contains the source data as a string</param>
        /// <param name="context">A transformer context</param>
        public string Transform(string data, TransformerContext? context = null)
        {
            return _transform.Transform(data, context, _extensionFunctions);
        }

        /****************************************************************************/
        /// <summary>
        /// Transforms the input json and writes to the output stream
        /// </summary>
        /// <param name="input">Contains the source data (stream, list or POCO) </param>
        /// <param name="output">A stream to write the results to</param>
        /// <param name="context">A transformer context</param>
        public void Transform(object input, Stream output, TransformerContext? context = null)
        {
            _transform.Transform(CheckPocoList(input), output, context, _extensionFunctions);
        }

        /****************************************************************************/
        /// <summary>
        /// Transforms the input json and writes to the output stream
        /// </summary>
        /// <param name="input">Contains the source data (stream, list or POCO) </param>
        /// <param name="output">A stream factory to write the results to</param>
        /// <param name="context">A transformer context</param>
        public void Transform(object input, IStreamFactory output, TransformerContext? context = null)
        {
            _transform.Transform(CheckPocoList(input), output, context, _extensionFunctions);
        }

        /****************************************************************************/
        /// <summary>
        /// Transforms the input list and outputs to the given stream
        /// </summary>
        /// <param name="input">Contains the source data </param>
        /// <param name="listName">An optional name for the list. If provided the input data is an object with an array with that name.</param>
        /// <param name="output">A stream to write the results to</param>
        /// <param name="context">A transformer context</param>
        public void Transform(IEnumerable list, string? listName, Stream output, TransformerContext? context = null)
        {
            _transform.Transform(CheckPocoList(list) as IEnumerable, listName, output, context, _extensionFunctions);
        }

        #endregion

        #region Child Classes

        /****************************************************************************/
        public class SyntaxException : JsonParseException
        {
            public SyntaxException(string error) : base(error, 0)
            {
            }

            public SyntaxException(string error, Exception inner) : base(error, inner)
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
            public string ErrorCode { get; } = "";
        }

        #endregion

        #region Private

        /****************************************************************************/
        private object CheckPocoList(object input)
        {
            if(input is IEnumerable<object> enm)
            { 
                if(enm.IsPocoList(out Type? type))
                    return new PocoEnumerableWrapper(type!, enm);
            }

            return input;
        }

        /****************************************************************************/
        internal static ExtensionFunctions CompileFunctions(IEnumerable? extensionFunctions)
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
