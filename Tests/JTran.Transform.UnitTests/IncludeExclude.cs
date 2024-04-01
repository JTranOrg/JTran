
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("IncludeExclude")]
    public class IncludeExcludeTests
    {
        [TestMethod]
        [DataRow("include", "soldiers", "include_expected")]
        [DataRow("include2", "soldiers", "include_expected")]
        public async Task Include_success(string transform, string data, string expectedFile)
        {
            var result   = await Test(transform, data);
            var expected = await TransformerTest.LoadSample("IncludeExclude." + expectedFile);

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(result)));
        }

        [TestMethod]
        [DataRow("exclude",  "soldiers", "exclude_expected")]
        [DataRow("exclude2", "soldiers", "exclude_expected")]
        public async Task Exclude_success(string transform, string data, string expectedFile)
        {
            var result   = await Test(transform, data);
            var expected = await TransformerTest.LoadSample("IncludeExclude." + expectedFile);

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(result)));
        }

        #region Private

        private async Task<string> Test(string transform, string data)
        {
            var result = await TransformerTest.Test("IncludeExclude." + transform, "IncludeExclude." + data);           
            var jobj   = JObject.Parse(result)!;
            
            Assert.IsNotNull(jobj);

            return result;
        }

        #endregion
    }
}