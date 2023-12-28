using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Syntax Errors")]
    public class SyntaxErrorTests
    {
        [TestMethod]
        [DataRow("foreach2", "customers3", "Missing expression for #foreach")]
        [DataRow("includenotfound", "customers3", "#include: file not found")]
        public async Task SyntaxErrorTests_success(string transform, string data, string msg)
        {
            var ex = await Assert.ThrowsExceptionAsync<Transformer.SyntaxException>( async ()=> await TransformerTest.Test("SyntaxErrors." + transform, data));

            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains(msg));
        }
    }
}