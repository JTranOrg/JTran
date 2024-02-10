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
        [DataRow("bob")]
        [DataRow("$bob")]
        [DataRow("//bob")]
        public void Precompiler_Precompile_single(string expressionStr)
        {
            var token = Test(expressionStr);

            Assert.AreEqual(0, token.Count);
            Assert.AreEqual(expressionStr, token.Value);
            Assert.AreEqual(Token.TokenType.Text, token.Type);
        }

        [TestMethod]
        [DataRow("10 - 3", "-")]
        [DataRow("10 + 3", "+")]
        public void Precompiler_Precompile_simple_expr(string expressionStr, string op)
        {
            var token = Test(expressionStr);

            Assert.AreEqual(3, token.Count);
            Assert.AreEqual(Token.TokenType.Expression, token.Type);

            var expr = token[1];

            Assert.AreEqual(op, expr.Value);
        }

        [TestMethod]
        [DataRow("10 - (3 + 1)", "-")]
        [DataRow("10 + (3 - 1)", "+")]
        public void Precompiler_Precompile_parens(string expressionStr, string op)
        {
            var token = Test(expressionStr);

            Assert.AreEqual(3, token.Count);
            Assert.AreEqual(Token.TokenType.Expression, token.Type);

            var exprOp = token[1];

            Assert.AreEqual(op, exprOp.Value);

            var expr = token[2];

            Assert.AreEqual(3, expr.Count);
            Assert.AreEqual(Token.TokenType.Expression, expr.Type);
        }

        [TestMethod]
        [DataRow("(a + b) - (3 + 1)", "-")]
        [DataRow("(b - a) + (3 - 1)", "+")]
        public void Precompiler_Precompile_parens_2(string expressionStr, string op)
        {
            var token = Test(expressionStr);

            Assert.AreEqual(3, token.Count);
            Assert.AreEqual(Token.TokenType.Expression, token.Type);

            var exprOp = token[1];

            Assert.AreEqual(op, exprOp.Value);

            var expr1 = token[0];
            var expr2 = token[2];

            Assert.AreEqual(3, expr1.Count);
            Assert.AreEqual(3, expr2.Count);
            Assert.AreEqual(Token.TokenType.Expression, expr1.Type);
            Assert.AreEqual(Token.TokenType.Expression, expr2.Type);
        }

        [TestMethod]
        [DataRow("10 - 3 + 3", "+")]
        [DataRow("10 - 3 + (9 - 1)", "+")]
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

            AssertExpression(expr, "Name", op, "'bob'");
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
        public void Precompiler_Precompile_func_1Param()
        {
            var function = TestExpression("getcity(Name)");

            Assert.AreEqual(Token.TokenType.Function, function.Type);
            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(1,          function.Count);

            var parm = function[0];

            Assert.AreEqual(0, parm.Count);
            Assert.AreEqual("Name", parm.Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_2Params()
        {
            var function = TestExpression("getcity(Name, State)");

            Assert.AreEqual(Token.TokenType.Function, function.Type);
            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(2,          function.Count);

            var parm1 = function[0];

            Assert.AreEqual(0, parm1.Count);
            Assert.AreEqual("Name", parm1.Value);

            var parm2 = function[1];

            Assert.AreEqual(0, parm2.Count);
            Assert.AreEqual("State", parm2.Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wExpression()
        {
            var function = TestExpression("getcity(Name == 'bob')");

            Assert.AreEqual(Token.TokenType.Function, function.Type);

            Assert.AreEqual("getcity",  function.Value);
            Assert.AreEqual(1,          function.Count);

            var parm = function[0];

            Assert.AreEqual(3,   parm.Count);
            Assert.AreEqual("Name",  parm[0].Value);
            Assert.AreEqual("==",    parm[1].Value);
            Assert.IsTrue(parm[1].IsOperator);
            Assert.AreEqual("bob",   parm[2].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wNoParams()
        {
            var function = Test("getcity()");

            AssertFunction(function, "getcity", 0);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams()
        {
            var result = Test("getcity(Name == 'bob', $Age)");

            var function = AssertFunction(result, "getcity", 2);

            var parm = AssertExpression(function[0], "Name", "==", "'bob'");

            Assert.AreEqual("$Age",  function[1].Value);
        }

        [TestMethod]
        public void Precompiler_Precompile_func_wParams2()
        {
            var function = Test("max(Husband.Birthdate, Wife.Birthdate)");

            Assert.AreEqual(Token.TokenType.Function, function.Type);

            Assert.AreEqual("max",  function.Value);
            Assert.AreEqual(2,      function.Count);

            Assert.AreEqual("Husband",  function[0][0].Value);
            Assert.AreEqual("Wife",     function[1][0].Value);
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

            AssertExpression(t1, "Name", "==", "'bob'");
            AssertExpression(t2, "City", "==", "'San Francisco'");
        }

        [TestMethod]
        [DataRow("a == b ? 1 : 99")]
        [DataRow("a || b ? 1 : 99")]
        [DataRow("[a || b ? 1 : 99, 3]")]
        [DataRow("[a || b ? 1 : 99, iif(a && b, 2, 98), x - 6 + y]")]
        public void Precompiler_Precompile_tertiary2(string expression)
        {
            var result = Test(expression);
            
            Assert.AreEqual(expression, result.ToString());
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

            AssertExpression(exprToken3[0], "Name", "==", "'bob'");
            AssertExpression(exprToken3[2], "City", "==", "'Seattle'");
            AssertExpression(exprToken1[2], "State", "==", "'WA'");
        }

        [TestMethod]
        [DataRow("a ? b : c")]
        [DataRow("Name == 'bob' || City == 'Seattle' && State == 'WA' ? 'Jones' : 'Anderson'")]
        public void Precompiler_Precompile_tertiary(string expression)
        {
            var expr = Test(expression);

            Assert.AreEqual(expression, expr.ToString());
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

            AssertExpression(exprToken1, "Name", "==", "'bob'");

            Assert.AreEqual("Anderson",     tertiary2[1].Value);
            Assert.AreEqual("Bell",         tertiary2[2].Value);

            AssertExpression(tertiary2[0], "City", "==", "'Chicago'");
        }

        [TestMethod]
        [DataRow("a.b")]
        [DataRow("a.b.c")]
        [DataRow("$a.b.c")]
        [DataRow("a[b]")]
        [DataRow("a[b.c]")]
        [DataRow("a[b.c.d]")]
        [DataRow("a.b + c.d")]
        [DataRow("a.b + c.d - e.f")]
        [DataRow("a[b == 'c'].d")]
        [DataRow("a[b] + c[d]")]
        [DataRow("a[b] + c[d] - e[f]")]
        [DataRow("a[b].c + c[d]")]
        [DataRow("a[b].c * d[e].f")]
        [DataRow("g[h].i[k]")]
        [DataRow("g[h].i[k.m[n]]")]
        [DataRow("g[h].i[j == k.m[n != p]]")]
        [DataRow("a[b].c + d[e].f - g[h].i[j == k.m[n != p]]")]
        public void Precompiler_Precompile_multipart_expression(string exprStr)
        {
            var expr = Test(exprStr);

            Assert.AreEqual(exprStr, expr.ToString());
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

            AssertExpression(expr2[0][0], "City", "==", "'Seattle'");
        }

        [TestMethod]
        public void Precompiler_Precompile_multipart5_expression()
        {
            var expr = TestExpression("Cars[Make == 'Chevy'].Tickets[Location == 'WA']");

            Assert.AreEqual(Token.TokenType.Multipart, expr.Type);

            Assert.AreEqual(2, expr.Count);

            var expr1 = expr[0];
            var expr2 = expr[1];

            Assert.AreEqual("Cars",   expr1.Value);
            Assert.AreEqual("Tickets",   expr2.Value);

            AssertExpression(expr1[0][0], "Make", "==", "'Chevy'");
            AssertExpression(expr2[0][0], "Location", "==", "'WA'");
        }        
        
        [TestMethod]
        public void Precompiler_Precompile_function_params_success()
        {
            var token = TestExpression("include(a, b, c)");
   
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
        [DataRow("Customers[1]")]
        [DataRow("Customers[Surname == 'Smith']")]
        [DataRow("Customers[1][2]")]
        public void Precompiler_Precompile_array_indexer(string exprStr)
        {
            var expr = Test(exprStr);

            Assert.AreEqual(exprStr, expr.ToString());
        }

        #endregion

        #region ExplicitArray

        [TestMethod]
        [DataRow("[1]")]
        [DataRow("[1, 2]")]
        [DataRow("[1, 2, 3]")]
        public void Precompiler_ExplicitArray_simple(string expression)
        {
            var expr = Test(expression);

            Assert.AreEqual(expression, expr.ToString());
        }

        [TestMethod]
        [DataRow("[a == b]")]
        [DataRow("[a == b, c != d]")]
        [DataRow("[a > b, c <= d, e >= f]")]
        public void Precompiler_ExplicitArray_expressions(string expression)
        {
            var expr = Test(expression);

            Assert.AreEqual(expression, expr.ToString());
        }

        [TestMethod]
        [DataRow("([1, 2, 3], 'bob')")]
        public void Precompiler_ExplicitArray_success_parms(string expression)
        {
            var token = Test(expression);

            Assert.AreEqual(2, token.Count);

            var array = token[0];

            Assert.AreEqual(Token.TokenType.ExplicitArray, array!.Type);
            Assert.AreEqual(3, array!.Count);

            var bob = token[1];

            Assert.AreEqual("bob", bob.Value);
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

        [TestMethod]
        [DataRow("a(b(c(d(e, f), g(h, i), j, k)))")]
        public void Precompiler_Precompile_nested_functions(string expr)
        {
            var tk = Test(expr);
            var a  = AssertFunction(tk,   "a", 1);
            var b  = AssertFunction(a[0], "b", 1);
            var c  = AssertFunction(b[0], "c", 4);
            var d  = AssertFunction(c[0], "d", 2);
                         AssertText(d[0], "e");
                         AssertText(d[1], "f");
            var g  = AssertFunction(c[1], "g", 2);
                         AssertText(g[0], "h");
                         AssertText(g[1], "i");
                         AssertText(c[2], "j");
                         AssertText(c[3], "k");
        }
        
        [TestMethod]
        [DataRow("a(b(c(d(e, ')'), f), '('))")]
        public void Precompiler_Precompile_complex(string expr)
        {
            var tk = Test(expr);
            var a  = AssertFunction(tk,   "a", 1);
            var b  = AssertFunction(a[0], "b", 2);
            var c  = AssertFunction(b[0], "c", 2);
            var d  = AssertFunction(c[0], "d", 2);
                         AssertText(d[0], "e");
                      AssertLiteral(d[1], ")");
                         AssertText(c[1], "f");
                      AssertLiteral(b[1], "(");
        }
        
        [TestMethod]
        [DataRow("a(b(c(d($name, ')'), $keywords.stuff), '('))")]
        public void Precompiler_Precompile_complex1a(string expr)
        {
            var token = Test(expr);

            Assert.AreEqual(Token.TokenType.Function, token.Type);
            Assert.AreEqual("a", token.Value);
            Assert.AreEqual(1, token.Count);

            var parm1 = token[0];

            Assert.AreEqual(Token.TokenType.Function, parm1.Type);
            Assert.AreEqual("b", parm1.Value);
            Assert.AreEqual(2, parm1.Count);

            var parm2 = parm1[0];

            Assert.AreEqual(Token.TokenType.Function, parm2.Type);
            Assert.AreEqual("c", parm2.Value);
            Assert.AreEqual(2, parm2.Count);

            var parm3 = parm2[0];

            Assert.AreEqual(Token.TokenType.Function, parm3.Type);
            Assert.AreEqual("d", parm3.Value);
            Assert.AreEqual(2, parm3.Count);
        }                
        
        [TestMethod]
        [DataRow("a.b == c.d")]
        public void Precompiler_Precompile_expr_multipart(string expression)
        {
            var result = Test(expression);
            
            Assert.AreEqual(expression, result.ToString());
            AssertExpression(result, "a.b", "==", "c.d");
        }
        
        [TestMethod]
        [DataRow("innerjoin(a.b == c.d)")]
        public void Precompiler_Precompile_complex2(string expression)
        {
            var result = Test(expression);
            var fn     = AssertFunction(result, "innerjoin", 1);
            
            Assert.AreEqual(expression, result.ToString());
            AssertExpression(fn[0], "a.b", "==", "c.d");
        }
        
        [TestMethod]
        [DataRow("a.b[x == 7] == c[y == 5].d")]
        public void Precompiler_Precompile_complex3(string expr)
        {
            var token = Test(expr);

            Assert.AreEqual(Token.TokenType.Expression, token.Type);
            Assert.AreEqual(3, token.Count);

            var left  = token[0];
            var right = token[2];

            Assert.AreEqual("==", token[1].Value);
            Assert.AreEqual(Token.TokenType.Multipart, left.Type);
            Assert.AreEqual(Token.TokenType.Multipart, right.Type);
            Assert.AreEqual("a", left[0].Value);
            Assert.AreEqual("c", right[0].Value);
        }
        
        [TestMethod]
        [DataRow("fn(a.b[x == 7] == c[y == 5].d, e)")]
        public void Precompiler_Precompile_complex3a(string expr)
        {
            var token = Test(expr);

            Assert.AreEqual(Token.TokenType.Function, token.Type);
            Assert.AreEqual("fn", token.Value);
            Assert.AreEqual(2, token.Count);
            
            var parm1 = token[0];

            Assert.AreEqual(Token.TokenType.Expression, parm1.Type);
            Assert.AreEqual(3, parm1.Count);

            var left  = parm1[0];
            var right = parm1[2];

            Assert.AreEqual("==", parm1[1].Value);
            Assert.AreEqual(Token.TokenType.Multipart, left.Type);
            Assert.AreEqual(Token.TokenType.Multipart, right.Type);
            Assert.AreEqual("a", left[0].Value);
            Assert.AreEqual("c", right[0].Value);
            
            var parm2 = token[1];

            Assert.AreEqual(Token.TokenType.Text, parm2.Type);
            Assert.AreEqual(0, parm2.Count);
        }
        
        [TestMethod]
        [DataRow("innerjoin($Drivers, $Cars, left.CarId == right.Id)")]
        public void Precompiler_Precompile_complex4(string expr)
        {
            var token = Test(expr);

            Assert.AreEqual(Token.TokenType.Function, token.Type);
            Assert.AreEqual("innerjoin", token.Value);
            Assert.AreEqual(3, token.Count);

            var parm1 = token[0];

            Assert.AreEqual(Token.TokenType.Text, parm1.Type);
            Assert.AreEqual("$Drivers", parm1.Value);

            var parm2 = token[1];

            Assert.AreEqual(Token.TokenType.Text, parm2.Type);
            Assert.AreEqual("$Cars", parm2.Value);

            var parm3 = token[2];

            Assert.AreEqual(Token.TokenType.Expression, parm3.Type);
            Assert.AreEqual(3, parm3.Count);

            var left  = parm3[0];
            var right = parm3[2];

            Assert.AreEqual("==", parm3[1].Value);
            Assert.AreEqual(Token.TokenType.Multipart, left.Type);
            Assert.AreEqual(Token.TokenType.Multipart, right.Type);
            Assert.AreEqual("left", left[0].Value);
            Assert.AreEqual("right", right[0].Value);
        }
        
        [TestMethod]
        [DataRow("bob !!!*** ted")]
        [DataRow("bob > ted + ")]
        [DataRow("+ bob > ted")]
        public void Precompiler_Precompile_error(string expr)
        {
            Assert.ThrowsException<Transformer.SyntaxException>(()=> Test(expr));
        }

        #endregion

        #region Private

        private Token AssertFunction(Token? token, string value, int numParams)
        {
            return AssertToken(token, value, numParams, Token.TokenType.Function);
        }

        private Token AssertText(Token? token, string value)
        {
            return AssertNonContainer(token, value, Token.TokenType.Text);
        }

        private Token AssertLiteral(Token? token, string value)
        {
            return AssertNonContainer(token, value, Token.TokenType.Literal);
        }

        private Token AssertToken(Token? token, string value, int numParams, Token.TokenType type)
        {
            Assert.IsNotNull(token);
            Assert.AreEqual(type, token.Type);
            Assert.AreEqual(value, token.Value);
            Assert.AreEqual(numParams, token.Count);

            return token!;
        }

        private Token AssertNonContainer(Token? token, string value, Token.TokenType type)
        {
            return AssertToken(token, value, 0, type);
        }

        private Token AssertExpression(Token? expr, string left, string op, string right)
        {
            AssertToken(expr, "", 3, Token.TokenType.Expression);

            Assert.AreEqual(left,  expr![0].ToString());
            Assert.AreEqual(op,    expr[1].Value);
            Assert.AreEqual(right, expr[2].ToString());

            return expr!;
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

        #endregion
    }
}
