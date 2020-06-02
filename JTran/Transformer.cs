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
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    public class Transformer
    {
        private readonly CompiledTransform _transform;

        /****************************************************************************/
        /// <summary>
        /// Construct a new Transformer
        /// </summary>
        /// <param name="transform">The JSON that defines the transform</param>
        public Transformer(string transform)
        {
            _transform = CompiledTransform.Compile(transform);
        }

        /****************************************************************************/
        public string Transform(string data, TransformerContext context = null)
        {
            return _transform.Transform(data, context);
        }

        /****************************************************************************/
        public class SyntaxException : Exception
        {
            public SyntaxException(string error) : base(error)
            {

            }
        }
    }
}
