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
        char ReadNext();    
        void GoBack();    
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterReader : ICharacterReader
    {
        private string? _currentLine;
        private int     _position = -1;
        private char?    _back;
        private char    _last = '\0';
        private readonly TextReader _reader;

        internal CharacterReader(Stream stream) 
        { 
            //_reader = new StreamReader(stream);
            _reader = new StreamReader(stream, Encoding.UTF8, true, 64 * 1024);
        }

        public char ReadNext()
        {
            if(_back != null)
            {
                _last = _back.Value;
                _back = null;
                return _last;
            }

            while(_position == -1 || _position >= _currentLine.Length)
            {
                _currentLine = _reader.ReadLine();

                if(_currentLine == null)
                    throw new ArgumentOutOfRangeException(nameof(ReadNext));

                if(_currentLine.Length == 0)
                   continue;

                _position = 0;
                break;
            }

            return _last = _currentLine[_position++];
        
        }

        public void GoBack()
        {
            if(_last == '\0')
                throw new IndexOutOfRangeException(nameof(GoBack));

            _back = _last;
        }
    }
}
