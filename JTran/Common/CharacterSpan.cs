/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran.Common							            
 *             File: CharacterSpan.cs					    		        
 *        Class(es): ICharacterSpan, CharacterSpan
 *          Purpose: Class/Interface to reference a span of characters                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 21 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JTran.Common
{
    /****************************************************************************/
    /****************************************************************************/
    internal interface ICharacterSpan : IReadOnlyList<char>, IEquatable<ICharacterSpan>, IStringValue
    {
        bool IsJTranProperty    { get; }
        int  Length             { get; }

        bool            IsNullOrWhiteSpace(int start = 0, int length = -1);
        void            CopyTo(char[] buffer, int offset, int length = -1);
        bool            Contains(ICharacterSpan find);
        bool            StartsWith(string compare);
        int             IndexOf(char ch, int start = 0);
        int             IndexOf(ICharacterSpan find);
        ICharacterSpan  SubstringBefore(char ch, int skip = 0);
        ICharacterSpan  SubstringAfter(char ch);
        ICharacterSpan  Substring(int index, int length = -1000);
        ICharacterSpan  FormatForJsonOutput(bool addDoubleQuotes = false);
        decimal         ParseNumber();
        bool            TryParseNumber(out decimal result);
        bool            Equals(string compare);
        bool            Equals(ICharacterSpan compare);
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterSpan : ICharacterSpan
    {
        private readonly char[]? _source;
        private readonly int     _offset;
        private readonly int     _length;
        private int              _hashCode = 0;
        private readonly bool    _isJTranProperty;

        #region Constructors

        /****************************************************************************/
        internal CharacterSpan()
        {
            _source = null;
            _offset = 0;
            _length = 0;

            _isJTranProperty = false;
        }

        /****************************************************************************/
        internal CharacterSpan(char[] source, int offset, int length, bool jtranProp = false)
        {
            _source          = source;
            _offset          = offset;
            _length          = length;
            _isJTranProperty = jtranProp;

            if(jtranProp)
                _ = GetHashCode();
        }

       /****************************************************************************/
        internal CharacterSpan(string source, bool jtranProp = false)
        {
            _source          = source.ToArray();
            _offset          = 0;
            _length          = source.Length;
            _isJTranProperty = jtranProp;
        }

        /****************************************************************************/
        internal CharacterSpan(ICharacterSpan source, int offset, int length)
        {
            if(source is CharacterSpan cspan)
            { 
                _source = cspan._source;
                _offset = cspan._offset + offset;
            }
            else 
            {
                // ???
            }

            _length          = length;
            _isJTranProperty = false;

            if(_offset + _length > _source?.Length)
                throw new ArgumentOutOfRangeException();
        }

        #endregion

        internal static readonly ICharacterSpan Empty = new CharacterSpan();

        /****************************************************************************/
        internal static ICharacterSpan FromString(string s, bool addDoubleQuotes = false, bool isJtranProp = false)
        {
            // ??? addDoubleQuotes
            // ??? need a lightweight version of ICharacterSpan that wraps a string and make common interface for both, e.g. IString
            return new CharacterSpan(s, isJtranProp);
        }
        
        /****************************************************************************/
        internal static bool IsNullOrWhiteSpace(char ch)
        {
            switch(ch)
            {
                case ' ': 
                case '\r':
                case '\n':
                case '\t': return true;
                default:   return false;
            }
        }
                        
        /****************************************************************************/
        internal static ICharacterSpan Join(IList<ICharacterSpan> list, char separator)
        {
            var length = list.Count-1;

            foreach(var item in list) 
                length += item.Length;
            
            var buffer = new char[length];
            var offset = 0;

            for(var i = 0; i < list.Count; ++i)
            { 
                var item = list[i];

                item.CopyTo(buffer, offset);

                offset += item.Length;

                if(i < list.Count-1)
                    buffer[offset++] = separator;
            }

            return new CharacterSpan(buffer, 0, length);
        }

        internal static readonly ICharacterSpan True  = CharacterSpan.FromString("true");
        internal static readonly ICharacterSpan False = CharacterSpan.FromString("false");
        internal static readonly ICharacterSpan Null  = CharacterSpan.FromString("null");

        #region ICharacterSpan

        public virtual int Length           => _length;
        public bool        IsJTranProperty  => _isJTranProperty;

        /****************************************************************************/
        public bool IsNullOrWhiteSpace(int start = 0, int length = -1)
        {
            if(start >= this.Length)
                return true;

            if(length == -1)
                length = this.Length - (int)start;

            var n = start + length;

            for(var i = start; i < n; ++i)
            { 
                if(!IsNullOrWhiteSpace(this[i]))
                    return false;
            }

            return true;
        }

        /****************************************************************************/
        public bool Contains(ICharacterSpan find)
        {
            return IndexOf(find) != -1;
        }

        /****************************************************************************/
        public bool StartsWith(string compare)
        {
            if(this.Length < compare.Length)
                return false;

            for(var i = 0; i < compare.Length; ++i)
            { 
                if(this[i] != compare[i])
                    return false;
            }

            return true;
        }

        /****************************************************************************/
        public int IndexOf(char ch, int start = 0)
        {
            for(var i = start; i < (this.Length - start); ++i)
            { 
                if(this[i] == ch)
                    return i;
            }

            return -1;
        }

        /****************************************************************************/
        public int IndexOf(ICharacterSpan find)
        {
            if(this.Length < find.Length || find.Length == 0 || this.Length == 0)
                return -1;

            var i = 0;

            while((this.Length-i) >= find.Length)
            {
                var index = this.IndexOf(find[0], i);

                if(index == -1)
                    return -1;
                
                for(var j = 1; j < find.Length-1; ++j)
                {
                    if(this[index + j] != find[j])
                        goto Next;
                }

                return i;
                
              Next:
                ++i;
            }

            return -1;
        }

        /****************************************************************************/
        public ICharacterSpan SubstringBefore(char ch, int skip = 0)
        {
            for(var i = skip; i < this.Length; ++i)
            { 
                if(this[i] == ch)
                    return new CharacterSpan(this, skip, i-skip);
            }

            if(skip != 0)
                return this.Substring(skip, this.Length - skip);

            return this;
        }

        /****************************************************************************/
        public ICharacterSpan SubstringAfter(char ch)
        {
            for(var i = 0; i < this.Length; ++i)
            { 
                if(this[i] == ch)
                {
                    if((this.Length - i - 1) == 0)
                        return Empty;
                    
                    return new CharacterSpan(this, i+1, this.Length - i - 1);
                }
            }

            return Empty;
        }

        /****************************************************************************/
        public ICharacterSpan Substring(int index, int length = -1000)
        {
            return new CharacterSpan(this, index, length == -1000 ? this.Length - index : (length < 0 ? this.Length - index + length : length));
        }

        /****************************************************************************/
        public char PeekChar() 
        {
            return this[0];
        }

        /****************************************************************************/
        public ICharacterSpan FormatForJsonOutput(bool addDoubleQuotes = false) 
        {
            if(this.Length == 0 && !addDoubleQuotes)
                return this;

            var bufferIndex = 0;
            char[]? buffer = GetBuffer(null, -1, addDoubleQuotes, ref bufferIndex);

            for(int i = 0; i < this.Length; ++i)
            {
                var ch = this[i];

                if(_escapeCharacters.ContainsKey(ch))
                {
                    var replacement = _escapeCharacters[ch];

                    if(buffer == null)
                        buffer = GetBuffer(buffer, i, addDoubleQuotes, ref bufferIndex);
                    else 
                        buffer = CheckBuffer(buffer, bufferIndex);
                     
                    buffer[bufferIndex++] = '\\';
                    buffer[bufferIndex++] = replacement;
                }
                else if(buffer != null)
                { 
                    if(bufferIndex == buffer.Length)
                        Array.Resize(ref buffer, bufferIndex + 32);

                    buffer[bufferIndex++] = ch;
                }
            }

            if(addDoubleQuotes)
            {
                if((bufferIndex+2) >= buffer.Length)
                    bufferIndex += 0;

                buffer = CheckBuffer(buffer, bufferIndex);
                buffer[bufferIndex++] = '"';
            }

            if(buffer != null)
                return new CharacterSpan(buffer, 0, bufferIndex);

            return this;
        }

        /****************************************************************************/
        public decimal ParseNumber() 
        {
            if(TryParseNumber(out decimal result))
                return result;

            return 0m;
        }
        
        /****************************************************************************/
        public bool TryParseNumber(out decimal result)
        {
            result = 0m;

            if(this.Length == 0)
                return false;                

            var multiplier = 1m;
            var negative   = 1m;
            var isDecimal = false;

            for(var i = 0; i < this.Length; ++i)
            {
                var ch = this[i];

                if(char.IsDigit(ch))
                {
                    if(isDecimal)
                    {
                        var add = (ch - '0') / multiplier;

                        result += add;
                        multiplier *= 10m;
                    }
                    else
                    {
                        result *= multiplier;
                        result += ch - '0';
                        multiplier = 10m;
                    }
                 }
                else if(ch == '.')
                {
                    isDecimal = true;
                    multiplier = 10m;
                }
                else if(ch == '-')
                {
                    if(i != 0 || negative == -1m)
                        throw new JsonParseException("Missplaced negative sign", 0L);

                    negative = -1m;
                }
                else
                    return false;
            }

            result *= negative;
                
            return true;
        }

        /****************************************************************************/
        public bool Equals(string compare)
        {
            if(compare == null)
                return _source == null;

            var len = compare.Length;

            if(this.Length == 0)
                return len == 0;

            if(len != this.Length)
                return false;

            for (var i = 0; i < compare.Length; ++i) 
            {
                if(this[i] != compare[i])
                    return false;
            }

            return true;
        }
        
        /****************************************************************************/
        public virtual void CopyTo(char[] buffer, int offset, int length = -1)
        {
            if(_length != 0)
                Array.Copy(_source, _offset, buffer, offset, length == -1 ? _length : length);
        }

        #endregion

        /****************************************************************************/
        public override string ToString()
        {
            if(this.Length == 0)
                return String.Empty;

            return new String(_source, _offset, _length);
        }

        /****************************************************************************/
        public override int GetHashCode()
        {
            if(_hashCode == 0)
            { 
                if(this.Length == 0)
                    _hashCode = 0;
                else
                { 
                    var comparer = EqualityComparer<int>.Default;

                    for(int i = 0; i < this.Length; i += 2) 
                    { 
                        var value1 = (int)this[i];
                        var value2 = (i < this.Length-1) ? (int)this[i+1] : 0;
                        var value  = value1 << 16 | value2;
                        
                        _hashCode = i == 0 ? comparer.GetHashCode(value) : CombineHashCodes(_hashCode, comparer.GetHashCode(value));
                    }
                }
            }

            return _hashCode;
        }        

        #region IStringValue

        public object? Value => this;

        #endregion

        #region IReadOnlyList

        public int Count => this.Length;

        public virtual char this[int index] => _source![_offset + index];

        public IEnumerator<char> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable

        public bool Equals(ICharacterSpan other)
        {
            if(other == null)
                return _source == null;

            return this.GetHashCode() == other.GetHashCode();
        }

        #endregion

        #region Private

        /****************************************************************************/
        private static readonly Dictionary<char, char> _escapeCharacters = new Dictionary<char, char> 
        {
            { '\\', '\\' },
            { '"',  '"' },
            { '\r', 'r' },
            { '\n', 'n' },
            { '\t', 't' },
            { '\f', 'f' },
            { '\b', 'b' }
        };
        
        /****************************************************************************/
        private char[] GetBuffer(char[]? buffer, int index, bool addDoubleQuotes, ref int bufferIndex) 
        {        
            if(buffer == null && !addDoubleQuotes && index == -1)
                return buffer;

            buffer = new char[(int)((this.Length + (addDoubleQuotes ? 2 : 0)) / 16) * 16 + 16];

            if(addDoubleQuotes)
                buffer[0] = '\"';

            if(index > 0)
                Array.Copy(_source, _offset, buffer,  addDoubleQuotes ? 1 : 0, index);

            bufferIndex = index == -1 ? 0 : index;

            if(addDoubleQuotes)
                ++bufferIndex;

            return buffer;
        }

        /****************************************************************************/
        private char[] CheckBuffer(char[] buffer, int bufferIndex) 
        {        
            if((bufferIndex + 2) >= buffer.Length)
                Array.Resize(ref buffer, (int)(bufferIndex / 16) * 16 + 32);

            return buffer;
        }
        
        private static bool IsEscapedCharacter(char ch)
        {
            if(ch == '\\') return true;
            if(ch == '"')  return true;
            if(ch == '\r') return true;
            if(ch == '\n') return true;
            if(ch == '\t') return true;
            if(ch == '\f') return true;
            if(ch == '\b') return true;

            return false;
        }

        /****************************************************************************/
        private static int CombineHashCodes(int h1, int h2) 
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterSpanGroup 
    { 
        private readonly Dictionary<string, ICharacterSpan> _dict = new();

        /****************************************************************************/
        internal CharacterSpanGroup()
        {
        }

        /****************************************************************************/
        internal ICharacterSpan Get(string key)
        {
            if(!_dict.ContainsKey(key))
                _dict.Add(key, CharacterSpan.FromString(key));

            return _dict[key];
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterSpanBuilder
    {
        private char[]       _buffer;
        private int          _offset = 0;
        private int          _length = 0;
        private readonly int _bufferSize;

        /****************************************************************************/
        internal CharacterSpanBuilder(int bufferSize = 16 * 1024)
        {
            _bufferSize = bufferSize;
            _buffer = new char[_bufferSize];
        }

        internal int Length => _length;

        /****************************************************************************/
        internal void Append(char ch)
        {
            if((_offset + _length + 1) > _bufferSize)
            {
                var newBuffer = new char[_bufferSize];

                Array.Copy(_buffer, _offset, newBuffer, 0, _length);

                _offset = 0;
                _buffer = newBuffer;
            }

            _buffer[_offset + _length++] = ch;
        }

        /****************************************************************************/
        public ICharacterSpan Current  
        {
            get
            { 
                var result = new CharacterSpan(_buffer, _offset, _length);

                _offset += _length;
                _length = 0;

                return result;
            }
        }
    }
}
