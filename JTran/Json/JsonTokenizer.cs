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
        private readonly CharacterSpanFactory _factory = new();

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
                while(reader.ReadNext())
                {
                    ch = reader.Current!;

                    if(!char.IsWhiteSpace(ch))
                        break;
                }

                this.TokenLineNumber = lineNumber = reader.LineNumber;

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

                bool escape = false;

                if(ch == '\"')
                    doubleQuoted = true;
                else if(ch == '\'')
                    singleQuoted = true;
                else
                    _factory.Append(ch);

                while(reader.ReadNext())
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
                            reader.GoBack(); 
                            break;
                        }

                        if(ch == '\"' || ch == '\'')
                            throw new JsonParseException("Missing end quotes", lineNumber-1); 
                    }

                  Append:

                    _factory.Append(ch);
                    previousChar = ch;
                }
            }
            catch(ArgumentOutOfRangeException)
            { 
                if(_factory.Length == 0)
                    throw new JsonParseException("Unexpected end of file", lineNumber);
            }

            this.TokenValue = _factory.Current;

            if(!doubleQuoted && !singleQuoted)
            {
                if(this.TokenValue is CharacterSpan span)
                { 
                    if(span.Equals("null"))
                        return JsonToken.TokenType.Null;    

                    if(span.Equals("true"))
                        return JsonToken.TokenType.Boolean; 
                        
                    if(span.Equals("false"))
                        return JsonToken.TokenType.Boolean; 

                    if(span.TryParseNumber(out double dVal))
                    { 
                        this.TokenValue = dVal;

                        return JsonToken.TokenType.Number; 
                    }

                    return JsonToken.TokenType.Text; 
                }
            }

            return JsonToken.TokenType.Text; 
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
