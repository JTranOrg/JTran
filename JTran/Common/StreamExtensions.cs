/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: StreamExtensions.cs					    		        
 *        Class(es): StreamExtensions				         		            
 *          Purpose: Extension methods for stream                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 2 Dec Apr 2023                                             
 *                                                                          
 *   Copyright (c) 2023-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JTran.Common.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class StreamExtensions
    {
        /****************************************************************************/
        /// <summary>
        /// Reads in from string and converts into a string
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="encoder">Text encoder. Will default to UTFEncoding</param>
        /// <returns>The resulting string</returns>
        internal static async Task<string> ReadStringAsync(this Stream stream, Encoding? encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            if(stream is MemoryStream memStream)
            { 
                var array = memStream.ToArray();
                var arrLen = array.Length;
                var str = encoder.GetString(memStream.ToArray());
                var atrLen = str.Length;

                return str;
            }

            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            try
            { 
                using(var mem = new MemoryStream())
                { 
                    await stream.CopyToAsync(mem).ConfigureAwait(false);

                    var str = encoder.GetString(mem.ToArray());
                        str = str.Trim();

                        return str;
                }
            }
            finally
            { 
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
            }
        }

        /****************************************************************************/
        /// <summary>
        /// Reads in from string and converts into a string
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="encoder">Text encoder. Will default to UTFEncoding</param>
        /// <returns>The resulting string</returns>
        internal static string ReadString(this Stream stream, Encoding? encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            try
            { 
                using(var mem = new MemoryStream())
                { 
                    stream.CopyTo(mem);

                    return encoder.GetString(mem.ToArray());
                }
            }
            finally
            { 
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
            }
        }    
    }
}
