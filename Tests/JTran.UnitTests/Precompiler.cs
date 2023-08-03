using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Parser;
using JTranParser = JTran.Parser.Parser;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class PrecompilerTests
    {
        [TestMethod]
        public void Precompiler_Precompile_simple_expression()
        {
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Name == 'bob' && City == 'San Francisco'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(3,               tokens2.Count);

            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.AreEqual("&&",                       tokens2[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, tokens2[2].Type);


            var exprToken1 = tokens2[0] as ExpressionToken;
            var exprToken2 = tokens2[2] as ExpressionToken;

            Assert.AreEqual("Name",          exprToken1.Children[0].Value);
            Assert.AreEqual("==",            exprToken1.Children[1].Value);
            Assert.AreEqual("bob",           exprToken1.Children[2].Value);

            Assert.AreEqual("City",          exprToken2.Children[0].Value);
            Assert.AreEqual("==",            exprToken2.Children[1].Value);
            Assert.AreEqual("San Francisco", exprToken2.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_complex_wParens_expression()
        {
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
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
            var parser  = new JTranParser();
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

            Assert.AreEqual(3, exprToken1.Children.Count);

            Assert.AreEqual(Token.TokenType.Expression, exprToken1.Children[0].Type);
            Assert.AreEqual("&&",                       exprToken1.Children[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken1.Children[2].Type);

            var exprToken3 = exprToken1.Children[0] as ExpressionToken;
            var exprToken4 = exprToken1.Children[2] as ExpressionToken;

            Assert.AreEqual("Name",          exprToken3.Children[0].Value);
            Assert.AreEqual("==",            exprToken3.Children[1].Value);
            Assert.AreEqual("bob",           exprToken3.Children[2].Value);

            Assert.AreEqual("City",          exprToken4.Children[0].Value);
            Assert.AreEqual("==",            exprToken4.Children[1].Value);
            Assert.AreEqual("Seattle",       exprToken4.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_double_and_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Name == 'bob' && City == 'Seattle' && State == 'WA' ? 'Jones' : 'Anderson'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(5,               tokens2.Count);

            Assert.AreEqual(Token.TokenType.Expression, tokens2[0].Type);
            Assert.AreEqual("?",            tokens2[1].Value);
            Assert.AreEqual("Jones",        tokens2[2].Value);
            Assert.AreEqual(":",            tokens2[3].Value);
            Assert.AreEqual("Anderson",     tokens2[4].Value);

            var exprToken1 = tokens2[0] as ExpressionToken;

            Assert.AreEqual(3, exprToken1.Children.Count);

            Assert.AreEqual(Token.TokenType.Expression, exprToken1.Children[0].Type);
            Assert.AreEqual("&&",                       exprToken1.Children[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken1.Children[2].Type);

            var exprToken3 = exprToken1.Children[0] as ExpressionToken;

            Assert.AreEqual("Name",          exprToken3.Children[0].Value);
            Assert.AreEqual("==",            exprToken3.Children[1].Value);
            Assert.AreEqual("bob",           exprToken3.Children[2].Value);

            var exprToken4 = exprToken1.Children[2] as ExpressionToken;

            Assert.AreEqual(3, exprToken4.Children.Count);
            Assert.AreEqual(Token.TokenType.Expression, exprToken4.Children[0].Type);
            Assert.AreEqual("&&",                       exprToken4.Children[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken4.Children[2].Type);

            var exprToken5 = exprToken4.Children[0] as ExpressionToken;
            var exprToken6 = exprToken4.Children[2] as ExpressionToken;

            Assert.AreEqual("City",          exprToken5.Children[0].Value);
            Assert.AreEqual("==",            exprToken5.Children[1].Value);
            Assert.AreEqual("Seattle",       exprToken5.Children[2].Value);

            Assert.AreEqual("State",          exprToken6.Children[0].Value);
            Assert.AreEqual("==",             exprToken6.Children[1].Value);
            Assert.AreEqual("WA",             exprToken6.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_nested_expression()
        {
            var parser  = new JTranParser();
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

        [TestMethod]
        public void Precompiler_Precompile_multipart_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Customer.Name.Last");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,                     tokens2.Count);
            Assert.AreEqual("Customer.Name.Last",  tokens2[0].Value);

        }

        [TestMethod]
        public void Precompiler_Precompile_multipart2_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("$Customer.Name.Last");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,                     tokens2.Count);
            Assert.AreEqual("$Customer.Name.Last",  tokens2[0].Value);

        }

        [TestMethod]
        public void Precompiler_Precompile_multipart3_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("$Customer.FirstName + $Customer.LastName");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(3,                      tokens2.Count);
            Assert.AreEqual("$Customer.FirstName",  tokens2[0].Value);
            Assert.AreEqual("+",                    tokens2[1].Value);
            Assert.AreEqual("$Customer.LastName",   tokens2[2].Value);
        }

        
        [TestMethod]
        public void Precompiler_Precompile_sortforeach_Success()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("#foreach(sort(Customers, Name), {})");
            var tokens2 = Precompiler.Precompile(tokens);
   
            Assert.IsNotNull(tokens2);
            Assert.AreEqual(6,                  tokens2.Count);
            Assert.AreEqual("#foreach",         tokens2[0].Value);
            Assert.AreEqual("(",                tokens2[1].Value);

            Assert.AreEqual(",",                tokens2[3].Value);
            Assert.AreEqual("{}",               tokens2[4].Value);
            Assert.AreEqual(")",                tokens2[5].Value);

            var exprToken = tokens2[2] as ExpressionToken;
         
            Assert.IsNotNull(               exprToken);
            Assert.AreEqual(6,              exprToken.Children.Count);
            Assert.AreEqual("sort",         exprToken.Children[0].Value);
            Assert.AreEqual("(",            exprToken.Children[1].Value);
            Assert.AreEqual("Customers",    exprToken.Children[2].Value);
            Assert.AreEqual(",",            exprToken.Children[3].Value);
            Assert.AreEqual("Name",         exprToken.Children[4].Value);
            Assert.AreEqual(")",            exprToken.Children[5].Value);

        }

    }
}
