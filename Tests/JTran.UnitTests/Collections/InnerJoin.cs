
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;
using JTran.Collections;
using JTran.Expressions;
using JTranParser = JTran.Parser.ExpressionParser;
using System.Dynamic;
using JTran.Parser;

namespace JTran.UnitTests
{
    [TestClass]
    public class InnerJoinTests
    {
        [TestMethod]
        public void InnerJoin_Success()
        {
            var left = new List<Driver> 
            { 
                new Driver { CarId = "3", FirstName = "Linda", LastName = "Martinez" },
                new Driver { CarId = "1", FirstName = "Bob",   LastName = "Yumigata" },
                new Driver { CarId = "2", FirstName = "Frank", LastName = "Anderson" }
            };

            var right = new List<Automobile> 
            { 
                new Automobile { Id = "1", Make = "Chevy",   Model = "Camaro",   Year = 1970 },
                new Automobile { Id = "2", Make = "Pontiac", Model = "Firebird", Year = 1969 },
                new Automobile { Id = "3", Make = "Audi",    Model = "RS5",      Year = 2024 }
            };
 
            var result = Test(left, right, 3);

            AssertDriver(result[0], "Linda", "Audi");
            AssertDriver(result[1], "Bob", "Chevy");
            AssertDriver(result[2], "Frank", "Pontiac");
        }

        [TestMethod]
        public void InnerJoin_Success2()
        {
            var left = new List<Driver> 
            { 
                new Driver { CarId = "3", FirstName = "Linda",  LastName = "Martinez" },
                new Driver { CarId = "1", FirstName = "Bob",    LastName = "Yumigata" },
                new Driver { CarId = "2", FirstName = "Frank",  LastName = "Anderson" },
                new Driver { CarId = "1", FirstName = "John",   LastName = "Li" },
                new Driver { CarId = "2", FirstName = "Greg",   LastName = "House" }
            };

            var right = new List<Automobile> 
            { 
                new Automobile { Id = "1", Make = "Chevy",   Model = "Camaro",   Year = 1970 },
                new Automobile { Id = "2", Make = "Pontiac", Model = "Firebird", Year = 1969 },
                new Automobile { Id = "3", Make = "Audi",    Model = "RS5",      Year = 2024 }
            };
 
            var result = Test(left, right, 5);

            AssertDriver(result[0], "Linda", "Audi");
            AssertDriver(result[1], "Bob",   "Chevy");
            AssertDriver(result[2], "Frank", "Pontiac");
            AssertDriver(result[3], "John",  "Chevy");
            AssertDriver(result[4], "Greg",  "Pontiac");
        }

        private void AssertDriver(object result, string name, string model)
        {       
            var left   = (result as JsonObject)![CharacterSpan.FromString("left")] as Driver;
            var right  = (result as JsonObject)![CharacterSpan.FromString("right")] as Automobile;

            Assert.IsNotNull(left);
            Assert.IsNotNull(right);

            Assert.AreEqual(model, right.Make);
            Assert.AreEqual(name, left.FirstName);
        }

        private List<object> Test(object left, object right, int numObjects)
        {
            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("left.CarId == right.Id"));
            var innerJoin    = new InnerOuterJoin(left, right, expression, context, true);
            var result       = new List<object>(innerJoin);

            Assert.AreEqual(numObjects, result.Count);

            return result;
        }

        public class Automobile
        {
            public string  Id       { get; set; } = "";
            public string  Make     { get; set; } = "";
            public string  Model    { get; set; } = "";
            public int     Year     { get; set; } 
        }

         public class Driver
        {
            public string       CarId     { get; set; } = "";
            public string       FirstName { get; set; } = "";
            public string       LastName  { get; set; } = "";
        }
    }

    internal static class Extensions
    {
        internal static IReadOnlyList<Token> Parse(this JTranParser parser, string data)
        {
            return parser.Parse(CharacterSpan.FromString(data));
        }
    }
}
