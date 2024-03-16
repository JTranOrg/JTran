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
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface ICharacterSpan : IReadOnlyList<char>, IEquatable<ICharacterSpan>, IStringValue
    {
        int  Length               { get; }
        bool HasEscapeCharacters  { get; }

        void                    CopyTo(char[] buffer, int offset, int length = -1);
        bool                    StartsWith(string compare);
        int                     IndexOf(char ch, int start = 0);
        int                     IndexOf(ICharacterSpan find, int start = 0);
        ICharacterSpan          SubstringBefore(char ch, int skip = 0);
        ICharacterSpan          SubstringAfter(char ch);
        ICharacterSpan          Substring(int index, int length = -1000);
        ICharacterSpan          FormatForJsonOutput();
        IList<ICharacterSpan>   Split(char separator, bool trim = true);
        IList<ICharacterSpan>   Split(ICharacterSpan separator, bool trim = true);
        bool                    Equals(string compare);

        #region Default Implementations

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
                        return false;

                    negative = -1m;
                }
                else
                    return false;
            }

            result *= negative;
                
            return true;
        }

        /****************************************************************************/
        public bool IsNullOrWhiteSpace(int start = 0, int length = -1)
        {
            if(this == null)
                return true;

            if (start >= this.Length)
                return true;

            if(length == -1)
                length = this.Length - (int)start;

            var n = start + length;

            for(var i = start; i < n; ++i)
            { 
                if(!CharacterSpan.IsNullOrWhiteSpace(this[i]))
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
        public bool Contains(char ch)
        {
            return IndexOf(ch) != -1;
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class CharacterSpan : ICharacterSpan
    {
        private char[]? _source;
        private int     _offset;
        private int     _length;
        private int     _hashCode = 0;
        private bool    _hasEscapeCharacters;
        private string? _str;

        private static Dictionary<string, ICharacterSpan> _cache = new();

        #region Constructors

        /****************************************************************************/
        internal CharacterSpan()
        {
            _source = null;
            _offset = 0;
            _length = 0;
        }

        /****************************************************************************/
        internal CharacterSpan(char[] source, int offset, int length = -1, bool hasEscapeCharacters = false)
        {
            _source              = source;
            _offset              = offset;
            _length              = length == -1 ? source.Length : length;
            _hasEscapeCharacters = hasEscapeCharacters;
        }

        /****************************************************************************/
        internal CharacterSpan(string source)
        {
            _source              = source.ToArray();
            _offset              = 0;
            _length              = source.Length;
            _hasEscapeCharacters = true;
        }

        /****************************************************************************/
        internal CharacterSpan(ICharacterSpan source, int offset = 0, int length = -1)
        {
            if(source is CharacterSpan cspan)
            { 
                _source = cspan._source;
                _offset = cspan._offset + offset;
            }
            else 
            {
                throw new NotSupportedException();
            }

            _length          = length == -1 ? source.Length - offset : length;
            _hasEscapeCharacters = source.HasEscapeCharacters;

            if(_offset + _length > _source?.Length)
                throw new ArgumentOutOfRangeException();
        }

        #endregion

        /****************************************************************************/
        internal static ICharacterSpan Clone(ICharacterSpan source)
        {
            if(source is CharacterSpan cspan)
            {
                var buffer = new char[source.Length];

                Array.Copy(cspan._source, cspan._offset, buffer, 0, source.Length);

                return new CharacterSpan(buffer, 0, source.Length, source.HasEscapeCharacters);
            }

            throw new NotSupportedException();
        }

        internal static readonly ICharacterSpan Empty = new CharacterSpan();

        /****************************************************************************/
        internal void Reset(char[] source, int offset, int length, bool hasEscapeCharacters = false)
        {
            _source              = source;
            _offset              = offset;
            _length              = length;
            _hasEscapeCharacters = hasEscapeCharacters;
            _hashCode            = 0;
        }

        /****************************************************************************/
        internal static ICharacterSpan FromString(string s, bool cacheable = false)
        {
            if(cacheable && _cache.ContainsKey(s))
                return _cache[s];

            var cspan = new CharacterSpan(s);

            if(cacheable)
                _cache.TryAdd(s, cspan);

            cspan._str = s;

            return cspan;
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

        public virtual int Length              => _length;
        public bool        HasEscapeCharacters => _hasEscapeCharacters;

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
            for(var i = start; i < this.Length; ++i)
            { 
                if(this[i] == ch)
                    return i;
            }

            return -1;
        }

        /****************************************************************************/
        public int IndexOf(ICharacterSpan find, int start = 0)
        {
            var thisLength = this.Length;
            var findLength = find.Length;

            if(thisLength < findLength || findLength == 0 || thisLength == 0)
                return -1;

            var i = start;

            while((thisLength-i) >= findLength)
            {
                var index = this.IndexOf(find[0], i);

                if(index == -1)
                    return -1;
                
                for(var j = 1; j < findLength-1; ++j)
                {
                    if(this[index + j] != find[j])
                        goto Next;
                }

                return index;
                
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
        public ICharacterSpan FormatForJsonOutput() 
        {
            if(this.Length == 0 || !this.HasEscapeCharacters)
                return this;

            var bufferIndex = 0;
            char[]? buffer = GetBuffer(null, -1, ref bufferIndex);

            for(int i = 0; i < this.Length; ++i)
            {
                var ch = this[i];

                if(_escapeCharacters.ContainsKey(ch))
                {
                    var replacement = _escapeCharacters[ch];

                    if(buffer == null)
                        buffer = GetBuffer(buffer, i, ref bufferIndex);
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

            if(buffer != null)
                return new CharacterSpan(buffer, 0, bufferIndex);

            return this;
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

        /****************************************************************************/
        public IList<ICharacterSpan> Split(ICharacterSpan separator, bool trim = true)
        {
            ICharacterSpan cspan = this;

            if(!cspan.Contains(separator))
                return [this];

            var list = new List<ICharacterSpan>();
            var start = 0;

            while(start < _length) 
            { 
                var index = this.IndexOf(separator, start);

                if(index == -1)
                {
                    list.Add(Create(_source!, _offset + start, _length - start, this.HasEscapeCharacters, trim));
                    break;
                }

                list.Add(Create(_source!, _offset + start, index - start, this.HasEscapeCharacters, trim));

                start = index + separator.Length;
            }

            return list;
        }

        /****************************************************************************/
        public IList<ICharacterSpan> Split(char separator, bool trim = true)
        {
            ICharacterSpan cspan = this;

            if(!cspan.Contains(separator))
                return [this];

            var list = new List<ICharacterSpan>();
            var start = 0;

            while(start < _length) 
            { 
                var index = this.IndexOf(separator, start);

                if(index == -1)
                {
                    list.Add(Create(_source!, _offset + start, _length - start, this.HasEscapeCharacters, trim));
                    break;
                }

                list.Add(Create(_source!, _offset + start, index - start, this.HasEscapeCharacters, trim));

                start = index + 1;
            }

            return list;
        }

        /****************************************************************************/
        private static ICharacterSpan Create(char[] source, int offset, int length, bool hasEscapeCharacters, bool trim)
        {
            if(trim)
                return Trim(source!, offset, length, hasEscapeCharacters);

            return new CharacterSpan(source!, offset, length, hasEscapeCharacters);
        }

        /****************************************************************************/
        internal static ICharacterSpan Trim(char[] source, int offset, int length, bool hasEscapeCharacters = false)
        {
            var nonWhitespaceBegin = IndexOfNonWhiteSpace(source, offset, length);

            if(nonWhitespaceBegin == -1)
                return CharacterSpan.Empty;

            var nonWhitespaceEnd = LastIndexOfNonWhiteSpace(source, nonWhitespaceBegin, length - (nonWhitespaceBegin - offset));
            
            length = nonWhitespaceEnd - nonWhitespaceBegin + 1;

            return new CharacterSpan(source!, nonWhitespaceBegin, length, hasEscapeCharacters);
        }

        /****************************************************************************/
        internal static int IndexOfNonWhiteSpace(char[]? source, int start, int length)
        {
            if(source == null)
                return -1;

            var end = start + length;

            while(start < end) 
            {
                if(!char.IsWhiteSpace(source[start]))
                    return start;

                ++start;
            }

            return -1;
        }

        /****************************************************************************/
        internal static int LastIndexOfNonWhiteSpace(char[]? source, int start, int length)
        {
            if(source == null)
                return -1;

            var end = start + length;

            while(start < end) 
            {
                if(!char.IsWhiteSpace(source[end-1]))
                    return end-1;

                end = start + --length;
            }

            return -1;
        }

        #endregion

        /****************************************************************************/
        public override string ToString()
        {
            if(this.Length == 0)
                return String.Empty;

            if(_str != null)
                return _str;

            return _str = new String(_source!, _offset, _length);
        }

        /****************************************************************************/
        public static int GetHashCode(ICharacterSpan cspan)
        {
            var comparer = EqualityComparer<int>.Default;
            var hashCode = 0;

            for(int i = 0; i < cspan.Length; i += 2) 
            { 
                var value1 = (int)cspan[i];
                var value2 = (i < cspan.Length-1) ? (int)cspan[i+1] : 0;
                var value  = value1 << 16 | value2;
                        
                hashCode = i == 0 ? comparer.GetHashCode(value) : CombineHashCodes(hashCode, comparer.GetHashCode(value));
            }

            return hashCode;
        }

        /****************************************************************************/
        public override int GetHashCode()
        {
            if(_hashCode == 0)
            { 
                if(this.Length == 0)
                    _hashCode = 0;
                else
                    _hashCode = GetHashCode(this);
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

        /****************************************************************************/
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
        private char[] GetBuffer(char[]? buffer, int index, ref int bufferIndex) 
        {        
            if(buffer == null && index == -1)
                return buffer;

            buffer = new char[(int)((this.Length) / 16) * 16 + 16];

            if(index > 0)
                Array.Copy(_source, _offset, buffer, 0, index);

            bufferIndex = index == -1 ? 0 : index;

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
    internal class CharacterSpanBuilder
    {
        private char[] _buffer;
        private int    _length = 0;
        private int    _bufferSize;
        private readonly CharacterSpan _cspan = new();

        /****************************************************************************/
        internal CharacterSpanBuilder(int bufferSize = 16 * 1024)
        {
            _bufferSize = bufferSize;
            _buffer = new char[_bufferSize];
        }

        internal int Length => _length;
        internal bool HasEscapeCharacters { get; set; }

        /****************************************************************************/
        internal void Append(char ch)
        {
            // 16k should be enough but just in case
            if((_length + 1) > _bufferSize)
            {
                _bufferSize = (int)((_bufferSize + _length + 1) / 1024) * 1024 + 1024;

                Array.Resize(ref _buffer, _bufferSize);
            }

            _buffer[_length++] = ch;
        }

        /****************************************************************************/
        public ICharacterSpan Current  
        {
            get
            { 
                _cspan.Reset(_buffer, 0, _length, hasEscapeCharacters: HasEscapeCharacters);

                _length = 0;

                return _cspan;
            }
        }
    }
}
