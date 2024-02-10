using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Variables")]
    public class VariableTests
    {
        [TestMethod]
        [DataRow("variable", "customers")]
        public async Task Simple_success(string transform, string data)
        {
            var result    = await TransformerTest.Test("Variables." + transform, data);
            var customers = JObject.Parse(result.ToString())["Customers"] as JArray;

             Assert.AreEqual("Acme Widgets",  customers![0]["Company"]!.ToString());
             Assert.AreEqual("Smith",         customers[0]["Surname"]!.ToString());
        }

    }
}