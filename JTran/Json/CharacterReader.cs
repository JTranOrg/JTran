/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran.Json						            
 *             File: CharacterReader.cs					    		        
 *        Class(es): CharacterReader				         		            
 *          Purpose: Reads characters from a stream of string 1 character at a time               
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 19 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

using JTran.Common;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{
    /****************************************************************************/
    /****************************************************************************/
    internal interface ICharacterReader
    {
        bool ReadNext(bool skipWhiteSpace = false, bool quoted = false);    
        void GoBack();    
        char Current    { get; }
        long LineNumber { get; }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterReader : ICharacterReader, IDisposable
    {
        private int       _position = -1;
        private char?     _back;
        private char      _last = '\0';
        private long      _lineNumber = 1L;
        private char      _ch = '\0';
        private char[]    _buffer;
        private int       _bufferRead = 0;
        private int       _bufferSize = 64 * 1024;

        private TextReader? _reader;

        internal CharacterReader(Stream stream) 
        { 
            var streamBufferSize = (int)(_bufferSize*1.5);
            var streamLen        = stream.Length;

            while(_bufferSize > streamLen && _bufferSize > 4096)
            {
                int newBufferSize = _bufferSize/2;

                if(newBufferSize < streamLen)
                    break;

                _bufferSize = newBufferSize;
            }

            streamBufferSize = (int)(_bufferSize);

            _reader = new StreamReader(stream, Encoding.UTF8, true, streamBufferSize);
            _buffer = new char[_bufferSize];
        }

        ~CharacterReader()
        {
            Dispose();
        }

        internal CharacterReader(string str) 
        { 
            _reader = new StringReader(str);
            _buffer = new char[_bufferSize];
        }

        public char Current    => _ch;
        public long LineNumber => _lineNumber;

        public bool ReadNext(bool skipWhiteSpace, bool quoted = false)
        {
            return InternalReadNext('\0', skipWhiteSpace, quoted);
        }

        private bool InternalReadNext(char prev, bool skipWhiteSpace, bool quoted)
        {
            while(true)
            { 
                if(_back != null)
                {
                    _last = _back.Value;
                    _back = null;
                    _ch   = _last;

                    return true;
                }

                if(_bufferRead == 0 || _position >= _bufferRead)
                {
                    _bufferRead = _reader.ReadBlock(_buffer, 0,_bufferSize);
                    
                    if(_bufferRead == 0)
                    {
                        _ch = '\0';
                        return false;
                    }

                    _position = 0;
                }

                var ch = _buffer[_position++];   

                if(ch == '\r' || ch == '\n')
                { 
                    if(quoted)
                    { 
                        _ch = ch;
                        ++_lineNumber;
                        return true;
                    }

                    return InternalReadNext(ch, skipWhiteSpace, false);
                }

                 if(prev == '\r' || prev == '\n')
                    ++_lineNumber;

                if(skipWhiteSpace && (ch == ' ' || ch == '\t'))
                { 
                    prev = ch;
                    continue;
                }

                _ch = _last = ch;
                break;
            }

            return true;
        }

        public void GoBack()
        {
            if(_last == '\0')
                throw new IndexOutOfRangeException(nameof(GoBack));

            _back = _last;
        }

        public void Dispose()
        {
            if(_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }
    }
}
