using System.IO;
using System.Text;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;
using JTran.Json;
using Newtonsoft.Json;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonParser")]
    public class JsonParserTests
    {
        [TestMethod]
        [DataRow("singlecustomer")]
        public void JsonParser_Parse_success(string fileName)
        {
            var data       = LoadSample(fileName);
            var parser     = new Json.Parser();
            using var strm = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var result     = parser.Parse(strm);

            Assert.IsNotNull(result);

            var json       = result.ToJson();
            var customer = JsonConvert.DeserializeObject<Customer>(json);

            Assert.IsNotNull(customer);
            Assert.AreEqual("Smith", customer.LastName);
        }

        #region Private 

        private static string LoadSample(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.UnitTests.JsonParser.Samples.{name}.json");
            
            return stream!.ReadString();
        }

        private class Customer
        {
            public string LastName  { get; set; } = "";
            public string FirstName { get; set; } = "";
            public int    Age       { get; set; }
            public string Address   { get; set; } = "";
        } 

        private class CustomerContainer
        {
            public string          SpecialCustomer    { get; set; } = "";
            public List<Customer>? Customers          { get; set; }
        } 

        #endregion
    }
}
