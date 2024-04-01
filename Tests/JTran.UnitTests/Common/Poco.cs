using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Poco")]
    public class PocoTests
    {
        [TestMethod]
        public void Poco_ToDictionary()
        {
            var car = new Automobile
            {
                Make  = "Chevy",
                Model = "Camaro",
                Year  = 1969,
                Color = "Blue",
            };

            var poco = Poco.FromObject(car);
            var dict = poco.ToDictionary(car)!;

            Assert.AreEqual("Chevy",    dict["Make"].ToString());
            Assert.AreEqual("Camaro",   dict["Model"].ToString());
            Assert.AreEqual(1969,       int.Parse(dict["Year"]!.ToString()!));
            Assert.AreEqual("Blue",     dict["Color"].ToString());

            var car2 = new Automobile
            {
                Make  = "Pontiac",
                Model = "GTO",
                Year  = 1970,
                Color = "Black",
            };

            var poco1 = Poco.FromObject(car2);
            dict = poco1.ToDictionary(car2)!;

            Assert.AreEqual("Pontiac",    dict["Make"].ToString());
            Assert.AreEqual("GTO",        dict["Model"].ToString());
            Assert.AreEqual(1970,         int.Parse(dict["Year"]!.ToString()!));
            Assert.AreEqual("Black",      dict["Color"].ToString());
        }
    }
}
