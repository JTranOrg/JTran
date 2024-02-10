using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("if")]
    public class IfTests
    {
        [TestMethod]
        [DataRow("if", "customers", "Acme Widgets")]
        public async Task If_success(string transform, string data, string expected)
        {
            var result = await TransformerTest.Test("If." + transform, data);
            var jobj = JObject.Parse(result);

            Assert.AreEqual(expected, jobj!["Make"]);
            Assert.AreEqual("Pierce", jobj!["Make2"]);
        }
    }
}