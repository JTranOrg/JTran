using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using JTran;
using JTran.Expressions;

namespace JTran.UnitTests
{
    [TestClass]
    public class FunctionTests
    {
        [TestMethod]
        public void Function_Evaluate_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "floor");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.NumberValue(12.7M) };

            var result = func.Evaluate(parameters, context);

            Assert.AreEqual(12M, result);
        }

        [TestMethod]
        public void Function_Evaluate2_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "substring");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value("frank"), new JTran.Expressions.NumberValue(1M), new JTran.Expressions.NumberValue(3M) };

            var result = func.Evaluate(parameters, context);

            Assert.AreEqual("ran", result);
        }
    }
}
