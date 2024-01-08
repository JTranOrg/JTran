
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
        internal JsonToken ReadNextToken(ICharacterReader reader, ref long lineNumber) 
        {
            var ch = '\0';

            StringBuilder? sb = null;
            var token = new JsonToken(lineNumber);
            var doubleQuoted = false;
            var singleQuoted = false;
            var previousChar = '\0';

            try
            { 
                while(true)
                {
                    ch = reader.ReadNext(ref lineNumber);

                    if(!char.IsWhiteSpace(ch))
                        break;
                }

                switch(ch)
                {
                    case '\0': token.Type = JsonToken.TokenType.EOF;         token.Value = "end of file"; return token;
                    case '{':  token.Type = JsonToken.TokenType.BeginObject; token.Value = "{"; return token;
                    case '}':  token.Type = JsonToken.TokenType.EndObject;   token.Value = "}"; return token;
                    case '[':  token.Type = JsonToken.TokenType.BeginArray;  token.Value = "["; return token;
                    case ']':  token.Type = JsonToken.TokenType.EndArray;    token.Value = "]"; return token;
                    case ':':  token.Type = JsonToken.TokenType.Property;    token.Value = ":"; return token;
                    case ',':  token.Type = JsonToken.TokenType.Comma;       token.Value = ","; return token;
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
                            throw new JsonParseException("Missing end quotes", lineNumber); 
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
                var val = sb.ToString();

                token.Value = val;

                if(!doubleQuoted && !singleQuoted)
                {
                    switch (val)
                    { 
                        case "null":  token.Type = JsonToken.TokenType.Null;    break;
                        case "true":  token.Type = JsonToken.TokenType.Boolean;  break;
                        case "false": token.Type = JsonToken.TokenType.Boolean; break;
                        default: 
                        {
                            if(double.TryParse(val, out double dVal))
                            { 
                                token.Type  = JsonToken.TokenType.Number; 
                                token.Value = dVal;
                            }
                            else
                                token.Type = JsonToken.TokenType.Text; 

                            break;
                        }
                    }
                }
                else
                    token.Type = JsonToken.TokenType.Text; 
            }

            return token;
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

    internal struct JsonToken
    {
        public JsonToken(long lineNumber)
        {
            this.LineNumber = lineNumber;
        }

        internal TokenType Type       { get; set; } = TokenType.EOF;
        internal object    Value      { get; set; } = "";
        internal long      LineNumber { get; }

        internal enum TokenType
        {
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
