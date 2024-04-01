
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Collections;

namespace JTran.UnitTests
{
    [TestClass]
    public class UnionTests
    {
        [TestMethod]
        public void Union_Success()
        {
            var list1  = new List<string> { "red", "green", "blue" };
            var list2  = new List<string> { "orange", "purple", "yellow" };
            var union  = new Union<string>(list1, list2);
            var result = new List<string>(union);

            Assert.AreEqual(6, result.Count);

            Assert.AreEqual("red",    result[0]);
            Assert.AreEqual("green",  result[1]);
            Assert.AreEqual("blue",   result[2]);
            Assert.AreEqual("orange", result[3]);
            Assert.AreEqual("purple", result[4]);
            Assert.AreEqual("yellow", result[5]);
        }

        [TestMethod]
        public void Union_Success2()
        {
            var list1  = new List<string> { "red", "green", "blue" };
            var list2  = new List<string>();
            var list3  = new List<string> { "orange", "purple", "yellow" };
            var union  = new Union<string>(list1, list2, list3);
            var result = new List<string>(union);

            Assert.AreEqual(6, result.Count);

            Assert.AreEqual("red",    result[0]);
            Assert.AreEqual("green",  result[1]);
            Assert.AreEqual("blue",   result[2]);
            Assert.AreEqual("orange", result[3]);
            Assert.AreEqual("purple", result[4]);
            Assert.AreEqual("yellow", result[5]);
        }

        [TestMethod]
        public void Union_Where()
        {
            var list1  = new List<string> { "red", "green", "blue" };
            var list2  = new List<string>();
            var list3  = new List<string> { "orange", "purple", "yellow" };
            var union  = new Union<string>(list1, list2, list3);
            var result = union.Where(s => s.Contains('r')).ToList();

            Assert.AreEqual(4, result.Count);

            Assert.AreEqual("red",    result[0]);
            Assert.AreEqual("green",  result[1]);
            Assert.AreEqual("orange", result[2]);
            Assert.AreEqual("purple", result[3]);
        }
    }
}
