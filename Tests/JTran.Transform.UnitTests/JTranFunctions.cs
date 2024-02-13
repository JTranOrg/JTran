
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("JTran Functions")]
    public class JTranFunctionTests
    {
        [TestMethod]
        [DataRow("after", "after")]
        public async Task Functions_after(string transform, string data)
        {
            var result = await Test(transform, data);

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Year: 1964 }"), JObject.Parse(result)));
        }

        [TestMethod]
        [DataRow("nested", "nested")]
        public async Task Functions_nested(string transform, string data)
        {
            var result = await Test(transform, data);
            var json   = JObject.Parse(result);
   
            Assert.AreEqual("Topsoil", json!["Products"]![0]!["Name"]!.ToString());
            Assert.AreEqual("Paint",   json!["Products"]![1]!["Name"]!.ToString());
        }

        #region Private

        public async Task<string> Test(string transform, string data)
        {
            var result = await TransformerTest.Test("JTranFunctions." + transform, "JTranFunctions." + data);           
            var jobj   = JObject.Parse(result)!;

            Assert.IsNotNull(jobj);

            return result;
        }

        #endregion
    }
}