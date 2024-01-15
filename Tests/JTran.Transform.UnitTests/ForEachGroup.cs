using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("ForEachGroup")]
    public class ForEachGroupTests
    {
        [TestMethod]
        [DataRow("foreachgroup1",   "drivers")]
        [DataRow("foreachgroup2",   "drivers")]
        [DataRow("foreachgroup3",   "only1_group")]
        [DataRow("no_array_name",   "drivers")] 
        [DataRow("multiple_fields", "drivers2")] 
        public async Task ForEachGroup_success(string transform, string data)
        {
            var result   = await TransformerTest.Test("ForEachGroup." + transform, "ForEachGroup." + data);
            var expected = await TransformerTest.LoadSample("ForEachGroup." + transform + "_expected");

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(result)));
        }
    }
}