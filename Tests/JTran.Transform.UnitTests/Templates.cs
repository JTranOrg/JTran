
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Templates")]
    public class TemplatesTests
    {
        [TestMethod]
        [DataRow("after", "after")]
        [DataRow("after2", "after")]
        [DataRow("aselement", "after")]
        public async Task Templates_after2(string transform, string data)
        {
            var result = await Test(transform, data);
            var expected = "{ customer: { FirstName: \"bob\", Year: 1965 } }";

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(result)));
        }

        #region Private

        public async Task<string> Test(string transform, string data)
        {
            var result = await TransformerTest.Test("Templates." + transform, "Templates." + data);           
            var jobj   = JObject.Parse(result)!;

            Assert.IsNotNull(jobj);

            return result;
        }

        #endregion
    }
}