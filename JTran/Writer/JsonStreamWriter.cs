/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: JsonStreamWriter.cs					    		        
 *        Class(es): JsonStreamWriter				         		            
 *          Purpose: Write json to to a stream                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 27 Dec 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2021 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using JTran.Extensions;

namespace JTran
{
    /// <summary>
    /// Class for writing json to a stream 
    /// </summary>
    public class JsonStreamWriter : JsonWriter, IDisposable
    {
        private readonly Stream _output;

        /****************************************************************************/
        public JsonStreamWriter(Stream output, int indent = 4) : base(indent)
        {
            _output = output;
        }

        internal Stream Output => _output;

        /****************************************************************************/
        public void AppendComma()
        {
            AppendLine(",");
        }

        /****************************************************************************/
        protected override void AppendLine(string line)
        {
            Append(line + "\r\n");
        }        

        /****************************************************************************/
        protected override void Append(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            _output.Write(bytes, 0, bytes.Length);
        }

        /****************************************************************************/
        protected override void Append(Stream strm)
        {
            strm.CopyTo(_output);
        }

        /****************************************************************************/
        public void Dispose()
        {
            // Do nothing for now
        }
    }
}
