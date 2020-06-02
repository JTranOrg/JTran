﻿/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: Compiler.cs					    		        
 *        Class(es): Compiler				         		            
 *          Purpose: Compiles tokenized expression into a runnable object model                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using JTran.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    /// <summary>
    /// Parse an expression into a list of tokens
    /// </summary>
    internal class Compiler
    {
        private readonly Stack<ComplexExpression> _stack = new Stack<ComplexExpression>();

        /*****************************************************************************/
        public Compiler()
        {
        }

        public class SyntaxErrorException : Exception
        {
            public SyntaxErrorException(string msg) : base(msg) { }
        }

        /*****************************************************************************/
        public static IExpression Compile(string expr)
        {
            var parser   = new Parser();
            var compiler = new Compiler();

            return compiler.Compile(parser.Parse(expr));
        }

        /*****************************************************************************/
        public IExpression Compile(IReadOnlyList<Token> tokens)
        {
            var stokens = new Stack<Token>(tokens.Reverse());

            return CreateExpression(stokens, out string lastToken);
        }
        
        private const string _beginBoundary = @"[(";
        private const string _endBoundary   = @"]),:";

        /*****************************************************************************/
        public IExpression CreateExpression(Stack<Token> tokens, out string lastToken)
        {
            IExpression left  = null;
            IExpression right = null;
            IOperator   op    = null;

            lastToken = null;

            while(tokens.Count > 0)
            {
                var token = tokens.Pop();
                IExpression expr = null;

                switch(token.Type)
                {
                    case Token.TokenType.Text:
                    {
                        if(tokens.Count > 0 && tokens.Peek().Value == "(")
                            expr = CreateFunction(tokens, token.Value.EnsureDoesNotStartWith("."));
                        else
                            expr = CreateValue(token);

                        var current = right ?? left;

                        if(current is MultiPartDataValue multiPart)
                        { 
                            multiPart.AddPart(expr);
                            continue;
                        }

                        break;
                    }

                    case Token.TokenType.Number:
                        expr = new NumberValue(decimal.Parse(token.Value));
                        break;

                    case Token.TokenType.Literal:
                        expr = new Value(token.Value);
                        break;

                    case Token.TokenType.Operator:
                    {
                        if(_beginBoundary.Contains(token.Value))
                        {
                            expr = CreateExpression(tokens, out lastToken);

                            if(token.Value == "[")
                            {
                                var current = right ?? left;

                                if(!(current is MultiPartDataValue))
                                {
                                    if(right != null)
                                        current = right = new MultiPartDataValue(right);
                                    else
                                        current = left = new MultiPartDataValue(left);
                                }

                                if(current is MultiPartDataValue mcurrent)
                                    mcurrent.AddPart(new Indexer(expr));

                                continue;
                            }

                            break;
                        }

                        if(_endBoundary.Contains(token.Value))
                        { 
                            lastToken = token.Value;
                            goto BreakOut;
                        }

                        if(token.Value == "?")
                        {
                            var tertiary = new TertiaryExpression();

                            tertiary.Conditional = CreateExpression(left, op, right);
                            tertiary.IfTrue = CreateExpression(tokens, out string lastToken2);

                            if(lastToken2 != ":")
                                throw new SyntaxErrorException("Missing ':' in tertiary expression");

                            tertiary.IfFalse = CreateExpression(tokens, out string lastToken3);

                            left = tertiary;
                            right = null;
                            op = null;

                            continue;
                        }

                        var newOp = CreateOperator(token.Value);

                        if(op == null)
                        { 
                            op = newOp;
                            continue;
                        }

                        if(op.Precedence >= newOp.Precedence)
                        {
                           left  = new ComplexExpression { Left = left, Operator = op, Right = right };
                           op    = newOp;
                           right = CreateExpression(tokens, out lastToken);

                           continue;
                        }
                        else
                        {
                           var newExpr = CreateExpression(tokens, out lastToken);
                           right = new ComplexExpression { Left = right, Operator = newOp, Right = newExpr };
 
                           continue;
                        }
                   }
                }

                if(left == null)
                    left = expr;
                else if(right == null)
                    right = expr;
            }

          BreakOut:

            return CreateExpression(left, op, right);
        }

        /*****************************************************************************/
        private IExpression CreateExpression(IExpression left, IOperator op, IExpression right)
        {            
            if(left != null && op != null && right != null)
                return new ComplexExpression { Left = left, Operator = op, Right = right };

            return left;
        }

        /*****************************************************************************/
        private IExpression CreateFunction(Stack<Token> tokens, string name)
        {        
            var func = new FunctionCall(name);

            // Pop off the begin paren
            tokens.Pop();

            while(true)
            {
                var param = CreateExpression(tokens, out string lastChar);

                if(param != null)
                    func.AddParameter(param);

                if(lastChar == ")")
                    break;
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
                    return new NumberValue(decimal.Parse(token.Value));

                case Token.TokenType.Text:
                { 
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
                    throw new SyntaxErrorException("Unknown token type");
            }
        }

        /*****************************************************************************/
        private static IOperator CreateOperator(string op)
        {
            switch(op)
            {
                case "==": return new EqualOperator();
                case "!=": return new NotEqualOperator();
                case ">":  return new GreaterThanOperator();
                case ">=": return new GreaterThanEqualOperator();
                case "<":  return new LessThanOperator();
                case "<=": return new LessThanEqualOperator();
                case "+":  return new AdditionOperator();
                case "-":  return new SubtractionOperator();
                case "*":  return new MultiplyOperator();
                case "/":  return new DivisionOperator();
                case "%":  return new ModulusOperator();
                case "&&": return new AndOperator();
                case "||": return new OrOperator();

                default:
                    throw new SyntaxErrorException($"'{op}' is an invalid operator");
            }
        }
    }
}