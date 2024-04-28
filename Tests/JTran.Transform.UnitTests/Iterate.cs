using JTran.Streams;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Iterate")]
    public class IterateTests
    {
        [TestMethod]
        [DataRow("iterate", "customers")]
        public async Task Iterate_success(string transform, string data)
        {
            var result = await TransformerTest.Test(transform, data);

            var all = JObject.Parse(result);
            var drivers = all!["Drivers"]! as JArray;

            Assert.AreEqual(3, drivers!.Count);
        }

        [TestMethod]
        [DataRow("iterate_output", "customers")]
        public async Task Iterate2_success(string transform, string data)
        {
            var result = await TransformerTest.Test(transform,data);

            var drivers = JArray.Parse(result);

            Assert.AreEqual(3, drivers!.Count);
        }

        #if DEBUG

        [TestMethod]
        [DataRow("iterate_output", "customers")]
        public async Task Iterate2_multi_output(string transformName, string dataName)
        {
            using var input = TransformerTest.LoadSampleStream(dataName);
            var transform   = await TransformerTest.LoadTransform(transformName);
            var transformer = new JTran.Transformer(transform);
            await using var output = new FileStreamFactory((index)=> $"c:\\Documents\\Testing\\JTran\\Iterate\\iterate{index+1}.json");

            transformer.Transform(input, output);
        }

        #endif
    }
}