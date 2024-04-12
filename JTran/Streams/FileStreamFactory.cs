/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: FileStreamFactory.cs					    		        
 *        Class(es): StreamFactory, FileStreamFactory				         		            
 *          Purpose: Classes for generating multiple streams and files                  
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

using System;
using System.IO;
using System.Threading.Tasks;

namespace JTran.Streams
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Class to generate files for outputting transformations to multiple outputs
    /// </summary>
    public abstract class StreamFactory : IStreamFactory, IAsyncDisposable
    {
        private readonly TaskDepot _taskDepot = new();

        /****************************************************************************/
        public StreamFactory() 
        {
        }

        /****************************************************************************/
        /// <summary>
        /// Creates a new stream. Note that the caller will not close the stream
        /// </summary>
        /// <param name="index">Index of the json object that will be output to the stream (zero based)</param>
        /// <returns>A new (open) stream</returns>
        public virtual Stream BeginStream(int index)
        {
            return new MemoryStream();
        }

        /****************************************************************************/
        /// <summary>
        /// Notification that the caller is done with the stream. The stream should be closed/disposed here
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="index"></param>
        public virtual void EndStream(Stream stream, int index)
        {
            _taskDepot.Add(SaveFile(stream, index));
        }

        protected abstract Task HandleStream(Stream stream, int index);

        /****************************************************************************/
        public ValueTask DisposeAsync()
        {
            return _taskDepot.DisposeAsync();
        }

        /****************************************************************************/
        private async Task SaveFile(Stream stream, int index)
        {
            try
            {
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);

                await HandleStream(stream, index);
            }
            finally 
            { 
                if(stream != null)
                    await stream.DisposeAsync();
            }
        }
    }    

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Class to generate files for outputting transformations to multiple outputs
    /// </summary>
    public class FileStreamFactory : StreamFactory
    {
        private readonly Func<int, string> _fileNameGenerator;

        /****************************************************************************/
        public FileStreamFactory(Func<int, string> fileNameGenerator) 
        {
            _fileNameGenerator = fileNameGenerator;
        }

        /****************************************************************************/
        protected override async Task HandleStream(Stream stream, int index)
        {
            var path = _fileNameGenerator(index);

            using var output = File.Create(path);

            await stream.CopyToAsync(output);
        }
    }    
}
