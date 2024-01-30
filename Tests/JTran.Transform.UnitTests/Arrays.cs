using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Arrays")]
    public class ArrayTests
    {
        [TestMethod]
        [DataRow("variable", "automobiles")]
        [DataRow("array_source", "array_source")]
        public async Task Arrays_success(string transform, string data)
        {
            var result = await Test(transform, data);
            var array   = JObject.Parse(result.ToString())["Cars"] as JArray;

             Assert.AreEqual("Chevy",  array![0]!["Make"]!.ToString());
             Assert.AreEqual("Camaro", array![0]!["Model"]!.ToString());

             Assert.AreEqual("Pontiac",  array![1]!["Make"]!.ToString());
             Assert.AreEqual("Firebird", array![1]!["Model"]!.ToString());
        }

        [TestMethod]
        [DataRow("array_output", "array_source")]
        public async Task Arrays_output_success(string transform, string data)
        {
            var array = await TestArray(transform, data);

             Assert.AreEqual("Chevybob",    array![0]!["Make"]!.ToString());
             Assert.AreEqual("Camarobob",   array![0]!["Model"]!.ToString());

             Assert.AreEqual("Pontiacbob",  array![1]!["Make"]!.ToString());
             Assert.AreEqual("Firebirdbob", array![1]!["Model"]!.ToString());
        }

        #region Private

        public async Task<JObject> Test(string transform, string data)
        {
            var result = await TransformerTest.Test("Arrays." + transform, "Arrays." + data);           
            var jobj   = JObject.Parse(result)!;

            Assert.IsNotNull(jobj);

            return jobj;
        }

        public async Task<JArray> TestArray(string transform, string data)
        {
            var result = await TransformerTest.Test("Arrays." + transform, "Arrays." + data);           
            var array   = JArray.Parse(result)!;

            Assert.IsNotNull(array);

            return array;
        }

        #endregion   
    }
}