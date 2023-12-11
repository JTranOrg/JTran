using System.IO;
using System.Text;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Performance")]
    public class PerformanceTests
    {
        #region Performance Tests

        [TestMethod]
        [TestCategory("Legacy")]
        [DataRow("customers")]
        public void LegacyParser_Parse_success(string fileName)
        {
            var data       = LoadSample(fileName);
            var parser     = new Json.LegacyParser();
            using var strm = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var result     = parser.Parse(strm);

            Assert.IsNotNull(result);
        }

        #endregion

        #region Private 

        private static string LoadSample(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.UnitTests.JsonParser.Samples.{name}.json");
            
            return stream!.ReadString();
        }

        #endregion
    }
}
