using JTran.Streams;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("ObjectIndexer")]
    public class ObjectIndexerTests
    {
        [TestMethod]
        [DataRow("ObjectIndexer.objectindexer", "customers")]
        public async Task ObjectIndexer_success(string transform, string data)
        {
            var result = await TransformerTest.Test(transform, data);

            var all = JArray.Parse(result);

            Assert.AreEqual(3, all!.Count);
            Assert.AreEqual("General Motors", all[0]["Manufacturer"]);
        }

        [TestMethod]
        [DataRow("ObjectIndexer.objectindexer_poco", "customers")]
        public async Task ObjectIndexer_poco_success(string transform, string data)
        {
            var result = await TransformerTest.TestObject(transform, _makes, true);

            var all = JArray.Parse(result);

            Assert.AreEqual(3, all!.Count);
            Assert.AreEqual("General Motors", all[0]["Manufacturer"]);
        }

        private static Makes _makes = new();

        private class Makes
        {
            public Make Chevy { get; set; } = new Make { Manufacturer = "General Motors" };
            public Make Audi  { get; set; } = new Make { Manufacturer = "Volkswagon Group" };
            public Make Honda { get; set; } = new Make { Manufacturer = "Honda" };

        }

        private class Make
        {
            public string? Manufacturer { get; set; }
            public string? Founded { get; set; }
        }
    }
}