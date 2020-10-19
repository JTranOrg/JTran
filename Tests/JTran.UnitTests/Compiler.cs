using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran;
using JTran.Expressions;

using Moq;
using JTran.Extensions;
using System.Collections;
using System.Xml.Schema;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Expressions")]
    public class CompilerTests
    {
        [TestMethod]
        public void Compiler_StringEquality_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Name == 'Fred'"));
            var context    = new ExpressionContext(CreateTestData(new {Name = "Fred" } ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_NumberEquality_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age == 22"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22 } ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberGreaterThan_True()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age > 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22 } ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberGreaterThan_val_is_null()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age > 21"));
            var context    = new ExpressionContext(CreateTestData(new {} ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberGreaterThan_False()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age > 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 18 } ));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }
        
        [TestMethod]
        public void Compiler_NumberLessThan_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Age < 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19 } ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_complex_bool_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("$IsLicensed && Licensed && Age < 21"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, Licensed = true } ) );
   
            context.SetVariable("IsLicensed", true);

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Addition_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Year + Age"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual(2010L, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Addition_Multiple_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Year + Age - 4"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual(2006L, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Addition_concat_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("'Bob' + 'Fred'"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("BobFred", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Addition_concat2_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("'Bob' + 32 + 'Fred'"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("Bob32Fred", expression.Evaluate(context));
        }        
        
        [TestMethod]
        public void Compiler_Addition_multipart_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Customer.FirstName + Customer.LastName"));
            var context    = new ExpressionContext(CreateTestData(new {Customer = new { FirstName = "Bob", LastName = "Jones"} } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual("BobJones", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Addition_multipart2_Success()
        {
            var expression = Compile("$Customer.FirstName + $Customer.LastName");

            var tContext = new TransformerContext { Arguments = new Dictionary<string, object> 
                                                                  { 
                                                                    {"Customer", new { FirstName = "Bob", LastName = "Jones"} }
                                                                  }
                                                  };

            var context    = new ExpressionContext(CreateTestData(new {} ), "", tContext);

            Assert.IsNotNull(expression);
            Assert.AreEqual("BobJones", expression.Evaluate(context));
        }

        private IExpression Compile(string expression)
        {        
            var parser      = new Parser();
            var tokens      = parser.Parse("$Customer.FirstName + $Customer.LastName");
            var compiler    = new Compiler();

            return compiler.Compile(tokens);
        }

        [TestMethod]
        public void Compiler_Subtraction_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Year - Age"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, Year = 1988 } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(1966L, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multiplication_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Salary * Months"));
            var context    = new ExpressionContext(CreateTestData(new {Salary = 100, Months = 12 } ));

            Assert.IsNotNull(expression);
            Assert.AreEqual(1200L, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Precedence_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("100 + Salary * Months");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Salary = 100, Months = 12 } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(1300L, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Division_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Paycheck / Months"));
            var context    = new ExpressionContext(CreateTestData(new {Paycheck = 1200, Months = 12 } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(100L, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_TertiaryOperator_Left()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age >= 21 ? 'Beer' : 'Coke'");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual("Coke", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_TertiaryOperator_Complex()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Name == 'bob' && Age >= 21 ? FirstName : LastName");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Name = "bob", Age = 35, FirstName = "Robert", LastName = "Jones" } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual("Robert", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multple_Ands_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("$isactive && Type == $Type3 && $LicenseState != $CA && $LicenseState != $Unknown");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 42, Type = 2} ), "", 
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
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Husband.Birthdate > Wife.Birthdate"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1980-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = (String)null }} ));

            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_DateTime_use_string_1_is_missing_GreaterThan_true()
        {
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
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
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("max(Husband.Birthdate, Wife.Birthdate)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1986-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual("1986-04-05T10:00:00.0000000", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_DateTime_min()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("min(Husband.Birthdate, Wife.Birthdate)"));
            var context    = new ExpressionContext(CreateTestData(new { Husband = new { Birthdate = "1986-04-05T10:00:00" },
                                                                        Wife    = new { Birthdate = "1984-10-22T10:00:00" }}), 
                                                                        extensionFunctions: Transformer.CompileFunctions(null) );

            Assert.IsNotNull(expression);
            Assert.AreEqual("1984-10-22T10:00:00.0000000", expression.Evaluate(context));
        }

        #endregion

        #endregion

        #region Multipart

        [TestMethod]
        public void Compiler_Multipart_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Car.Engine.Cylinders"));
            var context    = new ExpressionContext(CreateTestData(new { Car = new { Engine = new { Cylinders = 8 } }  } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(8m, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multipart_missing_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Car.Engine.Cylinders"));
            var context    = new ExpressionContext(CreateTestData(new { Car = new { Tires = new { Tread = .5 } }  } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(null, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_Multipart_null_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("Car.Engine.Cylinders"));
            var context    = new ExpressionContext(CreateTestData(new { Car = new { Engine = (Automobile)null, Tires = new { Tread = .5 } }  } ));
   
            Assert.IsNotNull(expression);
            Assert.AreEqual(null, expression.Evaluate(context));
        }

        #endregion

        #region Parenthesis

        [TestMethod]
        public void Compiler_Parenthesis_And_True()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Age >= 21) && (State == 'WA')"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 22, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_And_False()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Age >= 21) && (State == 'WA')"));
            var context    = new ExpressionContext(CreateTestData(new {Age = 19, State = "WA" } ));
   
            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_Or_True()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Year <= 1962) || (Year >= 1969)"));
            var context    = new ExpressionContext(CreateTestData(new {Year = 1961} ));
   
            Assert.IsNotNull(expression);
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_Or_False()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var expression = compiler.Compile(parser.Parse("(Year <= 1962) || (Year >= 1969)"));
            var context    = new ExpressionContext(CreateTestData(new {Year = 1967} ));
   
            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_Parenthesis_Math__Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("(100 + Salary) * Months - 20");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Salary = 100, Months = 12} ));
   
           Assert.IsNotNull(expression);
            Assert.AreEqual(2380L, expression.Evaluate(context));
        }

        #endregion

        #region Array Indexers

        [TestMethod]
        public void Compiler_Array_Indexer_Number_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[2]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars} ));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual("Dodge", result.GetSingleValue("Make", null));
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Number_int_array_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[0]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = new List<int> {21, 35, 62} } ));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(21m, decimal.Parse(result.ToString()));
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Number_int_array_as_var_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("$Indices[0]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Model = "Chevy" } ));
   
            context.SetVariable("Indices", new List<int> {21, 35, 62} );
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(21m, decimal.Parse(result.ToString()));
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Expression_Number_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[$WhichCar]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars, WhichCar = 2} ), "", new TransformerContext { Arguments = new Dictionary<string, object> { {"WhichCar", 2}}});
   
            Assert.IsNotNull(expression);

            dynamic result = expression.Evaluate(context);

            Assert.AreEqual("Dodge", result.Make);
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Expression_Number_multilevel_var_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[$Which.Car]");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars, WhichCar = 2} ), "", new TransformerContext { Arguments = new Dictionary<string, object> { {"Which", new Dictionary<string, object> { {"Car", 2}}}}});
   
            Assert.IsNotNull(expression);

            dynamic result = expression.Evaluate(context);

            Assert.AreEqual("Dodge", result.Make);
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Expression_Where_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[Make == 'Chevy']");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ));
   
            Assert.IsNotNull(expression);

            dynamic result = expression.Evaluate(context);

            Assert.AreEqual(3, (result as IList).Count);
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Multi_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[Make == 'Chevy'].Tickets[Location == 'WA']");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars3} ));
   
            Assert.IsNotNull(expression);

            dynamic result = expression.Evaluate(context);
            decimal amount = decimal.Parse(result.Amount.ToString());

            Assert.AreEqual(120M, amount);
        }

        [TestMethod]
        public void Compiler_Array_Indexer_Multi2_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Cars[Make == 'Chevy'].Tickets[Location == 'WA']");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars4} ));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context) as IList<object>;

            Assert.IsNotNull(result);
            dynamic firstTicket = result[0];
            dynamic firstAmount = firstTicket.Amount;
            decimal dFirstAmount = decimal.Parse(firstAmount.ToString());
            dynamic secondTicket = result[1];
            dynamic secondAmount = secondTicket.Amount;
            decimal dSecondAmount = decimal.Parse(secondAmount.ToString());

            Assert.AreEqual(180M, dFirstAmount);
            Assert.AreEqual(400M, dSecondAmount);
        }

        #endregion

        #region Functions

        [TestMethod]
        public void Compiler_function_floor_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("floor(3.5)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(3M, result);
        }

        [TestMethod]
        public void Compiler_function_ceiling_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("ceiling(3.5)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(4M, result);
        }

        [TestMethod]
        public void Compiler_function_floorceiling_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("floor(ceiling(3.5))");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Cars = _cars2} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsNotNull(expression);

            var result = expression.Evaluate(context);

            Assert.AreEqual(4M, result);
        }

        [TestMethod]
        public void Compiler_function_not_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("not(Make == 'Chevy')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Make = "Chevy"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsFalse(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_normalize_space_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("normalizespace(Make + ' ' + ' Camaro ')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Make = "Chevy"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("Chevy Camaro", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_string_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("string(2 + Year)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("2012", expression.Evaluate(context).ToString());
        }

        [TestMethod]
        public void Compiler_function_lowercase_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("lowercase('ABcDeF')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("abcdef", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_uppercase_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("uppercase('ABcDeF')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("ABCDEF", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_number_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age < number(MaxAge)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "10"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_number_null_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age < number(MaxAge)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {MaxAge = "10"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_number_notnumber_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age > number(MaxAge)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "bob"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_isnumber_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("Age < (isnumber(MaxAge) ? MaxAge : 10000)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Age = 8, MaxAge = "bob"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.IsTrue(expression.EvaluateToBool(context));
        }

        [TestMethod]
        public void Compiler_function_string2_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("string(1) + string(2) + string(3)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("123", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_stringlength_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("stringlength('crop')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(4, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_substring_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("substring('franklin', 0, 5)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("frank", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_indexof_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("indexof('franklin', 'ank')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(2, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_substringafter_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("substringafter('beebop', 'bee')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("bop", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_substringafter2_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("substringafter('beebop' + Foo, 'bee')");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Foo = "Foo"} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("bopFoo", expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_multiple_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("substring(normalizespace(substringbefore(substringafter('Franklin Delano Roosevelt', 'Franklin'), 'Roosevelt')), 1, 4)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new {Year = 2010} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual("elan", expression.Evaluate(context));
        }

        #region Aggregate/Array Functions

        [TestMethod]
        public void Compiler_function_count_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("count(Cars)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy"}, new { Model = "Pontiac"} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(2, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_sum_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("sum(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1000M }, new { Model = "Pontiac", SaleAmount = 2000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(3000M, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_avg_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("avg(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(2400M, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_avg2_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("avg(SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Model = "Chevy", SaleAmount = 1200M } ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(1200M, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_max_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("max(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy",    SaleAmount = 1200M }, 
                                                                                                  new { Model = "Pontiac",  SaleAmount = 2000M},  
                                                                                                  new { Model = "Cadillac", SaleAmount = 4000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(4000M, expression.Evaluate(context));
        }

        [TestMethod]
        public void Compiler_function_min_Success()
        {
            var parser     = new Parser();
            var compiler   = new Compiler();
            var tokens     = parser.Parse("min(Cars.SaleAmount)");
            var expression = compiler.Compile(tokens);
            var context    = new ExpressionContext(CreateTestData(new { Cars = new List<object> { new { Model = "Chevy", SaleAmount = 1200M }, new { Model = "Pontiac", SaleAmount = 2000M},  new { Model = "Cadillac", SaleAmount = 4000M} }} ), extensionFunctions: Transformer.CompileFunctions(null));
   
            Assert.AreEqual(1200M, expression.Evaluate(context));
        }

        #endregion

        #endregion

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
            new Automobile { Make = "Pontiac", Model = "Firebird" },
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

        private class Automobile
        {
            public string       Make    { get; set; }
            public string       Model   { get; set; }
            public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        }

        private class Ticket
        {
            public string  Location  { get; set; }
            public decimal Amount    { get; set; }
        }

        private object CreateTestData(object obj)
        {
            return JObject.FromObject(obj).ToString().JsonToExpando();
        }
    }
}
