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

using System;
using System.IO;
using System.Text;

using JTran.Common;
using JTran.Extensions;

namespace JTran
{
    /// <summary>
    /// Class for writing json to a stream 
    /// </summary>
    internal class JsonStreamWriter : JsonWriter, IDisposable
    {
        private readonly Stream? _output;

        private readonly char[]? _buffer;
        private int              _bufferWritten = 0;
        private const int        _bufferSize    = 4096;

        /****************************************************************************/
        public JsonStreamWriter(Stream output, int indent = 4) : base(indent)
        {
            _output = output;
            _buffer = new char[_bufferSize];
        }

        /****************************************************************************/
        public JsonStreamWriter(int indent = 4) : base(indent)
        {
        }

        internal Stream Output => _output;

        /****************************************************************************/
        protected override void Append(char ch)
        {
            if(_bufferWritten + 1 > _bufferSize)
                Flush();

            _buffer![_bufferWritten++] = ch;
        }

        /****************************************************************************/
        protected override void AppendLine(char ch)
        {
            if(_bufferWritten + 3 > _bufferSize)
                Flush();

            _buffer![_bufferWritten++] = ch;
            _buffer[_bufferWritten++] = '\r';
            _buffer[_bufferWritten++] = '\n';
        }

        /****************************************************************************/
        protected override void AppendBoolean(bool bval)
        {
            if(_bufferWritten + (bval ? 4 : 5) > _bufferSize)
                Flush();

             if(bval)
             { 
                _buffer![_bufferWritten++] = 't';
                _buffer[_bufferWritten++] = 'r';
                _buffer[_bufferWritten++] = 'u';
                _buffer[_bufferWritten++] = 'e';
             }
             else
             { 
                _buffer![_bufferWritten++] = 'f';
                _buffer[_bufferWritten++] = 'a';
                _buffer[_bufferWritten++] = 'l';
                _buffer[_bufferWritten++] = 's';
                _buffer[_bufferWritten++] = 'e';
             }
        }

        /****************************************************************************/
        protected override void AppendNull()
        {
            if(_bufferWritten + 4 > _bufferSize)
                Flush();

            _buffer![_bufferWritten++] = 'n';
            _buffer[_bufferWritten++] = 'u';
            _buffer[_bufferWritten++] = 'l';
            _buffer[_bufferWritten++] = 'l';
        }

        /****************************************************************************/
        protected override void AppendNumber(decimal val)
        {
            Append(CharacterSpan.FromString(val.ToString().ReplaceEnding(".0", ""))); 
        }

        /****************************************************************************/
        protected override void AppendNewline()
        {
            if(_bufferWritten + 2 > _bufferSize)
                Flush();

            _buffer![_bufferWritten++] = '\r';
            _buffer[_bufferWritten++] = '\n';
        }

        private byte[] _flushBuffer = new byte[512];

        /****************************************************************************/
        private void Flush()
        {
            if(_bufferWritten > 0)
            {
                var numBytes = UTF8Encoding.Default.GetByteCount(_buffer, 0, _bufferWritten);

                if(numBytes > _flushBuffer.Length)
                    _flushBuffer = new byte[((int)(numBytes / 32) * 32) + 32];

                numBytes = UTF8Encoding.Default.GetBytes(_buffer, 0, _bufferWritten, _flushBuffer, 0);

                _output!.Write(_flushBuffer, 0, numBytes);

                _bufferWritten = 0;
            }
        }

        /****************************************************************************/
        protected void AppendNewLine()
        {
            if(_bufferWritten + 2 > _bufferSize)
                Flush();

            _buffer![_bufferWritten++] = '\r';
            _buffer[_bufferWritten++] = '\n';
        }

        /****************************************************************************/
        protected override void AppendLine(ICharacterSpan line)
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
                _buffer![_bufferWritten++] = ' ';
        }        

        /****************************************************************************/
        protected override void Append(ICharacterSpan text)
        {
             if(_bufferWritten + text.Length > _bufferSize)
                Flush();

             text.CopyTo(_buffer!, _bufferWritten);

             _bufferWritten += text.Length;
        }

        /****************************************************************************/
        public void Dispose()
        {
            Flush();
        }
    }
}
