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
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
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
        private const string _endBoundary   = @"]),";

        /*****************************************************************************/
        internal static IList<Token> Precompile(IEnumerable<Token> tokens)
        {
            return InnerPrecompile(new Queue<Token>(tokens), out Token last);
        }

        #region Private 

        /*****************************************************************************/
        private static List<Token> InnerPrecompile(Queue<Token> tokens, out Token last)
        {
            var outputTokens = new List<Token>();

            last = null;

            while(tokens.Count > 0)
            {
                var token = tokens.Dequeue();

                last = token;

                if(token.Value == "?")
                {
                    HandleTertiary(tokens, outputTokens, token);
                    break;
                }

                if(token.Value == ":")
                {
                    break;
                }

                if(_beginBoundary.Contains(token.Value))
                {
                    var expressionToken = new ExpressionToken();

                    expressionToken.Children = InnerPrecompile(tokens, out last);

                    if(token.Value == "[")
                    {
                        if(last.Value != "]")
                            throw new Transformer.SyntaxException("Missing \"]\" in array indexer expression");

                        outputTokens.Add(token);
                        outputTokens.Add(expressionToken);
                        outputTokens.Add(last);
                    }
                    else
                        outputTokens.Add(expressionToken);

                    continue;
                }

                if(_endBoundary.Contains(token.Value))
                    break;

                // Is it a function call?
                if(token.Type == Token.TokenType.Text && tokens.Count > 0 && tokens.Peek().Value == "(")
                {
                    outputTokens.Add(token);

                    var paren = tokens.Dequeue();
                    outputTokens.Add(paren);

                    while(true)
                    { 
                        var expressionToken = new ExpressionToken();

                        expressionToken.Children = InnerPrecompile(tokens, out last);

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
            return expressionToken.Children.Count == 1 ? expressionToken.Children[0] : expressionToken;
        }

        /*****************************************************************************/
        internal static void HandleTertiary(Queue<Token> tokens, IList<Token> outputTokens, Token token)
        {
            var expressionToken = new ExpressionToken() { Children = new List<Token>(outputTokens) };

            outputTokens.Clear();
            outputTokens.Add(expressionToken);
            outputTokens.Add(token);

            expressionToken = new ExpressionToken();

            expressionToken.Children = InnerPrecompile(tokens, out Token last);

            outputTokens.Add(ExpressionOrSingle(expressionToken));

            if(last.Value != ":")
                throw new Transformer.SyntaxException("Missing \":\" in tertiary expression");

            outputTokens.Add(last);

            expressionToken = new ExpressionToken();

            expressionToken.Children = InnerPrecompile(tokens, out Token last2);

            outputTokens.Add(ExpressionOrSingle(expressionToken));

            return;
        }

        #endregion
    }
}
