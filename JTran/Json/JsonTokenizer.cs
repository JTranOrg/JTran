/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran.Json						            
 *             File: JsonTokenizer.cs					    		        
 *        Class(es): JsonTokenizer				         		            
 *          Purpose: Reads a "token" from a text source               
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 19 Jan 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Runtime.CompilerServices;
using System.Text;

using JTran.Common;
using JTran.Expressions;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{   
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse thru text and return a token
    /// </summary>
    internal sealed class JsonTokenizer
    {
        private readonly CharacterSpanBuilder _builder = new();

        internal object? TokenValue      { get; set; }
        internal long    TokenLineNumber { get; set; } = 0L;

        /****************************************************************************/
        internal JsonToken.TokenType ReadNextToken(ICharacterReader reader, ref long lineNumber) 
        {
            var ch           = '\0';
            var doubleQuoted = false;
            var singleQuoted = false;
            var previousChar = '\0';

            this.TokenLineNumber = lineNumber;

            try
            { 
                if(!reader.ReadNext(true))
                {
                    this.TokenLineNumber = lineNumber = reader.LineNumber;
                    this.TokenValue = "end of file"; 
                    return JsonToken.TokenType.EOF;
                }

                ch = reader.Current!;

                this.TokenLineNumber = lineNumber = reader.LineNumber;

                switch(ch)
                {
                    case '\0': return JsonToken.TokenType.EOF;         
                    case '{':  return JsonToken.TokenType.BeginObject; 
                    case '}':  return JsonToken.TokenType.EndObject;   
                    case '[':  return JsonToken.TokenType.BeginArray;  
                    case ']':  return JsonToken.TokenType.EndArray;    
                    case ':':  return JsonToken.TokenType.Property;    
                    case ',':  return JsonToken.TokenType.Comma;       
                    default:   break;
                }

                bool escape = false;

                if(ch == '\"')
                    doubleQuoted = true;
                else if(ch == '\'')
                    singleQuoted = true;
                else
                    _builder.Append(ch);

                while(reader.ReadNext(quoted: doubleQuoted || singleQuoted))
                {
                    this.TokenLineNumber = lineNumber = reader.LineNumber;
                    ch = reader.Current;

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
                            if(ch != ' ' && ch != '\t')
                                reader.GoBack(); 

                            break;
                        }

                        if(ch == '\"' || ch == '\'')
                            throw new JsonParseException("Missing end quotes", lineNumber-1); 
                    }
                    else if((ch == '\r' || ch == '\n'))
                        throw new JsonParseException("Missing end quotes", lineNumber-1); 

                  Append:

                    _builder.Append(ch);
                    previousChar = ch;
                }
            }
            catch(ArgumentOutOfRangeException)
            { 
                if(_builder.Length == 0)
                    throw new JsonParseException("Unexpected end of file", lineNumber);
            }

            this.TokenValue = _builder.Current;

            if(!doubleQuoted && !singleQuoted)
            {
                if(this.TokenValue is ICharacterSpan span)
                { 
                    if(span.Equals(CharacterSpan.Null))
                        return JsonToken.TokenType.Null;    

                    if(span.Equals(CharacterSpan.True))
                        return JsonToken.TokenType.Boolean; 
                        
                    if(span.Equals(CharacterSpan.False))
                        return JsonToken.TokenType.Boolean; 

                    if(span.TryParseNumber(out decimal dVal))
                    { 
                        this.TokenValue = dVal;

                        return JsonToken.TokenType.Number; 
                    }

                    return JsonToken.TokenType.Text; 
                }
            }
            else if(ch == '\r' || ch == '\n')
                throw new JsonParseException("Missing end quote", lineNumber);

            return JsonToken.TokenType.Text; 
        }    
    }

    internal static class Extensions
    {
        public static bool IsSeparator(this char ch)
        {
            switch(ch)
            {
                case '{': 
                case '}': 
                case '[': 
                case ']': 
                case ':': 
                case ',': 
                case ' ': 
                case '\r':
                case '\n':
                case '\t': return true;
                default:   return false;
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
