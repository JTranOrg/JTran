
using System.IO;
using System.Runtime.CompilerServices;
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
    internal interface IJsonParser
    {
        object Parse(Stream stream);
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

        private long _lineNumber = 1;
        private ICharacterReader? _reader;

        /****************************************************************************/
        internal Parser(IJsonModelBuilder modelBuilder)
        { 
            _modelBuilder = modelBuilder;
        }
       
        /****************************************************************************/
        public object Parse(string json) 
        {
            using var reader = new CharacterReader(json);

            return InnerParse(reader);
        }
       
        /****************************************************************************/
        public object Parse(Stream stream) 
        {
            using var reader = new CharacterReader(stream);

            return InnerParse(reader);
        }

        #region Private

        private readonly CharacterSpan _empty = new CharacterSpan();

        /****************************************************************************/
        public object InnerParse(ICharacterReader reader) 
        {
            _lineNumber = 0;
            _reader = reader;

            try
            { 
                var tokenType = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginObject) 
                    return BeginObject(_empty, null, null, _lineNumber);

                if(tokenType == JsonToken.TokenType.BeginArray) 
                    return BeginArray(_empty, null, _lineNumber);
            }
            catch(JsonParseException ex)
            {
                if(ex.LineNumber == -1L)
                    ex.LineNumber = _tokenizer.TokenLineNumber;

                throw;
            }

            throw new JsonParseException("Invalid json", _lineNumber);
        }

        private object BeginObject(CharacterSpan? name, object parent, object? previous, long lineNumber) 
        {
            var ex = name == null ? _modelBuilder.AddObject(parent, lineNumber) 
                                  : _modelBuilder.AddObject(name, parent, previous, lineNumber);

            var previousTokenType = JsonToken.TokenType.BeginObject;
            object? runningPrevious = null;

            while(true)
            {
                var tokenType = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

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
                        var propName = _tokenizer.TokenValue as CharacterSpan;
                        
                        runningPrevious = BeginProperty(propName!, ex, runningPrevious);

                        break;
                    }

                    default:
                        throw new JsonParseException($"Unexpected token: {_tokenizer.TokenValue}", _lineNumber-1);
                }

                previousTokenType = tokenType;
            }
        }

        private object BeginArray(CharacterSpan? name, object parent, long lineNumber) 
        {
            var array = name == null ? _modelBuilder.AddArray(parent, lineNumber) 
                                                             : _modelBuilder.AddArray(name, parent, lineNumber);

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
                        break;

                    case JsonToken.TokenType.BeginArray:
                        BeginArray(null, array, _lineNumber);
                        continue;

                    case JsonToken.TokenType.BeginObject:
                        BeginObject(null, array, null, _lineNumber);
                        continue;

                    case JsonToken.TokenType.Null:      
                        _modelBuilder.AddNull(array, _lineNumber); 
                        continue;

                   case JsonToken.TokenType.Number:   
                        _modelBuilder.AddNumber((double)_tokenizer.TokenValue!, array, _lineNumber); 
                        continue;

                    default:
                    { 
                        if(_tokenizer.TokenValue is CharacterSpan cspan)
                        { 
                            switch(tokenType)
                            { 
                                case JsonToken.TokenType.Boolean:   
                                    _modelBuilder.AddBoolean(cspan.Equals("true"), array, _lineNumber); 
                                    continue;
                            
                                case JsonToken.TokenType.Text:
                                    _modelBuilder.AddText(cspan, array, _lineNumber);                 
                                    continue;
                            }
                        }

                        throw new JsonParseException($"Unexpected token: {_tokenizer.TokenValue}", _lineNumber);
                    }
                }

                break;
            }

            return array;
        }

        private object BeginProperty(CharacterSpan name, object parent, object? previous) 
        {
            var lineNumber = _lineNumber;
            var tokenType = _tokenizer.ReadNextToken(_reader!, ref _lineNumber);

            if(tokenType == JsonToken.TokenType.Property)
            { 
                tokenType = _tokenizer.ReadNextToken(_reader!, ref _lineNumber);

                switch(tokenType)
                {
                    case JsonToken.TokenType.BeginObject: return BeginObject(name, parent, previous, lineNumber);
                    case JsonToken.TokenType.BeginArray:  return BeginArray(name, parent, lineNumber);
                    case JsonToken.TokenType.Number:      return _modelBuilder.AddNumber(name, (double)_tokenizer.TokenValue!, parent, previous, _lineNumber);       
                    case JsonToken.TokenType.Null:        return _modelBuilder.AddNull(name, parent, previous, _lineNumber);       

                    default:
                    {
                        if(_tokenizer!.TokenValue is CharacterSpan cspan)
                        { 
                            if(tokenType == JsonToken.TokenType.Text)
                                return _modelBuilder.AddText(name, cspan, parent, previous, _lineNumber);  

                            if(tokenType == JsonToken.TokenType.Boolean)
                                return _modelBuilder.AddBoolean(name, cspan.Equals("true"), parent, previous, _lineNumber);       
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
