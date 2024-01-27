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
        public async Task Arrays_variable_success(string transform, string data)
        {
            var result = await Test(transform, data);
            var array   = JObject.Parse(result.ToString())["Cars"] as JArray;

             Assert.AreEqual("Chevy",  array![0]!["Make"]!.ToString());
             Assert.AreEqual("Camaro", array![0]!["Model"]!.ToString());

             Assert.AreEqual("Pontiac",  array![1]!["Make"]!.ToString());
             Assert.AreEqual("Firebird", array![1]!["Model"]!.ToString());
        }

        #region Private

        public async Task<JObject> Test(string transform, string data)
        {
            var result = await TransformerTest.Test("Arrays." + transform, data);           
            var jobj   = JObject.Parse(result)!;

            Assert.IsNotNull(jobj);

            return jobj;
        }

        #endregion   
    }
}