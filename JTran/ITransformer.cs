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

using System.Collections;
using System.IO;

using JTran.Streams;

namespace JTran
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Generic interface for transforms
    /// </summary>
    /// <typeparam name="T">Used for dependency injection differentiation only</typeparam>
    public interface ITransformer<T>
    {
        /****************************************************************************/
        /// <summary>
        /// Transforms the input json and returns the output as a string
        /// </summary>
        /// <param name="input">Contains the source data as a string</param>
        /// <param name="context">A transformer context</param>
        public string Transform(string data, TransformerContext? context = null);

        /****************************************************************************/
        /// <summary>
        /// Transforms the input json and writes to the output stream
        /// </summary>
        /// <param name="input">Contains the source data (stream, list or POCO) </param>
        /// <param name="output">A stream to write the results to</param>
        /// <param name="context">A transformer context</param>
        void Transform(object input, Stream output, TransformerContext? context = null);

        /****************************************************************************/
        /// <summary>
        /// Transforms the input json and writes to the output stream
        /// </summary>
        /// <param name="input">Contains the source data (stream, list or POCO) </param>
        /// <param name="output">A stream factory to write the results to</param>
        /// <param name="context">A transformer context</param>
        void Transform(object input, IStreamFactory output, TransformerContext? context = null);

        /****************************************************************************/
        /// <summary>
        /// Transforms the input list and outputs to the given stream
        /// </summary>
        /// <param name="input">Contains the source data </param>
        /// <param name="listName">An optional name for the list. If provided the input data is an object with an array with that name.</param>
        /// <param name="output">A stream to write the results to</param>
        /// <param name="context">A transformer context</param>
        void Transform(IEnumerable list, string? listName, Stream output, TransformerContext? context = null);
    }
}
