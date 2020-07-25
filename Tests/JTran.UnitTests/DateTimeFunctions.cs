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
    public class DateTimeFunctionsTests
    {
        [TestMethod]
        public void DateTimeFunctions_currentdatetime_Success()
        {
            var expression = Compile("currentdatetime()");
            var context    = CreateContext(new {Year = 2010} );
            var dt         = DateTime.Now;
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual(dt.ToString("s"), result);
        }

        [TestMethod]
        public void DateTimeFunctions_currentdate_Success()
        {
            var expression = Compile("currentdate()");
            var context    = CreateContext(new {Year = 2010} );
            var dt         = DateTime.Now.Date;
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual(dt.ToString("yyyy-MM-dd"), result);
        }

        [TestMethod]
        public void DateTimeFunctions_currentdatetimeutc_Success()
        {
            var expression = Compile("CurrentDateTimeUTC()");
            var context    = CreateContext(new {Year = 2010} );
            var dt         = DateTime.UtcNow;
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual(dt.ToString("s"), result);
        }

        [TestMethod]
        public void DateTimeFunctions_currentdateutc_Success()
        {
            var expression = Compile("CurrentDateUTC()");
            var context    = CreateContext(new {Year = 2010} );
            var dt         = DateTime.UtcNow.Date;
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual(dt.ToString("yyyy-MM-dd"), result);
        }

        [TestMethod]
        public void DateTimeFunctions_date_Success()
        {
            var context = CreateContext(new {Year = 2010} );
   
            Assert.AreEqual("2000-07-01",  Compile("date('2000-07-01T10:14:45')").Evaluate(context));
            Assert.AreEqual("bob",         Compile("date('bob')").Evaluate(context));
        }

        [TestMethod]
        public void DateTimeFunctions_addyears_Success()
        {
            var dt         = DateTime.Now;
            var expression = Compile("addyears('2000-07-01T10:00:00', 2)");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("2002-07-01T10:00:00", result);
        }

        [TestMethod]
        public void DateTimeFunctions_addmonths_Success()
        {
            var dt         = DateTime.Now;
            var expression = Compile("addmonths('2000-07-01T10:00:00', 2)");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("2000-09-01T10:00:00", result);
        }

        [TestMethod]
        public void DateTimeFunctions_adddays_Success()
        {
            var dt         = DateTime.Now;
            var expression = Compile("adddays('2000-07-01T10:00:00', 2)");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("2000-07-03T10:00:00", result);
        }

        [TestMethod]
        public void DateTimeFunctions_addhours_Success()
        {
            var dt         = DateTime.Now;
            var expression = Compile("addhours('2000-07-01T10:00:00', 2)");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("2000-07-01T12:00:00", result);
        }

        [TestMethod]
        public void DateTimeFunctions_addminutes_Success()
        {
            var dt         = DateTime.Now;
            var expression = Compile("addminutes('2000-07-01T10:00:00', 2)");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("2000-07-01T10:02:00", result);
        }

        [TestMethod]
        public void DateTimeFunctions_addseconds_Success()
        {
            var dt         = DateTime.Now;
            var expression = Compile("addseconds('2000-07-01T10:00:00', 2)");
            var context    = CreateContext(new {Year = 2010} );
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual("2000-07-01T10:00:02", result);
        }

        [TestMethod]
        public void DateTimeFunctions_daydex_Success()
        {
            var context = CreateContext(new {Year = 2010} );
   
            Assert.AreEqual(36706, Compile($"daydex('2000-07-01T10:00:00')").Evaluate(context));
            Assert.AreEqual(0, Compile("daydex('bob')").Evaluate(context));
            Assert.AreEqual(2958463, Compile($"daydex('{DateTime.MaxValue.ToString("s")}')").Evaluate(context));
        }

        [TestMethod]
        public void DateTimeFunctions_formatdatetime_Success()
        {
            var context = CreateContext(new {Year = 2010} );
   
            Assert.AreEqual("July 1, 2000", Compile($"formatdatetime('2000-07-01T10:00:00', 'MMMM d, yyyy')").Evaluate(context));
            Assert.AreEqual("bob", Compile($"formatdatetime('bob', 'MMMM d, yyyy')").Evaluate(context));
        }

        [TestMethod]
        public void DateTimeFunctions_components_Success()
        {
            var context = CreateContext(new {Year = 2010} );
   
            Assert.AreEqual(2000, Compile($"year('2000-07-01T10:00:00')").Evaluate(context));
            Assert.AreEqual(7,    Compile($"month('2000-07-01T10:00:00')").Evaluate(context));
            Assert.AreEqual(1,    Compile($"day('2000-07-01T10:00:00')").Evaluate(context));
            Assert.AreEqual(10,   Compile($"hour('2000-07-01T10:00:00')").Evaluate(context));
            Assert.AreEqual(27,   Compile($"minute('2000-07-01T10:27:00')").Evaluate(context));
            Assert.AreEqual(11,   Compile($"second('2000-07-01T10:00:11')").Evaluate(context));
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
