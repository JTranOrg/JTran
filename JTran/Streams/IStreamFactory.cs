/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: IStreamFactory.cs					    		        
 *        Class(es): IStreamFactory				         		            
 *          Purpose: Interface for generating multiple streams                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 4 Apr 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

 using System.IO;
 using System.Threading.Tasks;

namespace JTran.Streams
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Interface to generate streams for outputting transformations to multiple outputs
    /// </summary>
    public interface IStreamFactory
    {
        /// <summary>
        /// Creates a new stream. Note that the caller will not close the stream
        /// </summary>
        /// <param name="index">Index of the json object that will be output to the stream (zero based)</param>
        /// <returns>A new (open) stream</returns>
        Stream BeginStream(int index);

        /// <summary>
        /// Notification that the caller is done with the stream. The stream should be closed/disposed here
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="index"></param>
        void EndStream(Stream stream, int index);
    }    
}
