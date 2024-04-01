using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Map")]
    public class MapTests
    {
        [TestMethod]
        [DataRow("CA", "West", "map")]
        [DataRow("WA", "PNW", "map")]
        [DataRow("OR", "PNW", "map")]
        [DataRow("NY", "East", "map")]
        [DataRow("MO", "Midwest", "map")]
        [DataRow("AR", "South", "map")]
        [DataRow("CA", "West", "map2")]
        [DataRow("WA", "PNW", "map2")]
        [DataRow("OR", "PNW", "map2")]
        [DataRow("NY", "East", "map2")]
        [DataRow("MO", "Midwest", "map2")]
        [DataRow("AR", "South", "map2")]
        [DataRow("CA", "West", "variablecontent")]
        [DataRow("WA", "PNW", "variablecontent")]
        [DataRow("OR", "PNW", "variablecontent")]
        [DataRow("NY", "East", "variablecontent")]
        [DataRow("MO", "Midwest", "variablecontent")]
        [DataRow("AR", "South", "variablecontent")]
        [DataRow("CA", "West", "map4")]
        [DataRow("WA", "PNW", "map4")]
        [DataRow("OR", "PNW", "map4")]
        [DataRow("NY", "East", "map4")]
        [DataRow("MO", "Midwest", "map4")]
        [DataRow("AR", "South", "map4")]
        public  async Task Map_success(string state, string region, string transform)
        {
            var result = await TransformerTest.Test($"Map.{transform}", "Map.address", dataTransform: (s)=> s.Replace("{0}", state));
            var json   = JObject.Parse(result);
   
            Assert.AreEqual(region, json!["Region"]!.ToString());
        }

        [TestMethod]
        [DataRow("CA", "West", "map_expressions")]
        [DataRow("WA", "Puget Sound", "map_expressions")]
        [DataRow("OR", "PNW", "map_expressions")]
        [DataRow("NY", "East", "map_expressions")]
        [DataRow("MO", "Midwest", "map_expressions")]
        [DataRow("AR", "South", "map_expressions")]

        [DataRow("CA", "West", "map_expressions2")]
        [DataRow("WA", "Puget Sound", "map_expressions2")]
        [DataRow("OR", "PNW", "map_expressions2")]
        [DataRow("NY", "East", "map_expressions2")]
        [DataRow("MO", "Midwest", "map_expressions2")]
        [DataRow("AR", "South", "map_expressions2")]

        [DataRow("WA", "Puget Sound", "map_property")]
        public async Task Map_wExpressions(string state, string region, string transform)
        {
            var result = await TransformerTest.Test($"Map.{transform}", "Map.address", dataTransform: (s)=> s.Replace("{0}", state));
            var json   = JObject.Parse(result);
   
            Assert.AreEqual(region, json!["Region"]!.ToString());
        }

        [TestMethod]
        [DataRow("CA", "West", "map")]
        [DataRow("CA", "West", "map2")]
        [DataRow("CA", "West", "map3")]
        public  async Task Map_empty(string state, string region, string transform)
        {
            var result = await TransformerTest.Test($"Map.{transform}", "Map.emptydata", dataTransform: (s)=> s.Replace("{0}", state));
            var json   = JObject.Parse(result);
   
            Assert.AreEqual("South", json!["Region"]!.ToString());
        }

        [TestMethod]
        public  async Task Map_array()
        {
            var result = await TransformerTest.Test($"Map.map5", "Map.addresses");
            var array   = JArray.Parse(result);
   
            Assert.AreEqual("WA PNW",      array![0]!.ToString());
            Assert.AreEqual("CA West",     array![1]!.ToString());
            Assert.AreEqual("NJ East",     array![2]!.ToString());
            Assert.AreEqual("WY Mountain", array![3]!.ToString());
            Assert.AreEqual("IL Midwest",  array![4]!.ToString());
        }
    }
}