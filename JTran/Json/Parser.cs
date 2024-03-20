
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JTran.Collections;
using JTran.Common;

[assembly: InternalsVisibleTo("JTran.UnitTests")]
[assembly: InternalsVisibleTo("JTran.PerformanceTests")]

namespace JTran.Json
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model
    /// </summary>
    internal interface IJsonParser : IDisposable
    {
        object Parse(Stream stream, bool allowDeferredArrays = false);
        object Parse(string str);
    }
       
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into a json or jtran object model
    /// </summary>
    internal class Parser : IJsonParser
    {
        private readonly JsonTokenizer _tokenizer = new JsonTokenizer();
        private readonly IJsonModelBuilder _modelBuilder;
        private readonly Dictionary<ICharacterSpan, ICharacterSpan> _nameCache = new();
        private ICharacterReader? _reader;

        private long _lineNumber = 1;

        /****************************************************************************/
        internal Parser(IJsonModelBuilder modelBuilder)
        { 
            _modelBuilder = modelBuilder;
        }
       
        /****************************************************************************/
        public object Parse(string json) 
        {
            _reader = new CharacterReader(json);

            return InnerParse(false);
        }
       
        /****************************************************************************/
        public object Parse(Stream stream, bool allowDeferredArrays = false) 
        {
            _reader = new CharacterReader(stream);

            return InnerParse(allowDeferredArrays);
        }

        /****************************************************************************/
        public bool ArrayInnerParse(ICharacterReader reader) 
        {
            _lineNumber = 0;
            _reader = reader;

            try
            { 
                var tokenType = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginObject) 
                    throw new JsonParseException("Only top level arrays are allowed", _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginArray) 
                    return true;
            }
            catch(JsonParseException ex)
            {
                if(ex.LineNumber == -1L)
                    ex.LineNumber = _tokenizer.TokenLineNumber;

                throw;
            }

            throw new JsonParseException("Invalid json", _lineNumber);
        }

        /****************************************************************************/
        public object InnerParse(bool allowDeferredArrays) 
        {
            _lineNumber = 0;

            try
            { 
                var tokenType = _tokenizer.ReadNextToken(_reader!, ref _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginObject) 
                    return BeginObject(CharacterSpan.Empty, null, null, _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginArray && !allowDeferredArrays) 
                    return BeginArray(CharacterSpan.Empty, null, _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginArray) 
                    return new DeferredParseArray(this);
            }
            catch(JsonParseException ex)
            {
                if(ex.LineNumber == -1L)
                    ex.LineNumber = _tokenizer.TokenLineNumber;

                throw;
            }

            throw new JsonParseException("Invalid json", _lineNumber);
        }

        #region IDisposable

        /****************************************************************************/
        public void Dispose()
        {
            if(_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        #endregion

        #region Private

        /****************************************************************************/
        private ICharacterSpan GetName(ICharacterSpan name)
        {
            if(name.Length == 0)
                return CharacterSpan.Empty;

            if (_nameCache.ContainsKey(name))
                return _nameCache[name];

            var newName = CharacterSpan.Clone(name);

            _nameCache.Add(newName, newName);

            newName.Cached = true;

            return newName;
        }

        /****************************************************************************/
        private ICharacterSpan GetValue(ICharacterSpan val)
        {
            return CharacterSpan.Clone(val);
        }

        /****************************************************************************/
        private object BeginObject(ICharacterSpan? name, object parent, object? previous, long lineNumber) 
        {
            var ex = name == null ? _modelBuilder.AddObject(parent, lineNumber) 
                                  : _modelBuilder.AddObject(GetName(name), parent, previous, lineNumber);

            var previousTokenType = JsonToken.TokenType.BeginObject;
            object? runningPrevious = null;

            while(true)
            {
                var tokenType = _tokenizer.ReadNextToken(_reader!, ref _lineNumber);

                switch(tokenType)
                {
                    case JsonToken.TokenType.EOF:
                        throw new JsonParseException("Unexpected end of file", _lineNumber);

                    case JsonToken.TokenType.EndObject:
                        return ex;

                    case JsonToken.TokenType.Comma:
                        break;

                    case JsonToken.TokenType.Text when (previousTokenType == JsonToken.TokenType.BeginObject || previousTokenType == JsonToken.TokenType.Comma):
                    {                    
                        var propName = GetName(_tokenizer.TokenValue as ICharacterSpan);
                        
                        runningPrevious = BeginProperty(propName!, ex, runningPrevious);

                        break;
                    }

                    default:
                        throw new JsonParseException($"Unexpected token: {_tokenizer.TokenValue}", _lineNumber-1);
                }

                previousTokenType = tokenType;
            }
        }

        /****************************************************************************/
        internal object ReadArrayItem(object array) 
        {
            while(true)
            { 
                var tokenType = _tokenizer!.ReadNextToken(_reader!, ref _lineNumber);

                switch(tokenType)
                {
                    case JsonToken.TokenType.EOF:
                        throw new JsonParseException("Unexpected end of file", _lineNumber);

                    case JsonToken.TokenType.Comma:
                        continue;

                    case JsonToken.TokenType.EndArray:
                        return null;

                    case JsonToken.TokenType.BeginArray:
                        return BeginArray(null, array, _lineNumber);

                    case JsonToken.TokenType.BeginObject:
                        return BeginObject(null, array, null, _lineNumber);

                    case JsonToken.TokenType.Null:      
                        return _modelBuilder.AddNull(array, _lineNumber); 

                    case JsonToken.TokenType.Number:   
                        return _modelBuilder.AddNumber((decimal)_tokenizer.TokenValue!, array, _lineNumber); 

                    case JsonToken.TokenType.Boolean:   
                        return _modelBuilder.AddBoolean((bool)_tokenizer.TokenValue!, array, _lineNumber); 

                    default:
                    { 
                        if(_tokenizer.TokenValue is ICharacterSpan cspan)
                            return _modelBuilder.AddText(GetValue(cspan), array, _lineNumber);                 

                        throw new JsonParseException($"Unexpected token: {_tokenizer.TokenValue}", _lineNumber);
                    }
                }
            }
        }

        private object BeginArray(ICharacterSpan? name, object parent, long lineNumber) 
        {
            var array = name == null ? _modelBuilder.AddArray(parent, lineNumber) 
                                     : _modelBuilder.AddArray(name, parent, lineNumber);

            while(true)
            {
                var item = ReadArrayItem(array);

                if (item == null)
                    break;
            }

            return array;
        }

        private object BeginProperty(ICharacterSpan name, object parent, object? previous) 
        {
            var lineNumber = _lineNumber;
            var tokenType = _tokenizer.ReadNextToken(_reader!, ref _lineNumber);

            if(tokenType == JsonToken.TokenType.Property)
            { 
                tokenType = _tokenizer.ReadNextToken(_reader!, ref _lineNumber);

                switch(tokenType)
                {
                    case JsonToken.TokenType.BeginObject: return BeginObject(GetName(name), parent, previous, lineNumber);
                    case JsonToken.TokenType.BeginArray:  return BeginArray(GetName(name), parent, lineNumber);
                    case JsonToken.TokenType.Number:      return _modelBuilder.AddNumber(GetName(name), (decimal)_tokenizer.TokenValue!, parent, previous, _lineNumber);       
                    case JsonToken.TokenType.Boolean:     return _modelBuilder.AddBoolean(GetName(name), (bool)_tokenizer.TokenValue!, parent, previous, _lineNumber);       
                    case JsonToken.TokenType.Null:        return _modelBuilder.AddNull(GetName(name), parent, previous, _lineNumber);       

                    default:
                    {
                        if(_tokenizer!.TokenValue is ICharacterSpan cspan)
                        { 
                            if(tokenType == JsonToken.TokenType.Text)
                                return _modelBuilder.AddText(GetName(name), GetValue(cspan), parent, previous, _lineNumber);  
                        }

                        break;
                    }
                }
            }

            throw new JsonParseException($"Unexpected token: {_tokenizer.TokenValue}", lineNumber);
        }

        #endregion
    }
}
