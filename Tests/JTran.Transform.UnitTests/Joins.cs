using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Joins")]
    public class JoinTests
    {
        [TestMethod]
        [DataRow("innerjoin", "customers")]
        public async Task InnerJoin_success(string transform, string data)
        {
            var result = await TransformerTest.Test("Joins." + transform, data);

            var drivers = JArray.Parse(result);

            Assert.AreEqual(3, drivers!.Count);

            var driver = drivers[0] as JObject;
            var left   = driver!["left"] as JObject;
            var right  = driver["right"] as JObject;

            Assert.IsNotNull(driver);

            Assert.AreEqual("Chevy",          right!["Make"]);
            Assert.AreEqual("Camaro",         right!["Model"]);
            Assert.AreEqual(1969,             right!["Year"]);
                    
            Assert.AreEqual("Linda",          left!["FirstName"]);
            Assert.AreEqual("Martinez",       left!["LastName"]);
            Assert.AreEqual("Seattle, WA",    left!["From"]);

            driver = drivers[1] as JObject;
            left   = driver!["left"] as JObject;
            right  = driver["right"] as JObject;

            Assert.IsNotNull(driver);

            Assert.AreEqual("Audi",            right!["Make"]);
            Assert.AreEqual("RS5",              right!["Model"]);
            Assert.AreEqual(2024,               right!["Year"]);
                    
            Assert.AreEqual("Bob",              left!["FirstName"]);
            Assert.AreEqual("Yumigata",         left!["LastName"]);
            Assert.AreEqual("Los Angeles, CA",  left!["From"]);

            driver = drivers[2] as JObject;
            left   = driver!["left"] as JObject;
            right  = driver["right"] as JObject;

            Assert.IsNotNull(driver);

            Assert.AreEqual("Pontiac",            right!["Make"]);
            Assert.AreEqual("Firebird",           right!["Model"]);
            Assert.AreEqual(1970,                 right!["Year"]);
                    
            Assert.AreEqual("Frank",              left!["FirstName"]);
            Assert.AreEqual("Anderson",           left!["LastName"]);
            Assert.AreEqual("Casper, WY",         left!["From"]);
        }                                         
    }
}