
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran.Json;
using Castle.Core.Configuration;

namespace JTran.UnitTests
{
    [TestClass]
    public class JsonObjectExtensionsTests
    {
        [TestMethod]
        public void JsonObjectExtensions_ToJson_Success()
        {
            var exp = _data1.ToJsonObject() as JsonObject;
            var json = exp.ToJson();
            
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(json), JObject.Parse(_data1)));
        }

        [TestMethod]
        public void JsonObjectExtensions_ToJson_date_Success()
        {
            var auto = new Automobile { Make = "Chevy", Model = "Camaro", Sold = DateTime.Parse("2023-12-06").ToString("o") };
            var json1 = JsonConvert.SerializeObject(auto);

            var exp = json1.ToJsonObject() as JsonObject;
            var json = exp.ToJson();
            var auto2 = JsonConvert.DeserializeObject<Automobile>(json);

            Assert.IsNotNull(auto2);

            Assert.AreEqual("Chevy",                auto2.Make);
            Assert.AreEqual("Camaro",               auto2.Model);
            Assert.AreEqual("2023-12-06T00:00:00.0000000",  auto2.Sold);
            Assert.AreEqual(0m,                     auto2.Cost);
            Assert.IsFalse(auto2.Used);
            Assert.IsNull(auto2.Engine);
            Assert.AreEqual(0, auto2.Drivers.Count);
        }

        [TestMethod]
        public void JsonObjectExtensions_ToObject()
        {
            var auto = new Automobile 
            { 
                Make = "Chevy", 
                Model = "Camaro", 
                Sold = DateTime.Parse("2023-12-15T00:00:00").ToString("s"),
                Cost = 14000m,
                Used = true,
                Engine = new Engine
                {
                    Displacement = 350.5,
                    Configuration = Engine.CylinderConfiguration.V,
                    Cylinders = 8,

                },
                Drivers = new List<Driver> 
                { 
                    new Driver
                    {
                        Name = "Alex MackIntosh",
                        Age = 34,
                        Gender = true
                    },
                    new Driver
                    {
                        Name = "Roberta Martinez",
                        Age = 29,
                        Gender = false
                    },
                    new Driver
                    {
                        Name = "Kyle Jones",
                        Age = 37,
                        Gender = true
                    }
                }
            };
            var json1 = JsonConvert.SerializeObject(auto);

            var exp = json1.ToJsonObject() as JsonObject;
            var obj = exp.ToObject<Automobile2>();

            Assert.IsNotNull(obj);

            Assert.AreEqual("Chevy",                obj.Make);
            Assert.AreEqual("Camaro",               obj.Model);
            Assert.AreEqual("2023-12-15T00:00:00",  obj.Sold);
            Assert.AreEqual(14000d,                 obj.Cost);
            Assert.IsTrue(obj.Used);
            Assert.IsNotNull(obj.Engine);
            Assert.AreEqual((int)Engine.CylinderConfiguration.V, (int)obj.Engine.Configuration);
            Assert.AreEqual(8,                      obj.Engine.Cylinders);
            Assert.AreEqual(350,                    obj.Engine.Displacement);
            Assert.IsNotNull(obj.Drivers);
            Assert.AreEqual(3,                      obj.Drivers.Count);
            Assert.AreEqual("Alex MackIntosh",      obj.Drivers[0].Name);
            Assert.AreEqual("Roberta Martinez",     obj.Drivers[1].Name);
            Assert.AreEqual("Kyle Jones",           obj.Drivers[2].Name);
       }
        
        private class Automobile
        {
            public string  Make    { get; set; } = "";
            public string  Model   { get; set; } = "";
            public string  Sold    { get; set; } = "";
            public decimal Cost    { get; set; }
            public bool    Used    { get; set; }
            public Engine? Engine  { get; set; }

            public List<Driver> Drivers { get; set; } = new List<Driver>();
        }

        private class Automobile2
        {
            public string   Make    { get; set; } = "";
            public string   Model   { get; set; } = "";
            public string   Sold    { get; set; } = "";
            public double   Cost    { get; set; }
            public bool     Used    { get; set; }
            public Engine2? Engine  { get; set; }

            public List<Driver2> Drivers { get; set; } = new List<Driver2>();
        }

        private class Engine
        {
            public CylinderConfiguration  Configuration { get; set; } = CylinderConfiguration.Flat;
            public int            Cylinders     { get; set; } 
            public double         Displacement  { get; set; }

            public enum CylinderConfiguration
            {
                V = 1,
                Inline = 2,
                Flat = 3
            }
        }

        private class Engine2
        {
            public CylinderConfiguration  Configuration { get; set; } = CylinderConfiguration.V;
            public int            Cylinders     { get; set; } 
            public int            Displacement  { get; set; }

            public enum CylinderConfiguration
            {
                V = 1,
                Inline = 2,
                Flat = 3
            }
        }

        private class Driver
        {
            public string Name      { get; set; } = "";
            public int    Age       { get; set; }
            public bool   Gender    { get; set; } = true;
        }

        private class Driver2
        {
            public string Name      { get; set; } = "";
            public double Age       { get; set; }
            public bool   Gender    { get; set; } = true;
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
