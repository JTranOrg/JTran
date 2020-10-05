using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran;
using JTran.Expressions;

using Moq;
using JTran.Extensions;
using System.Collections;
using System.Xml.Schema;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class PrecompilerTests
    {
        [TestMethod]
        public void Precompiler_Precompile_simple_expression()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("Name == 'bob'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(3,       tokens2.Count);
            Assert.AreEqual("Name",  tokens2[0].Value);
            Assert.AreEqual("==",    tokens2[1].Value);
            Assert.AreEqual("bob",   tokens2[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_simple_wParens()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("(Name == 'bob')");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,       tokens2.Count);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.IsTrue(tokens2[0] is ExpressionToken);
            Assert.AreEqual(3,       (tokens2[0] as ExpressionToken).Children.Count);
        }

        [TestMethod]
        public void Precompiler_Precompile_simple_func()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("getcity(Name == 'bob')");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(4,          tokens2.Count);
            Assert.AreEqual("getcity",  tokens2[0].Value);
            Assert.AreEqual("(",        tokens2[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[2].Type);
            Assert.AreEqual(")",        tokens2[3].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("getcity(Name == 'bob', $Age)");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(6,          tokens2.Count);
            Assert.AreEqual("getcity",  tokens2[0].Value);
            Assert.AreEqual("(",        tokens2[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[2].Type);
            Assert.AreEqual(",",        tokens2[3].Value);
            Assert.AreEqual("$Age",     tokens2[4].Value);
            Assert.AreEqual(")",        tokens2[5].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams_nested()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("getcity(Name == 'bob', getstate(ZipCode == 98033, Role == 'Engineer'))");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(6,                          tokens2.Count);
            Assert.AreEqual("getcity",                  tokens2[0].Value);
            Assert.AreEqual("(",                        tokens2[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[2].Type);
            Assert.AreEqual(",",                        tokens2[3].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[4].Type);
            Assert.AreEqual(")",                        tokens2[5].Value);

            var expr1 = tokens2[2] as ExpressionToken;
            var expr2 = tokens2[4] as ExpressionToken;

            Assert.AreEqual(3,       expr1.Children.Count);
            Assert.AreEqual("Name",  expr1.Children[0].Value);
            Assert.AreEqual("==",    expr1.Children[1].Value);
            Assert.AreEqual("bob",   expr1.Children[2].Value);

            Assert.AreEqual(6,              expr2.Children.Count);
            Assert.AreEqual("getstate",     expr2.Children[0].Value);
            Assert.AreEqual("(",            expr2.Children[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, expr2.Children[2].Type);
            Assert.AreEqual(",",            expr2.Children[3].Value);
            Assert.AreEqual(Token.TokenType.Expression, expr2.Children[4].Type);
            Assert.AreEqual(")",            expr2.Children[5].Value);

        }

        [TestMethod]
        public void Precompiler_Precompile_complex_expression()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("Name == 'bob' && City == 'San Francisco'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(7,               tokens2.Count);
            Assert.AreEqual("Name",          tokens2[0].Value);
            Assert.AreEqual("==",            tokens2[1].Value);
            Assert.AreEqual("bob",           tokens2[2].Value);
            Assert.AreEqual("&&",            tokens2[3].Value);
            Assert.AreEqual("City",          tokens2[4].Value);
            Assert.AreEqual("==",            tokens2[5].Value);
            Assert.AreEqual("San Francisco", tokens2[6].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_complex_wParens_expression()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("(Name == 'bob') && (City == 'San Francisco')");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(3,               tokens2.Count);

            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.AreEqual("&&",            tokens2[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[2].Type);

            var exprToken1 = tokens2[0] as ExpressionToken;
            var exprToken2 = tokens2[2] as ExpressionToken;

            Assert.AreEqual(3,   exprToken1.Children.Count);
            Assert.AreEqual(3,   exprToken2.Children.Count);

            Assert.AreEqual("Name",          exprToken1.Children[0].Value);
            Assert.AreEqual("==",            exprToken1.Children[1].Value);
            Assert.AreEqual("bob",           exprToken1.Children[2].Value);

            Assert.AreEqual("City",          exprToken2.Children[0].Value);
            Assert.AreEqual("==",            exprToken2.Children[1].Value);
            Assert.AreEqual("San Francisco", exprToken2.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_expression()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("Name == 'bob' ? 'Jones' : 'Anderson'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(5,               tokens2.Count);

            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.AreEqual("?",            tokens2[1].Value);
            Assert.AreEqual("Jones",        tokens2[2].Value);
            Assert.AreEqual(":",            tokens2[3].Value);
            Assert.AreEqual("Anderson",     tokens2[4].Value);

            var exprToken1 = tokens2[0] as ExpressionToken;

            Assert.AreEqual(3,   exprToken1.Children.Count);

            Assert.AreEqual("Name",          exprToken1.Children[0].Value);
            Assert.AreEqual("==",            exprToken1.Children[1].Value);
            Assert.AreEqual("bob",           exprToken1.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_complex_expression()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("Name == 'bob' && City == 'Seattle' ? 'Jones' : 'Anderson'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(5,               tokens2.Count);

            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.AreEqual("?",            tokens2[1].Value);
            Assert.AreEqual("Jones",        tokens2[2].Value);
            Assert.AreEqual(":",            tokens2[3].Value);
            Assert.AreEqual("Anderson",     tokens2[4].Value);

            var exprToken1 = tokens2[0] as ExpressionToken;

            Assert.AreEqual(7,   exprToken1.Children.Count);

            Assert.AreEqual("Name",          exprToken1.Children[0].Value);
            Assert.AreEqual("==",            exprToken1.Children[1].Value);
            Assert.AreEqual("bob",           exprToken1.Children[2].Value);
            Assert.AreEqual("&&",            exprToken1.Children[3].Value);
            Assert.AreEqual("City",          exprToken1.Children[4].Value);
            Assert.AreEqual("==",            exprToken1.Children[5].Value);
            Assert.AreEqual("Seattle",       exprToken1.Children[6].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_nested_expression()
        {
            var parser  = new Parser();
            var tokens  = parser.Parse("Name == 'bob' ? 'Jones' : (City == 'Chicago' ? 'Anderson' : 'Bell')");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(5,               tokens2.Count);

            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.AreEqual("?",            tokens2[1].Value);
            Assert.AreEqual("Jones",        tokens2[2].Value);
            Assert.AreEqual(":",            tokens2[3].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[4].Type);

            var exprToken1 = tokens2[0] as ExpressionToken;
            var exprToken2 = tokens2[4] as ExpressionToken;

            Assert.AreEqual(3,   exprToken1.Children.Count);

            Assert.AreEqual("Name",          exprToken1.Children[0].Value);
            Assert.AreEqual("==",            exprToken1.Children[1].Value);
            Assert.AreEqual("bob",           exprToken1.Children[2].Value);

            Assert.AreEqual(5,               exprToken2.Children.Count);

            Assert.AreEqual(Token.TokenType.Expression, exprToken2.Children[0].Type);
            Assert.AreEqual("?",            exprToken2.Children[1].Value);
            Assert.AreEqual("Anderson",        exprToken2.Children[2].Value);
            Assert.AreEqual(":",            exprToken2.Children[3].Value);
            Assert.AreEqual("Bell",         exprToken2.Children[4].Value);

            var exprToken3 = exprToken2.Children[0] as ExpressionToken;

            Assert.AreEqual("City",          exprToken3.Children[0].Value);
            Assert.AreEqual("==",            exprToken3.Children[1].Value);
            Assert.AreEqual("Chicago",       exprToken3.Children[2].Value);

        }
    }
}
