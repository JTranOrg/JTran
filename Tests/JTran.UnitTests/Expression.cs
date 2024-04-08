using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using JTran;
using JTran.Expressions;

using Moq;

namespace JTran.UnitTests
{
    [TestClass]
    public class ExpressionTests
    {
        private ExpressionContext _context;

        [TestInitialize]
        public void Init()
        {
            _context = new ExpressionContext(null, (ExpressionContext?)null);
        }

        [TestMethod]
        public void Expression_Equal_Success()
        {
           var expression1 = new ComplexExpression { Left = new Value(88),    Right = new Value(88),    Operator = new EqualOperator() };
           var expression2 = new ComplexExpression { Left = new Value(88.99), Right = new Value(88.99), Operator = new EqualOperator() };
           var expression3 = new ComplexExpression { Left = new Value(true),  Right = new Value(true),  Operator = new EqualOperator() };
           var expression4 = new ComplexExpression { Left = new Value("bob"), Right = new Value("bob"), Operator = new EqualOperator() };

           Assert.IsTrue(expression1.EvaluateToBool(_context));
           Assert.IsTrue(expression2.EvaluateToBool(_context));
           Assert.IsTrue(expression3.EvaluateToBool(_context));
           Assert.IsTrue(expression4.EvaluateToBool(_context));
        }        
        
        [TestMethod]
        public void Expression_NotEqualOperator_Success()
        {
           var expression1 = new ComplexExpression { Left = new Value(88),    Right = new Value(48),     Operator = new NotEqualOperator() };
           var expression2 = new ComplexExpression { Left = new Value(88.99), Right = new Value(88.98),  Operator = new NotEqualOperator() };
           var expression3 = new ComplexExpression { Left = new Value(true),  Right = new Value(false),  Operator = new NotEqualOperator() };
           var expression4 = new ComplexExpression { Left = new Value("bob"), Right = new Value("fred"), Operator = new NotEqualOperator() };

           Assert.IsTrue(expression1.EvaluateToBool(_context));
           Assert.IsTrue(expression2.EvaluateToBool(_context));
           Assert.IsTrue(expression3.EvaluateToBool(_context));
           Assert.IsTrue(expression4.EvaluateToBool(_context));
        }        
        
        [TestMethod]
        public void Expression_And_Success()
        {
           var left       = new ComplexExpression { Left = new Value(88), Right = new Value(88), Operator = new EqualOperator() };
           var right      = new ComplexExpression { Left = new Value(55), Right = new Value(55), Operator = new EqualOperator() };
           var expression = new ComplexExpression { Left = left, Right = right, Operator = new AndOperator() };

           Assert.IsTrue(expression.EvaluateToBool(_context));
        }
        
        [TestMethod]
        public void Expression_Or_Success()
        {
           var left       = new ComplexExpression { Left = new Value(88), Right = new Value(16.789), Operator = new EqualOperator() };
           var right      = new ComplexExpression { Left = new Value(55), Right = new Value(55), Operator = new EqualOperator() };
           var expression = new ComplexExpression { Left = left, Right = right, Operator = new OrOperator() };

           Assert.IsTrue(expression.EvaluateToBool(_context));
        }
        
        [TestMethod]
        public void Expression_GreaterThan_Success()
        {
           var expression1 = new ComplexExpression { Left = new Value(127),       Right = new Value(5),            Operator = new GreaterThanOperator() };
           var expression2 = new ComplexExpression { Left = new Value(127.77),    Right = new Value(127.76999),    Operator = new GreaterThanOperator() };
           var expression3 = new ComplexExpression { Left = new Value(true),      Right = new Value(false),        Operator = new GreaterThanOperator() };
           var expression4 = new ComplexExpression { Left = new Value("bobfred"), Right = new Value("bob"),        Operator = new GreaterThanOperator() };

           Assert.IsTrue(expression1.EvaluateToBool(_context));
           Assert.IsTrue(expression2.EvaluateToBool(_context));
           Assert.IsTrue(expression3.EvaluateToBool(_context));
           Assert.IsTrue(expression4.EvaluateToBool(_context));
        }
    }
}
