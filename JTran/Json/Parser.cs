using System;
using System.Dynamic;
using System.IO;

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

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
        private long _lineNumber = 1;
        private ICharacterReader _reader;
        private IJsonModelBuilder _modelBuilder;
        private readonly JsonTokenizer _tokenizer = new JsonTokenizer();

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

        /****************************************************************************/
        public object InnerParse(ICharacterReader reader) 
        {
            _lineNumber = 0;

            _reader = reader;

            var token = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

            // ??? Add support for array
            if(token.Type != JsonToken.TokenType.BeginObject) 
                throw new JsonParseException($"Unexpected token: {token.Value}", _lineNumber);

            return BeginObject("", null);
        }

        private object BeginObject(string? name, object parent) 
        {
            var ex = name == null ? _modelBuilder.AddObject(parent) : _modelBuilder.AddObject(name, parent);
            var previousTokenType = JsonToken.TokenType.BeginObject;

            while(true)
            {
                var token = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

                switch(token.Type)
                {
                    case JsonToken.TokenType.EOF:
                        throw new JsonParseException("Unexpected end of file", _lineNumber);

                    case JsonToken.TokenType.EndObject:
                        return ex;

                    case JsonToken.TokenType.Comma:
                        break;

                    case JsonToken.TokenType.Text when (previousTokenType == JsonToken.TokenType.BeginObject || previousTokenType == JsonToken.TokenType.Comma):
                    {                    
                        var propName = token.Value.ToString();
                        
                        BeginProperty(propName, ex);

                        break;
                    }

                    default:
                        throw new JsonParseException($"Unexpected token: {token.Value}", _lineNumber);
                }

                previousTokenType = token.Type;
            }
        }

        private object BeginArray(string? name, object parent) 
        {
            var array = name == null ? _modelBuilder.AddArray(parent) : _modelBuilder.AddArray(name, parent);

            while(true)
            {
                var token = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

                switch(token.Type)
                {
                    case JsonToken.TokenType.EOF:
                        throw new JsonParseException("Unexpected end of file", _lineNumber);

                    case JsonToken.TokenType.Comma:
                        continue;

                    case JsonToken.TokenType.EndArray:
                        break;

                    case JsonToken.TokenType.BeginArray:
                    {
                        BeginArray(null, array);
                        continue;
                    }

                    case JsonToken.TokenType.BeginObject:
                    {
                        BeginObject(null, array);
                        continue;
                    }

                    case JsonToken.TokenType.Number:    _modelBuilder.AddNumber(double.Parse(token.Value.ToString()), array); continue;
                    case JsonToken.TokenType.Null:      _modelBuilder.AddNull(array);                                         continue;
                    case JsonToken.TokenType.Boolean:   _modelBuilder.AddBoolean(token.Value.ToString() == "true", array);    continue;
                    case JsonToken.TokenType.Text:      _modelBuilder.AddText(token.Value.ToString(), array);                 continue;

                    default:
                        throw new JsonParseException($"Unexpected token: {token.Value}", _lineNumber);
                }

                break;
            }

            return array;
        }

        private object BeginProperty(string name, object parent) 
        {
            var token = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

             if(token.Type == JsonToken.TokenType.Property)
             { 
                token = _tokenizer.ReadNextToken(_reader, ref _lineNumber);

                switch(token.Type)
                {
                    case JsonToken.TokenType.Text:        return _modelBuilder.AddText(name, token.Value.ToString(), parent);      
                    case JsonToken.TokenType.Number:      return _modelBuilder.AddNumber(name, double.Parse(token.Value.ToString()), parent);       
                    case JsonToken.TokenType.Boolean:     return _modelBuilder.AddBoolean(name, token.Value.ToString() == "true", parent);       
                    case JsonToken.TokenType.Null:        return _modelBuilder.AddNull(name, parent);       
                    case JsonToken.TokenType.BeginObject: return BeginObject(name, parent);
                    case JsonToken.TokenType.BeginArray:  return BeginArray(name, parent);

                    default:
                        break;
                }
            }

            throw new JsonParseException($"Unexpected token: {token.Value}", _lineNumber);
        }

        #endregion
    }
}
