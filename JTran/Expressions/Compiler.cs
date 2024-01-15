/***************************************************************************
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
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using JTran.Extensions;
using JTran.Parser;
using JTranParser = JTran.Parser.Parser;

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
        internal static IExpression Compile(string expr)
        {
            var parser   = new JTranParser();
            var compiler = new Compiler();

            return compiler.Compile(parser.Parse(expr));
        }

        /*****************************************************************************/
        internal IExpression Compile(IReadOnlyList<Token> tokens)
        {
            var precompiled = Precompiler.Precompile(tokens);

            return InnerCompile(precompiled);
        }
        
        /*****************************************************************************/
        internal IExpression InnerCompile(IEnumerable<Token> tokens)
        {
            var stokens = new Stack<Token>(tokens.Reverse());

            return CreateExpression(stokens, out string lastChar);
        }
        
        private const string _beginBoundary = @"[(";
        private const string _endBoundary   = @"]),:";

        /*****************************************************************************/
        public IExpression CreateArrayExpression(ExpressionToken token, Stack<Token> tokens)
        {
            var items = token.Children;

            var array = new ArrayExpression();

            foreach(var item in items)
            {
                if(item.Value == ",")
                    continue;

                IExpression? expr;

                switch(item.Type)
                {
                    case Token.TokenType.Number:
                        expr = new NumberValue(double.Parse(item.Value));
                        break;

                    case Token.TokenType.Literal:
                        expr = new Value(item.Value);
                        break;

                    case Token.TokenType.Expression:
                        expr = InnerCompile((item as ExpressionToken)!.Children);
                        break;

                    case Token.TokenType.Function:
                        expr = CreateFunction(item as ExpressionToken);
                        break;

                    case Token.TokenType.Text:
                        expr = CreateValue(item);
                        break;

                    default:
                        throw new System.Exception("What is this???");
                }

                array.SubExpressions.Add(expr);
            }

            return array;
        }

        /*****************************************************************************/
        public IExpression CreateExpression(Stack<Token> tokens, out string lastToken)
        {
            IExpression? left  = null;
            IExpression? right = null;
            object?      last  = null;
            IOperator?   op    = null;

            lastToken = null;

            while(tokens.Count > 0)
            {
                var token = tokens.Pop();
                IExpression expr = null;

                lastToken = token.Value;

                switch(token.Type)
                {
                    case Token.TokenType.Expression:
                    {
                        var expressionToken = token as ExpressionToken;

                        expr = InnerCompile(expressionToken.Children);

                        break;
                    }

                    case Token.TokenType.Function:
                    {
                        expr = CreateFunction(token as ExpressionToken);

                        var current = right ?? left;

                        if(!(last is IOperator) && current is MultiPartDataValue multiPart)
                        { 
                            multiPart.AddPart(expr);
                            continue;
                        }

                        break;
                    }

                    case Token.TokenType.Array:
                    case Token.TokenType.Text:
                    {
                        expr = CreateValue(token);

                        bool bContinue = false;
                        var  current   = right ?? left;
                        MultiPartDataValue? mcurrent = null;

                        if(!(last is IOperator) && current is MultiPartDataValue multiPart)
                        { 
                            mcurrent = multiPart;
                            multiPart.AddPart(expr);
                            bContinue = true;
                        }

                        if(token.Type == Token.TokenType.Array)
                        {
                            var array = token as ExpressionToken;

                            if(mcurrent == null)
                            { 
                                mcurrent = new MultiPartDataValue(expr);
                                expr = mcurrent;
                            }

                            foreach(ExpressionToken indexer in array!.Children)
                            {
                                var indexerExpr = InnerCompile(indexer!.Children);

                                mcurrent.AddPart(new Indexer(indexerExpr));
                            }
                        }

                        if(bContinue)
                            continue;

                        break;
                    }

                    case Token.TokenType.Number:
                        expr = new NumberValue(double.Parse(token.Value));
                        break;

                    case Token.TokenType.Literal:
                        expr = new Value(token.Value);
                        break;

                    case Token.TokenType.Tertiary:
                    { 
                        var tertiary  = new TertiaryExpression();
                        var exprToken = token as ExpressionToken;

                        tertiary.Conditional = InnerCompile(new List<Token> { exprToken!.Children[0] });
                        tertiary.IfTrue      = InnerCompile(new List<Token> { exprToken!.Children[1] });
                        tertiary.IfFalse     = InnerCompile(new List<Token> { exprToken!.Children[2] });
                        
                        left = tertiary;
                        right = null;
                        op = null;

                        continue;
                    }

                    case Token.TokenType.Operator:
                    {
                        if(_beginBoundary.Contains(token.Value))
                        {
                            if(token.Value == "[" && tokens.Peek() is ExpressionToken exprArray)
                            { 
                                expr = CreateArrayExpression(exprArray, tokens);
                                tokens.Pop(); // Pop off the array
                                tokens.Pop(); // Pop off the end array
                                goto SetLeftRight;
                            }

                            expr = CreateExpression(tokens, out lastToken);

                            if(token.Value == "[")
                            {
                                var current = EnsureMultiPart(ref left, ref right);

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

                        var newOp = CreateOperator(token.Value);

                        if(op == null)
                        { 
                            op = newOp;
                            last = op;
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

              SetLeftRight:

                if(left == null)
                    left = expr;
                else if(right == null)
                    right = expr;

                last = expr;
            }

          BreakOut:

            return CreateExpression(left, op, right);
        }

        #region Private
       
        /*****************************************************************************/
        private IExpression? EnsureMultiPart(ref IExpression left, ref IExpression? right)
        {
            var current = right ?? left;

            if(!(current is MultiPartDataValue))
            {
                if(right != null)
                    current = right = new MultiPartDataValue(right);
                else
                    current = left = new MultiPartDataValue(left);
            }

            return current;
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
        private IExpression CreateFunction(ExpressionToken token)
        {        
            var func = new FunctionCall(token.Value.EnsureDoesNotStartWith("."));

            foreach(var child in token.Children)
            {
                var param = InnerCompile(new List<Token> { child });

                if(param != null)
                    func.AddParameter(param);
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

        /*****************************************************************************/
        private static IOperator CreateOperator(string op)
        {
            switch(op)
            {
                case "==":  return new EqualOperator();
                case "!=":  return new NotEqualOperator();
                case ">":   return new GreaterThanOperator();
                case ">=":  return new GreaterThanEqualOperator();
                case "<":   return new LessThanOperator();
                case "<=":  return new LessThanEqualOperator();
                case "+":   return new AdditionOperator();
                case "-":   return new SubtractionOperator();
                case "*":   return new MultiplyOperator();
                case "/":   return new DivisionOperator();
                case "%":   return new ModulusOperator();
                case "&&":  return new AndOperator();
                case "and": return new AndOperator();
                case "||":  return new OrOperator();
                case "or":  return new OrOperator();

                default:
                    throw new Transformer.SyntaxException($"'{op}' is an invalid operator");
            }
        }

        #endregion
    }
}
