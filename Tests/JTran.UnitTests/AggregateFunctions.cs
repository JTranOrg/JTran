using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Json;

using JTranParser = JTran.Parser.ExpressionParser;
using JTran.Common;

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
   
            Assert.AreEqual("54321", result.ToString());
        }

        [TestMethod]
        public void AggregateFunctions_reverse_array_Success()
        {
            var expression = Compile("reverse(Cars)");
            var context    = CreateContext(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }}  );
            var result     = (expression.Evaluate(context) as IEnumerable<object>).ToList();
   
            Assert.AreEqual("Cadillac", (result[0] as JsonObject)!["Model"].ToString());
            Assert.AreEqual("Pontiac",  (result[1] as JsonObject)!["Model"].ToString());
            Assert.AreEqual("Chevy",    (result[2] as JsonObject)!["Model"].ToString());
        }

        [TestMethod]
        public void AggregateFunctions_required_Success()
        {
            var expression = Compile("required(Cars, 'Cars is required')");
            var context    = CreateContext(new { Cars = new List<object> {}}  );
            var ex         = Assert.ThrowsException<Transformer.UserError>( ()=> expression.Evaluate(context));

            Assert.AreEqual("Cars is required", ex.Message);
        }

        [TestMethod]
        public void AggregateFunctions_required2_Success()
        {
            List<object> data = null;
            var expression = Compile("required(Cars, 'Cars is required')");
            var context    = CreateContext(new { Cars = data}  );
            var ex         = Assert.ThrowsException<Transformer.UserError>( ()=> expression.Evaluate(context));

            Assert.AreEqual("Cars is required", ex.Message);
        }

        [TestMethod]
        public void AggregateFunctions_required3_Success()
        {
            var expression = Compile("required(Cars, 'Cars is required')");
            var context    = CreateContext(new { Cars = ""}  );
            var ex         = Assert.ThrowsException<Transformer.UserError>( ()=> expression.Evaluate(context));

            Assert.AreEqual("Cars is required", ex.Message);
        }

        [TestMethod]
        public void AggregateFunctions_required4_Success()
        {
            var expression = Compile("required(Cars, 'Cars is required')");
            var context    = CreateContext(new { Cars = "  "}  );
            var ex         = Assert.ThrowsException<Transformer.UserError>( ()=> expression.Evaluate(context));

            Assert.AreEqual("Cars is required", ex.Message);
        }

        [TestMethod]
        public void AggregateFunctions_first_array_Success()
        {
            var expression = Compile("first(Cars)");
            var context    = CreateContext(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }}  );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("Chevy", (result as JsonObject)![CharacterSpan.FromString("Model")].ToString());
        }

        [TestMethod]
        public void AggregateFunctions_last_array_Success()
        {
            var expression = Compile("last(Cars)");
            var context    = CreateContext(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }}  );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("Cadillac", (result as JsonObject)![CharacterSpan.FromString("Model")].ToString());
        }

        [TestMethod]
        public void AggregateFunctions_join_Success()
        {
            var expression = Compile("join([1, 2, 3], ', ')");
            var context    = CreateContext(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }}  );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("1, 2, 3", result.ToString());
        }

        [TestMethod]
        public void AggregateFunctions_join2_Success()
        {
            var expression = Compile("join(['bob', 'fred', 'linda'], ', ')");
            var context    = CreateContext(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }}  );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("bob, fred, linda", result.ToString());
        }

        private IExpression Compile(string expr)
        {
            var parser   = new JTranParser();
            var compiler = new Compiler();
            var tokens   = parser.Parse(expr);

            return compiler.Compile(tokens);
        }

        private ExpressionContext CreateContext(object data)
        {
            return new ExpressionContext(CreateTestData(data), extensionFunctions: Transformer.CompileFunctions(null));
        }

        private object CreateTestData(object obj)
        {
            return JObject.FromObject(obj).ToString().ToJsonObject();
        }
    }
}
