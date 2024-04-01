
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Collections;
using JTran.Expressions;
using JTranParser = JTran.Parser.ExpressionParser;
using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    public class WhereClauseTests
    {
        [TestMethod]
        public void WhereClause_Success()
        {
            var list         = new List<string> { "red", "green", "blue" };
            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("@ > 'r'"));
            var whereClause  = new WhereClause<string>(list, expression, context);
            var result       = new List<string>(whereClause);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("red", result[0]);
        }

        [TestMethod]
        public void WhereClause_Success2()
        {
            var list = new List<Automobile> 
            { 
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Camaro",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Pontiac",
                    Model = "GTO",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Audi",
                    Model = "RS5",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Corvette",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Silverado",
                    Year  = 1969,
                    Color = "Blue",
                }
            };

            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("Make == 'Chevy'"));
            var whereClause  = new WhereClause<object>(list, expression, context);
            var result       = new List<object>(whereClause.ToList());

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual("Chevy", (result![0] as Automobile)!.Make);
        }

        [TestMethod]
        public void PocoWhereClause_Success()
        {
            var list = new List<Automobile> 
            { 
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Camaro",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Pontiac",
                    Model = "GTO",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Audi",
                    Model = "RS5",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Corvette",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Silverado",
                    Year  = 1969,
                    Color = "Blue",
                }
            };

            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("Make == 'Chevy'"));
            var whereClause  = new PocoWhereClause(typeof(Automobile), list, expression, context);
            var result       = new List<object>(whereClause);

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual("Chevy", (result[0] as IObject)!.GetPropertyValue(CharacterSpan.FromString("Make")));
        }
    }
}
