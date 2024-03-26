using System.IO;
using System.Text;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran.Common;
using JTran.Json;

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
        [DataRow("booleanarray")]
        public void JsonParser_Parse_array_non_object_values(string fileName)
        {
            var json = TestParse(fileName);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);

            var array = jobj["Cars"] as JArray;

            Assert.IsNotNull(array);
            Assert.AreEqual(3, array!.Count);
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

        [TestMethod]
        [DataRow("empties")]
        public void JsonParser_Parse_JTran(string fileName)
        {
            var json = TestParse(fileName, true);
            var jobj = JObject.Parse(json);

            Assert.IsNotNull(jobj);
        }

        #region Parse Errors

        [TestMethod]
        [DataRow("singlecustomer_invalidescape", 8)]
        public void JsonParser_Parse_error(string fileName, int lineNumber)
        {
            var data       = LoadSample(fileName);
            using var parser     = new Json.Parser(new JsonModelBuilder());
            using var strm = new MemoryStream(Encoding.UTF8.GetBytes(data));
            
            var ex = Assert.ThrowsException<JsonParseException>( ()=> parser.Parse(strm) );

            Assert.IsNotNull(ex);
            Assert.AreEqual(lineNumber, ex.LineNumber);
            Assert.AreEqual(lineNumber.ToString(), ex.Data["LineNumber"]);
        }

        [TestMethod]
        [DataRow("array_not_supported")]
        public void JsonParser_Parse_array(string fileName)
        {
            var data         = LoadSample(fileName);
            using var parser = new Json.Parser(new JsonModelBuilder());
            using var strm   = new MemoryStream(Encoding.UTF8.GetBytes(data));
            
            var result = parser.Parse(strm);

           Assert.IsNotNull(result);
           Assert.IsTrue(result is IEnumerable<object>);
        }

        [TestMethod]
        [DataRow("not_json_at_all")]
        public void JsonParser_Parse_invalid_json_files(string fileName)
        {
            var data       = LoadSample(fileName);
            using var parser     = new Json.Parser(new JsonModelBuilder());
            using var strm = new MemoryStream(Encoding.UTF8.GetBytes(data));
            
            var ex = Assert.ThrowsException<JsonParseException>( ()=> parser.Parse(strm) );

           Assert.AreEqual(1, ex.LineNumber);
           Assert.AreEqual("1", ex.Data["LineNumber"]);
           Assert.IsTrue(ex.Message.StartsWith("Invalid json"));
        }

        [TestMethod]
        [DataRow("missing_quotes", 2)]
        [DataRow("missing_quotes2", 9)]
        [DataRow("missing_quotes3", 10)]
        [DataRow("missing_quotes4", 2)]
        public void JsonParser_Parse_missing_quotes(string fileName, long lineNumber)
        {
            var data   = LoadSample(fileName);
            using var parser = new Json.Parser(new JsonModelBuilder());
            
            var ex = Assert.ThrowsException<JsonParseException>( ()=> parser.Parse(data) );

            Assert.IsNotNull(ex);
            Assert.AreEqual(lineNumber, ex.LineNumber);
            Assert.AreEqual(lineNumber.ToString(), ex.Data["LineNumber"]);
            Assert.IsTrue(ex.Message.StartsWith("Missing end quotes"));
        }

        [TestMethod]
        [DataRow("missing_brace", 6)]
        [DataRow("missing_comma", 7)]
        public void JsonParser_Parse_unexpected_token(string fileName, long lineNumber)
        {
            var data   = LoadSample(fileName);
            using var parser = new Json.Parser(new JsonModelBuilder());
            
            var ex = Assert.ThrowsException<JsonParseException>( ()=> parser.Parse(data) );

            Assert.IsNotNull(ex);
            Assert.AreEqual(lineNumber, ex.LineNumber);
            Assert.AreEqual(lineNumber.ToString(), ex.Data["LineNumber"]);
            Assert.IsTrue(ex.Message.StartsWith("Unexpected token"));
        }

        #endregion

        #region Private 

        private string TestParse(string fileName, bool jtran = false)
        {
            using var parser = new Json.Parser(new JsonModelBuilder());
            var data         = LoadSample(fileName, jtran);
            var result       = parser.Parse(data) as JsonObject;

            Assert.IsNotNull(result);

            var json = result.ToJson();

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            return json;
        }

        private static string LoadSample(string name, bool jtran = false)
        {
            var ext = jtran ? "jtran" : "json";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.UnitTests.JsonParser.Samples.{name}.{ext}");
            
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
