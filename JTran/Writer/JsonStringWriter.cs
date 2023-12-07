/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer 						                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: JsonStringWriter.cs					    		        
 *        Class(es): JsonStringWriter				         		            
 *          Purpose: Write json to to a string                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 27 Dec 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.Text;
using System.IO;

using JTran.Common;
using JTran.Extensions;

namespace JTran
{
    /// <summary>
    /// Class for writing json to string 
    /// </summary>
    internal class JsonStringWriter : JsonWriter
    {
        private readonly StringBuilder _output = new StringBuilder();

        /****************************************************************************/
        internal JsonStringWriter(int indent = 4) : base(indent)
        {
        }

        /****************************************************************************/
        public override string ToString()
        {
            return _output.ToString();
        }

        /****************************************************************************/
        protected override void AppendLine(string line)
        {
            _output.AppendLine(line);
        }        

        /****************************************************************************/
        protected override void Append(string text)
        {
            _output.Append(text);
        } 

        /****************************************************************************/
        protected override void Append(Stream strm)
        {
            var text = strm.ReadString();

            _output.Append(text);
        } 

        /****************************************************************************/
        protected override string FormatForJsonOutput(string s)
        {
            return s.FormatForJsonOutput();
        }

       /****************************************************************************/
        protected override string FormatForOutput(object s, bool forceString = false)
        {
            return s.FormatForOutput(forceString, true);
        }
    }
}
