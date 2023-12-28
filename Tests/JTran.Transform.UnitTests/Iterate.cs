using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var result = await TransformerTest.Test(transform,data);

            var all = JObject.Parse(result);
            var drivers = all!["Drivers"]! as JArray;

            Assert.AreEqual(3, drivers!.Count);
        }
    }
}