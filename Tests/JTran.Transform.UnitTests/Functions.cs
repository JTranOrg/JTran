
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Functions")]
    public class FunctionTests
    {
        #region union
 
        [TestMethod]
        [DataRow("union", "union", null, null)]
        [DataRow("union2", "union", "Chevy", "Pontiac")]
        public async Task Functions_union(string transform, string data, string make1, string make2)
        {
            var jobj  = await Test(transform, data);
            var array = jobj["Cars"] as JArray;

            Assert.AreEqual(6, array!.Count);

            Assert.AreEqual("Linda",  array[0]["Driver"].ToString());
            Assert.AreEqual("Bob",    array[1]["Driver"].ToString());
            Assert.AreEqual("John",   array[2]["Driver"].ToString());
            Assert.AreEqual("Mary",   array[3]["Driver"].ToString());
            Assert.AreEqual("Oliver", array[4]["Driver"].ToString());
            Assert.AreEqual("Elena",  array[5]["Driver"].ToString());

            Assert.AreEqual(make1, array[0]["Make"]?.ToString());
            Assert.AreEqual(make1, array[1]["Make"]?.ToString());
            Assert.AreEqual(make1, array[2]["Make"]?.ToString());
            Assert.AreEqual(make2, array[3]["Make"]?.ToString());
            Assert.AreEqual(make2, array[4]["Make"]?.ToString());
            Assert.AreEqual(make2, array[5]["Make"]?.ToString());
        }

        #endregion

        #region coalesce
 
        [TestMethod]
        [DataRow("coalesce", "coalesce")]
        public async Task Functions_coalesce(string transform, string data)
        {
            var jobj = await Test(transform, data);

            Assert.AreEqual("Frank", jobj["Driver"]!.ToString());
        }

        #endregion

        #region coalescenumber

        [TestMethod]
        [DataRow("coalescenumber", "coalescenumber")]
        public async Task Functions_coalescenumber(string transform, string data)
        {
            var jobj = await Test(transform, data);

            Assert.AreEqual(33d, double.Parse(jobj["Driver"]!.ToString()));
        }

        #endregion

        #region iif

        [TestMethod]
        [DataRow("iif", "iif")]
        public async Task Functions_iif(string transform, string data)
        {
            var jobj = await Test(transform, data);

            Assert.AreEqual("Wilma", jobj["Driver1"]!.ToString());
            Assert.AreEqual("Fred",  jobj["Driver2"]!.ToString());
        }        
        
        #endregion

        #region position

        [TestMethod]
        [DataRow("position", "cars")]
        public async Task Functions_position(string transform, string data)
        {
            var result = await TransformerTest.Test("Functions." + transform, data);           
            var driver = JsonConvert.DeserializeObject<DriverContainer2>(result)!;

            Assert.AreEqual(3,          driver.Driver!.Vehicles.Count);
            Assert.AreEqual("Chevy",    driver.Driver.Vehicles[0].Brand);
            Assert.AreEqual("Corvette", driver.Driver.Vehicles[0].Model);
            Assert.AreEqual(1964,       driver.Driver.Vehicles[0].Year);
            Assert.AreEqual("Blue",     driver.Driver.Vehicles[0].Color);

            Assert.AreEqual(0,          driver.Driver.Vehicles[0]!.Index);
            Assert.AreEqual(1,          driver.Driver.Vehicles[1]!.Index);
            Assert.AreEqual(2,          driver.Driver.Vehicles[2]!.Index);
        }        
        
        #endregion

        #region sort

        [TestMethod]
        [DataRow("sort", "sort")]
        [DataRow("sortnumbers", "sortnumbers")]
        [DataRow("sortmultiple", "sortmultiple")]
        public async Task Functions_sort_success(string transform, string data)
        {
            var jobj = await Test(transform, data);
            var array = jobj["Drivers"] as JArray;

            Assert.AreEqual("Akira",   array![0]!["Name"]!.ToString());
            Assert.AreEqual("Odyssey", array![1]!["Name"]!.ToString());
            Assert.AreEqual("Pegasus", array![2]!["Name"]!.ToString());
        }

        [TestMethod]
        [DataRow("sortdesc", "sort")]
        [DataRow("sortnumbersdesc", "sortnumbers")]
        [DataRow("sortmultipledesc", "sortmultiple")]
        public async Task Functions_sort_desc(string transform, string data)
        {
            var jobj = await Test(transform, data);
            var array = jobj["Drivers"] as JArray;

            Assert.AreEqual("Pegasus", array![0]!["Name"]!.ToString());
            Assert.AreEqual("Odyssey", array![1]!["Name"]!.ToString());
            Assert.AreEqual("Akira",   array![2]!["Name"]!.ToString());
        }

        #endregion

        #region Private

       public async Task<JObject> Test(string transform, string data)
        {
            var result = await TransformerTest.Test("Functions." + transform, "Functions." + data);           
            var jobj   = JObject.Parse(result)!;

            Assert.IsNotNull(jobj);

            return jobj;
        }

        #endregion
    }
}