using System.IO;
using System.Text;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran.Common;
using JTran.Json;
using System.Dynamic;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonParser")]
    public class JsonParserTests
    {
        [TestMethod]
        [DataRow("simplesinglecustomer", false, "")]
        [DataRow("singlecustomer", true, "123 E\\lm's St")]
        public void JsonParser_Parse_success(string fileName, bool address, string street)
        {
            var json     = TestParse(fileName);
            var customer = JsonConvert.DeserializeObject<Customer>(json);

            Assert.IsNotNull(customer);
            Assert.AreEqual("Smith", customer.LastName);
            Assert.AreEqual(34, customer.Age);

            if(address)
            { 
                Assert.IsNotNull(customer.Address);
                Assert.AreEqual("CA",               customer.Address!.State);
                Assert.AreEqual(street,             customer.Address!.Street);
                Assert.AreEqual("Spri\"ngfield",    customer.Address!.City);
                Assert.AreEqual("94\"332",          customer.Address!.Zip);
            }
        }

        [TestMethod]
        [DataRow("customers2")]
        public void JsonParser_Parse2_success(string fileName)
        {
            var json = TestParse(fileName);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);
        }

        [TestMethod]
        [DataRow("singlecustomer_unquoted_property_names")]
        public void JsonParser_Parse_unquoted_property_names(string fileName)
        {
            var json     = TestParse(fileName);
            var customer = JsonConvert.DeserializeObject<Customer>(json);

            Assert.IsNotNull(customer);
            Assert.AreEqual("Smith", customer.LastName);
            Assert.AreEqual(34, customer.Age);

            Assert.IsNotNull(customer.Address);
            Assert.AreEqual("CA",               customer.Address!.State);
            Assert.AreEqual("123 Elm's St",     customer.Address!.Street);
            Assert.AreEqual("Springfield",    customer.Address!.City);
            Assert.AreEqual("94332",          customer.Address!.Zip);
        }

        [TestMethod]
        [DataRow("singlecustomerarray")]
        public void JsonParser_Parse_array_success(string fileName)
        {
            var json      = TestParse(fileName);
            var customers = JsonConvert.DeserializeObject<CustomerContainer>(json);

            Assert.IsNotNull(customers);
            Assert.IsNotNull(customers.Customers);
            Assert.AreEqual(1, customers.Customers.Count);
            Assert.AreEqual("Smith", customers.Customers[0].LastName);
            Assert.IsNotNull(customers.Customers[0].Address);
            Assert.AreEqual("CA", customers.Customers[0].Address!.State);
            Assert.AreEqual(34, customers.Customers[0].Age);
        }

        [TestMethod]
        [DataRow("array_unquoted_prop_names")]
        public void JsonParser_Parse_array_unquoted_property_names(string fileName)
        {
            var json = TestParse(fileName);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);

            Assert.IsNotNull(jobj["Cars"] as JArray);
            Assert.AreEqual(3, (jobj["Cars"] as JArray)!.Count);
        }

        [TestMethod]
        [DataRow("stringarray")]
        [DataRow("numberarray")]
        public void JsonParser_Parse_array_non_object_values(string fileName)
        {
            var json = TestParse(fileName);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);

            Assert.IsNotNull(jobj["Cars"] as JArray);
            Assert.AreEqual(3, (jobj["Cars"] as JArray)!.Count);
        }

        [TestMethod]
        [DataRow("array_of_arrays")]
        public void JsonParser_Parse_array_of_arrays(string fileName)
        {
            var json = TestParse(fileName);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);

            var array = jobj["Rows"]  as JArray;
            var array2 = array![0] as JArray;

            Assert.AreEqual(3, array2!.Count);
            Assert.AreEqual(1, array2[0]);
        }

        [TestMethod]
        [DataRow("empties")]
        public void JsonParser_Parse_empties(string fileName)
        {
            var json = TestParse(fileName);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);

            var driver = jobj["Driver"];

            Assert.IsNotNull(driver);

            var mechanics = jobj["Mechanics"];

            Assert.IsNotNull(mechanics);

            var trackNo = jobj["TrackNo"];

            Assert.IsNotNull(trackNo);

            var trackName = jobj["TrackName"];

            Assert.IsNotNull(trackName);
            Assert.AreEqual(0, trackName.ToString().Length);
        }

        #region Parse Errors

        [TestMethod]
        [DataRow("singlecustomer_invalidescape")]
        public void JsonParser_Parse_error(string fileName)
        {
            var data       = LoadSample(fileName);
            var parser     = new Json.Parser();
            using var strm = new MemoryStream(Encoding.UTF8.GetBytes(data));
            
            var ex = Assert.ThrowsException<Json.Parser.ParseError>( ()=> parser.Parse(strm) );

            Assert.IsNotNull(ex);
        }

        #endregion

        #region Private 

        private string TestParse(string fileName)
        {
            var data       = LoadSample(fileName);
            var parser     = new Json.Parser();
            using var strm = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var result     = parser.Parse(strm);

            Assert.IsNotNull(result);

            var json = result.ToJson();

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            return json;
        }

        private static string LoadSample(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.UnitTests.JsonParser.Samples.{name}.json");
            
            return stream!.ReadString();
        }

        private class Customer
        {
            public string  LastName  { get; set; } = "";
            public string  FirstName { get; set; } = "";
            public int     Age       { get; set; }
            public Address Address { get; set; } = new Address();
        } 

        private class Address
        {
            public string Street  { get; set; } = "";
            public string City    { get; set; } = "";
            public string State   { get; set; } = "";
            public string Zip     { get; set; } = "";
        } 

        private class CustomerContainer
        {
            public string?         SpecialCustomer    { get; set; }
            public List<Customer>? Customers          { get; set; }
        } 

        #endregion
    }
}
