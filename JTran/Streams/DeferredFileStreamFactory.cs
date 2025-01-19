/***************************************************************************
 *                                                                          
 *    JTran.Project - Consolidates all of the attributes necessary to do a transform  							                    
 *                                                                          
 *        Namespace: JTran.Streams							            
 *             File: DeferredFileStreamFactory.cs					    		        
 *        Class(es): DeferredFileStreamFactory			         		            
 *          Purpose: Provides streams for outputting array transforms to multiple files                
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 11 Apr 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.IO;

namespace JTran.Streams
{
    /****************************************************************************/
    /****************************************************************************/
    internal class DeferredFileStreamFactory : IStreamFactory
    {
        private readonly string _baseFilePath;

        /****************************************************************************/
        internal DeferredFileStreamFactory(string baseFilePath) 
        {
            _baseFilePath = baseFilePath;
        }

        internal DeferredFileStream? CurrentStream { get; private set; }

        /****************************************************************************/
        /// <summary>
        /// Creates a new stream. Note that the caller will not close the stream
        /// </summary>
        /// <param name="index">Index of the json object that will be output to the stream (zero based)</param>
        /// <returns>A new (open) stream</returns>
        public virtual Stream BeginStream(int index)
        {
            return this.CurrentStream = new DeferredFileStream(string.Format(_baseFilePath, index));
        }

        /****************************************************************************/
        /// <summary>
        /// Notification that the caller is done with the stream. The stream should be closed/disposed here
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="index"></param>
        public virtual void EndStream(Stream stream, int index)
        {
            this.CurrentStream?.Dispose();
            this.CurrentStream = null;
        }
    }
}
