using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Expressions;
using System.Linq;

namespace JTran.UnitTests
{
    [TestClass]
    public class FunctionTests
    {
        [TestMethod]
        public void Function_floor_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "floor");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.NumberValue(12.7d) };

            var result = func.Evaluate(parameters, context);

            Assert.AreEqual(12d, result);
        }

        [TestMethod]
        public void Function_substring_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "substring");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value("frank"), new JTran.Expressions.NumberValue(1d), new JTran.Expressions.NumberValue(3d) };

            var result = func.Evaluate(parameters, context);

            Assert.AreEqual("ran", result);
        }

        [TestMethod]
        public void Function_coalesce_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "coalesce");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value(null), new JTran.Expressions.Value(""), new JTran.Expressions.Value("frank") };

            var result = func.Evaluate(parameters, context);

            Assert.AreEqual("frank", result);
        }

        [TestMethod]
        public void Function_precision_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "precision");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.NumberValue(12.7345d), new JTran.Expressions.NumberValue(2) };

            var result = (double)func.Evaluate(parameters, context);

            Assert.AreEqual(12.73d, result);
        }

        [TestMethod]
        [DataRow("0", true)]
        [DataRow("1", false)]
        [DataRow("-123232", false)]
        [DataRow("0.000005", false)]
        public void Function_empty_number_Success(string input, bool result)
        {
            var func       = new Function(new BuiltinFunctions(), "empty");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.NumberValue(double.Parse(input)) };

            Assert.AreEqual(result, Convert.ToBoolean(func.Evaluate(parameters, context)));
        }
        [TestMethod]
        public void Function_guid_success()
        {
            var func       = new Function(new BuiltinFunctions(), "guid");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression>();

            var result1 = func.Evaluate(parameters, context).ToString();
            var result2 = func.Evaluate(parameters, context).ToString();

            Assert.IsTrue(Guid.TryParse(result1, out Guid val1));
            Assert.IsTrue(Guid.TryParse(result2, out Guid val2));

            Assert.AreNotEqual(Guid.Empty, val1);
            Assert.AreNotEqual(Guid.Empty, val2);
            Assert.AreNotEqual(val1, val2);
        }

        [TestMethod]
        [DataRow("", true)]
        [DataRow(" ", true)]
        [DataRow("  ", true)]
        [DataRow("\t ", true)]
        [DataRow("ds", false)]
        public void Function_empty_string_Success(string input, bool result)
        {
            var func       = new Function(new BuiltinFunctions(), "empty");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value(input) };

            Assert.AreEqual(result, Convert.ToBoolean(func.Evaluate(parameters, context)));
        }

        [TestMethod]
        public void Function_empty_array_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "empty");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value(null) };

            Assert.IsTrue(Convert.ToBoolean(func.Evaluate(parameters, context)));

            var parameters2 = new List<IExpression> { new JTran.Expressions.Value(new List<string>()) };

            Assert.IsTrue(Convert.ToBoolean(func.Evaluate(parameters2, context)));

            var parameters3 = new List<IExpression> { new JTran.Expressions.Value(new List<string> { "bob" } ) };

            Assert.IsFalse(Convert.ToBoolean(func.Evaluate(parameters3, context)));
        }

        [TestMethod]
        public void Function_empty_object_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "empty");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value(new {}) };

            Assert.IsTrue(Convert.ToBoolean(func.Evaluate(parameters, context)));
            
            var parameters2 = new List<IExpression> { new JTran.Expressions.Value(new { Name = "fred"}) };

            Assert.IsFalse(Convert.ToBoolean(func.Evaluate(parameters2, context)));
        }

        [TestMethod]
        public void Function_split_Success()
        {
            var func       = new Function(new BuiltinFunctions(), "split");
            var context    = new ExpressionContext("bob");
            var parameters = new List<IExpression> { new JTran.Expressions.Value("bob"), new JTran.Expressions.Value(",") };

            var result = func.Evaluate(parameters, context) as IEnumerable<string>;

            Assert.AreEqual(1, result.Count());

            parameters = new List<IExpression> { new JTran.Expressions.Value(""), new JTran.Expressions.Value(",") };

            result = func.Evaluate(parameters, context) as IEnumerable<string>;

            Assert.AreEqual(0, result.Count());

            parameters = new List<IExpression> { new JTran.Expressions.Value(" bob;fred ; george  "), new JTran.Expressions.Value(";") };

           var result2 = (func.Evaluate(parameters, context) as IEnumerable<string>).ToList();

            Assert.AreEqual(3, result2.Count);
            Assert.AreEqual("bob",    result2[0]);
            Assert.AreEqual("fred",   result2[1]);
            Assert.AreEqual("george", result2[2]);
        }
    }
}
