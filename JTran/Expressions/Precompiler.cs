/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Precompiler.cs					    		        
 *        Class(es): Precompiler				         		            
 *          Purpose: Parses a list of tokens and creates a hierarchy of subexpressions                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 4 Oct 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
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

using JTran.Common.Extensions;
using JTran.Parser;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    /// <summary>
    /// Parses a list of tokens and creates a hierarchy of subexpressions       
    /// </summary>
    internal static class Precompiler
    {
        private readonly static IDictionary<string, string> _boundary = new Dictionary<string, string>
        {
            { "[", "]" },
            { "(", ")" }
        };

        private readonly static IDictionary<string, string> _delimiters = new Dictionary<string, string>
        {
            { ",", "," }
        };

        private readonly static IDictionary<string, bool> _validOperators = new Dictionary<string, bool>
        {
            { "*", true },
            { "/", true },
            { "%", true }, 
            { "+", true }, 
            { "-", true },
            { "<", true },
            { ">", true },
            { "<=", true }, 
            { ">=", true },  
            { "==", true },  
            { "!=", true }, 
            { "&&", true },  
            { "and", true },
            { "||", true },  
            { "or", true },
            { ",", true },
            { "?", true },
            { ":", true }
        };

        /*****************************************************************************/
        internal static Token Precompile(IEnumerable<Token> tokens)
        {
            var output = InnerPrecompile(new Queue<Token>(tokens), null);

            return output;
        }

        #region Private 

        /*****************************************************************************/
        private static Token InnerPrecompile(Queue<Token> tokens, string? endBoundary = null, Func<Token, bool>? endIt = null)
        {
            var outputTokens = new List<Token>();
            var left = -1;
            Token? commaDelimited = null;
            Token? multiPart = null;

            while(tokens.Count > 0) 
            { 
                var token = tokens.Peek();

                // Is it the end?
                if(endIt != null && endIt(token))
                    break;

               token = tokens.Dequeue();

                if(token.IsOperator && endBoundary == token.Value)
                    break; 

                if(multiPart != null && (token.IsOperator || _boundary.ContainsKey(token.Value)))
                {
                    ReduceItem(outputTokens, left, multiPart, Token.TokenType.Multipart);
                    multiPart = null;
                }

                // Turn the contents of "()" or "[]" into a single expression
                if(token.IsOperator && _boundary.ContainsKey(token.Value))
                { 
                    var newToken  = InnerPrecompile(tokens, _boundary[token.Value]);
                    var lastToken = outputTokens.LastOrDefault();

                    // Is this a function call?
                    if(lastToken != null && lastToken.Type == Token.TokenType.Text && token.IsBeginParen)
                    { 
                        lastToken.Type = Token.TokenType.Function;

                        if(newToken != null)
                        { 
                            if(newToken.Type == Token.TokenType.CommaDelimited) 
                                lastToken.Merge(newToken);
                            else
                                lastToken.Add(newToken);
                        }

                        continue;
                    }

                    // Is this an array indexer?
                    else if(lastToken != null && token.IsBeginArray && (lastToken.Type == Token.TokenType.Text || lastToken.Type == Token.TokenType.Array))
                    { 
                        lastToken.Type = Token.TokenType.Array;

                        if(token.IsExpression && token.Count > 0)
                        {
                            token.Type = Token.TokenType.ArrayIndexer;
                            lastToken.Add(newToken);
                        }
                        else
                            lastToken.Add(new Token("", Token.TokenType.ArrayIndexer) { newToken });

                        continue;
                    }

                    // Is this an explicit array?
                    else if(token.IsBeginArray)
                    { 
                        var array = new Token("", Token.TokenType.ExplicitArray);

                        if(newToken != null)
                            array.Merge(newToken);

                        newToken = array;
                    }

                    outputTokens.Add(newToken);
                    continue;
                }
               
                // Parse everything before the comma
                else if(token.IsComma)
                { 
                    var previous = commaDelimited;

                    commaDelimited = ReduceItem(outputTokens, left, commaDelimited, Token.TokenType.CommaDelimited);

                    if(previous == null)
                        left = left == -1 ? outputTokens.Count : left+1;

                    continue;
                }
               
                // Is this a multi-part?
                else if(token.IsMultiDot)
                { 
                    left = ReduceMultipart(outputTokens, tokens, left);
                    continue;
                }

                else if((token.IsOperator || token.IsPunctuation) && !_validOperators.ContainsKey(token.Value))
                    throw new Transformer.SyntaxException($"Invalid operator: {token.Value}");

                outputTokens.Add(token);
                
                if(left == -1)
                    left = outputTokens.Count - 1;
            }

            if(multiPart != null) 
                ReduceItem(outputTokens, left, multiPart, Token.TokenType.Multipart);

            if(commaDelimited != null) 
                ReduceItem(outputTokens, left, commaDelimited, Token.TokenType.CommaDelimited);
            else if(outputTokens.Count > 1) 
                return Reduce(outputTokens);

            if(outputTokens.Count == 0)
                return null;

            return outputTokens[0];
        }

        private static int ReduceMultipart(List<Token> outputTokens, Queue<Token> tokens, int left)
        {    
            var lastPart  = outputTokens.Last();

            outputTokens.RemoveAt(outputTokens.Count-1);

            var multiPart = outputTokens.LastOrDefault();

            if(multiPart == null || multiPart.Type != Token.TokenType.Multipart)
            {
                multiPart = new Token("", Token.TokenType.Multipart);
                outputTokens.Add(multiPart);
            }

            multiPart.Add(lastPart);

            if(!tokens.Peek().IsOperator)
            { 
                var result = InnerPrecompile(tokens, null, (t)=> !t.IsMultiDot && t.Type != Token.TokenType.Text && !t.IsBeginArray);

                if(result.Type == Token.TokenType.Multipart)
                    multiPart.Merge(result);
                else                
                    multiPart.Add(result);
            }

            return outputTokens.Count - 1;
        }

        private static Token ReduceItem(List<Token> outputTokens, int left, Token? container, Token.TokenType type)
        {        
            left = left > 0 ? left : 0;

            var numItems = outputTokens.Count - left;
            Token? arrayItem = null;

            if(numItems == 0)
                arrayItem = outputTokens[0];
            else if(numItems == 1)
                arrayItem = outputTokens[left];
            else
                arrayItem = Reduce(outputTokens.Skip(left).ToList());

            // Remove the items you just reduced
            for(var i = 0; i < numItems; ++i)
                outputTokens.RemoveAt(left);

            if(container == null)
            {
                container = new Token("", type);
                outputTokens.Add(container);
            }

            container.Add(arrayItem);

            return container;
        }

        // Listed in operator precedence
        private static List<IEnumerable<string>> _operators = new List<IEnumerable<string>>
        {
            new [] { "*", "/", "%" }, 
            new [] { "%" }, 
            new [] { "+", "-" }, 
            new [] { "<" }, 
            new [] { ">" }, 
            new [] { "<=" },  
            new [] { ">=" },  
            new [] { "==" },  
            new [] { "!=" },  
            new [] { "&&", "and" }, 
            new [] { "||", "or" }
        };

        /*****************************************************************************/
        private static Token Reduce(IList<Token> outputTokens)
        {
            foreach(var ops in _operators)
            { 
                while(ReduceExpression(outputTokens, ops))
                    ;

                if(outputTokens.Count < 3)
                    break;
            }

            if(outputTokens.Count == 5 && outputTokens[1].Value == "?" && outputTokens[3].Value == ":")
            {
                var newToken = new Token("", Token.TokenType.Tertiary);

                newToken.Add(outputTokens[0]);
                newToken.Add(outputTokens[2]);
                newToken.Add(outputTokens[4]);

                return newToken;
            }

            return outputTokens.First();
        }

        /*****************************************************************************/
        private static bool ReduceExpression(IList<Token> outputTokens, IEnumerable<string> checkOps)
        {
            if(outputTokens.Count >= 3)
            { 
                for(var i = 1; i < outputTokens.Count(); ++i)
                { 
                    var op = outputTokens[i];

                    if(!op.IsOperator || !checkOps.Contains(op.Value))
                        continue;

                    var leftToken = outputTokens[i-1];

                    outputTokens.RemoveAt(i-1);
                    outputTokens.RemoveAt(i-1);

                    var rightToken = outputTokens[i-1];
                    var exprToken = new Token("", Token.TokenType.Expression);

                    outputTokens.RemoveAt(i-1);
                    
                    exprToken.Add(leftToken);
                    exprToken.Add(op);
                    exprToken.Add(rightToken);

                    outputTokens.Insert(i-1, exprToken);

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
