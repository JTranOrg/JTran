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
            var drivers = JsonConvert.DeserializeObject<List<Entry>>(result);

            Assert.AreEqual(4, drivers!.Count);

            AssertDriver2(drivers[0], "Linda Martinez", "Seattle, WA",     "Chevy",   "Camaro",   1969);
            AssertDriver2(drivers[1], "Bob Yumigata",   "Los Angeles, CA", "Audi",    "RS5",      2024);
            AssertDriver2(drivers[2], "Bob Newhart",    "Hollywood, CA",   null,      null,       null);
            AssertDriver2(drivers[3], "Frank Anderson", "Casper, WY",      "Pontiac", "Firebird", 1970);
        }

        #region Private

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

        private void AssertDriver2(Entry driver, string name, string from, string? make, string? model, int? year)
        {       
            Assert.AreEqual(name,   driver.Driver);
            Assert.AreEqual(from,   driver.From);
            Assert.AreEqual(make,   driver.Make);
            Assert.AreEqual(model,  driver.Model);
            Assert.AreEqual(year,   driver.Year);
        }

        public class Entry
        {
            public string? Driver { get; set; }
            public string? From   { get; set; }
            public string? Make   { get; set; }
            public string? Model  { get; set; }
            public int?    Year   { get; set; }
        }
        #endregion 
    }
}