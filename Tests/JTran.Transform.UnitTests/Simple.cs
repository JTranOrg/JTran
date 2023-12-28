using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Simple")]
    public class SimpleTests
    {
        [TestMethod]
        [DataRow("noelements", "simpleperson")]
        public async Task Simple_success(string transform, string data)
        {
            var result = await TransformerTest.Test(transform, data);
            var jobj   = JObject.Parse(result.ToString());

             Assert.AreEqual("Chevy",    jobj["Make"]!.ToString());
             Assert.AreEqual("Corvette", jobj["Model"]!.ToString());
             Assert.AreEqual(1964,       int.Parse(jobj["Year"]!.ToString()));
        }

        [TestMethod]
        [DataRow("dotattributes",  "car1")]
        public async Task Simple_dotattributes(string transform, string data)
        {
            await TransformerTest.Test(transform, data);
        }

        [TestMethod]
        [DataRow("dotattributes",  "car2")]
        public async Task Simple_escaped_character(string transform, string data)
        {
             await TransformerTest.Test(transform, data);
        }

        [TestMethod]
        [DataRow("dotattributes2",  "car2")]
        public async Task Simple_escaped_name(string transform, string data)
        {
             var result = await TransformerTest.Test(transform, data);

             var jobj = JObject.Parse(result.ToString());

             Assert.AreEqual("Chevrolet \\Chevy\\", jobj["M\\a\\ke"]!.ToString());

        }
    }
}