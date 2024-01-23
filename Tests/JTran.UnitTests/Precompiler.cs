using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Parser;
using JTranParser = JTran.Parser.ExpressionParser;
using System;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class PrecompilerTests
    {
        [TestMethod]
        [DataRow("10 - 3 + (9 - 1)", "-")]
        [DataRow("10 - 3 * (3 - 1)", "-")]
        [DataRow("10 - 3 * 3 - 1",   "-")]
        [DataRow("5 * 3 - (3 + 1)",  "-")]
        public void Precompiler_Precompile_precedence(string expressionStr, string op)
        {
            var token = Test(expressionStr);

            Assert.AreEqual(3, token.Count);

            var expr = token[1];

            Assert.AreEqual(op, expr.Value);
        }

        [TestMethod]
        [DataRow("Name <= 'bob'",   "<=")]
        [DataRow("Name < 'bob'" ,   "<")]
        [DataRow("Name >= 'bob'",   ">=")]
        [DataRow("Name > 'bob'",    ">")]
        [DataRow("Name == 'bob'",   "==")]
        [DataRow("Name != 'bob'",   "!=")]
        [DataRow("(Name == 'bob')", "==")]
        public void Precompiler_Precompile_comparison_expression(string expressionStr, string op)
        {
            var expr = TestExpression(expressionStr);

            AssertExpression(expr, "Name", op, "bob");
        }

        [TestMethod]
        public void Precompiler_Precompile_nested_funcs()
        {
            var func1 = TestExpression("floor(ceiling(3.5))");

            Assert.AreEqual(Token.TokenType.Function, func1.Type);

            Assert.AreEqual(1, func1.Count);

            var func2 = func1[0];

            Assert.IsNotNull(func2);
            Assert.AreEqual(1, func2!.Count);
            Assert.AreEqual(3.5d, double.Parse(func2![0]!.ToString()));
        }


        [TestMethod]
        public void Precompiler_Precompile_simple_func()
        {
            var function = TestExpression("getcity(Name == 'bob')");

            Assert.AreEqual(Token.TokenType.Function, function.Type);

             Assert.AreEqual("getcity", function.Value);
            Assert.AreEqual(1,          function.Count);

            var parm = function[0];

            Assert.AreEqual("Name",  parm[0].Value);
            Assert.AreEqual("==",    parm[1].Value);
            Assert.AreEqual("bob",   parm[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams()
        {
            var function = TestExpression("getcity(Name == 'bob', $Age)");

            Assert.AreEqual(Token.TokenType.Function, function.Type);

            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(2,          function.Count);

            var parm = function[0];

            Assert.AreEqual(3,   parm.Count);
            Assert.AreEqual("Name",  parm[0].Value);
            Assert.AreEqual("==",    parm[1].Value);
            Assert.IsTrue(parm[1].IsOperator);
            Assert.AreEqual("bob",   parm[2].Value);

            Assert.AreEqual("$Age",  function[1].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams_nested()
        {
            var function = TestExpression("getcity(Name == 'bob', getstate(ZipCode == 98033, Role == 'Engineer'))");

            Assert.AreEqual(Token.TokenType.Function, function.Type);

            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(2,          function.Count);

            var parm = function[0];

            Assert.AreEqual(3,        parm.Count);
            Assert.AreEqual("Name",  parm[0].Value);
            Assert.AreEqual("==",    parm[1].Value);
            Assert.IsTrue(parm[1].IsOperator);
            Assert.AreEqual("bob",   parm[2].Value);

            var parm2 = function[1];

            Assert.AreEqual(2,              parm2.Count);
            Assert.AreEqual("getstate",     parm2.Value);

            var parm2a = parm2[0];

            Assert.AreEqual(3,              parm2a.Count);

            var parm2b = parm2[0];

            Assert.AreEqual(3,              parm2b.Count);
        }

        [TestMethod]
        public void Precompiler_Precompile_complex_expression()
        {
            var token = Test("Name == 'bob' && City == 'San Francisco'");

            Assert.AreEqual(3, token.Count);

            Assert.AreEqual(Token.TokenType.Expression, token[0].Type);
            Assert.AreEqual("&&",                       token[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, token[2].Type);

            var exprToken1 = token[0];
            var exprToken2 = token[2];

            Assert.AreEqual("Name",          exprToken1[0].Value);
            Assert.AreEqual("==",            exprToken1[1].Value);
            Assert.AreEqual("bob",           exprToken1[2].Value);

            Assert.AreEqual("City",          exprToken2[0].Value);
            Assert.AreEqual("==",            exprToken2[1].Value);
            Assert.AreEqual("San Francisco", exprToken2[2].Value);
        }

        [TestMethod]
        [DataRow("(Name == 'bob') && (City == 'San Francisco')", "&&")]
        [DataRow("Name == 'bob' && City == 'San Francisco'",     "&&")]
        [DataRow("(Name == 'bob') || (City == 'San Francisco')", "||")]
        [DataRow("Name == 'bob' || City == 'San Francisco'",     "||")]
        public void Precompiler_Precompile_complex_expression2(string expression, string expectedOp)
        {
            var expr = TestExpression(expression);

            var t1 = expr[0];
            var op = expr[1];
            var t2 = expr[2];

            Assert.AreEqual(expectedOp, op.Value);

            AssertExpression(t1, "Name", "==", "bob");
            AssertExpression(t2, "City", "==", "San Francisco");
        }

        [TestMethod]
        [DataRow("Name == 'bob' ? 'Jones' : 'Anderson'")]
        public void Precompiler_Precompile_tertiary_expression(string expression)
        {
            var expr = Test(expression);

            Assert.AreEqual(Token.TokenType.Tertiary, expr.Type);
            Assert.AreEqual("Jones",        expr[1].Value);
            Assert.AreEqual("Anderson",     expr[2].Value);

            var exprToken1 = expr[0];

            Assert.AreEqual(3, exprToken1.Count);

            Assert.AreEqual("Name",          exprToken1[0].Value);
            Assert.AreEqual("==",            exprToken1[1].Value);
            Assert.AreEqual("bob",           exprToken1[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_complex_expression()
        {
            var tertiary = TestExpression("Name == 'bob' && City == 'Seattle' ? 'Jones' : 'Anderson'");

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);

            Assert.AreEqual("Jones",        tertiary[1].Value);
            Assert.AreEqual("Anderson",     tertiary[2].Value);

            var exprToken1 = tertiary[0];

            Assert.AreEqual(3, exprToken1.Count);

            Assert.AreEqual(Token.TokenType.Expression, exprToken1[0].Type);
            Assert.AreEqual("&&",                       exprToken1[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken1[2].Type);

            var exprToken3 = exprToken1[0];
            var exprToken4 = exprToken1[2];

            Assert.AreEqual("Name",          exprToken3[0].Value);
            Assert.AreEqual("==",            exprToken3[1].Value);
            Assert.AreEqual("bob",           exprToken3[2].Value);

            Assert.AreEqual("City",          exprToken4[0].Value);
            Assert.AreEqual("==",            exprToken4[1].Value);
            Assert.AreEqual("Seattle",       exprToken4[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_double_and_expression()
        {
            var tertiary = TestExpression("Name == 'bob' && City == 'Seattle' && State == 'WA' ? 'Jones' : 'Anderson'");

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);

            Assert.AreEqual("Jones",        tertiary[1].Value);
            Assert.AreEqual("Anderson",     tertiary[2].Value);

            var exprToken1 = tertiary[0];

            Assert.AreEqual(Token.TokenType.Expression, exprToken1[0].Type);
            Assert.AreEqual("&&",                       exprToken1[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken1[2].Type);

            var exprToken3 = exprToken1[0];

            Assert.AreEqual(Token.TokenType.Expression, exprToken3[0].Type);
            Assert.AreEqual("&&",                       exprToken3[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken3[2].Type);

            AssertExpression(exprToken3[0], "Name", "==", "bob");
            AssertExpression(exprToken3[2], "City", "==", "Seattle");
            AssertExpression(exprToken1[2], "State", "==", "WA");
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_and_or()
        {
            var tertiary = TestExpression("Name == 'bob' || City == 'Seattle' && State == 'WA' ? 'Jones' : 'Anderson'");

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);

            Assert.AreEqual("Jones",        tertiary[1].Value);
            Assert.AreEqual("Anderson",     tertiary[2].Value);

            var exprToken1 = tertiary[0];

            Assert.AreEqual(Token.TokenType.Expression, exprToken1[0].Type);
            Assert.AreEqual(Token.TokenType.Expression, exprToken1[2].Type);
            Assert.AreEqual("||",                       exprToken1[1].Value);

            var exprToken3 = exprToken1[0];

            Assert.AreEqual("Name",     exprToken3[0].Value);
            Assert.AreEqual("==",       exprToken3[1].Value);
            Assert.AreEqual("bob",      exprToken3[2].Value);

            var exprToken4 = exprToken1[2];

            Assert.AreEqual(3, exprToken4.Count);
            Assert.AreEqual(Token.TokenType.Expression, exprToken4[0].Type);
            Assert.AreEqual("&&",                       exprToken4[1].Value);
            Assert.AreEqual(Token.TokenType.Expression, exprToken4[2].Type);

            var exprToken5 = exprToken4[0];

            Assert.AreEqual(3, exprToken5.Count);
            Assert.AreEqual("City",     exprToken5[0].Value);
            Assert.AreEqual("==",       exprToken5[1].Value);
            Assert.AreEqual("Seattle",  exprToken5[2].Value);

            var exprToken6 = exprToken4[2];

            Assert.AreEqual("State",    exprToken6[0].Value);
            Assert.AreEqual("==",       exprToken6[1].Value);
            Assert.AreEqual("WA",       exprToken6[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_tertiary_nested_expression()
        {
            var tertiary = TestExpression("Name == 'bob' ? 'Jones' : (City == 'Chicago' ? 'Anderson' : 'Bell')");

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary.Type);
            Assert.AreEqual("Jones",        tertiary[1].Value);

            var exprToken1 = tertiary[0];
            var tertiary2 = tertiary[2];

            Assert.AreEqual(Token.TokenType.Tertiary, tertiary2.Type);
            Assert.AreEqual(3,   exprToken1.Count);

            Assert.AreEqual("Name",          exprToken1[0].Value);
            Assert.AreEqual("==",            exprToken1[1].Value);
            Assert.AreEqual("bob",           exprToken1[2].Value);

            Assert.AreEqual("Anderson",     tertiary2[1].Value);
            Assert.AreEqual("Bell",         tertiary2[2].Value);

            var exprToken3 = tertiary2[0];

            Assert.AreEqual("City",          exprToken3[0].Value);
            Assert.AreEqual("==",            exprToken3[1].Value);
            Assert.AreEqual("Chicago",       exprToken3[2].Value);

        }

        [TestMethod]
        public void Precompiler_Precompile_multipart_expression()
        {
            var expr = Test("Customer.Name.Last");

            Assert.AreEqual(Token.TokenType.Multipart, expr.Type);

            Assert.AreEqual("Customer", expr[0].Value);
            Assert.AreEqual("Name", expr[1].Value);
            Assert.AreEqual("Last", expr[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_multipart2_expression()
        {
            var expr = Test("$Customer.Name.Last");

            Assert.AreEqual("$Customer", expr[0].Value);
            Assert.AreEqual("Name", expr[1].Value);
            Assert.AreEqual("Last", expr[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_multipart3_expression()
        {
            var expr = TestExpression("$Customer.FirstName + $Customer.LastName");

            Assert.AreEqual(3,                          expr.Count);
            Assert.AreEqual(Token.TokenType.Multipart,  expr[0].Type);
            Assert.AreEqual("+",                        expr[1].Value);
            Assert.AreEqual(Token.TokenType.Multipart,  expr[2].Type);
        }

        [TestMethod]
        public void Precompiler_Precompile_multipart4_expression()
        {
            var expr = TestExpression("Customer[City == 'Seattle'].FirstName");

            Assert.AreEqual(Token.TokenType.Multipart, expr.Type);

            Assert.AreEqual(2, expr.Count);
            Assert.AreEqual("FirstName",   expr[1].Value);
            var expr2 = expr[0];

            Assert.AreEqual(Token.TokenType.Array, expr2.Type);
            Assert.AreEqual("Customer",   expr2.Value);

            AssertExpression(expr2[0][0], "City", "==", "Seattle");
        }
        
        [TestMethod]
        public void Precompiler_Precompile_function_params_success()
        {
            var token = TestExpression("include(a, b, c))");
   
            Assert.AreEqual("include", token.Value);
            Assert.AreEqual(Token.TokenType.Function, token.Type);

            Assert.AreEqual(3,   token.Count);
            Assert.AreEqual("a", token[0].Value);
            Assert.AreEqual("b", token[1].Value);
            Assert.AreEqual("c", token[2].Value);
        }  
        
        [TestMethod]
        public void Precompiler_Precompile_sortforeach_success()
        {
            var function = TestExpression("include(sort(Customers, Name), 'bob')");
   
            Assert.AreEqual("include",  function.Value);
            Assert.AreEqual(2,          function.Count);
            Assert.AreEqual("sort",     function[0].Value);
            Assert.AreEqual("bob",      function[1].Value);

            var elemParm1 = function[0];
         
            Assert.AreEqual(2,              elemParm1.Count);
            Assert.AreEqual("Customers",    elemParm1[0].Value);
            Assert.AreEqual("Name",         elemParm1[1].Value);
        }

        #region Conditionals

        [TestMethod]
        [DataRow("(a + b) * c")]
        [DataRow("a + b * c")]
        [DataRow("a && b || c")]
        [DataRow("a || b && c || d && b")]
        [DataRow("Name == 'bob' && City == 'San Francisco'")]
        public void Precompiler_Precompile_success(string expression)
        {
            var token = TestExpression(expression)!;

            Assert.AreEqual(3, token.Count);
        }                

        [TestMethod]
        [DataRow("a && b || c", 3)]
        public void Precompiler_Precompile_conditionals(string expression, int numTokens)
        {
            var token = TestExpression(expression);
            var t1 = token[0];
            var op = token[1];
            var t2 = token[2];
   
            Assert.IsNotNull(t1);
            Assert.IsNotNull(t2);

            Assert.AreEqual("||", op.Value);
            Assert.AreEqual("c", t2.Value);

            AssertExpression(t1, "a", "&&", "b");
        }        
        
        [TestMethod]
        [DataRow("a || b && c")]
        public void Precompiler_Precompile_conditionals2(string expression)
        {
            var token = TestExpression(expression);
            var t1 = token[0];
            var op = token[1];
            var t2 = token[2];
   
            Assert.AreEqual("a", t1.Value);
            Assert.AreEqual("||", op.Value);

            AssertExpression(t2, "b", "&&", "c");
        }        
        
        [TestMethod]
        [DataRow("a || b && c || d && b")]
        public void Precompiler_Precompile_conditionals3(string expression)
        {
            var token = TestExpression(expression);
            var t1 = token[0];
            var op = token[1];
            var t2 = token[2];
   
            Assert.AreEqual("||", op.Value);

            var t1a  = t1[0];
            var t1op = t1[1];
            var t1b  = t1[2];

            Assert.AreEqual("a",  t1a.Value);
            Assert.AreEqual("||", t1op.Value);

            AssertExpression(t1b, "b", "&&", "c");
            AssertExpression(t2, "d", "&&", "b");
        }        
        
        #endregion

        #region Array Indexers

        [TestMethod]
        public void Precompiler_Precompile_array_indexer_single()
        {
            var array = Test("Customers[1]");

            Assert.AreEqual(1, array.Count);

            var arrayIndexer = array[0];

            Assert.AreEqual(Token.TokenType.Array, array.Type);
            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer.Type);

            Assert.AreEqual("Customers", array.Value);
            Assert.AreEqual(1, int.Parse(arrayIndexer[0].Value));
        }

        [TestMethod]
        public void Precompiler_Precompile_array_indexer_single_expression()
        {
            var array = Test("Customers[Surname == 'Smith']");

            Assert.AreEqual(1, array.Count);

            var arrayIndexer = array[0];

            Assert.AreEqual(Token.TokenType.Array, array.Type);
            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer.Type);

            Assert.AreEqual("Customers", array.Value);
            var expr1 = arrayIndexer[0];

            Assert.AreEqual(3, expr1.Count);
            Assert.AreEqual("Surname", expr1[0].Value);
            Assert.AreEqual("==",      expr1[1].Value);
            Assert.AreEqual("Smith",   expr1[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_array_indexer_two()
        {
           var array = Test("Customers[1][2]");

            Assert.AreEqual(2, array.Count);

            var arrayIndexer1 = array[0];
            var arrayIndexer2 = array[1];

            Assert.AreEqual(Token.TokenType.Array, array.Type);
            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer1.Type);
            Assert.AreEqual(Token.TokenType.ArrayIndexer, arrayIndexer2.Type);

            Assert.AreEqual("Customers", array.Value);
            Assert.AreEqual(1, int.Parse(arrayIndexer1[0].Value));
            Assert.AreEqual(2, int.Parse(arrayIndexer2[0].Value));
        }

        #endregion

        #region ExplicitArray

        [TestMethod]
        [DataRow("([1, 2, 3], 'bob')")]
        public void Precompiler_ExplicitArray_success(string expression)
        {
            var token = Test(expression);

            Assert.AreEqual(2, token.Count);

            var array = token[0];

            Assert.AreEqual(Token.TokenType.ExplicitArray, array!.Type);
            Assert.AreEqual(3, array!.Count);
        }

        #endregion

        #region Params

        [TestMethod]
        [DataRow("(a, b)", 2)]
        [DataRow("(a, b + c)", 2)]
        [DataRow("(a * x + y - (4 + z), b + c, (a + b) * c)", 3)]
        public void Precompiler_Precompile_params(string expression, int numParams)
        {
            var token = Test(expression);
   
            Assert.AreEqual(Token.TokenType.CommaDelimited, token.Type);
            Assert.AreEqual(numParams, token.Count);
        }

        #endregion

        private void AssertExpression(Token expr, string left, string op, string right)
        {
            Assert.IsNotNull(expr);
            Assert.AreEqual(3,     expr.Count);
            Assert.AreEqual(left,  expr[0].Value);
            Assert.AreEqual(op,    expr[1].Value);
            Assert.AreEqual(right, expr[2].Value);
        }        
               
        private Token TestExpression(string expression)
        {
            var token = Test(expression);
   
            Assert.IsTrue(token.IsExpression);

            return token;
        }

        private Token Test(string expression)
        {
            var parser = new JTranParser();
            var tokens = parser.Parse(expression);
            var token  =  Precompiler.Precompile(tokens);

            Assert.IsNotNull(token);

            return token;

        }
    }
}
