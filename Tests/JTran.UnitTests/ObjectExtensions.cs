using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Extensions;
using JTran.Json;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq;
using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        [TestMethod]
        public void ObjectExtensions_GetValue_Success()
        {
            var obj = _data1.ToJsonObject();

            Assert.AreEqual("John",  obj.GetValue("FirstName", null).ToString());
            Assert.AreEqual("Chevy", obj.GetValue("Car.Make", null).ToString());
            Assert.AreEqual(375m,    obj.GetValue("Car.Engine.Displacement", null));
            Assert.AreEqual(210.79M, Convert.ToDecimal(obj.GetSingleValue("Car.ServiceCalls.Invoice", null)));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_Fail()
        {
            var obj = _data1.ToJsonObject();

            Assert.IsNull(obj.GetValue("Car.DontHaveThisProp", null));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_wGGParent_Success()
        {
            var obj = _datagg1.ToJsonObject();
            var driver = obj.GetValue("parent.Driver", null);

            Assert.AreEqual("Talahooga Race Night", driver.GetValue("/Name", null).ToString());
            Assert.AreEqual("January Events", driver.GetValue("//Name", null).ToString());
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_var_Success()
        {
            var obj = _data1.ToJsonObject();

            Assert.AreEqual("Bob", obj.GetValue("$EventCoordinator", new ExpressionContext(null, "", new TransformerContext { Arguments = new Dictionary<string, object> { {"EventCoordinator", "Bob" }}})));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_varObject_Success()
        {
            var obj = _data1.ToJsonObject();

            Assert.AreEqual("226-555-1212", obj.GetValue("$EventCoordinator.Phone", new ExpressionContext(null, "", new TransformerContext { Arguments = new Dictionary<string, object> { {"EventCoordinator", new {Phone = "226-555-1212"} }}})));
        }

        [TestMethod]
        public void ObjectExtensions_GroupKey_Success()
        {
            var obj = new { Make = "Chevy", Model = "Corvette", Year = 1956 };
            var ex  = obj.GetGroupByKey(new [] { "Make", "Model" });

            Assert.AreEqual("Chevy", ex["Make"]);
            Assert.AreEqual("Corvette", ex["Model"]);
            Assert.ThrowsException<KeyNotFoundException>(()=> ex["Year"]);
        }

        public class Automobile
        {
            public string Make  { get; set; }
            public string Model { get; set; }
        }

        [TestMethod]
        public void ObjectExtensions_EnsureEnumerable_Success()
        {
            var obj = new Automobile { Make = "Chevy", Model = "Corvette" };
            var enm = obj.EnsureEnumerable();
            var t = enm.GetType();
            var list = new List<Automobile>(enm);

            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void EnumerableExtensions_IsSingle_Success()
        {
            var list1 = Array.Empty<object>();
            var list2 = new [] { "bob" };
            var list3 = new [] { "bob", "fred" };

            Assert.IsFalse(list1.IsSingle());
            Assert.IsTrue(list2.IsSingle());
            Assert.IsFalse(list3.IsSingle());
        }

        #region Data
        
        private static readonly string _data1 =
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
                   Scheduled:   '2020-01-01T08:00:00',
                   Estimate:    100.0
                 },
                 {
                   Description: 'Tune up',
                   Scheduled:   '2020-03-01T08:00:00',
                   Estimate:    200.0,
                   Invoice:     210.79
                 }
               ]
            }
        }";

        private static readonly string _datagg1 =
        @"{
            Name: 'January Events',
            parent:
            {
                Name: 'Talahooga Race Night',
                Driver: 
                {
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
                           Scheduled:   '2020-01-01T08:00:00',
                           Estimate:    100.0
                         },
                         {
                           Description: 'Tune up',
                           Scheduled:   '2020-03-01T08:00:00',
                           Estimate:    200.0,
                           Invoice:     210.79
                         }
                       ]
                    }
                }
            }
        }";

        #endregion
    }
}
