using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Include")]
    public class IncludeTests
    {
        [TestMethod]
        [DataRow("include",  "include")]
        public async Task Include_success(string transform, string data)
        {
            var includeFile = await TransformerTest.LoadTransform("Include.otherfile");
            var result      = await TransformerTest.Test("Include." + transform, "Include." + data, includeSource: new Dictionary<string, string> { { "otherfile.jtran", includeFile } });

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob\", Year: 1965 }"), JObject.Parse(result)));
        }

        [TestMethod]
        [DataRow("include2",  "include")]
        public async Task Include2_success(string transform, string data)
        {
            var includeFile = await TransformerTest.LoadTransform("Include.otherfile");
            var include2File = await TransformerTest.LoadTransform("Include.otherfile2");
            var result      = await TransformerTest.Test("Include." + transform, "Include." + data, includeSource: new Dictionary<string, string> { { "otherfile.jtran", includeFile }, { "otherfile2.jtran", include2File } });

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob_blah\", Year: 1965 }"), JObject.Parse(result)));
        }

    }
}