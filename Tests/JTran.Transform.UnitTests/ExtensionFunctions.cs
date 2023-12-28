using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("ExtensionFunctions")]
    public class ExtensionFunctionTests
    {
        [TestMethod]
        [DataRow("extensionfunction",  "automobiles")]
        public async Task ExtensionFunctions_success(string transform, string data)
        {
            var result = await TransformerTest.Test(transform, data, new object[] { new ExtFunctions() } );
            var json = JObject.Parse(result)!;

            Assert.AreEqual("xCamaro",   json["Owner"]!["Cars"]!["Chevy"]!["Model"]!.ToString());
            Assert.AreEqual("xFirebird", json["Owner"]!["Cars"]!["Pontiac"]!["Model"]!.ToString());
            Assert.AreEqual("xCha\\rrger",  json["Owner"]!["Cars"]!["Dodge"]!["Model"]!.ToString());
            Assert.AreEqual("yGreen",    json["Owner"]!["Cars"]!["Chevy"]!["Color"]!.ToString());
            Assert.AreEqual("yBlue",     json["Owner"]!["Cars"]!["Pontiac"]!["Color"]!.ToString());
            Assert.AreEqual("yBlack",    json["Owner"]!["Cars"]!["Dodge"]!["Color"]!.ToString());
        }

        [TestMethod]
        [DataRow("extensionfunction2",  "automobiles")]
        public async Task ExtensionFunctions_throws(string transform, string data)
        {
            var ex = await Assert.ThrowsExceptionAsync<TargetInvocationException>( async ()=> await TransformerTest.Test(transform, data, new object[] { new ExtFunctions() } ));

            Assert.IsTrue(ex.Message.Contains("badmama"));
        }

        #region Private 

        private class ExtFunctions
        {
            public string addx(string val)     { return "x" + val; }
            public string addy(string val)     { return "y" + val; }
            public string badmama(string val)  { throw new Exception("boink!"); }
        }

        #endregion
    }
}