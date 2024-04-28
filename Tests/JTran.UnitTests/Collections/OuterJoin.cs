
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Collections;
using JTran.Expressions;
using JTranParser = JTran.Parser.ExpressionParser;
using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    public class OuterJoinTests
    {
        [TestMethod]
        public void OuterJoin_Success()
        {
            var left = new List<Driver> 
            { 
                new Driver { CarId = "3",  FirstName = "Linda", LastName = "Martinez" },
                new Driver { CarId = "1",  FirstName = "Bob",   LastName = "Yumigata" },
                new Driver { CarId = "2",  FirstName = "Frank", LastName = "Anderson" },
                new Driver { CarId = "33", FirstName = "Tim",   LastName = "Newhart" }
            };

            var right = new List<Automobile> 
            { 
                new Automobile { Id = "1", Make = "Chevy",   Model = "Camaro",   Year = 1970 },
                new Automobile { Id = "2", Make = "Pontiac", Model = "Firebird", Year = 1969 },
                new Automobile { Id = "3", Make = "Audi",    Model = "RS5",      Year = 2024 }
            };
 
            var result = Test(left, right, 4);

            AssertDriver(result[0], "Linda", "Audi");
            AssertDriver(result[1], "Bob",   "Chevy");
            AssertDriver(result[2], "Frank", "Pontiac");
            AssertDriver(result[3], "Tim",   null);
        }

        private void AssertDriver(object result, string name, string? model)
        {       
            var jobj  = result as JsonObject;
            var left  = jobj![CharacterSpan.FromString("left")] as Driver;
            var right = jobj[CharacterSpan.FromString("right")] as Automobile;

            Assert.IsNotNull(left);

            Assert.AreEqual(model, right?.Make);
            Assert.AreEqual(name, left.FirstName);
        }

        private List<object> Test(object left, object right, int numObjects)
        {
            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("left.CarId == right.Id"));
            var OuterJoin    = new InnerOuterJoin(left, right, expression, context, false);
            var result       = new List<object>(OuterJoin);

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
}
