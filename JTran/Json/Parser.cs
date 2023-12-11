using System;
using System.Dynamic;
using System.IO;

using System.Runtime.CompilerServices;
using System.Collections.Generic;

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
        ExpandoObject Parse(Stream stream);
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model
    /// </summary>
    internal class Parser : IJsonParser
    {
        private long _lineNumber = 1;
        private ICharacterReader _reader;
        private readonly JsonTokenizer _tokenizer = new JsonTokenizer();

        /****************************************************************************/
        internal Parser()
        { 
        }

        /****************************************************************************/
        /****************************************************************************/
        internal class ParseError : Exception
        {
            internal ParseError(string msg, long lineNo) : base(msg)
            {
                this.Data.Add("LineNumber", lineNo.ToString());
            }
        }
        
        /****************************************************************************/
        public ExpandoObject Parse(Stream stream) 
        {
            var ex = new ExpandoObject();

            _lineNumber = 1;

            _reader = new CharacterReader(stream);

            var token = _tokenizer.ReadNextToken(_reader);

            // ??? Add support for array
            if(token.Type != JsonToken.TokenType.BeginObject) 
                throw new ParseError($"Unexpected token: {token.Value}", _lineNumber);

            return BeginObject();
        }

        #region Private

        private ExpandoObject BeginObject() 
        {
            var ex = new ExpandoObject();

            while(true)
            {
                var token = _tokenizer.ReadNextToken(_reader);

                switch(token.Type)
                {
                    case JsonToken.TokenType.EOF:
                        throw new ParseError("Unexpected end of file", _lineNumber);

                    case JsonToken.TokenType.EndObject:
                        return ex;

                    case JsonToken.TokenType.Comma:
                        continue;

                    case JsonToken.TokenType.Text:
                    {                    
                        var result = BeginProperty();

                        ex.TryAdd(token.Value, result);
                        break;
                    }

                    default:
                        throw new ParseError($"Unexpected token: {token.Value}", _lineNumber);
                }
            }
        }

        private object BeginArray() 
        {
            var array = new List<object>();

            while(true)
            {
                var token = _tokenizer.ReadNextToken(_reader);

                switch(token.Type)
                {
                    case JsonToken.TokenType.EOF:
                        throw new ParseError("Unexpected end of file", _lineNumber);

                    case JsonToken.TokenType.Comma:
                        continue;

                    case JsonToken.TokenType.EndArray:
                        break;

                    case JsonToken.TokenType.BeginArray:
                    {
                        var result = BeginArray();

                        array.Add(result);
                        continue;
                    }

                    case JsonToken.TokenType.BeginObject:
                    {
                        var result = BeginObject();

                        array.Add(result);
                        continue;
                    }

                    case JsonToken.TokenType.Number:    array.Add(double.Parse(token.Value)); continue;
                    case JsonToken.TokenType.Null:      array.Add(null);                      continue;
                    case JsonToken.TokenType.Boolean:   array.Add(token.Value == "true");     continue;
                    case JsonToken.TokenType.Text:      array.Add(token.Value);               continue;

                    default:
                        throw new ParseError($"Unexpected token: {token.Value}", _lineNumber);
                }

                break;
            }

            return array;
        }

        private object BeginProperty() 
        {
            var token = _tokenizer.ReadNextToken(_reader);

             if(token.Type == JsonToken.TokenType.Property)
             { 
                token = _tokenizer.ReadNextToken(_reader);

                switch(token.Type)
                {
                    case JsonToken.TokenType.Text:        return token.Value;       
                    case JsonToken.TokenType.Number:      return token.Value;       
                    case JsonToken.TokenType.Boolean:     return token.Value;       
                    case JsonToken.TokenType.Null:        return token.Value;       
                    case JsonToken.TokenType.BeginObject: return BeginObject();
                    case JsonToken.TokenType.BeginArray:  return BeginArray();

                    default:
                        break;
                }
            }

            throw new ParseError($"Unexpected token: {token.Value}", _lineNumber);
        }

        #endregion
    }
}
