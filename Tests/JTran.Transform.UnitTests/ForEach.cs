using System.Collections;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        [DataRow("explicit_array", "customers4")]
        [DataRow("explicit_array2", "customers4")]
        public async Task ForEach_explicit_array(string transform, string data)
        {
            var result = await TransformerTest.Test("ForEach." + transform, data);

            var jobj = JObject.Parse(result);
            var cars = jobj["Cars"] as JArray;

            Assert.AreEqual(3, cars!.Count);
            Assert.AreEqual("Chevy",   cars[0]!["Make"]!.ToString());
            Assert.AreEqual("Pontiac", cars[1]!["Make"]!.ToString());
            Assert.AreEqual("Audi",    cars[2]!["Make"]!.ToString());
        }

        [TestMethod]
        [DataRow("foreach4", "customers")]
        public async Task ForEach_simple_array(string transform, string data)
        {
            var result = await TransformerTest.Test("ForEach." + transform, data);

            var jobj = JObject.Parse(result);
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

        internal class TestEnumerable : IEnumerable
        {
            private readonly List<object> _list = new List<object>();

            internal TestEnumerable()
            {
            }

            internal void Add(object value) 
            {
                _list.Add(value);
            }

            public IEnumerator GetEnumerator()
            {
                return new TestEnumerator(_list.GetEnumerator());
            }

            private class TestEnumerator : IEnumerator
            {
                private readonly IEnumerator _enumerator;

                internal TestEnumerator(IEnumerator enumerator)
                {
                    _enumerator = enumerator;
                }

                public object Current => _enumerator.Current;

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    _enumerator.Reset();
                }
            }
        }

        [TestMethod]
        public async Task ForEach_array()
        {
           var list = new TestEnumerable();

           list.Add(new Automobile
                    {
                        Make  = "Chevrolet",
                        Model = "Corvette",
                        Year  = 1956,
                        Color = "Blue"
                    });

           list.Add(new Automobile
                {
                    Make  = "Pontiac",
                    Model = "Firebird",
                    Year  = 1969,
                    Color = "Green"
                    });

           list.Add(new Automobile
                {
                    Make  = "Chevrolet",
                    Model = "Camaro",
                    Year  = 1970,
                    Color = "Black"
                    });

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

        [TestMethod]
        [DataRow("nested", "nested")]
        public async Task Functions_nested(string transform, string data)
        {
            var result = await TransformerTest.Test("ForEach." + transform, "ForEach." + data);
            
            _ = JObject.Parse(result);

            var roster = JsonConvert.DeserializeObject<Roster>(result);

            Assert.AreEqual(3,               roster?.Owner?.Cars.Count);
            Assert.AreEqual("Chevy",         roster?.Owner?.Cars[0].Make);
            Assert.AreEqual("Camaro",        roster?.Owner?.Cars[0].Model);
            Assert.AreEqual(3,               roster?.Owner?.Cars[0].Mechanics.Count);
            Assert.AreEqual("Bob",           roster?.Owner?.Cars[0].Mechanics[0].FirstName);
            Assert.AreEqual("Mendez",        roster?.Owner?.Cars[0].Mechanics[0].LastName);
        }
    }
}