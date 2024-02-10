
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;
using JTran.Collections;
using JTran.Expressions;
using JTranParser = JTran.Parser.ExpressionParser;
using System.Dynamic;

namespace JTran.UnitTests
{
    [TestClass]
    public class InnerJoinTests
    {
        [TestMethod]
        public void InnerJoin_Success()
        {
            var right = new List<Automobile> 
            { 
                new Automobile { Id = "1", Make = "Chevy",   Model = "Camaro",   Year = 1970 },
                new Automobile { Id = "2", Make = "Pontiac", Model = "Firebird", Year = 1969 },
                new Automobile { Id = "3", Make = "Audi",    Model = "RS5",      Year = 2024 }
            };
            var left = new List<Driver> 
            { 
                new Driver { CarId = "3", FirstName = "Linda", LastName = "Martinez" },
                new Driver { CarId = "1", FirstName = "Bob",   LastName = "Yumigata" },
                new Driver { CarId = "2", FirstName = "Frank", LastName = "Anderson" }
            };

            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("left.CarId == right.Id"));
            var innerJoin    = new InnerJoin(left, right, expression, context);
            var result       = new List<object>(innerJoin);

            Assert.AreEqual(3, result.Count);

            var left1  = (result[0] as dynamic).left as Driver;
            var right1 = (result[0] as dynamic).right as Automobile;

            Assert.IsNotNull(left1);
            Assert.IsNotNull(right1);

            Assert.AreEqual("Audi", right1.Make);
            Assert.AreEqual("Linda", left1.FirstName);

            var left2  = (result[1] as dynamic).left as Driver;
            var right2 = (result[1] as dynamic).right as Automobile;

            Assert.IsNotNull(left2);
            Assert.IsNotNull(right2);

            Assert.AreEqual("Chevy", right2.Make);
            Assert.AreEqual("Bob", left2.FirstName);

            var left3  = (result[2] as dynamic).left as Driver;
            var right3 = (result[2] as dynamic).right as Automobile;

            Assert.IsNotNull(left3);
            Assert.IsNotNull(right3);

            Assert.AreEqual("Pontiac", right3.Make);
            Assert.AreEqual("Frank", left3.FirstName);
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
