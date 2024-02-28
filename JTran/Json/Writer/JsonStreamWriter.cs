/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: JsonStreamWriter.cs					    		        
 *        Class(es): JsonStreamWriter				         		            
 *          Purpose: Write json to to a stream                  
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

using JTran.Common;
using JTran.Extensions;
using System;
using System.Buffers;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;


namespace JTran
{
    /// <summary>
    /// Class for writing json to a stream 
    /// </summary>
    internal class JsonStreamWriter : JsonWriter, IDisposable
    {
        private readonly Stream _output;

        private readonly char[] _buffer;
        private int             _bufferWritten = 0;
        private const int       _bufferSize    = 4096;

        /****************************************************************************/
        public JsonStreamWriter(Stream output, int indent = 4) : base(indent)
        {
            _output = output;
            _buffer = new char[_bufferSize];
        }

        public JsonStreamWriter(int indent = 4) : base(indent)
        {
        }

        internal Stream Output => _output;

        /****************************************************************************/
        public void AppendComma()
        {
            AppendLine(',');
        }

        /****************************************************************************/
        [Obsolete]
        protected override void AppendLine(string line)
        {
             Append(line);
             AppendNewLine();
        }        

        /****************************************************************************/
        [Obsolete]
        protected override void Append(string text)
        {
            if(_bufferWritten + text.Length > _bufferSize)
                Flush();

            text.CopyTo(0, _buffer, _bufferWritten, text.Length);

            _bufferWritten += text.Length;
        }

        /****************************************************************************/
        protected override void Append(char ch)
        {
            if(_bufferWritten + 1 > _bufferSize)
                Flush();

            _buffer[_bufferWritten++] = ch;
        }

        /****************************************************************************/
        protected override void AppendLine(char ch)
        {
            if(_bufferWritten + 3 > _bufferSize)
                Flush();

            _buffer[_bufferWritten++] = ch;
            _buffer[_bufferWritten++] = '\r';
            _buffer[_bufferWritten++] = '\n';
        }

        /****************************************************************************/
        private void Flush()
        {
            if(_bufferWritten > 0)
            {
                var bytes = UTF8Encoding.Default.GetBytes(_buffer, 0, _bufferWritten); // ??? Use version that takes a buffer

                _output.Write(bytes, 0, bytes.Length);

                _bufferWritten = 0;
            }
        }

        /****************************************************************************/
        private void AppendNewLine()
        {
            if(_bufferWritten + 2 > _bufferSize)
                Flush();

            _buffer[_bufferWritten++] = '\r';
            _buffer[_bufferWritten++] = '\n';
        }

        /****************************************************************************/
        protected override void AppendLine(CharacterSpan line)
        {
            Append(line);
            AppendNewLine();
        }

        /****************************************************************************/
        protected override void AppendSpaces(int numSpaces)
        {
            if(_bufferWritten + numSpaces > _bufferSize)
                Flush();

            for(var i = 0; i < numSpaces; i++)
                _buffer[_bufferWritten++] = ' ';
        }        

        /****************************************************************************/
        protected override void Append(CharacterSpan text)
        {
             if(_bufferWritten + text.Length > _bufferSize)
                Flush();

             text.CopyTo(_buffer, _bufferWritten);

             _bufferWritten += text.Length;
        }

        /****************************************************************************/
        protected override void Append(Stream strm)
        {
            strm.CopyTo(_output);
        }

        /****************************************************************************/
        public void Dispose()
        {
            Flush();
        }

        /****************************************************************************/
        protected override CharacterSpan FormatForJsonOutput(CharacterSpan s)
        {
            return s.FormatForJsonOutput();
        }

       /****************************************************************************/
        protected override CharacterSpan FormatForOutput(object s, bool forceString = false)
        {
            return s.FormatForOutput(forceString, true);
        }
    }
}
