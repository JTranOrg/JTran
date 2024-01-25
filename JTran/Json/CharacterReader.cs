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
        char ReadNext(ref long lineNumber);    
        void GoBack();    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterReader : ICharacterReader, IDisposable
    {
        private string? _currentLine;
        private int     _position = -1;
        private char?   _back;
        private char    _last = '\0';
        private readonly TextReader _reader;

        internal CharacterReader(Stream stream) 
        { 
            _reader = new StreamReader(stream, Encoding.UTF8, true, 8 * 1024);
        }

       internal CharacterReader(string str) 
        { 
            _reader = new StringReader(str);
        }

        public char ReadNext(ref long lineNumber)
        {
            if(_back != null)
            {
                _last = _back.Value;
                _back = null;
                return _last;
            }

            if(_currentLine == null && _position != -1)
                throw new ArgumentOutOfRangeException(nameof(ReadNext));

            while(_position == -1 || _position >= (_currentLine?.Length ?? 0))
            {
                _currentLine = _reader.ReadLine();
                ++lineNumber;

                if(_currentLine == null)
                    throw new ArgumentOutOfRangeException(nameof(ReadNext));

                if(_currentLine.Length == 0)
                   continue;

                _position = 0;
                break;
            }

            return _last = _currentLine![_position++];
        
        }

        public void GoBack()
        {
            if(_last == '\0')
                throw new IndexOutOfRangeException(nameof(GoBack));

            _back = _last;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
