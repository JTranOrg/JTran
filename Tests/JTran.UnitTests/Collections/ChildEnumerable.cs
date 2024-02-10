
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.UnitTests
{
    [TestClass]
    public class ChildEnumerableTests
    {
        private static IEnumerable<Car> _cars = new List<Car> 
        { 
            new Car 
            { 
                Make = "Chevy",
                Model = "Chevelle",
                Drivers = null
            },
            new Car 
            { 
                Make = "Chevy",
                Model = "Camaro",
                Drivers = new []
                {
                    new Person { FirstName = "Joe", LastName = "Lopez" },
                    new Person { FirstName = "Linda", LastName = "Anderson" }
                }
            },
            new Car 
            { 
                Make = "Pontiac",
                Model = "GTO",
                Drivers = new []
                {
                    new Person { FirstName = "Fred", LastName = "Flintstone" },
                    new Person { FirstName = "Barney", LastName = "Rubble" }
                }
            },
            new Car 
            { 
                Make = "Audi",
                Model = "RS5",
                Drivers = new List<Person>()
            },
            new Car 
            { 
                Make = "Dodge",
                Model = "Charger",
                Drivers = new []
                {
                    new Person { FirstName = "Frank", LastName = "Enstein" },
                    new Person { FirstName = "Mary", LastName = "Shelly" }
                }
            }
        };

        private static IEnumerable<object> _carObjects = new List<object> 
        { 
            new CarObject 
            { 
                Make = "Chevy",
                Model = "Chevelle",
                Drivers = null
            },
            new CarObject 
            { 
                Make = "Chevy",
                Model = "Camaro",
                Drivers = new []
                {
                    new Person { FirstName = "Joe", LastName = "Lopez" },
                    new Person { FirstName = "Linda", LastName = "Anderson" }
                }
            },
            new CarObject 
            { 
                Make = "Pontiac",
                Model = "GTO",
                Drivers = new []
                {
                    new Person { FirstName = "Fred", LastName = "Flintstone" },
                    new Person { FirstName = "Barney", LastName = "Rubble" }
                }
            },
            new CarObject 
            { 
                Make = "Audi",
                Model = "RS5",
                Drivers = new List<Person>()
            },
            new CarObject 
            { 
                Make = "Dodge",
                Model = "Charger",
                Drivers = new []
                {
                    new Person { FirstName = "Frank", LastName = "Enstein" },
                    new Person { FirstName = "Mary", LastName = "Shelly" }
                }
            }
        };

        [TestMethod]
        public void ChildEnumerable_success()
        {
            var flattened  = new ChildEnumerable<Car, Person>(_cars, "Drivers");
            var result     = new List<Person>(flattened);

            Assert.AreEqual(6, result.Count);

            Assert.AreEqual("Joe", result[0].FirstName);
        }

        [TestMethod]
        public void ChildEnumerable_object_success()
        {
            var flattened  = new ChildEnumerable<object, object>(_carObjects, "Drivers");
            var result     = new List<object>(flattened);
            var json       = JsonConvert.SerializeObject(result);
            var array      = JArray.Parse(json);

            Assert.AreEqual(6, array.Count);

            Assert.AreEqual("Joe", array[0]["FirstName"]);
        }

        [TestMethod]
        public void ChildEnumerable_property_success()
        {
            var drivers   = new ChildEnumerable<object, object>(_carObjects, "Drivers");
            var firstNames  = new ChildEnumerable<object, object>(drivers, "FirstName");
            var result     = new List<object>(firstNames);
            var json       = JsonConvert.SerializeObject(result);
            var array      = JArray.Parse(json);

            Assert.AreEqual(6, array.Count);

            Assert.AreEqual("Joe", array[0]);
        }

        internal class Car
        {
            public string              Make    { get; set; } = "";
            public string              Model   { get; set; } = "";
            public IEnumerable<Person> Drivers { get; set; }
        }

        internal class CarObject
        {
            public string              Make    { get; set; } = "";
            public string              Model   { get; set; } = "";
            public IEnumerable<object> Drivers { get; set; }
        }

        internal class Person
        {
            public string FirstName { get; set; } = "";
            public string LastName { get; set; }  = "";

        }
    }
}
