
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("callelement")]
    public class CallElementTests
    {
        [TestMethod]
        [DataRow("element1", "element1", "{ 'AlmaMater': 'Harvard'}")]
        [DataRow("element2", "element1", "{ 'Name': 'Fred Flintstone' }")]
        [DataRow("element3", "element1", "{ 'automobile': { 'Vehicle': 'Chevy Silverado', 'Year': 1964, 'Driver': 'Davey Carson' } }")]
        public async Task CallElement_succeeds(string transform, string data, string expected)
        {
            var result = await TransformerTest.Test("CallElement." + transform, "Element." + data);

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(result)));
        }

        [TestMethod]
        [DataRow("element4", "element4")]
        public async Task CallElement_succeeds2(string transform, string data)
        {
            var result   = await TransformerTest.Test("CallElement." + transform, "Element." + data);
            var expected = await TransformerTest.LoadSample("Element." + data + "_expected2");

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(result)));
        }
    }
}