using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("iif")]
    public class IifTests
    {
        [TestMethod]
        [DataRow("iif2", "customers", "Audi")]
        [DataRow("iif", "customers", "Chevy")]
        public async Task IIf_success(string transform, string data, string expected)
        {
            var result = await TransformerTest.Test("Iif." + transform, data);
            var jobj = JObject.Parse(result);

            Assert.AreEqual(expected, jobj!["Make"]);
        }
    }
}