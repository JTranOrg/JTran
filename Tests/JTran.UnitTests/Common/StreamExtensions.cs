
using System.Collections;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("StreamExtensions")]
    public class StreamExtensionsTests
    {
        [TestMethod]
        public void StreamExtensions_ToObject()
        {
            var json = "{ 'Properties': { 'Make': 'Chevy', 'Model': 'Corvette' }, 'Engine': {'Type': 'V8', 'Displacement': '350', 'Properties': { 'Composition': 'Aluminum' } } }";

            var obj = Test<Automobile>(json);

            Assert.AreEqual("Chevy", obj.Properties["Make"]);
            Assert.AreEqual("Corvette", obj.Properties["Model"]);

            Assert.AreEqual("V8", obj.Engine.Type);
            Assert.AreEqual("350", obj.Engine.Displacement);
            Assert.AreEqual("Aluminum", obj.Engine.Properties["Composition"]);
        }

        private T Test<T>(string json) where T : class, new()
        {
            using var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(json));
            var obj = stream.ToObject<T>();

            Assert.IsNotNull(obj);

            return obj;
        }

        public class Automobile
        {
           public Dictionary<string, string>? Properties { get; set; }
           public Engine?                     Engine     { get; set; }
        }

        public class Engine
        {
            public string       Type          { get; set; } = "";
            public string       Displacement  { get; set; } = "";
            public IDictionary? Properties    { get; set; }
        }
    }
}
