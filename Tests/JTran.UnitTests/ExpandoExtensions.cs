using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    public class ExpandoExtensionsTests
    {
        [TestMethod]
        public void ExpandoExtensions_ToJson_Success()
        {
            var exp = _data1.JsonToExpando() as ExpandoObject;
            var json = exp.ToJson();
            
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(json), JObject.Parse(_data1)));
        }

        [TestMethod]
        public void ExpandoExtensions_ToJson_date_Success()
        {
            var auto = new Automobile { Make = "Chevy", Model = "Camaro", Sold = DateTime.UtcNow.AddDays(-10).ToString("o") };
            var json1 = JsonConvert.SerializeObject(auto);

            var exp = json1.JsonToExpando() as ExpandoObject;
            var json = exp.ToJson();

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(json1), JObject.Parse(json)));
        }
        
        private class Automobile
        {
            public string Make  { get; set; }
            public string Model { get; set; }
            public string Sold  { get; set; }
        }

        private static string _data1 =
        @"{
            FirstName: 'John',
            LastName:  'Smith',
            Car:   
            {
               Make:   'Chevy',
               Model:  'Corvette',
               Year:   1964,
               Color:  'Blue',
               Engine:
               {
                 Displacement:   375
               },
               ServiceCalls:
               [
                 {
                   Description: 'Change sparkplugs',
                   Estimate:    100
                 },
                 {
                   Description: 'Tune up',
                   Estimate:    200,
                   Invoice:     210.79
                 }
               ]
            }
        }";

    }
}
