using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Extensions;
using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void StringExtensions_FormatForJsonOutput_success()
        {
            Assert.AreEqual("",         "".FormatForJsonOutput());
            Assert.AreEqual("bob",      "bob".FormatForJsonOutput());
            Assert.AreEqual("b\\\\ob",  "b\\ob".FormatForJsonOutput());
            Assert.AreEqual("b\\\"ob",  "b\"ob".FormatForJsonOutput());
            Assert.AreEqual("James \\\\Jim\\\\ Smith",  "James \\Jim\\ Smith".FormatForJsonOutput());
            Assert.AreEqual("James \\\\快\\\\ Chan",  "James \\快\\ Chan".FormatForJsonOutput());
            Assert.AreEqual("James \\r Franklin",  "James \r Franklin".FormatForJsonOutput());
            Assert.AreEqual("James \\r\\n Franklin",  "James \r\n Franklin".FormatForJsonOutput());
            Assert.AreEqual("James \\\" Franklin",  "James \" Franklin".FormatForJsonOutput());
            Assert.AreEqual("James \\\"\\n Franklin",  "James \"\n Franklin".FormatForJsonOutput());
        }
    }
}
