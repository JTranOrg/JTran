using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.ExceptionServices;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Scenarios")]
    public class ScenarioTests
    {
        [TestMethod]
        [DataRow("customerorders", "sales_report")]
        [DataRow("customerorders", "sales_report_by_month")]
        public async Task Scenario_success(string dataGenerator, string transform)
        {
            // First generate test data
            var data = await TransformerTest.TestData("Scenarios." + dataGenerator, "{ 'hello': 'hello' }", extFunctions: new List<object> { new JTran.Random.RandomExtensions() } );

            // Then transform
            var result = await TransformerTest.TestData("Scenarios." + transform, data);

            Assert.IsNotNull(result);
        }
    }
}