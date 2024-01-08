using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Syntax Errors")]
    public class SyntaxErrorTests
    {
        [TestMethod]
        [DataRow("variable2", "customers", "Unknown element", 4)]
        [DataRow("foreach2", "customers3", "Missing expression for #foreach", 2)]
        [DataRow("includenotfound", "customers3", "#include: file not found", 2)]
        public async Task SyntaxErrorTests_throw(string transform, string data, string msg, long lineNumber)
        {
            var ex = await Assert.ThrowsExceptionAsync<Transformer.SyntaxException>( async ()=> await TransformerTest.Test("SyntaxErrors." + transform, data));

            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains(msg));
            Assert.IsTrue(ex.Data.Contains("LineNumber"));
            Assert.AreEqual(lineNumber.ToString(), ex.Data["LineNumber"]);
        }

        [TestMethod]
        [DataRow("variable", "customers", "Unexpected token", 4)]
        public async Task SyntaxErrorTests_jsonparser(string transform, string data, string msg, long lineNumber)
        {
            var ex = await Assert.ThrowsExceptionAsync<JsonParseException>( async ()=> await TransformerTest.Test("SyntaxErrors." + transform, data));

            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains(msg));
            Assert.IsTrue(ex.Data.Contains("LineNumber"));
            Assert.AreEqual(lineNumber.ToString(), ex.Data["LineNumber"]);
        }
    }
}