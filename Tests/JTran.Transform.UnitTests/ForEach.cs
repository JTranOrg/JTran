using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("ForEach")]
    public class ForEachTests
    {
        [TestMethod]
        [DataRow("foreach",  "customers2", true)]
        [DataRow("foreach2", "customers3", false)]
        public async Task ForEach_success(string transform, string data, bool specialCustomer)
        {
            var result = await TransformerTest.Test(transform,data);

            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            if(specialCustomer)
                Assert.AreEqual("Linda",     customers!.SpecialCustomer);

            Assert.AreEqual(4,               customers!.Customers!.Count);
            Assert.AreEqual("John",          customers.Customers[0].FirstName);
            Assert.AreEqual("Smith",         customers.Customers[0].LastName);
            Assert.AreEqual(34,              customers.Customers[0].Age);
            Assert.AreEqual("123 Elm St",    customers.Customers[0].Address);
                                             
            Assert.AreEqual("Mary",          customers.Customers[1].FirstName);
            Assert.AreEqual("Smith",         customers.Customers[1].LastName);
            Assert.AreEqual(32,              customers.Customers[1].Age);
            Assert.AreEqual("123 Elm St",    customers.Customers[1].Address);
                                             
            Assert.AreEqual("Fred",          customers.Customers[2].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[2].LastName);
            Assert.AreEqual(41,              customers.Customers[2].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[2].Address);
                                             
            Assert.AreEqual("Linda",         customers.Customers[3].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[3].LastName);
            Assert.AreEqual(39,              customers.Customers[3].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[3].Address);
        }

        [TestMethod]
        [DataRow("foreach3", "customers4")]
        public async Task ForEach_where_clause_success(string transform, string data)
        {
            var result = await TransformerTest.Test(transform,data);

            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            Assert.AreEqual(2,               customers!.Customers!.Count);
                                            
            Assert.AreEqual("Fred",          customers.Customers[0].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[0].LastName);
            Assert.AreEqual(41,              customers.Customers[0].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[0].Address);
                                             
            Assert.AreEqual("Linda",         customers.Customers[1].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[1].LastName);
            Assert.AreEqual(39,              customers.Customers[1].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[1].Address);
        }

        [TestMethod]
        public async Task ForEach_list()
        {
           var list = new List<Automobile>
            {
                new Automobile
                {
                    Make  = "Chevrolet",
                    Model = "Corvette",
                    Year  = 1956,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Pontiac",
                    Model = "Firebird",
                    Year  = 1969,
                    Color = "Green",
                },
                new Automobile
                {
                    Make  = "Chevrolet",
                    Model = "Camaro",
                    Year  = 1970,
                    Color = "Black",
                }
            };

            var result = await TransformerTest.TestList("list", list, "Automobiles");
            var owner = JsonConvert.DeserializeObject<Owner2>(result);

            Assert.IsNotNull(owner);
            Assert.IsNotNull(owner.Cars);
            Assert.AreEqual(3, owner.Cars.Count);

            Assert.AreEqual("Chevrolet", owner.Cars[0].Brand);
            Assert.AreEqual("Corvette",  owner.Cars[0].Model);
            Assert.AreEqual(1956,        owner.Cars[0].Year);

            Assert.AreEqual("Pontiac",   owner.Cars[1].Brand);
            Assert.AreEqual("Firebird",  owner.Cars[1].Model);
            Assert.AreEqual(1969,        owner.Cars[1].Year);

            Assert.AreEqual("Chevrolet", owner.Cars[2].Brand);
            Assert.AreEqual("Camaro",    owner.Cars[2].Model);
            Assert.AreEqual(1970,        owner.Cars[2].Year);
        }

        [TestMethod]
        public async Task ForEach_break_success()
        {
            var result = await TransformerTest.Test("foreachbreak", "customers");
            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            Assert.AreEqual(1,       customers!.Customers!.Count);
            Assert.AreEqual("John",  customers!.Customers[0].FirstName);
            Assert.AreEqual("Smith", customers!.Customers[0].LastName);
        }
    }
}