using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Extensions;
using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        [TestMethod]
        public void ObjectExtensions_GetValue_Success()
        {
            var obj = _data1.JsonToExpando();

            Assert.AreEqual("John",  obj.GetValue("FirstName", null));
            Assert.AreEqual("Chevy", obj.GetValue("Car.Make", null));
            Assert.AreEqual(375L,    obj.GetValue("Car.Engine.Displacement", null));
            Assert.AreEqual(210.79M, Convert.ToDecimal(obj.GetSingleValue("Car.ServiceCalls.Invoice", null)));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_Fail()
        {
            var obj = _data1.JsonToExpando();

            Assert.IsNull(obj.GetValue("Car.DontHaveThisProp", null));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_wGGParent_Success()
        {
            var obj = _datagg1.JsonToExpando();
            var driver = obj.GetValue("parent.Driver", null);

            Assert.AreEqual("Talahooga Race Night", driver.GetValue("/Name", null));
            Assert.AreEqual("January Events", driver.GetValue("//Name", null));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_var_Success()
        {
            var obj = _data1.JsonToExpando();

            Assert.AreEqual("Bob", obj.GetValue("$EventCoordinator", new ExpressionContext(null, "", new TransformerContext { Arguments = new Dictionary<string, object> { {"EventCoordinator", "Bob" }}})));
        }

        [TestMethod]
        public void ObjectExtensions_GetValue_varObject_Success()
        {
            var obj = _data1.JsonToExpando();

            Assert.AreEqual("226-555-1212", obj.GetValue("$EventCoordinator.Phone", new ExpressionContext(null, "", new TransformerContext { Arguments = new Dictionary<string, object> { {"EventCoordinator", new {Phone = "226-555-1212"} }}})));
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
