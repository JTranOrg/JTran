using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("assert")]
    public class AssertTests
    {
        [TestMethod]
        [DataRow("assert1", "assert1")]
        public async Task Assert_fails(string transform, string data)
        {
            var ex = await Assert.ThrowsExceptionAsync<AssertFailedException>( async ()=> await TransformerTest.Test("Assert." + transform, "Assert." + data));

            Assert.AreEqual(2, ex.LineNumber);
        }

        [TestMethod]
        [DataRow("assert2", "assert1")]
        public async Task Assert_succeeds(string transform, string data)
        {
            await TransformerTest.Test("Assert." + transform, "Assert." + data);
        }
    }
}