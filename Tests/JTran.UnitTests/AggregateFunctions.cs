using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran;
using JTran.Expressions;
using JTran.Extensions;
using System;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Built-in Functions")]
    public class AggregateFunctionsTests
    {
        [TestMethod]
        public void AggregateFunctions_reverse_string_Success()
        {
            var expression = Compile("reverse('12345')");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("54321", result);
        }

        [TestMethod]
        public void AggregateFunctions_reverse_array_Success()
        {
            var expression = Compile("reverse(Cars)");
            var context    = CreateContext(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }}  );
            var result     = expression.Evaluate(context) as IList<object>;
   
            Assert.AreEqual("Cadillac", (result[0] as IDictionary<string, object>)["Model"]);
            Assert.AreEqual("Pontiac",  (result[1] as IDictionary<string, object>)["Model"]);
            Assert.AreEqual("Chevy",    (result[2] as IDictionary<string, object>)["Model"]);
        }

        private IExpression Compile(string expr)
        {
            var parser   = new Parser();
            var compiler = new Compiler();
            var tokens   = parser.Parse(expr);

            return compiler.Compile(tokens);
        }

        private ExpressionContext CreateContext(object data)
        {
            return new ExpressionContext(CreateTestData(data), extensionFunctions: Transformer.CompileFunctions(null));;
        }

        private object CreateTestData(object obj)
        {
            return JObject.FromObject(obj).ToString().JsonToExpando();
        }
    }
}
