
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("CopyOf")]
    public class CopyOfTests
    {
        [TestMethod]
        [DataRow("expression",  "expression")]
        public async Task CopyOf_expression(string transform, string data)
        {
            var result  = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            var cars = jresult["Cars"] as JArray;

            Assert.IsNotNull(cars);
            Assert.AreEqual(2, cars.Count);
        }

        [TestMethod]
        [DataRow("copyof1", "copyof1")]
        public async Task CopyOf_success(string transform, string data)
        {
            var result  = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);
            Assert.IsNotNull(jresult["Invoice"]);

            Assert.AreEqual(945, jresult.PathValue<int>("Invoice.Muffler"));
            Assert.AreEqual(123, jresult.PathValue<int>("Invoice.Sparkplugs"));
            Assert.AreEqual(77,  jresult.PathValue<int>("Invoice.Solenoid"));
        }

        [TestMethod]
        [DataRow("copyof2", "copyof2")]
        public async Task CopyOf_success_2(string transform, string data)
        {
            var result  = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);
            Assert.IsNotNull(jresult["Vehicles"]);

            Assert.AreEqual("Chevy", jresult["Vehicles"]![0]!["Stuff"]!["Make"]);
        }

        [TestMethod]
        [DataRow("empty", "empty")]
        [DataRow("empty2", "empty")]
        public async Task CopyOf_empty(string transform, string data)
        {
            var result  = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            var invoice = (jresult["Invoice"]! as JObject)!;

            Assert.AreEqual("Chevy",    jresult["Brand"]?.ToString());
            Assert.AreEqual("Corvette", jresult["Model"]?.ToString());
            Assert.AreEqual(1964,       int.Parse(jresult["Year"]?.ToString()));
            Assert.AreEqual("Green",    jresult["Color"]?.ToString());
            Assert.IsNull(invoice?.ToString());
        }

        [TestMethod]
        [DataRow("copyof3", "copyof3")]
        public async Task CopyOf_success_3(string transform, string data)
        {
            var result  = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);
            Assert.IsNotNull(jresult["output"]);

            var val = jresult!["output"]!["modified"]!.Value<string>()!;

            var dtCheck = DateTimeOffset.Parse("2016-07-04T17:00:00.0000000-07:00");
            var dtResult = DateTimeOffset.Parse(val);

            Assert.AreEqual(dtCheck, dtResult);
        }

        [TestMethod]
        [DataRow("array_noobject", "array_noobject")]
        public async Task CopyOf_array_noobject(string transform, string data)
        {
            var result  = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            var cars = JsonConvert.DeserializeObject<CarsContainer>(result);

            Assert.AreEqual(3, cars!.Cars!.Count);
            Assert.AreEqual("Chevy",           cars.Cars[0].Make);
            Assert.AreEqual("Corvette",        cars.Cars[0].Model);
            Assert.AreEqual(1964,              cars.Cars[0].Year);
            Assert.AreEqual("Blue",            cars.Cars[0].Color);
            Assert.AreEqual("Fred Flintstone", cars.Cars[0].Driver);
        }

        [TestMethod]
        [DataRow("array_noobject2", "array_noobject2")]
        public async Task CopyOf_array_noobject2(string transform, string data)
        {
            var result = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var array  = JArray.Parse(result);

            Assert.IsNotNull(array);

            var cars = JsonConvert.DeserializeObject< List<Automobile4>>(result);

            Assert.AreEqual(3, cars!.Count);
            Assert.AreEqual("Chevy",           cars[0].Make);
            Assert.AreEqual("Corvette",        cars[0].Model);
            Assert.AreEqual(1964,              cars[0].Year);
            Assert.AreEqual("Blue",            cars[0].Color);
            Assert.AreEqual("Fred Flintstone", cars[0].Driver);
            Assert.AreEqual(396,     cars[0].Engine!.Displacement);
            Assert.AreEqual(8,       cars[0].Engine!.Cylinders);
            Assert.AreEqual("Holly", cars[0].Engine!.Carburation);
        }

        [TestMethod]
        [DataRow("array_noobject2", "array_noobject2")]
        public async Task CopyOf_list(string transform, string data)
        {
            var list = new List<Automobile>() 
            {
                new Automobile { Make = "Chevy",   Model = "Corvette", Year = 1964, Color = "Blue" },
                new Automobile { Make = "Pontiac", Model = "GTO",      Year = 1970, Color = "Black" }, 
                new Automobile { Make = "Audi",    Model = "RS5",      Year = 2024, Color = "Green" } 
            };

            var result = await TransformerTest.TestList("CopyOf." + data, list);
            var array  = JArray.Parse(result);

            Assert.IsNotNull(array);

            var cars = JsonConvert.DeserializeObject< List<Automobile3>>(result);

            Assert.AreEqual(3, cars!.Count);
            Assert.AreEqual("Chevy",           cars[0].Make);
            Assert.AreEqual("Corvette",        cars[0].Model);
            Assert.AreEqual(1964,              cars[0].Year);
            Assert.AreEqual("Blue",            cars[0].Color);
            Assert.AreEqual("Fred Flintstone", cars[0].Driver);
        }

        [TestMethod]
        [DataRow("array_noobject2", "array_noobject3")]
        public async Task CopyOf_array_noobject4(string transform, string data)
        {
            var result = await TransformerTest.Test("CopyOf." + transform, "CopyOf." + data);
            var array  = JArray.Parse(result);

            Assert.IsNotNull(array);

            var cars = JsonConvert.DeserializeObject< List<Automobile5>>(result);

            Assert.AreEqual(3, cars!.Count);
            Assert.AreEqual("Chevy",           cars[0].Make);
            Assert.AreEqual("Corvette",        cars[0].Model);
            Assert.AreEqual(1964,              cars[0].Year);
            Assert.AreEqual("Blue",            cars[0].Color);
            Assert.AreEqual("Fred Flintstone", cars[0].Driver);

            Assert.AreEqual(2, cars[0].Engines!.Count);
            Assert.AreEqual(396,     cars![0].Engines![0]!.Displacement);
            Assert.AreEqual(8,       cars![0].Engines![0]!.Cylinders);
            Assert.AreEqual("Holly", cars![0].Engines![0]!.Carburation);
        }
   }
}