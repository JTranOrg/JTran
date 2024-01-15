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
        private readonly static IDictionary<string, string> _beginBoundary = new Dictionary<string, string>
        {
            { "[", "]" },
            { "(", ")" }
        };

        /*****************************************************************************/
        internal static IList<Token> Precompile(IEnumerable<Token> tokens)
        {
            var output = InnerPrecompile(new Queue<Token>(tokens), false, null, out Token last);

            return output;
        }

        #region Private 

        /*****************************************************************************/
        internal static void ReduceExpressions(List<Token> tokens)
        {              
            var right = 0;
            var left = -1;

            while(right < tokens.Count)
            {
                var token = tokens[right];

                if(token.IsEndBoundary)
                { 
                    if(left != -1 && right - left > 1)
                    { 
                        tokens.RemoveAt(right);
                        ReduceExpression(tokens, left, right - left);
                        right = left;
                    }
                    else if(token.IsComma)
                    { 
                        tokens.RemoveAt(right);
                        left = -1;
                        continue;
                    }

                    left = -1;
                }
                else if(token.IsConditional || token.IsTertiary)
                { 
                    if(left != -1 && right - left > 1)
                    { 
                        ReduceExpression(tokens, left, right - left);
                        right = left + 1;
                    }

                    left = -1;
                }
                else if(left == -1 && !token.IsBoundary)
                    left = right;

                ++right;
            }

            if(left != -1)
                ReduceExpression(tokens, left, right - left);

            return;
        }

        /*****************************************************************************/
        private static void CheckPrecedence(List<Token> tokens)
        {        
            // Find all * and / and turn into subexpressions
            while(tokens.Count > 3)
            {
                var index = tokens.IndexOfAny( t=> t.Type == Token.TokenType.Operator && "*/%".Contains(t.Value)); 

                if(index < 1 || tokens.Count <= index+1)
                    break;

                ReduceExpression(tokens, index-1, 3);
            }
        }

        /*****************************************************************************/
        private static void ReduceExpression(List<Token> tokens, int start, int numItems)
        {
            if(numItems > 1) 
            {
                var newToken = new ExpressionToken();

                for(var i = 0; i < numItems; ++i)
                    newToken.Children.Add(tokens[start + i]);

                // Remove from token list
                for(var i = 0; i < numItems; ++i)
                    tokens.RemoveAt(start);

                // Insert new token into token list
                tokens.Insert(start, newToken);

                CheckPrecedence(newToken.Children);
            }

            if(tokens.Count == 5 && tokens[1].Value == "?" && tokens[3].Value == ":")
            {
                var newToken = new ExpressionToken();

                newToken.Type = Token.TokenType.Tertiary;

                newToken.Children.Add(tokens[0]);
                newToken.Children.Add(tokens[2]);
                newToken.Children.Add(tokens[4]);

                tokens.Clear();
                tokens.Add(newToken);
            }

            return;
        }

        /*****************************************************************************/
        private static List<Token> InnerPrecompile(Queue<Token> tokens, bool conditional, string? endBoundary, out Token last)
        {
            var outputTokens = new List<Token>();

            last = null;

            while(tokens.Count > 0)
            {
                var token = tokens.Peek();

                last = token;

                tokens.Dequeue();

                if(token.IsOperator)
                {
                    if(_beginBoundary.ContainsKey(token.Value))
                    {
                        var expressionToken = new ExpressionToken();

                        expressionToken.Children = InnerPrecompile(tokens, false, _beginBoundary[token.Value], out last);

                        if(token.Value == "[")
                        {
                            if(last.Value != "]")
                                throw new Transformer.SyntaxException("Missing \"]\" in array indexer expression");

                            var previous = outputTokens.LastOrDefault();

                            if(previous != null && previous.Type == Token.TokenType.Text)
                            {
                                var array = new ExpressionToken(Token.TokenType.Array);

                                array.Value = previous.Value;
                                expressionToken.Type = Token.TokenType.ArrayIndexer;
                                array.Children.Add(expressionToken);

                                outputTokens.RemoveAt(outputTokens.Count - 1);
                                outputTokens.Add(array);
                            }
                            else if(previous != null && previous.Type == Token.TokenType.Array)
                            {
                                var array = previous as ExpressionToken;

                                expressionToken.Type = Token.TokenType.ArrayIndexer;
                                array!.Children.Add(expressionToken);
                            }
                            else
                            { 
                                outputTokens.Add(token);
                                outputTokens.Add(ExpressionOrSingle(expressionToken));
                                outputTokens.Add(last);
                            }
                        }
                        else
                        { 
                            outputTokens.Add(ExpressionOrSingle(expressionToken));
                        }

                        continue;
                    }

                    if(token.IsEndBoundary && (endBoundary == null || endBoundary == ","))
                        break;

                    if(token.IsOperator && endBoundary == token?.Value)
                        goto ReduceExpression;

                    if(token.IsConditional)
                    {
                        PopExpression(tokens, outputTokens, token, true, out last);
                        continue;
                    }
                 }

                // Is it a function call?
                else if(token.Type == Token.TokenType.Text && tokens.Count > 0 && tokens.Peek().IsBeginParen)
                {
                    var functionToken = new ExpressionToken();

                    functionToken.Value = token.Value;
                    functionToken.Type = Token.TokenType.Function;

                    outputTokens.Add(functionToken);

                    tokens.Dequeue(); // Pop off the begin parenthesis

                    // Add parameters
                    while(true)
                    { 
                        functionToken.Children = InnerPrecompile(tokens, false, ")", out last);

                      //  functionToken.Children.Add(ExpressionOrSingle(expressionToken));
                       // outputTokens.Add(last);

                        if(!last.IsComma)
                        {
                            if(!last.IsEndParen)
                                throw new Transformer.SyntaxException("Missing \")\" in function call");

                            break;
                        }
                    }

                    continue;
                }

                outputTokens.Add(token);
            }

          ReduceExpression:
            ReduceExpressions(outputTokens);

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

            expressionToken.Children = InnerPrecompile(tokens, conditional, null, out last);

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

            expressionToken.Children = InnerPrecompile(tokens, false, null, out Token last2);

            outputTokens.Add(ExpressionOrSingle(expressionToken));

            return;
        }

        #endregion
    }
}
