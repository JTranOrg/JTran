using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Extensions;
using JTran.Common;
using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        public class Automobile
        {
            public string Make  { get; set; }
            public string Model { get; set; }
        }

        [TestMethod]
        public void ObjectExtensions_EnsureObjectEnumerable_Success()
        {
            var obj = new Automobile { Make = "Chevy", Model = "Corvette" };
            var enm = obj.EnsureObjectEnumerable();
            var t = enm.GetType();
            var list = enm.ToList();

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
