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
        bool ExpressionResult     { get; set; }

        void                    CopyTo(char[] buffer, int offset, int length = -1);
        bool                    StartsWith(string compare);
        ICharacterSpan          SubstringBefore(char ch, int skip = 0);
        ICharacterSpan          SubstringAfter(char ch);
        ICharacterSpan          Substring(int index, int length = -1000, bool trim = false);
        ICharacterSpan          FormatForJsonOutput();
        IList<ICharacterSpan>   Split(char separator, bool trim = true);
        IList<ICharacterSpan>   Split(ICharacterSpan separator, bool trim = true);
        bool                    Equals(string compare);
        ICharacterSpan          Trim(bool start = true, bool end = true);
        ICharacterSpan          Transform(Func<char, (bool Use, char NewVal)> transform);
        ICharacterSpan          Pad(char padChar, int length, bool left);
        ICharacterSpan          Remove(ICharacterSpan remove, int start = 0);
        ICharacterSpan          Replace(ICharacterSpan find, ICharacterSpan replace, int start = 0);
        ICharacterSpan          ReplaceEnding(ICharacterSpan find, ICharacterSpan replace);
        ICharacterSpan          Concat(ICharacterSpan val);

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
        public bool Find(ICharacterSpan find, out int index)
        {
            index = IndexOf(find);

            return index != -1;
        }

        /****************************************************************************/
        public bool Contains(char ch)
        {
            return IndexOf(ch) != -1;
        }

        /****************************************************************************/
        public bool EndsWith(ICharacterSpan ending)
        {
            var endLen = ending?.Length ?? 0;

            if(endLen == 0 || endLen > this.Length)
                return false;

            if(endLen == this.Length)
                return this.Equals(ending!);

            var x = this.Length-1;

            for(var i = endLen-1; i >= 0; --i)
                if(this[x--] != ending![i])
                    return false;

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
        public int LastIndexOf(ICharacterSpan find, int start = 0)
        {
            var thisLength = this.Length;
            var findLength = find.Length;

            if(thisLength < findLength || findLength == 0 || thisLength == 0)
                return -1;

            var index = this.IndexOf(find, start);

            if(index == -1)
                return -1;
            
            while((start + (findLength*2)) < thisLength)
            {
                start += findLength;

                var newIndex = this.IndexOf(find, start);

                if(newIndex == -1)
                    return index;

                index = newIndex;
            }

            return index;
        }

        /****************************************************************************/
        public int LastIndexOf(char ch, int start = 0)
        {
            for(var i = this.Length-1; i >= start; --i)
            { 
                if(this[i] == ch)
                    return i;
            }

            return -1;
        }

        /****************************************************************************/
        public ICharacterSpan LastItemIn(char separator)
        {
            var index = LastIndexOf(separator);

            if(index == -1) 
                return this;

            return Substring(index + 1, this.Length - index - 1);
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
        private string? _str;
        private bool    _hasEscapeCharacters;

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
        internal static CharacterSpan Clone(ICharacterSpan source)
        {
            if(source is CharacterSpan cspan)
            {
                var buffer = new char[source.Length];

                Array.Copy(cspan._source!, cspan._offset, buffer, 0, source.Length);

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
            _str                 = null;
        }

        /****************************************************************************/
        private ICharacterSpan Reset(int offset, int length)
        {
            _offset              = offset;
            _length              = length;
            _hashCode            = 0;
            _str                 = null;

            return this;
        }

        /****************************************************************************/
        internal static ICharacterSpan FromString(string s, bool cacheable = false)
        {
            if(cacheable && _cache.ContainsKey(s))
                return _cache[s];

            var cspan = new CharacterSpan(s);

            if(cacheable)
            { 
                if(_cache.TryAdd(s, cspan))
                    cspan.ExpressionResult = false;
            }

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
        internal static ICharacterSpan Join(IEnumerable<ICharacterSpan> list, char separator)
        {
            var listLen = 0;
            var length = 0;

            foreach(var item in list) 
            { 
                length += item.Length;
                ++listLen;
            }

            length += listLen-1;
           
            var buffer = new char[length];
            var offset = 0;
            var i = 0;

            foreach(var item in list) 
            { 
                item.CopyTo(buffer, offset);

                offset += item.Length;

                if(i++ < listLen-1)
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
        public bool        ExpressionResult { get; set; }

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
        public ICharacterSpan SubstringBefore(char ch, int skip = 0)
        {
            if(this.Length > 0)
            { 
                for(var i = skip; i < this.Length; ++i)
                { 
                    if(this[i] == ch)
                    { 
                        if(!this.ExpressionResult)
                            return new CharacterSpan(this, skip, i-skip);

                        return Reset(_offset + skip, i - skip);
                    }
                }

                if(skip != 0)
                    return this.Substring(skip, this.Length - skip);
            }

            return this;
        }

        /****************************************************************************/
        public ICharacterSpan SubstringAfter(char ch)
        {
            if(this.Length > 0)
            { 
                for(var i = 0; i < this.Length; ++i)
                { 
                    if(this[i] == ch)
                    {
                        if((this.Length - i - 1) == 0)
                            return Empty;
                    
                        if(this.ExpressionResult)
                            return Reset(i+1, this.Length - i - 1);

                        return new CharacterSpan(this, i+1, this.Length - i - 1);
                    }
                }
            }

            return Empty;
        }

        /****************************************************************************/
        public ICharacterSpan Substring(int index, int length = -1000, bool trim = false)
        {
            if(this.Length == 0)
                return this;

            length = length == -1000 ? this.Length - index : (length < 0 ? this.Length - index + length : length);

            if(length <= 0)
                return CharacterSpan.Empty;

            length = Math.Min(length, this.Length - index);

            if(trim)
                return Trim(_source!, _offset + index, length);

            if(!this.ExpressionResult)
                return new CharacterSpan(_source!, _offset + index, length);
                    
            return Reset(_offset + index, length);
        }

        /****************************************************************************/
        public ICharacterSpan Transform(Func<char, (bool Use, char NewVal)> transform)
        {
            if(this.Length == 0)
                return CharacterSpan.Empty;

            if(this.ExpressionResult)
                return TransformInPlace(transform);

            return TransformOutOfPlace(transform);
        }

        /****************************************************************************/
        public ICharacterSpan Pad(char padChar, int length, bool left)
        {
            if(this.Length >= length)
                return this;

            var buffer = new char[length];

            if(left)
            { 
                for(var i = 0; i < length - this.Length; ++i)
                    buffer[i] = padChar;
              
                if(_length > 0)
                    Array.Copy(_source!, _offset, buffer, length - this.Length, _length);
            }
            else
            {
                if(_length > 0)
                    Array.Copy(_source!, _offset, buffer, 0, _length);

                for(var i = _length; i < length; ++i)
                    buffer[i] = padChar;
            }

            return new CharacterSpan(buffer, 0, length);
        }

        /****************************************************************************/
        public ICharacterSpan Remove(ICharacterSpan remove, int start = 0)
        {
            if(this.Length == 0 || this.Length < remove.Length)
                return this;

            if(this.Equals(remove))
                return CharacterSpan.Empty;

            if(this.ExpressionResult)
                return RemoveOutOfPlace(remove, start);

            return RemoveInPlace(remove, start);
        }

        /****************************************************************************/
        public ICharacterSpan Replace(ICharacterSpan find, ICharacterSpan replace, int start = 0)
        {
            // These are no-ops
            if(find == null || this.Length == 0 || replace.Length > this.Length)
                return this;

            // If we're replacing with nothing then call Remove instead
            if(replace == null || replace.Length == 0)
                return this.Remove(find);

            ICharacterSpan thisSpan = this;

            var index = thisSpan.IndexOf(find, start);

            // If we don't find the string just return
            if(index == -1)
                return this;

            // If we're just replacing the whole string
            if(index == 0 && this.Length == find.Length)
                return replace;

            var replaceSpan = (replace is CharacterSpan cspan) ? cspan : CharacterSpan.Clone(replace);
            var destIndex   = start;
            var destLen     = this.Length;
            char[]? buffer  = CheckBuffer(null, destLen);

            if(start > 0)
                Array.Copy(_source!, _offset, buffer, 0, start);

            while(true)
            {
                index = thisSpan.IndexOf(find, start);
                
                if(index == -1)
                    break;

                destLen = (destLen - find.Length) + replace.Length;

                CheckBuffer(buffer, destLen);

                // Copy all characters before the search string
                if(index > start)
                { 
                    Array.Copy(_source!, _offset + start, buffer, destIndex, index - start);
                    destIndex += index - start;
                }

                // Now copy the replace string
                Array.Copy(replaceSpan._source!, replaceSpan._offset, buffer, destIndex, replaceSpan.Length);

                destIndex += replaceSpan.Length;
                start = index + find.Length;
            }

            if(start < thisSpan.Length)
            {
                CheckBuffer(buffer, destLen + thisSpan.Length - start);
                Array.Copy(_source!, _offset + start, buffer, destIndex, thisSpan.Length - start);
                destIndex += thisSpan.Length - start;
            }

            return new CharacterSpan(buffer, 0, destIndex);
        }

        /****************************************************************************/
        public ICharacterSpan ReplaceEnding(ICharacterSpan find, ICharacterSpan replace)
        {
            // These are no-ops
            if(find == null || this.Length == 0 || replace.Length > this.Length)
                return this;

            ICharacterSpan thisSpan = this;

            if(!thisSpan.EndsWith(find))
                return this;

            var index = thisSpan.LastIndexOf(find);

            // If we're just replacing the whole string
            if(index == 0 && this.Length == find.Length)
                return replace;

            return this.Replace(find, replace, index);
        }

        /****************************************************************************/
        private ICharacterSpan RemoveInPlace(ICharacterSpan remove, int start = 0)
        {
            ICharacterSpan thisSpan = this;
            var removeLen = remove.Length;
            var removed = false;

            while(true)
            {
                var index = thisSpan.IndexOf(remove, start);

                if(index == -1)
                    break;

                if(index == this.Length - removeLen)
                {
                    _length -= removeLen;
                    removed = true;
                    break;
                }
                else if(index == 0)
                {
                    _length -= removeLen;
                    _offset += removeLen;
                    removed = true;
                    continue;
                }

                var src = _offset + index + removeLen;
                var dst = _offset + index;
                var len = _length - index - removeLen;

                Array.Copy(_source!, src, _source!, dst, len);

                start += (index - start);
                _length -= removeLen;

                removed = true;
            }

            if(removed)
            {
                _str = null;
                _hashCode = 0;
            }

            return this;
        }

        /****************************************************************************/
        private ICharacterSpan RemoveOutOfPlace(ICharacterSpan remove, int start = 0)
        {
            ICharacterSpan thisSpan = this;
            var index = thisSpan.IndexOf(remove);

            if(index == -1)
                return this;

            return Clone(this).RemoveInPlace(remove, start);
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

                if(IsEscapedCharacter(ch))
                {
                    var replacement = _escapeCharacters[ch];

                    if(buffer == null)
                        buffer = GetBuffer(buffer, i, ref bufferIndex);
                    else 
                        buffer = CheckBuffer(buffer, bufferIndex+2);
                     
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
                Array.Copy(_source!, _offset, buffer, offset, length == -1 ? _length : length);
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
                var index = cspan.IndexOf(separator, start);

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
                var index = cspan.IndexOf(separator, start);

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
        public ICharacterSpan Trim(bool start = true, bool end = true)
        {
            if(this.Length == 0)
                return CharacterSpan.Empty;

            var nonWhitespaceBegin = start ? IndexOfNonWhiteSpace(_source, _offset, _length) : _offset;

            if(nonWhitespaceBegin == -1)
                return CharacterSpan.Empty;

            var newLength = _length - (nonWhitespaceBegin - _offset);

            var nonWhitespaceEnd = end ? LastIndexOfNonWhiteSpace(_source, nonWhitespaceBegin, newLength - (nonWhitespaceBegin - _offset)) : _offset + newLength;
            
            newLength = nonWhitespaceEnd - nonWhitespaceBegin + 1;

            if(nonWhitespaceBegin == _offset && newLength == _length)
                return this;

            if(!this.ExpressionResult)
                return new CharacterSpan(_source!, nonWhitespaceBegin, newLength, this.HasEscapeCharacters);

            return Reset(nonWhitespaceBegin, newLength);
        }

        /****************************************************************************/
        public ICharacterSpan Concat(ICharacterSpan val)
        {
            if(val == null || val.Length == 0)
                return this;

            if(this.Length == 0)
                return val;
            
            var buffer = new char[this.Length + val.Length];

            this.CopyTo(buffer, 0);
            val.CopyTo(buffer, this.Length);

            return new CharacterSpan(buffer, 0, this.Length + val.Length);
        }

        #endregion

        /****************************************************************************/
        internal static ICharacterSpan Trim(char[]? source, int offset, int length, bool hasEscapeCharacters = false, bool start = true, bool end = true)
        {
            if(source == null || length == 0)
                return CharacterSpan.Empty;

            var nonWhitespaceBegin = start ? IndexOfNonWhiteSpace(source, offset, length) : offset;

            if(nonWhitespaceBegin == -1)
                return CharacterSpan.Empty;

            var newLength = Math.Min(length, source.Length - offset);

            var nonWhitespaceEnd = end ? LastIndexOfNonWhiteSpace(source, nonWhitespaceBegin, newLength - (nonWhitespaceBegin - offset)) : offset + newLength;
            
            newLength = nonWhitespaceEnd - nonWhitespaceBegin + 1;

            return new CharacterSpan(source!, nonWhitespaceBegin, newLength, hasEscapeCharacters);
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
            var cspanLen = cspan.Length;

            for(int i = 0; i < cspanLen; i += 2) 
            { 
                var value1 = (int)cspan[i];
                var value2 = (i < cspanLen-1) ? (int)cspan[i+1] : 0;
                var value  = value1 << 16 | value2;
                        
                hashCode = i == 0 ? comparer.GetHashCode(value) : CombineHashCodes(hashCode, comparer.GetHashCode(value));
            }

            return hashCode;

            /****************************************************************************/
            static int CombineHashCodes(int h1, int h2) 
            {
                return (((h1 << 5) + h1) ^ h2);
            }
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
                Array.Copy(_source!, _offset, buffer, 0, index);

            bufferIndex = index == -1 ? 0 : index;

            return buffer;
        }

        /****************************************************************************/
        private char[] CheckBuffer(char[]? buffer, int length) 
        {        
            if(buffer == null)
                return new char[(int)((this.Length + 16) / 16) * 16];

            if(length >= buffer.Length)
                Array.Resize(ref buffer, (int)((length + 32) / 16) * 16);

            return buffer;
        }
        
        /****************************************************************************/
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
        private ICharacterSpan TransformOutOfPlace(Func<char, (bool Use, char NewVal)> transform)
        {
            for(int i = 0; i < this.Length; ++i)
            {
                var oldVal = this[i];
                var result = transform(oldVal);

                if(result.Use && oldVal == result.NewVal)
                    continue;

                return Clone(this).TransformInPlace(transform);
            }

            return this;
        }

        /****************************************************************************/
        private ICharacterSpan TransformInPlace(Func<char, (bool Use, char NewVal)> transform)
        {
            var index = 0;
            var notUse = 0;
            var length = this.Length;

            for(int i = 0; i < length; ++i)
            {
                var oldVal = this[i];
                var result = transform(oldVal);

                if(result.Use && oldVal == result.NewVal && notUse == 0)
                { 
                    ++index;
                    continue;
                }

                this._str = null;
                this._hashCode = 0;

                if(!result.Use)
                { 
                    ++notUse;
                    --this._length;
                    continue;
                }

                _source![_offset + index++] = result.NewVal;
            }

            return this;
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
        internal void RemoveLastCharacter()
        {
            if(_length > 0 ) 
            --_length;
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
