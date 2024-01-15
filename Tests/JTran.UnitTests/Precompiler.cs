using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Parser;
using JTranParser = JTran.Parser.Parser;
using System;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class PrecompilerTests
    {
        [TestMethod]
        [DataRow("10 - 3 + (9 - 1)", 5)]
        [DataRow("10 - 3 * (3 - 1)", 3)]
        [DataRow("10 - 3 * 3 - 1", 5)]
        [DataRow("5 * 3 - (3 + 1)", 3)]
        public void Precompiler_Precompile_precedence(string expressionStr, int numTokens)
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse(expressionStr);
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1, tokens2.Count);

            var expr = tokens2[0] as ExpressionToken;

            Assert.AreEqual(numTokens, expr.Children.Count);
        }

        [TestMethod]
        public void Precompiler_Precompile_simple_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Name == 'bob'");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.AreEqual(1, tokens2.Count);

            var expr = tokens2[0] as ExpressionToken;

            Assert.AreEqual(3, expr.Children.Count);

            Assert.AreEqual("Name",  expr.Children[0].Value);
            Assert.AreEqual("==",    expr.Children[1].Value);
            Assert.AreEqual("bob",   expr.Children[2].Value);
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
        public void Precompiler_Precompile_nested_funcs()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("floor(ceiling(3.5))");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,       tokens2.Count);
            Assert.AreEqual(Token.TokenType.Function, tokens2[0].Type);

            var func1 = tokens2[0] as ExpressionToken;

            Assert.IsNotNull(func1);
            Assert.AreEqual(1, func1!.Children!.Count);

            var func2 = func1!.Children![0] as ExpressionToken;

            Assert.IsNotNull(func2);
            Assert.AreEqual(1, func2!.Children!.Count);
            Assert.AreEqual(3.5d, double.Parse(func2!.Children![0]!.ToString()));
        }


        [TestMethod]
        public void Precompiler_Precompile_simple_func()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("getcity(Name == 'bob')");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,          tokens2.Count);
            Assert.AreEqual(Token.TokenType.Function, tokens2[0].Type);

            var function = tokens2[0] as ExpressionToken;

            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(1,          function.Children.Count);

            var parm = function.Children[0] as ExpressionToken;

            Assert.AreEqual("Name",  parm.Children[0].Value);
            Assert.AreEqual("==",    parm.Children[1].Value);
            Assert.AreEqual("bob",   parm.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("getcity(Name == 'bob', $Age)");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,          tokens2.Count);
            Assert.AreEqual(Token.TokenType.Function, tokens2[0].Type);

            var function = tokens2[0] as ExpressionToken;

            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(2,          function.Children.Count);

            var parm = function.Children[0] as ExpressionToken;

            Assert.AreEqual(3,   parm.Children.Count);
            Assert.AreEqual("Name",  parm.Children[0].Value);
            Assert.AreEqual("==",    parm.Children[1].Value);
            Assert.IsTrue(parm.Children[1].IsOperator);
            Assert.AreEqual("bob",   parm.Children[2].Value);

            Assert.AreEqual("$Age",  function.Children[1].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams_nested()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("getcity(Name == 'bob', getstate(ZipCode == 98033, Role == 'Engineer'))");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,          tokens2.Count);
            Assert.AreEqual(Token.TokenType.Function, tokens2[0].Type);

            var function = tokens2[0] as ExpressionToken;

            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(2,          function.Children.Count);

            var parm = function.Children[0] as ExpressionToken;

            Assert.AreEqual(3,        parm.Children.Count);
            Assert.AreEqual("Name",  parm.Children[0].Value);
            Assert.AreEqual("==",    parm.Children[1].Value);
            Assert.IsTrue(parm.Children[1].IsOperator);
            Assert.AreEqual("bob",   parm.Children[2].Value);

            var parm2 = function.Children[1] as ExpressionToken;

            Assert.AreEqual(2,              parm2.Children.Count);
            Assert.AreEqual("getstate",     parm2.Value);

            var parm2a = parm2.Children[0] as ExpressionToken;

            Assert.AreEqual(3,              parm2a.Children.Count);

            var parm2b = parm2.Children[0] as ExpressionToken;

            Assert.AreEqual(3,              parm2b.Children.Count);
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
            Assert.AreEqual(1,               tokens2.Count);

            var expr = tokens2[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Tertiary, expr.Type);
            Assert.AreEqual("Jones",        expr.Children[1].Value);
            Assert.AreEqual("Anderson",     expr.Children[2].Value);

            var exprToken1 = expr.Children[0] as ExpressionToken;

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
            Assert.AreEqual(1, tokens2.Count);

            var tertiary = tokens2[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);

            Assert.AreEqual("Jones",        tertiary.Children[1].Value);
            Assert.AreEqual("Anderson",     tertiary.Children[2].Value);

            var exprToken1 = tertiary.Children[0] as ExpressionToken;

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
            Assert.AreEqual(1, tokens2.Count);

            var tertiary = tokens2[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);

            Assert.AreEqual("Jones",        tertiary.Children[1].Value);
            Assert.AreEqual("Anderson",     tertiary.Children[2].Value);

            var exprToken1 = tertiary.Children[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Expression, exprToken1.Children[0].Type);
            Assert.AreEqual("&&",                       exprToken1.Children[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken1.Children[2].Type);

            var exprToken3 = exprToken1.Children[0] as ExpressionToken;

            Assert.AreEqual("Name", exprToken3.Children[0].Value);
            Assert.AreEqual("==",   exprToken3.Children[1].Value);
            Assert.AreEqual("bob",  exprToken3.Children[2].Value);

            var exprToken4 = exprToken1.Children[2] as ExpressionToken;

            Assert.AreEqual(3, exprToken4.Children.Count);
            Assert.AreEqual(Token.TokenType.Expression, exprToken4.Children[0].Type);
            Assert.AreEqual("&&",                       exprToken4.Children[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken4.Children[2].Type);

            var exprToken5 = exprToken4.Children[0] as ExpressionToken;
            var exprToken6 = exprToken4.Children[2] as ExpressionToken;

            Assert.AreEqual("City",     exprToken5.Children[0].Value);
            Assert.AreEqual("==",       exprToken5.Children[1].Value);
            Assert.AreEqual("Seattle",  exprToken5.Children[2].Value);

            Assert.AreEqual("State",    exprToken6.Children[0].Value);
            Assert.AreEqual("==",       exprToken6.Children[1].Value);
            Assert.AreEqual("WA",       exprToken6.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_nested_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Name == 'bob' ? 'Jones' : (City == 'Chicago' ? 'Anderson' : 'Bell')");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1, tokens2.Count);

            var tertiary = tokens2[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);
            Assert.AreEqual("Jones",        tertiary.Children[1].Value);

            var exprToken1 = tertiary.Children[0] as ExpressionToken;
            var tertiary2 = tertiary.Children[2] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary2.Type);
            Assert.AreEqual(3,   exprToken1.Children.Count);

            Assert.AreEqual("Name",          exprToken1.Children[0].Value);
            Assert.AreEqual("==",            exprToken1.Children[1].Value);
            Assert.AreEqual("bob",           exprToken1.Children[2].Value);

            Assert.AreEqual("Anderson",     tertiary2.Children[1].Value);
            Assert.AreEqual("Bell",         tertiary2.Children[2].Value);

            var exprToken3 = tertiary2.Children[0] as ExpressionToken;

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
            Assert.AreEqual(1,                      tokens2.Count);

            var expr = tokens2[0] as ExpressionToken;

            Assert.AreEqual(3,                      expr.Children.Count);
            Assert.AreEqual("$Customer.FirstName",  expr.Children[0].Value);
            Assert.AreEqual("+",                    expr.Children[1].Value);
            Assert.AreEqual("$Customer.LastName",   expr.Children[2].Value);
        }

        
        [TestMethod]
        public void Precompiler_Precompile_sortforeach_Success()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("include(sort(Customers, Name), 'bob')");
            var tokens2 = Precompiler.Precompile(tokens);
   
            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1,          tokens2.Count);
            Assert.AreEqual("include", tokens2[0].Value);

            var elemParm1 = tokens2[0] as ExpressionToken;

            Assert.AreEqual("sort",     elemParm1.Children[0].Value);
            Assert.AreEqual(2,          elemParm1.Children.Count);
            Assert.AreEqual("bob",       elemParm1.Children[1].Value);

            var elemParm1a = elemParm1.Children[0] as ExpressionToken;
         
            Assert.AreEqual(2,              elemParm1a.Children.Count);
            Assert.AreEqual("Customers",    elemParm1a.Children[0].Value);
            Assert.AreEqual("Name",         elemParm1a.Children[1].Value);
        }

        #region Array Indexers

        [TestMethod]
        public void Precompiler_Precompile_array_indexer_single()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Customers[1]");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1, tokens2.Count);

            var array        = tokens2[0] as ExpressionToken;

            Assert.AreEqual(1, array.Children.Count);

            var arrayIndexer = array.Children[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Array, array.Type);
            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer.Type);

            Assert.AreEqual("Customers", array.Value);
            Assert.AreEqual(1, int.Parse(arrayIndexer.Children[0].Value));
        }

        [TestMethod]
        public void Precompiler_Precompile_array_indexer_single_expression()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Customers[Surname == 'Smith']");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1, tokens2.Count);

            var array        = tokens2[0] as ExpressionToken;

            Assert.AreEqual(1, array.Children.Count);

            var arrayIndexer = array.Children[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Array, array.Type);
            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer.Type);

            Assert.AreEqual("Customers", array.Value);
            var expr1 = arrayIndexer.Children[0] as ExpressionToken;

            Assert.AreEqual(3, expr1.Children.Count);
            Assert.AreEqual("Surname", expr1.Children[0].Value);
            Assert.AreEqual("==",      expr1.Children[1].Value);
            Assert.AreEqual("Smith",   expr1.Children[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_array_indexer_two()
        {
            var parser  = new JTranParser();
            var tokens  = parser.Parse("Customers[1][2]");
            var tokens2 = Precompiler.Precompile(tokens);

            Assert.IsNotNull(tokens2);
            Assert.AreEqual(1, tokens2.Count);

            var array = tokens2[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.Array, array.Type);
            Assert.AreEqual("Customers", array.Value);
            Assert.AreEqual(2, array.Children.Count);

            var arrayIndexer1 = array.Children[0] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer1.Type);
            Assert.AreEqual(1, int.Parse(arrayIndexer1.Children[0].Value));

            var arrayIndexer2 = array.Children[1] as ExpressionToken;

            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer2.Type);
            Assert.AreEqual(2, int.Parse(arrayIndexer2.Children[0].Value));
        }

        #endregion
    }
}
