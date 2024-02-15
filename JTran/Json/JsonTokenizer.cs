
using JTran.Parser;
using System;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{   
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse thru text and return a token
    /// </summary>
    internal class JsonTokenizer
    {
        internal object? TokenValue      { get; set; }
        internal long    TokenLineNumber { get; set; } = 0L;

        internal JsonToken.TokenType ReadNextToken(ICharacterReader reader, ref long lineNumber) 
        {
            var ch = '\0';

            StringBuilder? sb = null;

            var doubleQuoted = false;
            var singleQuoted = false;
            var previousChar = '\0';

            this.TokenLineNumber = lineNumber;

            try
            { 
                while(true)
                {
                    ch = reader.ReadNext(ref lineNumber);

                    if(!char.IsWhiteSpace(ch))
                        break;
                }

                this.TokenLineNumber = lineNumber;

                switch(ch)
                {
                    case '\0': this.TokenValue = "end of file"; return JsonToken.TokenType.EOF;         
                    case '{':  this.TokenValue = "{";           return JsonToken.TokenType.BeginObject; 
                    case '}':  this.TokenValue = "}";           return JsonToken.TokenType.EndObject;   
                    case '[':  this.TokenValue = "[";           return JsonToken.TokenType.BeginArray;  
                    case ']':  this.TokenValue = "]";           return JsonToken.TokenType.EndArray;    
                    case ':':  this.TokenValue = ":";           return JsonToken.TokenType.Property;    
                    case ',':  this.TokenValue = ",";           return JsonToken.TokenType.Comma;       
                    default:   break;
                }

                sb = new StringBuilder();
                bool escape = false;

                if(ch == '\"')
                    doubleQuoted = true;
                else if(ch == '\'')
                    singleQuoted = true;
                else
                    sb.Append(ch);

                while(true)
                {
                    ch = reader.ReadNext(ref lineNumber);

                    this.TokenLineNumber = lineNumber;

                    if(escape)
                    {
                        switch(ch)
                        {
                            case 'r':    ch = '\r'; break;
                            case 'n':    ch = '\n'; break;
                            case 't':    ch = '\t'; break;
                            case 'b':    ch = '\b'; break;
                            case '\\':   ch = '\\'; break;
                            case '\"':   ch = '\"'; break;
                            case '\'':   ch = '\''; break;
                            default: 
                                throw new JsonParseException("Invalid escaped character", lineNumber);
                        }

                        escape = false;
                        goto Append;
                    }

                    if(ch == '\\')
                    {
                        escape = true;
                        continue;
                    }

                    if(ch == '\"' && doubleQuoted)
                        break;

                    if(ch == '\'' && singleQuoted)
                        break;

                    if(!doubleQuoted && !singleQuoted)
                    {
                        if(ch.IsSeparator())
                        {
                            reader.GoBack(); 
                            break;
                        }

                        if(ch == '\"' || ch == '\'')
                            throw new JsonParseException("Missing end quotes", lineNumber-1); 
                    }

                  Append:

                    sb.Append(ch); 
                    previousChar = ch;
                }
            }
            catch(ArgumentOutOfRangeException)
            { 
                if(sb == null)
                    throw new JsonParseException("Unexpected end of file", lineNumber);
            }

            if(sb != null)
            { 
                this.TokenValue = sb.ToString();

                if(!doubleQuoted && !singleQuoted)
                {
                    switch (this.TokenValue)
                    { 
                        case "null":  return JsonToken.TokenType.Null;    
                        case "true":  return JsonToken.TokenType.Boolean; 
                        case "false": return JsonToken.TokenType.Boolean; 

                        default: 
                        {
                            if(double.TryParse(this.TokenValue?.ToString(), out double dVal))
                            { 
                                this.TokenValue = dVal;

                                return JsonToken.TokenType.Number; 
                            }

                            return JsonToken.TokenType.Text; 
                        }
                    }
                }
                else
                    return JsonToken.TokenType.Text; 
            }

            return JsonToken.TokenType.Unknown;
        }    
    }

    internal static class Extensions
    {
        public static bool IsSeparator(this char ch)
        {
            switch(ch)
            {
                case '{':  return true;
                case '}':  return true;
                case '[':  return true;
                case ']':  return true;
                case ':':  return true;
                case ',':  return true;
                default:   return char.IsWhiteSpace(ch);
            }
        }
    }

    internal static class JsonToken
    {
        internal enum TokenType
        {
            Unknown,
            EOF,
            Text,
            Number,
            Boolean,
            Null,
            BeginObject,
            EndObject,
            BeginArray,
            EndArray,
            Property,
            Comma
        }  
    }
 }
