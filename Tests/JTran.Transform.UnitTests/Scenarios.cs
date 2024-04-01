
using JTran.Common;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Scenarios")]
    public class ScenarioTests
    {
        // Don't run this in non-debug environment. 1. It outputs to files and 2. They're huge files
      #if DEBUG

        [TestMethod]
        [DataRow("customerorders", 23, 17)]
        //[DataRow("customerorders", 185, 77)]
       // [DataRow("customerorders", 245, 157)]
        public async Task Scenario_SalesReports(string dataGenerator, int numCustomers, int numOrders)
        {
            // First generate test data
            await CreateData(dataGenerator, numCustomers, numOrders);

            // Generate these reports
            await Test(dataGenerator, "sales_report",           numCustomers, numOrders);
            await Test(dataGenerator, "sales_report_by_month",  numCustomers, numOrders);
            await Test(dataGenerator, "sales_report_by_city",   numCustomers, numOrders);
            await Test(dataGenerator, "sales_report_by_state",  numCustomers, numOrders);
        }

      #endif

        // Generate test data
        private async Task Test(string dataGenerator, string transform, int numCustomers, int numOrders)
        {
            using var input  = File.OpenRead($"c:\\Development\\Testing\\JTran\\{dataGenerator}_{numCustomers}_{numOrders}.json");
            using var output = File.Create($"c:\\Development\\Testing\\JTran\\{transform}_{numCustomers}_{numOrders}.json", 64 * 1024, FileOptions.SequentialScan);

            // Then transform
            await TransformerTest.TestData("Scenarios." + transform, input, output);
        }

        // Generate test data
        private async Task CreateData(string dataGenerator, int numCustomers, int numOrders)
        {
            var context = new TransformerContext() { Arguments = (new { NumCustomers = numCustomers, NumOrders = numOrders}).ToDictionary() };

            using var output = File.Create($"c:\\Development\\Testing\\JTran\\{dataGenerator}_{numCustomers}_{numOrders}.json", 64 * 1024, FileOptions.SequentialScan);

            await TransformerTest.TestData("Scenarios." + dataGenerator, 
                                                  "{ 'hello': 'hello' }", 
                                                  output,
                                                  extFunctions: new List<object> { new JTran.Random.RandomExtensions() },
                                                  context: context);
        }
    }
}