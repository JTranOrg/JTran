using Microsoft.VisualStudio.TestTools.UnitTesting;


using Moq;

using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    public class ExpressionContextTests
    {
        [TestMethod]
        public void ExpressionContext_GetVariable_wArgs()
        {
            var tContext = new TransformerContext { Arguments = new Dictionary<string, object> {{ "FirstName", "Fred" } }};

            var context = new ExpressionContext(new { Hello = "hello" }, "bob", tContext);  

            Assert.AreEqual("Fred", context.GetVariable(CharacterSpan.FromString("FirstName"), context).ToString());
        }

        [TestMethod]
        public void ExpressionContext_GetVariable_wArgs_throws()
        {
            var tContext = new TransformerContext { Arguments = new Dictionary<string, object> {{ "LastName", "Jones" } }};

            var context = new ExpressionContext(new { Hello = "hello" }, "bob", tContext);  

            Assert.ThrowsException<Transformer.SyntaxException>( ()=> context.GetVariable(CharacterSpan.FromString("FirstName"), context));
        }

        [TestMethod]
        public void ExpressionContext_GetVariable_noArgs_throws()
        {
            var tContext = new TransformerContext {};

            var context = new ExpressionContext(new { Hello = "hello" }, "bob", tContext);  

            Assert.ThrowsException<Transformer.SyntaxException>( ()=> context.GetVariable(CharacterSpan.FromString("FirstName"), context));
        }
    }
}
