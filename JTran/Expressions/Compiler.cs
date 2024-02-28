/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Compiler.cs					    		        
 *        Class(es): Compiler				         		            
 *          Purpose: Compiles tokenized expression into a runnable object model                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JTran.Common;
using JTran.Extensions;
using JTran.Parser;
using JTranParser = JTran.Parser.ExpressionParser;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    /// <summary>
    /// Parse an expression into a list of tokens
    /// </summary>
    internal class Compiler
    {
        /*****************************************************************************/
        internal Compiler()
        {
        }

        /*****************************************************************************/
        [Obsolete]
        internal static IExpression Compile(string expr)
        {
            var parser   = new JTranParser();
            var compiler = new Compiler();

            return compiler.Compile(parser.Parse(CharacterSpan.FromString(expr)));
        }

        /*****************************************************************************/
        internal static IExpression Compile(CharacterSpan expr)
        {
            var parser   = new JTranParser();
            var compiler = new Compiler();

            return compiler.Compile(parser.Parse(expr));
        }

        /*****************************************************************************/
        internal IExpression Compile(IReadOnlyList<Token> tokens)
        {
            var precompiled = Precompiler.Precompile(tokens);

            return InnerCompile(new [] { precompiled } );
        }
        
        /*****************************************************************************/
        internal IExpression InnerCompile(IEnumerable<Token> tokens)
        {
            var stokens = new Stack<Token>(tokens.Reverse());

            return CreateExpression(stokens, out string lastChar);
        }
        
        /*****************************************************************************/
        public IExpression CreateArrayExpression(Token token)
        {
            var array  = new ArrayExpression();
            var tokens = new List<Token>();

            foreach(var item in token)
            {
                tokens.Add(item);

                var expr = InnerCompile(tokens);

                array.SubExpressions.Add(expr);

                tokens.Clear();
            }

            return array;
        }

        /*****************************************************************************/
        public IExpression CreateExpression(Stack<Token> tokens, out string lastToken)
        {
            IExpression? leftExpr  = null;
            IExpression? rightExpr = null;
            object?      last      = null;
            IOperator?   op        = null;

            lastToken = null;

            while(tokens.Count > 0)
            {
                var token = tokens.Pop();
                IExpression? expr = null;

                lastToken = token.Value;

                switch(token.Type)
                {
                    case Token.TokenType.Literal:
                        expr = new Value(token.Value);
                        break;

                    case Token.TokenType.Text:
                        expr = CreateValue(token);
                        break;

                    case Token.TokenType.Number:
                        expr = new NumberValue(double.Parse(token.Value));
                        break;                    
                        
                    case Token.TokenType.Array:
                        expr = CreateIndexedArray(token);
                        break;

                    case Token.TokenType.ArrayIndexer:
                        expr = InnerCompile(token);
                        break;

                    case Token.TokenType.Multipart:
                        expr = CreateMultipartValue(token);
                        break;

                    case Token.TokenType.Expression:
                        expr = InnerCompile(token);
                        break;

                    case Token.TokenType.Function:
                        expr = CreateFunction(token);
                        break;

                    case Token.TokenType.Tertiary:
                    { 
                        var tertiary  = new TertiaryExpression();
                        var exprToken = token;

                        tertiary.Conditional = InnerCompile(new [] { exprToken[0] });
                        tertiary.IfTrue      = InnerCompile(new [] { exprToken[1] });
                        tertiary.IfFalse     = InnerCompile(new [] { exprToken[2] });
                        
                        leftExpr = tertiary;
                        rightExpr = null;
                        op = null;

                        continue;
                    }

                    case Token.TokenType.ExplicitArray:
                        expr = CreateArrayExpression(token);
                        break;

                    case Token.TokenType.Operator:
                    {
                        var newOp = CreateOperator(token.Value);

                        if(op == null)
                        { 
                            op = newOp;
                            last = op;
                            continue;
                        }

                        leftExpr  = new ComplexExpression { Left = leftExpr, Operator = op, Right = rightExpr };
                        op        = newOp;
                        rightExpr = CreateExpression(tokens, out lastToken);

                        continue;
                    }

                    default:
                        throw new Transformer.SyntaxException("Unknown token type");
                }

                if(leftExpr == null)
                    leftExpr = expr;
                else if(rightExpr == null)
                    rightExpr = expr;

                last = expr;
            }

            return CreateExpression(leftExpr, op, rightExpr);
        }

        #region Private
       
        /*****************************************************************************/
        private IExpression CreateIndexedArray(Token token)
        {
            var result = new MultiPartDataValue(CreateValue(token));

            foreach(var part in token)
            {
                result.AddPart(new Indexer(InnerCompile(new [] { part })));
            }

            return result;
        }

        /*****************************************************************************/
        private IExpression CreateMultipartValue(Token token)
        {
            var result = new MultiPartDataValue(null);

            foreach(var part in token)
            {
                result.AddPart(InnerCompile(new [] { part }));
            }

            return result;
        }

        /*****************************************************************************/
        private IExpression CreateExpression(IExpression left, IOperator op, IExpression right)
        {            
            if(left != null && right != null)
            {
                if(op != null)
                    return new ComplexExpression { Left = left, Operator = op, Right = right };

                if(right is DataValue)
                    return new ComplexExpression { Left = left, Operator = new DataPart(), Right = right };
            }

            return left;
        }

        /*****************************************************************************/
        private IExpression CreateFunction(Token token)
        {        
            var func = new FunctionCall(token.Value.EnsureDoesNotStartWith("."));

            if(token.Count > 0)
            { 
                foreach(var child in token)
                {
                    var param = InnerCompile(new [] { child });

                    if(param != null)
                        func.AddParameter(param);
                }
            }

            return func;
        }

        /*****************************************************************************/
        private IExpression CreateValue(Token token)
        {
            switch(token.Type)
            {
                case Token.TokenType.Literal:
                    return new Value(token.Value);

                case Token.TokenType.Number:
                    return new NumberValue(double.Parse(token.Value));

                case Token.TokenType.Array:
                case Token.TokenType.Text:
                { 
                    var lval = token.Value.ToLower();

                    if(lval == "true")
                        return new Value(true);

                    if(lval == "false")
                        return new Value(false);

                    if(token.Value.StartsWith("$"))
                    { 
                        if(token.Value.Contains("."))
                        {
                            var index   = token.Value.IndexOf(".");
                            var varName = token.Value.Substring(1, index - 1);
                            var parts   = new MultiPartDataValue(new VariableValue(varName));

                            parts.AddPart(new DataValue(token.Value.Substring(index + 1)));

                            return parts;
                        }
                    
                        return new VariableValue(token.Value.Substring(1));
                    }

                    return new DataValue(token.Value.EnsureDoesNotStartWith("."));
                }

                default:
                    throw new Transformer.SyntaxException("Unknown token type");
            }
        }

        private static IOperator _equalOperator            = new EqualOperator();
        private static IOperator _notEqualOperator         = new NotEqualOperator();
        private static IOperator _greaterThanOperator      = new GreaterThanOperator();
        private static IOperator _greaterThanEqualOperator = new GreaterThanEqualOperator();
        private static IOperator _lessThanOperator         = new LessThanOperator();
        private static IOperator _lessThanEqualOperator    = new LessThanEqualOperator();
        private static IOperator _additionOperator         = new AdditionOperator();
        private static IOperator _subtractionOperator      = new SubtractionOperator();
        private static IOperator _multiplyOperator         = new MultiplyOperator();
        private static IOperator _divisionOperator         = new DivisionOperator();
        private static IOperator _moduloOperator           = new ModuloOperator();
        private static IOperator _andOperator              = new AndOperator();
        private static IOperator _orOperator               = new OrOperator();

        private static Dictionary<string, IOperator> _operators = new Dictionary<string, IOperator>
        {          
            { "==",  _equalOperator            },
            { "!=",  _notEqualOperator         },
            { ">" ,  _greaterThanOperator      },
            { ">=",  _greaterThanEqualOperator },
            { "<" ,  _lessThanOperator         },
            { "<=",  _lessThanEqualOperator    },
            { "+" ,  _additionOperator         },
            { "-" ,  _subtractionOperator      },
            { "*" ,  _multiplyOperator         },
            { "/" ,  _divisionOperator         },
            { "%" ,  _moduloOperator           },
            { "&&",  _andOperator              },
            { "and", _andOperator              },
            { "||",  _orOperator               },
            { "or",  _orOperator               }
        };

        /*****************************************************************************/
        private static IOperator CreateOperator(string op)
        {
            if(!_operators.ContainsKey(op))
                throw new Transformer.SyntaxException($"'{op}' is an invalid operator");
            
            return _operators[op];
        }

        #endregion
    }
}
