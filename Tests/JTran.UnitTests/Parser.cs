using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using JTran;
using JTran.Expressions;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class ParserTests
    {
        [TestMethod]
        public void Parser_Equality_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("Name == 'Fred'");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,        tokens.Count);
            Assert.AreEqual("Name",   tokens[0].Value);
            Assert.AreEqual("==",     tokens[1].Value);
            Assert.AreEqual("Fred", tokens[2].Value);

            Assert.AreEqual(Token.TokenType.Text, tokens[0].Type);
            Assert.AreEqual(Token.TokenType.Operator, tokens[1].Type);
            Assert.AreEqual(Token.TokenType.Literal, tokens[2].Type);
        }

        [TestMethod]
        public void Expression_Where_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("Cars[Make == 'Chevy']");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(6,         tokens.Count);
            Assert.AreEqual("Cars",    tokens[0].Value);
            Assert.AreEqual("[",       tokens[1].Value);
            Assert.AreEqual("Make",    tokens[2].Value);
            Assert.AreEqual("==",      tokens[3].Value);
            Assert.AreEqual("Chevy", tokens[4].Value);
            Assert.AreEqual("]",       tokens[5].Value);
        }

        [TestMethod]
        public void Expression_Addition_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("FirstName + ' Fred ' + LastName");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(5,            tokens.Count);
            Assert.AreEqual("FirstName",  tokens[0].Value);
            Assert.AreEqual("+",          tokens[1].Value);
            Assert.AreEqual(" Fred ",   tokens[2].Value);
            Assert.AreEqual("+",          tokens[3].Value);
            Assert.AreEqual("LastName",   tokens[4].Value);
        }

        [TestMethod]
        public void Expression_Tertiary_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("FirstName ? FirstName : LastName");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(5,            tokens.Count);
            Assert.AreEqual("FirstName",  tokens[0].Value);
            Assert.AreEqual("?",          tokens[1].Value);
            Assert.AreEqual("FirstName",  tokens[2].Value);
            Assert.AreEqual(":",          tokens[3].Value);
            Assert.AreEqual("LastName",   tokens[4].Value);
        }        
        
        [TestMethod]
        public void Expression_Tertiary2_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("Name == Instructor && Age >= 21 ? FirstName : LastName");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(11,            tokens.Count);
            Assert.AreEqual("Name",        tokens[0].Value);
            Assert.AreEqual("==",          tokens[1].Value);
            Assert.AreEqual("Instructor",  tokens[2].Value);
            Assert.AreEqual("&&",          tokens[3].Value);
            Assert.AreEqual("Age",         tokens[4].Value);
            Assert.AreEqual(">=",          tokens[5].Value);
            Assert.AreEqual("21",          tokens[6].Value);
            Assert.AreEqual("?",           tokens[7].Value);
            Assert.AreEqual("FirstName",   tokens[8].Value);
            Assert.AreEqual(":",           tokens[9].Value);
            Assert.AreEqual("LastName",    tokens[10].Value);
        }

        [TestMethod]
        public void Expression_NullCoalescing_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("FirstName ?? LastName");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,            tokens.Count);
            Assert.AreEqual("FirstName",  tokens[0].Value);
            Assert.AreEqual("??",         tokens[1].Value);
            Assert.AreEqual("LastName",   tokens[2].Value);
        }

        [TestMethod]
        public void Expression_StringLiteral_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("FirstName == 'LastName = Bob'");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,                  tokens.Count);
            Assert.AreEqual("FirstName",        tokens[0].Value);
            Assert.AreEqual("==",               tokens[1].Value);
            Assert.AreEqual("LastName = Bob",   tokens[2].Value);
            Assert.AreEqual(Token.TokenType.Literal,   tokens[2].Type);
        }

        [TestMethod]
        public void Expression_Number_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("Wage == 21.5");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,          tokens.Count);
            Assert.AreEqual("Wage",     tokens[0].Value);
            Assert.AreEqual("==",       tokens[1].Value);
            Assert.AreEqual("21.5",     tokens[2].Value);
            Assert.AreEqual(Token.TokenType.Number,     tokens[2].Type);
        }

        [TestMethod]
        public void Expression_JTranField_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("#(Wage) == 21.5");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(6,          tokens.Count);
            Assert.AreEqual("#",        tokens[0].Value);
            Assert.AreEqual("(",        tokens[1].Value);
            Assert.AreEqual("Wage",     tokens[2].Value);
            Assert.AreEqual(")",        tokens[3].Value);
            Assert.AreEqual("==",       tokens[4].Value);
            Assert.AreEqual("21.5",     tokens[5].Value);
        }

        [TestMethod]
        public void Expression_Variable_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("$Wage == 21.5");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,          tokens.Count);
            Assert.AreEqual("$Wage",    tokens[0].Value);
            Assert.AreEqual("==",       tokens[1].Value);
            Assert.AreEqual("21.5",     tokens[2].Value);
        }

        [TestMethod]
        public void Expression_Multi_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("100 + Salary * Months");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(5,            tokens.Count);
            Assert.AreEqual("100",        tokens[0].Value);
            Assert.AreEqual("+",          tokens[1].Value);
            Assert.AreEqual("Salary",     tokens[2].Value);
            Assert.AreEqual("*",          tokens[3].Value);
            Assert.AreEqual("Months",     tokens[4].Value);

            Assert.AreEqual(Token.TokenType.Number,   tokens[0].Type);
            Assert.AreEqual(Token.TokenType.Operator, tokens[1].Type);
            Assert.AreEqual(Token.TokenType.Text,     tokens[2].Type);
            Assert.AreEqual(Token.TokenType.Operator, tokens[3].Type);
            Assert.AreEqual(Token.TokenType.Text,     tokens[4].Type);
        }

        [TestMethod]
        public void Expression_Multi2_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("fn(Val + 3, 4)");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(8,     tokens.Count);
            Assert.AreEqual("fn",  tokens[0].Value);
            Assert.AreEqual("(",   tokens[1].Value);
            Assert.AreEqual("Val", tokens[2].Value);
            Assert.AreEqual("+",   tokens[3].Value);
            Assert.AreEqual("3",   tokens[4].Value);
            Assert.AreEqual(",",   tokens[5].Value);
            Assert.AreEqual("4",   tokens[6].Value);
            Assert.AreEqual(")",   tokens[7].Value);
        }

        [TestMethod]
        public void Expression_sortforeach_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("#foreach(sort(Customers, Name), {})");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(11,            tokens.Count);
            Assert.AreEqual("#foreach",    tokens[0].Value);
            Assert.AreEqual("(",           tokens[1].Value);
            Assert.AreEqual("sort",        tokens[2].Value);
            Assert.AreEqual("(",           tokens[3].Value);
            Assert.AreEqual("Customers",   tokens[4].Value);
            Assert.AreEqual(",",           tokens[5].Value);
            Assert.AreEqual("Name",        tokens[6].Value);
            Assert.AreEqual(")",           tokens[7].Value);
            Assert.AreEqual(",",           tokens[8].Value);
            Assert.AreEqual("{}",          tokens[9].Value);
            Assert.AreEqual(")",           tokens[10].Value);
        }

        [TestMethod]
        public void Expression_blank_string_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("#('')");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(4,     tokens.Count);
            Assert.AreEqual("#",   tokens[0].Value);
            Assert.AreEqual("(",   tokens[1].Value);
            Assert.AreEqual("",    tokens[2].Value);
            Assert.AreEqual(")",   tokens[3].Value);
        }

        [TestMethod]
        public void Expression_blank_string2_Success()
        {
            var parser = new Parser();
            var tokens = parser.Parse("#(somefunc(''))");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(7,          tokens.Count);
            Assert.AreEqual("#",        tokens[0].Value);
            Assert.AreEqual("(",        tokens[1].Value);
            Assert.AreEqual("somefunc", tokens[2].Value);
            Assert.AreEqual("(",        tokens[3].Value);
            Assert.AreEqual("",         tokens[4].Value);
            Assert.AreEqual(")",        tokens[5].Value);
            Assert.AreEqual(")",        tokens[6].Value);
        }
    }
}
