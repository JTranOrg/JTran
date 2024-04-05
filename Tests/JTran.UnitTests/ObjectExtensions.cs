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
    }
}
