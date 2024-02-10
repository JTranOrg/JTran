
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Collections;
using JTran.Expressions;
using JTranParser = JTran.Parser.ExpressionParser;

namespace JTran.UnitTests
{
    [TestClass]
    public class WhereClauseTests
    {
        [TestMethod]
        public void WhereClause_Success()
        {
            var list         = new List<string> { "red", "green", "blue" };
            var context      = new ExpressionContext(null);
            var parser       = new JTranParser();
            var compiler     = new Compiler();
            var expression   = compiler.Compile(parser.Parse("@ > 'r'"));
            var whereClause  = new WhereClause<string>(list, expression, context);
            var result       = new List<string>(whereClause);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("red", result[0]);
        }
    }
}
