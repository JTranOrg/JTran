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
            if(tokens.Count == 0)
                return null;

            var result = new Token("", Token.TokenType.Text);

            while(tokens.Count > 0)
            {
                var token = tokens.Peek();

                // Is it the end?
                if(endIt != null && endIt(token))
                    break;

                token = tokens.Dequeue();

                if(token.IsOperator && token.Value == endBoundary)
                    break;

                // Turn the contents of "[]" into a single token
                if(token.IsBeginArray)
                { 
                    HandleArray(tokens, result);
                    continue;
                }

                // Turn the contents of "()" into a single token
                else if(token.IsBeginParen)
                { 
                    HandleParens(tokens, result);
                    continue;
                }

                else if(token.IsComma)
                {                   
                    result = Reduce(result, 2);
                    HandleComma(tokens, result);
                    continue;
                }

                else if(token.IsMultiDot)
                {                    
                    HandleMultipart(tokens, result);
                    continue;
                }

                // Check for invalid operators
                else if((token.IsOperator || token.IsPunctuation) && !_validOperators.ContainsKey(token.Value))
                    throw new Transformer.SyntaxException($"Invalid operator: {token.Value}");

                // Reduce expressions before a tertiary operator
                else if(token.IsTertiary)
                { 
                    Reduce(result, 2);
                }

                // Is it an expression
                else if(token.IsOperator)
                { 
                    if(result.Count == 0)
                        throw new Transformer.SyntaxException($"Missing left operand before {token.Value}");

                    result.Type = Token.TokenType.Expression;
                }

                result.Add(token);
            }

            result = Reduce(result);

            return result.Count == 1 ? result[0] : result;
        }

        /*****************************************************************************/
        private static void HandleMultipart(Queue<Token> tokens, Token result)
        {        
            var newToken = InnerPrecompile(tokens, endIt: (t)=> t.IsOperator && !t.IsMultiDot && !t.IsBeginBoundary);

            var last = result.Last();

            result.RemoveAt(result.Count - 1);

            var multipart = new Token("", Token.TokenType.Multipart);

            result.Add(multipart);

            multipart.Add(last);
            multipart.MergeOrAdd(newToken, Token.TokenType.Multipart);
        }

        /*****************************************************************************/
        private static void HandleComma(Queue<Token> tokens, Token result)
        {    
            var newToken = InnerPrecompile(tokens, endIt: (t)=> t.IsEndBoundary);

            result.Type = Token.TokenType.CommaDelimited;
                            
            result.MergeOrAdd(newToken, Token.TokenType.CommaDelimited);
        }

        /*****************************************************************************/
        private static void HandleParens(Queue<Token> tokens, Token result)
        {
            var last = result.LastOrDefault();

            var newToken = InnerPrecompile(tokens, Token.EndParen);

            if(last != null && last.Type == Token.TokenType.Text)
            {
                last.Type = Token.TokenType.Function;

                // No params?
                if(newToken.Value == "" && newToken.Type == Token.TokenType.Text)
                    return;

                if(newToken.Type == Token.TokenType.CommaDelimited)
                    last.Merge(newToken);
                else
                    last.Add(newToken);
            }
            else
                result.Add(newToken);

            return;
        }        
        
        /*****************************************************************************/
        private static void HandleArray(Queue<Token> tokens, Token result)
        {
            var newToken = InnerPrecompile(tokens, Token.EndArray);
            var last     = result.LastOrDefault();

            if(last != null && (last.Type == Token.TokenType.Text || last.Type == Token.TokenType.Array))
            { 
                last.Add(new Token("", Token.TokenType.ArrayIndexer) { newToken });
                last.Type = Token.TokenType.Array;
            }
            else
            { 
                var array = new Token("", Token.TokenType.ExplicitArray);

                array.MergeOrAdd(newToken, Token.TokenType.CommaDelimited);
                result.Add(array);
            }

            return;
        }

        /*****************************************************************************/
        private static Token MergeOrAdd(this Token token, Token merge, Token.TokenType type) 
        {
            if(merge.Type == type)
                token.Merge(merge);
            else
                token.Add(merge);

            return token;
        }

        /*****************************************************************************/
        // Listed in operator precedence
        private static List<IEnumerable<string>> _operators = new List<IEnumerable<string>>
        {
            new [] { "*", "/", "%" }, 
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
        private static Token Reduce(Token token, int numOperators = 3)
        {
            foreach(var ops in _operators)
            { 
                while(ReduceExpression(token, ops, numOperators))
                    ;

                if(token.Count < 3)
                    break;
            }

            if(token.Count == 5 && token[1].Value == "?" && token[3].Value == ":")
            {
                var newToken = new Token("", Token.TokenType.Tertiary);

                newToken.Add(token[0]);
                newToken.Add(token[2]);
                newToken.Add(token[4]);

                token.RemoveAt(0);
                token.RemoveAt(0);
                token.RemoveAt(0);
                token.RemoveAt(0);
                token.RemoveAt(0);

                token.Add(newToken);
            }

            return token;
        }

        /*****************************************************************************/
        private static bool ReduceExpression(IList<Token> outputTokens, IEnumerable<string> checkOps, int numOperators)
        {
            if(outputTokens.Count > numOperators)
            { 
                for(var i = 1; i < outputTokens.Count(); ++i)
                { 
                    var op = outputTokens[i];

                    if(!op.IsOperator || !checkOps.Contains(op.Value))
                        continue;

                    if((outputTokens.Count - i) < 2)
                        throw new Transformer.SyntaxException($"Missing operand after {op}");

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
