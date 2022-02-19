/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using JTran.Extensions;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    /// <summary>
    /// Parses a list of tokens and creates a hierarchy of subexpressions       
    /// </summary>
    internal static class Precompiler
    {
        private const string _beginBoundary = @"[(";
        private static IDictionary<string, bool> _endBoundary = new Dictionary<string, bool>
        {
            { "]", true },
            { ")", true },
            { ",", true }
        };

       private static IDictionary<string, bool> _conditionals = new Dictionary<string, bool>
        {
            { "&&", true },
            { "||", true }
        };

        /*****************************************************************************/
        internal static IList<Token> Precompile(IEnumerable<Token> tokens)
        {
            return InnerPrecompile(new Queue<Token>(tokens), false, out Token last);
        }

        #region Private 

        /*****************************************************************************/
        private static List<Token> InnerPrecompile(Queue<Token> tokens, bool conditional, out Token last)
        {
            var outputTokens = new List<Token>();

            last = null;

            while(tokens.Count > 0)
            {
                var token = tokens.Peek();

                last = token;

                if(token.Value == "?")
                {
                    if(!conditional)
                    {
                        tokens.Dequeue();
                        HandleTertiary(tokens, outputTokens, token);
                    }

                    break;
                }

                tokens.Dequeue();

                if(token.Value == ":")
                {
                    break;
                }

                if(token.Value.Length > 0 && token.Type != Token.TokenType.Literal)
                {
                    if(_beginBoundary.Contains(token.Value))
                    {
                        var expressionToken = new ExpressionToken();

                        expressionToken.Children = InnerPrecompile(tokens, false, out last);

                        if(token.Value == "[")
                        {
                            if(last.Value != "]")
                                throw new Transformer.SyntaxException("Missing \"]\" in array indexer expression");

                            outputTokens.Add(token);
                            outputTokens.Add(ExpressionOrSingle(expressionToken));
                            outputTokens.Add(last);
                        }
                        else
                            outputTokens.Add(ExpressionOrSingle(expressionToken));

                        continue;
                    }

                    if(_endBoundary.ContainsKey(token.Value))
                        break;

                    if(_conditionals.ContainsKey(token.Value))
                    {
                        PopExpression(tokens, outputTokens, token, true, out last);
                        continue;
                    }
                }

                // Is it a function call?
                if(token.Type == Token.TokenType.Text && tokens.Count > 0 && tokens.Peek().Value == "(")
                {
                    outputTokens.Add(token);

                    var paren = tokens.Dequeue();
                    outputTokens.Add(paren);

                    while(true)
                    { 
                        var expressionToken = new ExpressionToken();

                        expressionToken.Children = InnerPrecompile(tokens, false, out last);

                        outputTokens.Add(ExpressionOrSingle(expressionToken));
                        outputTokens.Add(last);

                        if(last.Value != ",")
                        {
                            if(last.Value != ")")
                                throw new Transformer.SyntaxException("Missing \")\" in function call");

                            break;
                        }
                    }

                    continue;
                }

                outputTokens.Add(token);
            }

            return outputTokens;
        }

        private static Token ExpressionOrSingle(ExpressionToken expressionToken)
        {
            if(expressionToken.Children.Count == 1)
            {
                var result = expressionToken.Children[0];

                if(result is ExpressionToken exprTokenInner)
                    return ExpressionOrSingle(exprTokenInner);

                return result;
            }
            return expressionToken;
        }

        /*****************************************************************************/
        private static void PopExpression(Queue<Token> tokens, IList<Token> outputTokens, Token token, bool conditional, out Token last)
        {
            var expressionToken = new ExpressionToken() { Children = new List<Token>(outputTokens) };

            outputTokens.Clear();
            outputTokens.Add(ExpressionOrSingle(expressionToken));
            outputTokens.Add(token);

            expressionToken = new ExpressionToken();

            expressionToken.Children = InnerPrecompile(tokens, conditional, out last);

            outputTokens.Add(ExpressionOrSingle(expressionToken));
        }

        /*****************************************************************************/
        internal static void HandleTertiary(Queue<Token> tokens, IList<Token> outputTokens, Token token)
        {
            PopExpression(tokens, outputTokens, token, false, out Token last);

            if(last.Value != ":")
                throw new Transformer.SyntaxException("Missing \":\" in tertiary expression");

            outputTokens.Add(last);

            var expressionToken = new ExpressionToken();

            expressionToken.Children = InnerPrecompile(tokens, false, out Token last2);

            outputTokens.Add(ExpressionOrSingle(expressionToken));

            return;
        }

        #endregion
    }
}
