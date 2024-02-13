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

            AssertDriver(drivers[0], "Linda", "Martinez", "Seattle, WA",     "Chevy",   "Camaro",   1969);
            AssertDriver(drivers[1], "Bob",   "Yumigata", "Los Angeles, CA", "Audi",    "RS5",      2024);
            AssertDriver(drivers[2], "Frank", "Anderson", "Casper, WY",      "Pontiac", "Firebird", 1970);
        }   
        
        [TestMethod]
        [DataRow("innerjoin2", "customers")]
        public async Task InnerJoin_success2(string transform, string data)
        {
            var result = await TransformerTest.Test("Joins." + transform, data);

            var drivers = JArray.Parse(result);

            Assert.AreEqual(5, drivers!.Count);

            AssertDriver(drivers[0], "Linda",  "Martinez", "Seattle, WA",     "Chevy",   "Camaro",   1969);
            AssertDriver(drivers[1], "Bob",    "Yumigata", "Los Angeles, CA", "Audi",    "RS5",      2024);
            AssertDriver(drivers[2], "Frank",  "Anderson", "Casper, WY",      "Pontiac", "Firebird", 1970);
            AssertDriver(drivers[3], "John",   "Li",       "Chicago, IL",     "Chevy",   "Camaro",   1969);
            AssertDriver(drivers[4], "Greg",   "House",    "Princeton, NJ",   "Pontiac", "Firebird", 1970);
        }   
        
        [TestMethod]
        [DataRow("outerjoin", "customers")]
        public async Task OuterJoin_success(string transform, string data)
        {
            var result = await TransformerTest.Test("Joins." + transform, data);

            var drivers = JArray.Parse(result);

            Assert.AreEqual(4, drivers!.Count);

            AssertDriver(drivers[0], "Linda", "Martinez", "Seattle, WA",     "Chevy",   "Camaro",   1969);
            AssertDriver(drivers[1], "Bob",   "Yumigata", "Los Angeles, CA", "Audi",    "RS5",      2024);
            AssertDriver(drivers[2], "Bob",   "Newhart",  "Hollywood, CA",   null,      null,       null);
            AssertDriver(drivers[3], "Frank", "Anderson", "Casper, WY",      "Pontiac", "Firebird", 1970);
        }   
        
        private void AssertDriver(object result, string firstName, string lastName, string from, string? make, string? model, int? year)
        {       
            var left  = (result as JObject)!["left"] as JObject;
            var right = (result as JObject)!["right"] as JObject;

            Assert.IsNotNull(left);

            Assert.AreEqual(firstName, left["FirstName"]);
            Assert.AreEqual(lastName,  left["LastName"]);
            Assert.AreEqual(from,      left["From"]);

            if(make == null)
            {
                Assert.IsTrue(string.IsNullOrEmpty(right?["Make"]?.ToString()));
                Assert.IsTrue(string.IsNullOrEmpty(right?["Model"]?.ToString()));
                Assert.IsNull(right?["Year"]);
            }
            else
            { 
                Assert.AreEqual(make,      right?["Make"]);
                Assert.AreEqual(model,     right?["Model"]);
                Assert.AreEqual(year,      right?["Year"]);
            }
        }
    }
}