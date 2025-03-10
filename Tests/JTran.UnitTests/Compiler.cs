using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Json;
using JTranParser = JTran.Parser.ExpressionParser;
using JTran.Common;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class CompilerTests
    {
        [TestMethod]
        public void Compiler_StringEquality_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Name == 'Fred'"));
            var context    = new ExpressionContext(CreateTestData(new {Name = "Fred" } ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_NumberEquality_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age == 22"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22 } ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberGreaterThan_True()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age > 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22 } ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberGreaterThan_val_is_null()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age > 21"));
            var context    = new ExpressionContext(CreateTestData(new {} ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberGreaterThan_False()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age > 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 18 } ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberLessThan_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age < 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19 } ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_complex_bool_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("$IsLicensed && Licensed && Age < 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, Licensed = true } ) );
   
            context.SetVariable(CharacterSpan.FromString("IsLicensed"), true);

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        [DataRow("-14 * .53 + 3.5", -3.92)]
        [DataRow("-14 - -.53 - 3.5", -16.97)]
        public void Compiler_complex_bool_Success(string expr, double val)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse(expr));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, Licensed = true } ) );
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(val, Convert.ToDouble(expression.Evaluate(context)));
        }

        [TestMethod]
        public void Compiler_Addition_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Year + Age"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual(2010m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Addition_Multiple_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Year + Age - 4"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual(2006m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Addition_concat_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("'Bob' + 'Fred'"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("BobFred", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_Addition_concat2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("'Bob' + 32 + 'Fred'"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("Bob32Fred", expression.Evaluate(context).ToString());
        }        
        
        [TestMethod]
        public void Compiler_Addition_multipart_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Customer.FirstName"));
            var context    = new ExpressionContext(CreateTestData(new {Customer = new { FirstName = "Bob", LastName = "Jones"} } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("Bob", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_Addition_multipart2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Customer.FirstName + Customer.LastName"));
            var context    = new ExpressionContext(CreateTestData(new {Customer = new { FirstName = "Bob", LastName = "Jones"} } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("BobJones", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_Addition_multipart3_Success()
        {
            var expression = Compile("$Customer.FirstName + $Customer.LastName");

            var tContext = new TransformerContext { Arguments = new Dictionary<string, object> 
                                                                  { 
                                                                    {"Customer", new { FirstName = "Bob", LastName = "Jones"} }
                                                                  }
                                                  };

            var context    = new ExpressionContext(CreateTestData(new {} ), CharacterSpan.Empty, tContext);

            Assert.IsNotNull(expression);
            Assert.AreEqual("BobJones", expression.Evaluate(context).ToString());
        }

        private IExpression Compile(string expression)
        {        
            var parser      = new JTranParser();
            var tokens      = parser.Parse("$Customer.FirstName + $Customer.LastName");
            var compiler    = new Compiler();

            return compiler.Compile(tokens);
        }

        [TestMethod]
        public void Compiler_Subtraction_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Year - Age"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(1966m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multiplication_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Salary * Months"));
            var context    = new ExpressionContext(CreateTestData(new {Salary = 100, Months = 12 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual(1200m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Precedence_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("100 + Salary * Months");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Salary = 100, Months = 12 } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(1300m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Division_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Paycheck / Months"));
            var context    = new ExpressionContext(CreateTestData(new {Paycheck = 1200, Months = 12 } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(100m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_TertiaryOperator_Left()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age >= 21 ? 'Beer' : 'Coke'");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual("Coke", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_TertiaryOperator_Complex()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Name == 'bob' && Age >= 21 ? FirstName : LastName");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Name = "bob", Age = 35, FirstName = "Robert", LastName = "Jones" } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual("Robert", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_Multiple_Ands_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("$isactive && Type == $Type3 && $LicenseState != $CA && $LicenseState != $Unknown");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 42, Type = 2} ), CharacterSpan.Empty, 
                                                                  new TransformerContext { Arguments = new Dictionary<string, object> 
                                                                  { 
                                                                    {"Type3", 2},
                                                                    {"LicenseState", 7 },
                                                                    {"CA", 1},
                                                                    {"Unknown", 0},
                                                                    {"isactive", true}
                                                                  }});
   
            Assert.IsNotNull(expression);

            var result = expression.EvaluateToBool(context);

            Assert.IsTrue(result);
        }

        #region DateTime

        #region Actual DateTime values

        [TestMethod]
        public void Compiler_DateTime_use_DateTime_GreaterThan_true()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Wife.Birthdate > Husband.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = new DateTime(1980, 4, 5) },
                                                                        Wife    = new { Birthdate = new DateTime(1984, 7, 22) }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_DateTime_GreaterThan_false()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Wife.Birthdate > Husband.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = new DateTime(1986, 4, 5) },
                                                                        Wife    = new { Birthdate = new DateTime(1984, 7, 22) }} ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_DateTime_LessThan_true()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate < Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = new DateTime(1980, 4, 5) },
                                                                        Wife    = new { Birthdate = new DateTime(1984, 7, 22) }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_DateTime_LessThan_false()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate < Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = new DateTime(1986, 4, 5) },
                                                                        Wife    = new { Birthdate = new DateTime(1984, 7, 22) }} ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        #endregion

        #region String values

        [TestMethod]
        public void Compiler_DateTime_use_string_GreaterThan_true()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Wife.Birthdate > Husband.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1980-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_string_1_is_null_GreaterThan_true()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate > Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1980-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = (String?)null }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_string_1_is_missing_GreaterThan_true()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate > Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1980-04-05T10:00:00" },
                                                                        Wife    = new { }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_string_GreaterThan_false()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Wife.Birthdate > Husband.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1986-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }} ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_string_LessThan_true()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate < Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1980-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_string_LessThan_false()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate < Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1986-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }} ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_max()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("max(Husband.Birthdate, Wife.Birthdate)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1986-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual("1986-04-05T10:00:00", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_DateTime_min()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("min(Husband.Birthdate, Wife.Birthdate)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1986-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual("1984-10-22T10:00:00", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_max_primitive()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("max(Husband.Age, Wife.Age)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Age = (short)90 },
                                                                        Wife    = new { Age = 89f }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual(90m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_min_primitive()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("min(Husband.Age, Wife.Age)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Age = (short)90 },
                                                                        Wife    = new { Age = (ushort)89 }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual(89m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_reverse_primitive()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("reverse(Husband.Age)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Age = (uint)90438921 },
                                                                        Wife    = new { Age = (ushort)89 }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual("12983409", expression.Evaluate(context));
        }

        #endregion

        #endregion

        #region Multipart

        [TestMethod]
        public void Compiler_Multipart_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Car.Engine.Cylinders"));
            var context    = new ExpressionContext(CreateTestData(new { Car = new { Engine = new { Cylinders = 8 } }  } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(8m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multipart_missing_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Car.Engine.Cylinders"));
            var context    = new ExpressionContext(CreateTestData(new { Car = new { Tires = new { Tread = .5 } }  } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(null, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multipart_null_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Car.Engine.Cylinders"));
            var context    = new ExpressionContext(CreateTestData(new { Car = new { Engine = (Automobile?)null, Tires = new { Tread = .5 } }  } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(null, expression.Evaluate(context));
        }

        #endregion

        #region Parenthesis

        [TestMethod]
        [DataRow("10 - 3 + (9 - 1)", 15)]
        [DataRow("10 - 3 * (3 - 1)", 4)]
        [DataRow("10 - 3 * 3 - 1", 0)]
        [DataRow("5 * 3 - (3 + 1)", 11)]
        [DataRow("12 - 3 / (4 - 1)", 11)]
        [DataRow("12 - 3 * 3 / (4 - 1)", 9)]
        [DataRow("12 - 3 * 3 % (4 - 2)", 11)]
        [DataRow("8 / 2 * 2", 8)]
        public void Compiler_precedence(string expressionStr, double result)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse(expressionStr);
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual((decimal)result, Convert.ToDecimal(expression.Evaluate(context)));
        }

        [TestMethod]
        public void Compiler_Parenthesis_And_True()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Age >= 21) && (State == 'WA')"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_And_False()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Age >= 21) && (State == 'WA')"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_Or_True()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Year <= 1962) || (Year >= 1969)"));
            var context    = new ExpressionContext(CreateTestData(new {Year = 1961} ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_Or_False()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Year <= 1962) || (Year >= 1969)"));
            var context    = new ExpressionContext(CreateTestData(new {Year = 1967} ));
   
            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_Math__Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("(100 + Salary) * Months - 20");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Salary = 100, Months = 12} ));
   
           Assert.IsNotNull(expression);
            Assert.AreEqual(2380m, expression.Evaluate(context));
        }

        #endregion

        #region Arrays

        [TestMethod]
        public void Compiler_Array_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("[1, 2, 3]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars} ));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.IsTrue(result is IEnumerable<object>);

            var list = ((IEnumerable<object>)result).ToList();
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, int.Parse(list![0].ToString()!));
            Assert.AreEqual(2, int.Parse(list![1].ToString()!));
            Assert.AreEqual(3, int.Parse(list![2].ToString()!));
        }


        [TestMethod]
        public void Compiler_Array_Success2()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("[a || b ? 1 : 99, iif(a && b, 2, 98), x - 6 + y]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {a = true, b = true, c = true, x = 9, y = 0} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.IsTrue(result is IEnumerable<object>);

            var list = ((IEnumerable<object>)result).ToList();
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, int.Parse(list?[0]?.ToString() ?? "0"));
            Assert.AreEqual(2, int.Parse(list?[1]?.ToString() ?? "0"));
            Assert.AreEqual(3, int.Parse(list?[2]?.ToString() ?? "0"));
        }

        [TestMethod]
        [DataRow("[5 + 6, 7, 8]", 11, 7, 8)]
        [DataRow("[5, 7 + 6, 8]", 5, 13, 8)]
        [DataRow("[5 + (6 + 1) - 3, 7 + 6, 8 * 2 / 4]", 9, 13, 4)]
        public void Compiler_Array2_Success(string expressionStr, int v1, int v2, int v3)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse(expressionStr);
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars} ));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.IsTrue(result is IEnumerable<object>);

            var list = ((IEnumerable<object>)result).ToList()!;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(v1, int.Parse(list[0]!.ToString()!));
            Assert.AreEqual(v2, int.Parse(list[1]!.ToString()!));
            Assert.AreEqual(v3, int.Parse(list[2]!.ToString()!));
        }

        #endregion

        #region Array Indexers

        [TestMethod]
        [DataRow("0", 21)]
        [DataRow("1", 35)]
        [DataRow("2", 62)]
        public void Compiler_Array_Indexer_Number_int_array_Success(string index, int expected)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse($"Cars[{index}]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = new List<int> {21, 35, 62}, bob = 2 } ));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(expected, int.Parse(result!.ToString()));
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Number_int_array_as_var_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("$Indices[0]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Model = "Chevy" } ));
   
            context.SetVariable(CharacterSpan.FromString("Indices"), new List<int> {21, 35, 62} );
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(21m, decimal.Parse(result!.ToString()!));
        }

        [TestMethod]
        [DataRow("Cars[$WhichCar", false)]
        [DataRow("Cars[$WhichCar", true)]
        public void Compiler_Array_Indexer_Expression_Number_Success(string expression, bool raw)
        {
            var result = TestCompiler<Automobile>(expression, new {Cars = _cars, WhichCar = 2}, new TransformerContext { Arguments = new Dictionary<string, object> { {"WhichCar", 2}}}, raw: raw);
   
            Assert.AreEqual("Dodge", result.Make);
        }

        [TestMethod]
        [DataRow("Cars[$Which.Car", false)]
        [DataRow("Cars[$Which.Car", true)]
        public void Compiler_Array_Indexer_Expression_Number_multilevel_var_Success(string expression, bool raw)
        {
            var result = TestCompiler<Automobile>(expression, new {Cars = _cars, WhichCar = 2}, new TransformerContext { Arguments = new Dictionary<string, object> { {"Which", new Dictionary<string, object> { {"Car", 2}}}}}, raw: raw);
   
            Assert.AreEqual("Dodge", result.Make);
        }

        [TestMethod]
        [DataRow("Cars[Make == 'Chevy']", false)]
        [DataRow("Cars[Make == 'Chevy']", true)]
        public void Compiler_Array_Indexer_Expression_Where_Success(string expression, bool raw)
        {
            var result = TestCompiler<List<Ticket>>(expression, new {Cars = _cars2}, raw: raw);
           
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [DataRow("Cars[Make == 'Chevy'].Tickets[Location == 'WA']", false)]
        [DataRow("Cars[Make == 'Chevy'].Tickets[Location == 'WA']", true)]
        public void Compiler_Array_Indexer_Multi_Success(string expression, bool raw)
        {
            var result = TestCompiler<Ticket>(expression, new {Cars = _cars3}, raw: raw);

            Assert.AreEqual(120M, result.Amount);
        }

        private static readonly ICharacterSpan _amount = CharacterSpan.FromString("Amount");

        [TestMethod]
        [DataRow("Cars[Make == 'Chevy'].Tickets[Location == 'WA']", false)]
        [DataRow("Cars[Make == 'Chevy'].Tickets[Location == 'WA']", true)]
        public void Compiler_Array_Indexer_Multi2_Success(string expression, bool raw)
        {
            var result = TestCompiler<List<Ticket>>(expression, new {Cars = _cars4}, raw: raw);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(180m, result[0].Amount);
            Assert.AreEqual(400m, result[1].Amount);
        }

        [TestMethod]
        [DataRow("Cars[Make == 'Chevy'].Tickets[Location == 'WA']", false)]
        [DataRow("Cars[Make == 'Chevy'].Tickets[Location == 'WA']", true)]
        public void Compiler_Array_Indexer_Multi3_Success(string expression, bool raw)
        {
            var result = TestCompiler<Ticket>(expression, new {Cars = _cars5}, raw: raw);

            Assert.AreEqual(120M, result.Amount);
        }

        #endregion

        #region Functions

        [TestMethod]
        public void Compiler_function_floor_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("floor(3.5)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(3m, result);
        }

        [TestMethod]
        public void Compiler_function_floor2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("floor(3 + 2.25)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(5m, result);
        }

        [TestMethod]
        public void Compiler_function_ceiling_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("ceiling(3.5)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(4m, result);
        }

        [TestMethod]
        public void Compiler_function_floorceiling_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("floor(ceiling(3.5))");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(4m, result);
        }

        [TestMethod]
        public void Compiler_function_pow_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("pow(10, 2)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(100m, result);
        }

        [TestMethod]
        public void Compiler_function_sqrt_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sqrt(100)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(10m, result);
        }

        [TestMethod]
        public void Compiler_function_not_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("not(Make == 'Chevy')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Make = "Chevy"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_normalize_space_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("normalizespace(Make + ' ' + ' Camaro ')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Make = "Chevy"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("Chevy Camaro", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_string_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("string(2 + Year)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("2012", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_string2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("string('007')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("007", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_sequence_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sequence(1, 5)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
            var list       = new List<decimal>((expression.Evaluate(context) as IList<object>)!.Select( i=> decimal.Parse(i?.ToString() ?? "0")));
   
            Assert.AreEqual(1m, list[0]);
            Assert.AreEqual(2m, list[1]);
            Assert.AreEqual(3m, list[2]);
            Assert.AreEqual(4m, list[3]);
            Assert.AreEqual(5m, list[4]);
        }

        [TestMethod]
        public void Compiler_function_sequence_increment_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sequence(2, 10, 2)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
            var list       = new List<decimal>((expression.Evaluate(context) as IList<object>)!.Select( i=> decimal.Parse(i?.ToString() ?? "0")));
   
            Assert.AreEqual(5,  list.Count);
            Assert.AreEqual(2m, list[0]);
            Assert.AreEqual(4m, list[1]);
            Assert.AreEqual(6m, list[2]);
            Assert.AreEqual(8m, list[3]);
            Assert.AreEqual(10m, list[4]);
        }

        [TestMethod]
        public void Compiler_function_sequence_backwards_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sequence(10, 2, -2)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
            var list       = new List<decimal>((expression.Evaluate(context) as IList<object>)!.Select( i=> decimal.Parse(i?.ToString() ?? "0")));
   
            Assert.AreEqual(5,  list.Count);
            Assert.AreEqual(10m, list[0]);
            Assert.AreEqual(8m, list[1]);
            Assert.AreEqual(6m, list[2]);
            Assert.AreEqual(4m, list[3]);
            Assert.AreEqual(2m, list[4]);
        }

        [TestMethod]
        public void Compiler_function_sequence_empty_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sequence('bob', 2, -2)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
            var list       = new List<decimal>((expression.Evaluate(context) as IList<object>)!.Select( i=> decimal.Parse(i?.ToString() ?? "0")));
   
            Assert.AreEqual(0,  list.Count);
        }

        [TestMethod]
        [DataRow("lowercase('')",       "")]
        [DataRow("lowercase(null)",     null)]
        [DataRow("lowercase('ABcDeF')", "abcdef")]
        [DataRow("lowercase('abc')",    "abc")]
        [DataRow("lowercase('ABC')",    "abc")]
        public void Compiler_function_lowercase(string expression, string? expected)
        {
            TestStringExpression(expression, expected);
        }

        [TestMethod]
        [DataRow("uppercase('')",       "")]
        [DataRow("uppercase(null)",     null)]
        [DataRow("uppercase('ABcDeF')", "ABCDEF")]
        [DataRow("uppercase('abc')",    "ABC")]
        [DataRow("uppercase('ABC')",    "ABC")]
        public void Compiler_function_uppercase(string expression, string? expected)
        {
            TestStringExpression(expression, expected);
        }

        [TestMethod]
        [DataRow("padleft('',           'abc', 8)", "aaaaaaaa")]
        [DataRow("padleft(null,         'a',   8)", "aaaaaaaa")]
        [DataRow("padleft('b',          'ab',  8)", "aaaaaaab")]
        [DataRow("padleft('12345678',   'a',   8)", "12345678")]
        [DataRow("padleft('1234567890', 'a',   8)", "1234567890")]
        [DataRow("padleft('bcde',       'a',   8)", "aaaabcde")]
        [DataRow("padleft('bcde',       'a',   0)", "bcde")]
        [DataRow("padleft('bcde',       '',    8)", "bcde")]
        public void Compiler_function_padleft(string expression, string? expected)
        {
            TestStringExpression(expression, expected);
        }

        [TestMethod]
        [DataRow("padright('',           'abc', 8)", "aaaaaaaa")]
        [DataRow("padright(null,         'a',   8)", "aaaaaaaa")]
        [DataRow("padright('b',          'ab',  8)", "baaaaaaa")]
        [DataRow("padright('12345678',   'a',   8)", "12345678")]
        [DataRow("padright('1234567890', 'a',   8)", "1234567890")]
        [DataRow("padright('bcde',       'a',   8)", "bcdeaaaa")]
        [DataRow("padright('bcde',       'a',   0)", "bcde")]
        [DataRow("padright('bcde',       '',    8)", "bcde")]
        public void Compiler_function_padright(string expression, string? expected)
        {
            TestStringExpression(expression, expected);
        }

        [TestMethod]
        [DataRow("normalizespace('')",        "")]
        [DataRow("normalizespace(null)",      null)]
        [DataRow("normalizespace('abcdef')",  "abcdef")]
        [DataRow("normalizespace('  abc  def  ')",  "abc def")]
        [DataRow("normalizespace('  a        b   c  de f  ')",  "a b c de f")]
        public void Compiler_function_normalizespace(string expression, string? expected)
        {
            TestStringExpression(expression, expected);
        }

        private void TestStringExpression(string expression, string? expected)
        {
            var parser   = new JTranParser();
            var compiler = new Compiler();
            var tokens   = parser.Parse(expression);
            var expr     = compiler.Compile(tokens);
            var context  = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(expected, expr.Evaluate(context)?.ToString());
        }

        [TestMethod]
        public void Compiler_function_uppercase_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("uppercase('ABcDeF')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("ABCDEF", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_number_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age < number(MaxAge)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "10"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_number_null_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age < number(MaxAge)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {MaxAge = "10"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_number_notnumber_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age > number(MaxAge)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "bob"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_isnumber_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("isnumber(MaxAge) ? MaxAge : 10000");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "bob"} ), extensionFunctions: Transformer.CompileFunctions(null));
            var result     = expression.Evaluate(context);
   
            Assert.AreEqual(10000m, (decimal)Convert.ChangeType(result, typeof(decimal)));
        }

        [TestMethod]
        public void Compiler_function_isnumber2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age < (isnumber(MaxAge) ? MaxAge : 10000)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "bob"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_string_padleft_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("string(padleft(7, '0', 3))");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("007", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_stringlength_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("stringlength('crop')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(4, expression.Evaluate(context));
        }

        [TestMethod]
        [DataRow("franklin", 0, 5,     "frank")]
        [DataRow("franklin", 5, -1000, "lin_ted")]
        [DataRow("",         0, 3,     "_te")]
        [DataRow("",         5, 3,     "")]
        [DataRow("franklin", 9000, 3,  "")]
        public void Compiler_function_substring(string? str, int index, int length, string? result)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse($"substring('{str}' + '_ted', {index}, {length})");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(result, expression.Evaluate(context)?.ToString());
        }

        [TestMethod]
        public void Compiler_function_indexof_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("indexof('franklin', 'ank')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(2, expression.Evaluate(context));
        }

        [TestMethod]
        [DataRow("franklin", "ran",           "f")]
        [DataRow("franklin", "lin",           "frank")]
        [DataRow("franklin", "franklin",      "")]
        [DataRow("franklin", "bobsyouruncle", "franklin")]
        [DataRow("beebop",   "/",             "beebop")]
        public void Compiler_function_substringbefore_Success(string str, string search, string result)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse($"substringbefore('{str}', '{search}')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(result, expression.Evaluate(context).ToString());
        }

        [TestMethod]
        [DataRow("franklin", "frank",   "lin")]
        [DataRow("franklin", "f",       "ranklin")]
        [DataRow("franklin", "franklin", "")]
        [DataRow("franklin", "bobsyouruncle", "")]
        [DataRow("franklin", "", "franklin")]
        public void Compiler_function_substringafter_Success(string str, string search, string result)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse($"substringafter('{str}', '{search}')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(result, expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_substringafter2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("substringafter('beebop' + Foo, 'bee')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Foo = "Foo"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("bopFoo", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        [DataRow("franklin", "frank",         true)]
        [DataRow("franklin", "f",             true)]
        [DataRow("franklin", "franklin",      true)]
        [DataRow("franklin", "bobsyouruncle", false)]
        [DataRow("franklin", "",              false)]
        [DataRow("", "",                      false)]
        public void Compiler_function_startswith(string str, string search, bool result)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse($"startswith('{str}', '{search}')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(result, expression.Evaluate(context));
        }


        [TestMethod]
        [DataRow("franklin", "lin",           true)]
        [DataRow("franklin", "n",             true)]
        [DataRow("franklin", "franklin",      true)]
        [DataRow("franklin", "bobsyouruncle", false)]
        [DataRow("franklin", "",              false)]
        [DataRow("", "",                      false)]
        public void Compiler_function_endswith(string str, string search, bool result)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse($"endswith('{str}', '{search}')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(result, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_multiple_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("substring(normalizespace(substringbefore(substringafter('Franklin Delano Roosevelt', 'Franklin'), 'Roosevelt')), 1, 4)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("elan", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_blank_string_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("'bob' + '' + ' jones'");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("bob jones", expression.Evaluate(context).ToString());
        }

        #region Aggregate/Array Functions

        [TestMethod]
        public void Compiler_function_count_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("count(Cars)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy"}, new { Model = "Pontiac"} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(2, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_first_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("first(Cars).Model");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy"}, new { Model = "Pontiac"} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("Chevy", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_contains_str_Success()
        {   
            Assert.IsTrue(EvaluateToBool("contains('batman', 'bat')"));
            Assert.IsTrue(EvaluateToBool("contains('batman', 'tma')"));
            Assert.IsFalse(EvaluateToBool("contains('batman', 'fred')"));
        }

        [TestMethod]
        public void Compiler_function_contains_list_Success()
        {   
            Assert.IsTrue(EvaluateToBool("contains(list, 'bat')", new List<object> { "man", "bat" }));
            Assert.IsTrue(EvaluateToBool("contains(list, 45)", new List<object> { 72, 85.5, 45.0 }));
            Assert.IsTrue(EvaluateToBool("contains(list, true)", new List<object> { false, true }));
        }

        [TestMethod]
        public void Compiler_function_sum_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sum(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1000M }, new { Model = "Pontiac", SaleAmount = 2000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(3000m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_avg_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("avg(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(2400m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_avg2_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("avg(SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Model = "Chevy", SaleAmount = 1200M } ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(1200m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_max_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("max(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy",    SaleAmount = 1200M }, 
                                                                                                  new { Model = "Pontiac",  SaleAmount = 2000M},  
                                                                                                  new { Model = "Cadillac", SaleAmount = 4000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(4000m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_min_Success()
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("min(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(1200m, expression.Evaluate(context));
        }

        #endregion

        #endregion

        #region Private

        private class DataWrapper<T>
        {
            public T? Data { get; set; }
            
        }
        private T TestCompiler<T>(string expressionStr, object data, TransformerContext? tContext = null, bool raw = false) where T : class, new()
        {
            var parser      = new JTranParser();
            var compiler    = new Compiler();
            var tokens      = parser.Parse(expressionStr);
            var expression  = compiler.Compile(tokens);

            Assert.IsNotNull(expression);

            var context = new ExpressionContext(raw ? data : CreateTestData(data), transformerContext: tContext);

            var result = expression.Evaluate(context);

            Assert.IsNotNull(result);

            var transformer = new Transformer("{ 'Data': '#copyof(@)' }");
            
            using var output = new MemoryStream();

            transformer.Transform(result, output);

            var str = output.ReadString();
   
            var rtnVal = output.ToObject<DataWrapper<T>>();

            Assert.IsNotNull(rtnVal);
            Assert.IsNotNull(rtnVal.Data);

            return rtnVal.Data;
        }

        private static List<Automobile> _cars = new List<Automobile>
        {
            new Automobile { Make = "Chevy",   Model = "Camaro" },
            new Automobile { Make = "Pontiac", Model = "Firebird" },
            new Automobile { Make = "Dodge",   Model = "Charger" },
            new Automobile { Make = "Ford",    Model = "Cobra" },
        };

        private static List<Automobile> _cars2 = new List<Automobile>
        {
            new Automobile { Make = "Chevy",   Model = "Camaro" },
            new Automobile { Make = "Pontiac", Model = "Firebird" },
            new Automobile { Make = "Dodge",   Model = "Charger" },
            new Automobile { Make = "Chevy",   Model = "Corvette" },
            new Automobile { Make = "Ford",    Model = "Cobra" },
            new Automobile { Make = "Chevy",   Model = "Malibu" },
        };

        private static List<Automobile> _cars3 = new List<Automobile>
        {
            new Automobile { Make = "Chevy",   Model = "Camaro", Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 120M }} },
            new Automobile { Make = "Pontiac", Model = "Firebird", Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 180M }, new Ticket { Location = "CA", Amount = 100M }} },
            new Automobile { Make = "Dodge",   Model = "Charger" },
            new Automobile { Make = "Chevy",   Model = "Corvette" },
            new Automobile { Make = "Ford",    Model = "Cobra" },
            new Automobile { Make = "Chevy",   Model = "Malibu" },
        };

        private static List<Automobile> _cars4 = new List<Automobile>
        {
            new Automobile { Make = "Chevy",   Model = "Camaro", Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 180M }, new Ticket { Location = "CA", Amount = 100M }} },
            new Automobile { Make = "Pontiac", Model = "Firebird" },
            new Automobile { Make = "Dodge",   Model = "Charger" },
            new Automobile { Make = "Chevy",   Model = "Corvette", Tickets = new List<Ticket> {new Ticket { Location = "CA", Amount = 300M }, new Ticket { Location = "WA", Amount = 400M }} },
            new Automobile { Make = "Ford",    Model = "Cobra" },
            new Automobile { Make = "Chevy",   Model = "Malibu" },
        };

        private static List<Automobile> _cars5 = new List<Automobile>
        {
            new Automobile { Make = "Chevy",   Model = "Camaro",   Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 120M }} },
            new Automobile { Make = "Pontiac", Model = "Firebird", Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 180M }, new Ticket { Location = "CA", Amount = 100M }} },
            new Automobile { Make = "Dodge",   Model = "Charger",  Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 180M }, new Ticket { Location = "CA", Amount = 100M }} },
            new Automobile { Make = "Chevy",   Model = "Corvette", Tickets = new List<Ticket> {new Ticket { Location = "CA", Amount = 300M }, new Ticket { Location = "OR", Amount = 400M }} },
            new Automobile { Make = "Ford",    Model = "Cobra",    Tickets = new List<Ticket> {new Ticket { Location = "WA", Amount = 180M }, new Ticket { Location = "CA", Amount = 100M }} },
            new Automobile { Make = "Chevy",   Model = "Malibu",   Tickets = new List<Ticket> {new Ticket { Location = "CO", Amount = 180M }, new Ticket { Location = "MA", Amount = 100M }} },
        };

        private class Automobile
        {
            public string       Make    { get; set; } = "";
            public string       Model   { get; set; } = "";
            public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        }

        private class Ticket
        {
            public string  Location  { get; set; } = "";
            public decimal Amount    { get; set; }
        }

        private static object CreateTestData(object obj)
        {
            var json = JObject.FromObject(obj).ToString();
            
            return json.ToJsonObject();
        }

        private static bool EvaluateToBool(string sExpr)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse(sExpr);
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy"}, new { Model = "Pontiac"} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            return expression.EvaluateToBool(context);
        }

        private static bool EvaluateToBool(string sExpr, IList<object> list)
        {
            var parser     = new JTranParser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse(sExpr);
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { list = list } ), extensionFunctions: Transformer.CompileFunctions(null));
   
            return expression.EvaluateToBool(context);
        }

        #endregion
    }
}
