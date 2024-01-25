using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Parser;
using JTranParser = JTran.Parser.ExpressionParser;
using System.Xml.Linq;

namespace JTran.Parser.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class ParserTests
    {
        [TestMethod]
        public void Parser_Equality_Success()
        {
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
            var parser = new JTranParser();
            var tokens = parser.Parse("FirstName == 'LastName = Bob'");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,                  tokens.Count);
            Assert.AreEqual("FirstName",        tokens[0].Value);
            Assert.AreEqual("==",               tokens[1].Value);
            Assert.AreEqual("LastName = Bob",   tokens[2].Value);
            Assert.AreEqual(Token.TokenType.Literal,   tokens[2].Type);
        }

        [TestMethod]
        [DataRow("2")]
        [DataRow("2.5")]
        [DataRow(".5")]
        [DataRow("-2.5")]
        [DataRow("21.5")]
        public void Expression_Decimal_Success(string val)
        {
            var parser = new JTranParser();
            var tokens = parser.Parse(val);
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1,         tokens.Count);
            Assert.AreEqual(val,     tokens[0].Value);
            Assert.AreEqual(Token.TokenType.Number, tokens[0].Type);
        }

        [TestMethod]
        public void Expression_Number_Success()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("Wage == 21.5");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,          tokens.Count);
            Assert.AreEqual("Wage",     tokens[0].Value);
            Assert.AreEqual("==",       tokens[1].Value);
            Assert.AreEqual("21.5",     tokens[2].Value);
            Assert.AreEqual(Token.TokenType.Number,     tokens[2].Type);
        }

        [TestMethod]
        public void Expression_Variable_Success()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("$Wage == 21.5");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(3,          tokens.Count);
            Assert.AreEqual("$Wage",    tokens[0].Value);
            Assert.AreEqual("==",       tokens[1].Value);
            Assert.AreEqual("21.5",     tokens[2].Value);
        }

        [TestMethod]
        [DataRow("Type3")]
        [DataRow("$Type3")]
        [DataRow("Ty1pe3")]
        [DataRow("$Ty1pe3")]
        [DataRow("T2x4x5x")]
        public void Expression_name_w_number(string val)
        {
            var parser = new JTranParser();
            var tokens = parser.Parse(val);
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1,    tokens.Count);
            Assert.AreEqual(val,  tokens[0].Value);
            Assert.AreEqual(Token.TokenType.Text, tokens[0].Type);
        }        
        
        [TestMethod]
        [DataRow("floor(ceiling(3.5))", 7, 4)]
        [DataRow("ceiling(2.88, 34.99, 3.5)", 8, 6)]
        [DataRow("floor(2.11, ceiling(3.5))", 9, 6)]
        [DataRow("-14 * .53 + 3.5", 5, 4)]
        [DataRow("-14 - -.53 - 3.5", 5, 4)]
        public void Expression_name_w_number2(string expr, int count, int index)
        {
            var parser = new JTranParser();
            var tokens = parser.Parse(expr);
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(count,    tokens.Count);
            Assert.AreEqual("3.5", tokens[index].Value);
        }
        
        [TestMethod]
        public void Expression_Multi_Success()
        {
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
        public void Expression_Multipart_success()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("Customer.Name.First");
   
            Assert.AreEqual(5,          tokens.Count);
            Assert.AreEqual("Customer", tokens[0].Value);
            Assert.AreEqual(".",        tokens[1].Value);
            Assert.AreEqual("Name",     tokens[2].Value);
            Assert.AreEqual(".",        tokens[3].Value);
            Assert.AreEqual("First",    tokens[4].Value);
        }

        [TestMethod]
        public void Expression_sortforeach_Success()
        {
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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
            var parser = new JTranParser();
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

        [TestMethod]
        public void Expression_commas_Success()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("(a, b)");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(5, tokens.Count);
            Assert.AreEqual(Token.TokenType.Operator, tokens[2].Type);
        }

        [TestMethod]
        public void Expression_array_indexer_and_multipart()
        {
            var parser = new JTranParser();
            var tokens = parser.Parse("Customer[City == 'Seattle'].FirstName");
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(8, tokens.Count);
            Assert.AreEqual(".", tokens[6].Value);
            Assert.AreEqual("FirstName", tokens[7].Value);
        }

        [TestMethod]
        [DataRow("normalizespace(RemoveEnding(RemoveAnyEnding(RemoveEnding($name, ')'), $keywords.keywords), '('))", 21)]
        public void Expression_complex(string expr, int count)
        {
            var parser = new JTranParser();
            var tokens = parser.Parse(expr);
   
            Assert.IsNotNull(tokens);
            Assert.AreEqual(count, tokens.Count);
        }
    }
}
