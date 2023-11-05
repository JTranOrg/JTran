using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JTran.UnitTests
{
    [TestClass]
    public class TransformerTests
    {
        [TestMethod]
        public void Transformer_Transform_Success()
        {
            var transformer = new JTran.Transformer(_transform1, null);
            var result      = transformer.Transform(_data1, null);
   
            Assert.AreNotEqual(_transform1, _data1);
        }       
        
        [TestMethod]
        public void Transformer_Transform_Issue_58()
        {
            var transformer = new JTran.Transformer(_transform58, new object[] {});
            var context     = new TransformerContext { Arguments = new Dictionary<string, object>() };
            var result      = transformer.Transform(_source58, context);
   
            Assert.AreNotEqual(_transform58, _source58);
        }

        #region ForEach

        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach1, null);
            var result      = transformer.Transform(_data2);
   
            Assert.AreNotEqual(_transformForEach1, _data2);

            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            Assert.AreEqual("Linda",              customers.SpecialCustomer);
            Assert.AreEqual(4,               customers.Customers.Count);
            Assert.AreEqual("John",          customers.Customers[0].FirstName);
            Assert.AreEqual("Smith",         customers.Customers[0].LastName);
            Assert.AreEqual(34,              customers.Customers[0].Age);
            Assert.AreEqual("123 Elm St",    customers.Customers[0].Address);
                                             
            Assert.AreEqual("Mary",          customers.Customers[1].FirstName);
            Assert.AreEqual("Smith",         customers.Customers[1].LastName);
            Assert.AreEqual(32,              customers.Customers[1].Age);
            Assert.AreEqual("123 Elm St",    customers.Customers[1].Address);
                                             
            Assert.AreEqual("Fred",          customers.Customers[2].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[2].LastName);
            Assert.AreEqual(41,              customers.Customers[2].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[2].Address);
                                             
            Assert.AreEqual("Linda",         customers.Customers[3].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[3].LastName);
            Assert.AreEqual(39,              customers.Customers[3].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[3].Address);
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach2_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach2, null);
            var result      = transformer.Transform(_data3);
   
            Assert.AreNotEqual(_transformForEach2, _data3);

            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            Assert.AreEqual(4,               customers.Customers.Count);
            Assert.AreEqual("John",          customers.Customers[0].FirstName);
            Assert.AreEqual("Smith",         customers.Customers[0].LastName);
            Assert.AreEqual(34,              customers.Customers[0].Age);
            Assert.AreEqual("123 Elm St",    customers.Customers[0].Address);
                                             
            Assert.AreEqual("Mary",          customers.Customers[1].FirstName);
            Assert.AreEqual("Smith",         customers.Customers[1].LastName);
            Assert.AreEqual(32,              customers.Customers[1].Age);
            Assert.AreEqual("123 Elm St",    customers.Customers[1].Address);
                                             
            Assert.AreEqual("Fred",          customers.Customers[2].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[2].LastName);
            Assert.AreEqual(41,              customers.Customers[2].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[2].Address);
                                             
            Assert.AreEqual("Linda",         customers.Customers[3].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[3].LastName);
            Assert.AreEqual(39,              customers.Customers[3].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[3].Address);
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach3_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach3, null);
            var result      = transformer.Transform(_data4, null);
   
            Assert.AreNotEqual(_transformForEach3, _data4);

            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            Assert.AreEqual(2,               customers.Customers.Count);
            Assert.AreEqual("Fred",          customers.Customers[0].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[0].LastName);
            Assert.AreEqual(41,              customers.Customers[0].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[0].Address);
                                             
            Assert.AreEqual("Linda",         customers.Customers[1].FirstName);
            Assert.AreEqual("Anderson",      customers.Customers[1].LastName);
            Assert.AreEqual(39,              customers.Customers[1].Age);
            Assert.AreEqual("375 Maple Ave", customers.Customers[1].Address);
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach_noarray_Success()
        {
            var transformer = new JTran.Transformer(_transformForEachNoArray, null);
            var result      = transformer.Transform(_data5);
   
            Assert.AreNotEqual(_transformForEachNoArray, _data5);

            var json = JObject.Parse(result);

            Assert.AreEqual("Camaro", json["Owner"]["Cars"]["Chevy"]["Model"].ToString());
            Assert.AreEqual("Firebird", json["Owner"]["Cars"]["Pontiac"]["Model"].ToString());
            Assert.AreEqual("Charger", json["Owner"]["Cars"]["Dodge"]["Model"].ToString());
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach_nested__Success()
        {
            var transformer = new JTran.Transformer(_transformNestedForEach, null);
            var result      = transformer.Transform(_dataNested, null);
   
            Assert.AreNotEqual(_transformNestedForEach, _dataNested);

            var roster = JsonConvert.DeserializeObject<Roster>(result);

            Assert.AreEqual(3,               roster.Owner.Cars.Count);
            Assert.AreEqual("Chevy",         roster.Owner.Cars[0].Make);
            Assert.AreEqual("Camaro",        roster.Owner.Cars[0].Model);
            Assert.AreEqual(3,               roster.Owner.Cars[0].Mechanics.Count);
            Assert.AreEqual("Bob",           roster.Owner.Cars[0].Mechanics[0].FirstName);
            Assert.AreEqual("Mendez",        roster.Owner.Cars[0].Mechanics[0].LastName);
        }
        
        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach_nested_single_innner__Success()
        {
            var transformer = new JTran.Transformer(_transformNestedForEach, null);
            var result      = transformer.Transform(_dataNested2, null);
   
            Assert.AreNotEqual(_transformNestedForEach, _dataNested2);

            var roster = JsonConvert.DeserializeObject<Roster>(result);

            Assert.AreEqual(3,               roster.Owner.Cars.Count);
            Assert.AreEqual("Chevy",         roster.Owner.Cars[0].Make);
            Assert.AreEqual("Camaro",        roster.Owner.Cars[0].Model);
            Assert.AreEqual(1,               roster.Owner.Cars[0].Mechanics.Count);
            Assert.AreEqual("Bob",           roster.Owner.Cars[0].Mechanics[0].FirstName);
            Assert.AreEqual("Mendez",        roster.Owner.Cars[0].Mechanics[0].LastName);
        }
        
        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_null_data_Success()
        {
            var transformer = new JTran.Transformer(_transformForEachNoArray, null);
            var result      = transformer.Transform(_dataNull);
   
            Assert.AreNotEqual(_transformForEachNoArray, _dataNull);

            var json = JObject.Parse(result);

            Assert.AreEqual("Camaro", json["Owner"]["Cars"]["Chevy"]["Model"].ToString());
            Assert.AreEqual("Firebird", json["Owner"]["Cars"]["Pontiac"]["Model"].ToString());

            var dodgeModel = json["Owner"]["Cars"]["Dodge"]["Model"];

            Assert.AreEqual(null, dodgeModel.Values().FirstOrDefault());
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public void Transformer_Transform_ForEach_break_Success()
        {
            var transformer = new JTran.Transformer(_transformForEachBreak, null);
            var result      = transformer.Transform(_data4, null);
   
            Assert.AreNotEqual(_transformForEachBreak, _data4);

            var customers = JsonConvert.DeserializeObject<CustomerContainer>(result);

            Assert.AreEqual(1,       customers.Customers.Count);
            Assert.AreEqual("John",  customers.Customers[0].FirstName);
            Assert.AreEqual("Smith", customers.Customers[0].LastName);
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public async Task Transformer_Transform_ForEach_list_Success()
        {
            var list = new List<Automobile>
            {
                new Automobile
                {
                    Make  = "Chevrolet",
                    Model = "Corvette",
                    Year  = 1956,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Pontiac",
                    Model = "Firebird",
                    Year  = 1969,
                    Color = "Green",
                },
                new Automobile
                {
                    Make  = "Chevrolet",
                    Model = "Camaro",
                    Year  = 1970,
                    Color = "Black",
                }
            };

            var transformer = new JTran.Transformer(_transformList, null);
            var result = "";

            using(var output = new MemoryStream())
            { 
                transformer.Transform(list, "Automobiles", output);

                result = await output.ReadStringAsync();
            }

            var owner = JsonConvert.DeserializeObject<Owner2>(result);

            Assert.IsNotNull(owner);
            Assert.IsNotNull(owner.Cars);
            Assert.AreEqual(3, owner.Cars.Count);

            Assert.AreEqual("Chevrolet", owner.Cars[0].Brand);
            Assert.AreEqual("Corvette",  owner.Cars[0].Model);
            Assert.AreEqual(1956,        owner.Cars[0].Year);

            Assert.AreEqual("Pontiac",   owner.Cars[1].Brand);
            Assert.AreEqual("Firebird",  owner.Cars[1].Model);
            Assert.AreEqual(1969,        owner.Cars[1].Year);

            Assert.AreEqual("Chevrolet", owner.Cars[2].Brand);
            Assert.AreEqual("Camaro",    owner.Cars[2].Model);
            Assert.AreEqual(1970,        owner.Cars[2].Year);
        }

        private IEnumerable<Automobile> GetList()
        {
            yield return new Automobile
            {
                Make  = "Chevrolet",
                Model = "Corvette",
                Year  = 1956,
                Color = "Blue",
            };

            yield return new Automobile
            {
                Make  = "Pontiac",
                Model = "Firebird",
                Year  = 1969,
                Color = "Green",
            };

            yield return new Automobile
            {
                Make  = "Chevrolet",
                Model = "Camaro",
                Year  = 1970,
                Color = "Black",
            };
        }

        [TestMethod]
        [TestCategory("ForEach")]
        public async Task Transformer_Transform_ForEach_yielded_list_Success()
        {
            var list = GetList();

            var transformer = new JTran.Transformer(_transformList, null);
            var result = "";

            using(var output = new MemoryStream())
            { 
                transformer.Transform(list, "Automobiles", output);

                result = await output.ReadStringAsync();
            }

            var owner = JsonConvert.DeserializeObject<Owner2>(result);

            Assert.IsNotNull(owner);
            Assert.IsNotNull(owner.Cars);
            Assert.AreEqual(3, owner.Cars.Count);

            Assert.AreEqual("Chevrolet", owner.Cars[0].Brand);
            Assert.AreEqual("Corvette",  owner.Cars[0].Model);
            Assert.AreEqual(1956,        owner.Cars[0].Year);

            Assert.AreEqual("Pontiac",   owner.Cars[1].Brand);
            Assert.AreEqual("Firebird",  owner.Cars[1].Model);
            Assert.AreEqual(1969,        owner.Cars[1].Year);

            Assert.AreEqual("Chevrolet", owner.Cars[2].Brand);
            Assert.AreEqual("Camaro",    owner.Cars[2].Model);
            Assert.AreEqual(1970,        owner.Cars[2].Year);
        }

        private static readonly string _transformForEachBreak =
        "{" + 
         "    '#foreach(Customers, Customers)':" + 
         "    {" + 
         "        LastName:    '#(Surname)'," + 
         "        FirstName:   '#(Name)'," + 
         "        Age:         '#(Age)'," + 
         "        Address:     '#(Address)'," + 
         "        '#break':     ''" + 
         "    }" + 
         "}";        
         
         private static readonly string _transformForEachContinue =
        "{" + 
        "      '#variable(John)': 'John'," + 
         "    '#foreach(Customers, Customers)':" + 
         "    {" + 
         "        '#if(Name == $John)': '#continue'," + 
         "        LastName:    '#(Surname)'," + 
         "        FirstName:   '#(Name)'," + 
         "        Age:         '#(Age)'," + 
         "        Address:     '#(Address)'" + 
         "    }" + 
         "}";

        #endregion

        [TestMethod]
        public void Transformer_Transform_Email_Fails()
        {
            var transformer = new JTran.Transformer(_transformEmail, null);

             Assert.ThrowsException<Transformer.SyntaxException>( ()=> transformer.Transform(_dataEmail) );
        }

        [TestMethod]
        public void Transformer_Transform_Map_Fails()
        {
             Assert.ThrowsException<Transformer.SyntaxException>( ()=> new JTran.Transformer(_mapError, null) );
        }

        #region ExtensionFunction

        [TestMethod]
        public void Transformer_Transform_ExtensionFunction_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformExtFunction, new object[] { new ExtFunctions() } );
            var result      = transformer.Transform(_data5);
   
            Assert.AreNotEqual(_transformExtFunction, _data5);

            var json = JObject.Parse(result);

            Assert.AreEqual("xCamaro",   json["Owner"]["Cars"]["Chevy"]["Model"].ToString());
            Assert.AreEqual("xFirebird", json["Owner"]["Cars"]["Pontiac"]["Model"].ToString());
            Assert.AreEqual("xCharger",  json["Owner"]["Cars"]["Dodge"]["Model"].ToString());
            Assert.AreEqual("yGreen",    json["Owner"]["Cars"]["Chevy"]["Color"].ToString());
            Assert.AreEqual("yBlue",     json["Owner"]["Cars"]["Pontiac"]["Color"].ToString());
            Assert.AreEqual("yBlack",    json["Owner"]["Cars"]["Dodge"]["Color"].ToString());
        }

        [TestMethod]
        public void Transformer_Transform_ExtensionFunction_ClassParam_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformExtFunction2, new object[] { new ExtFunctions2() } );
            var result      = transformer.Transform(_data5);
   
            Assert.AreNotEqual(_transformExtFunction2, _data5);

            var json = JObject.Parse(result);

            Assert.AreEqual("Camaro",   json["Owner"]["Cars"]["Chevy"]["Model"].ToString());
            Assert.AreEqual("Firebird", json["Owner"]["Cars"]["Pontiac"]["Model"].ToString());
            Assert.AreEqual("Charger",  json["Owner"]["Cars"]["Dodge"]["Model"].ToString());
            Assert.AreEqual("Green",    json["Owner"]["Cars"]["Chevy"]["Color"].ToString());
            Assert.AreEqual("Blue",     json["Owner"]["Cars"]["Pontiac"]["Color"].ToString());
            Assert.AreEqual("Black",    json["Owner"]["Cars"]["Dodge"]["Color"].ToString());
        }

        #endregion

        [TestMethod]
        public void Transformer_Transform_datetimeformat_Succeeds()
        {
            var transformer = new JTran.Transformer(_datetimeformat, null);
            var result      = transformer.Transform(_datetimeformatData);
   
            Assert.AreNotEqual(_transformNullReference, _dataNullReference);

            var json = JObject.Parse(result);

            Assert.AreEqual("1:48:52.0000", json["Time"].ToString());
        }

        /*[TestMethod]
        public void Transformer_Transform_removeany_Succeeds()
        {
            var transformer = new JTran.Transformer(_removeany, null);
            var result      = transformer.Transform(_removeanyData);
   
            Assert.AreNotEqual(_transformNullReference, _dataNullReference);

            var json = JObject.Parse(result);

            Assert.AreEqual("5554561234", json["Phone"].ToString());
        }*/

        private static readonly string _datetimeformat =
        "{ Time: \"#(formatdatetime(Time, 'h:mm:ss.ffff'))\" }";

        private static readonly string _datetimeformatData =
        @"{
             Time: '2020-11-12T01:48:52.00000'
        }";

        [TestMethod]
        public void Transformer_Transform_null_reference_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformNullReference, null);
            var result      = transformer.Transform(_dataNullReference);
   
            Assert.AreNotEqual(_transformNullReference, _dataNullReference);

            var json = JObject.Parse(result);

            Assert.AreEqual(null, json["Owner"]["Cars"]["Chevy"]);
        }

        [TestMethod]
        public void Transformer_Transform_null_reference2_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformNullReference2, null);
            var result      = transformer.Transform(_dataNullReference);
   
            Assert.AreNotEqual(_transformNullReference2, _dataNullReference);

            var json = JObject.Parse(result);

            Assert.AreEqual(null, json["Owner"]["Cars"]["Chevy"]);
        }

        [TestMethod]
        public void Transformer_Transform_null_reference3_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformNullReference3, null);
            var result      = transformer.Transform(_dataNullReference3);
   
            Assert.AreNotEqual(_transformNullReference3, _dataNullReference3);

            var json = JObject.Parse(result);

            Assert.AreEqual(null, json["Owner"]["Cars"]["Chevy"]);
        }

        [TestMethod]
        public void Transformer_Transform_bool_var_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformBool, null);
            var result      = transformer.Transform(_dataBool);
   
            Assert.AreNotEqual(_transformBool, _dataBool);

            var json = JObject.Parse(result);

            Assert.AreEqual("Camaro", json["Owner"]["Cars"]["Chevy"]["Model"].ToString());
        }

        #region Scope Symbol

        [TestMethod]
        public void Transformer_Transform_scope_symbol_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformScopeSymbol, null);
            var result      = transformer.Transform(_dataNull);
   
            Assert.AreNotEqual(_transformScopeSymbol, _dataNull);

            var json = JObject.Parse(result);

            Assert.AreEqual("Camaro", json["Cars"][0]["Details"]["Model"].ToString());
        }

        private static readonly string _transformScopeSymbol = 
        @"{
             '#foreach(Automobiles, Cars)':
             {
                '#variable(this)':   '#(@)',

                'Owner':        '#(//Owner)',
                'Details':      '#copyof($this)' 
            }
        }";

        #endregion

        [TestMethod]
        public void Transformer_Transform_tertiary_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformTertiary, null);
            var result      = transformer.Transform(_dataNull);
   
            Assert.AreNotEqual(_transformTertiary, _dataNull);

            var json = JObject.Parse(result);

            Assert.AreEqual("abc", json["MyProp"].ToString());
        }
        
        #region ForEachGroup

        [TestMethod]
        public void Transformer_Transform_ForEachGroup_Success()
        {
            var transformer = new JTran.Transformer(_transformForEachGroup1, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformForEachGroup1, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(_resultForEachGroup1), JObject.Parse(result)));
        }

        [TestMethod]
        public void Transformer_Transform_ForEachGroup_variables_Success()
        {
            var transformer = new JTran.Transformer(_transformForEachGroup2, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(_resultForEachGroup2), JObject.Parse(result)));
        }

        [TestMethod]
        public void Transformer_Transform_ForEachGroup_variable2s_Success()
        {
            var transformer = new JTran.Transformer(LoadSample("only1_group.jtran"), null);
            var result      = transformer.Transform(LoadSample("only1_group.json"));
   
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(_resultForEachGroup3), JObject.Parse(result)));
        }

        #endregion

        #region Template

        [TestMethod]
        [TestCategory("Template")]
        public void Transformer_Transform_Template_Success()
        {
            var transformer = new JTran.Transformer(_transformTemplate2, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformTemplate2, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob\", Year: 1965 }"), JObject.Parse(result)));
        }

        #region Call template with variable

        [TestMethod]
        [TestCategory("Template")]
        public void Transformer_Transform_Template_var_Success()
        {
            var transformer = new JTran.Transformer(_transformTemplateVar, null);
            var result      = transformer.Transform(_dataForEachCars);
   
            Assert.AreNotEqual(_transformTemplateVar, _dataForEachCars);
        }

        private static readonly string _transformTemplateVar =
        @"{
             '#variable(mycars)': '#(Cars)',

             '#template(cars, somecars)': 
             {
                '#foreach($somecars, Cars)':
                {
                   '#noobject': '#copyof(@)'
                }
             },

            '#calltemplate(cars)':
            {
                somecars: '#($mycars)'
            }
        }";

        private static readonly string _dataForEachCars = 
        @"{
            Cars:
            [
                {
                    Make:      'Chevy',
                    Model:     'Corvette',
                    Year:      1964,
                    Color:     'Blue'
                },
                {
                    Make:      'Pontiac',
                    Model:     'Firebird',
                    Year:      1964,
                    Color:     'Blue'
                }
            ]
        }";

        #endregion

        [TestMethod]
        [TestCategory("Template")]
        public void Transformer_Transform_Template_aftercall_Success()
        {
            var transformer = new JTran.Transformer(_transformTemplate3, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformTemplate3, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob\", Year: 1965 }"), JObject.Parse(result)));
        }

        [TestMethod]
        [TestCategory("Template")]
        public void Transformer_Transform_Template_scoped_Success()
        {
            var transformer = new JTran.Transformer(_transformTemplate4, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformTemplate4, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ inner: { FirstName: \"bob\", Year: 1965 } }"), JObject.Parse(result)));
        }

        [TestMethod]
        [TestCategory("Template")]
        public void Transformer_Transform_Template_twice_Success()
        {
            var transformer = new JTran.Transformer(_transformTemplateTwice, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformTemplateTwice, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ First: { FirstName: \"bob\", Year: 1965 },  Second: { FirstName: \"fred\", Year: 1965 } }"), JObject.Parse(result)));
        }

        private static readonly string _transformTemplate2 =
        @"{
             '#template(DisplayName, name)': 
             {
                'FirstName':  '#($name)',
                'Year':       1965
             },

            '#calltemplate(DisplayName)':
            {
                name: 'bob'
            }
        }";

        private static readonly string _transformTemplate3 =
        @"{
            '#calltemplate(DisplayName)':
            {
                name: 'bob',
            },

            '#template(DisplayName, name)': 
            {
                'FirstName':  '#($name)',
                'Year':       1965
            }
        }";

        private static readonly string _transformTemplate4 =
        @"{
            'inner':
            {
                '#calltemplate(DisplayName)':
                {
                    name: 'bob'
                },
            },

            '#template(DisplayName, name)': 
            {
                'FirstName':  '#($name)',
                'Year':       1965
            }
        }";

        private static readonly string _transformTemplateTwice =
        @"{
             '#template(DisplayName, name)': 
             {
                'FirstName':  '#($name)',
                'Year':       1965
             },

             First:
             {
                '#calltemplate(DisplayName)':
                {
                    name: 'bob'
                }
             },

             Second:
             {
                '#calltemplate(DisplayName)':
                {
                    name: 'fred'
                }
             }
        }";


        #endregion

        #region Function

        [TestMethod]
        [TestCategory("Function")]
        public void Transformer_Transform_Function_Success()
        {
            var transformer = new JTran.Transformer(_transformFunction, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformFunction, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Year: 1964 }"), JObject.Parse(result)));
        }

        [TestMethod]
        [TestCategory("Function")]
        public void Transformer_Transform_Function_after__Success()
        {
            var transformer = new JTran.Transformer(_transformFunctionAfter, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformFunctionAfter, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Year: 1964 }"), JObject.Parse(result)));
        }

        [TestMethod]
        [TestCategory("Function")]
        public void Transformer_Transform_Function2_Success()
        {
            var transformer = new JTran.Transformer(_transformFunction2, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformTemplate2, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Car: { Make: 'Chevy', Model: 'Corvette', Year: 1964 } }"), JObject.Parse(result)));
        }

        [TestMethod]
        [TestCategory("Function")]
        public void Transformer_Transform_Function_wparams_Success()
        {
            var transformer = new JTran.Transformer(_transformFunctionParams, null);
            var result      = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformFunctionParams, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Year: 1967 }"), JObject.Parse(result)));
        }

        [TestMethod]
        [TestCategory("Function")]
        public void Transformer_Transform_Function_nested_Success()
        {
            var transformSource = LoadTransform("nestedfunction");
            var transformer     = new JTran.Transformer(transformSource, null);
            var result          = transformer.Transform(_dataProducts);
            var json            = JObject.Parse(result);
   
            Assert.AreNotEqual(transformSource, _dataProducts);
            Assert.AreEqual("Topsoil", json["Products"][0]["Name"].ToString());
            Assert.AreEqual("Paint",   json["Products"][1]["Name"].ToString());
        }

        private static readonly string _dataProducts =
        @"{
	        'Products':
	        [
   		        {
       		        'Name':   'Topsoil (yards)',
       		        'UOM':    'yard'
     	        },
   		        {
       		        'Name':   'Paint (gallons)',
       		        'UOM':    'gallon'
     	        }
            ]
        }";

        private static readonly string _transformFunction =
        @"{
             '#function(BestYear)': 
             {
                return:       1964
             },

             Year: '#(BestYear(3))'
        }";

        private static readonly string _transformFunctionAfter =
        @"{
             Year: '#(BestYear(3))',

             '#function(BestYear)': 
             {
                return:       1964
             }
        }";

        private static readonly string _transformFunctionParams =
        @"{
             '#function(BestYear, add)': 
             {
                return:       '#(1964 + $add)'
             },

             Year: '#(BestYear(3))'
        }";

        private static readonly string _transformFunction2 =
        @"{
             '#function(BestCar)': 
             {  
                Make:   'Chevy',
                Model:  'Corvette',
                Year:   1964
             },

             Car:       '#(BestCar())'
        }";

        #endregion

        #region Include

        [TestMethod]
        public void Transformer_Transform_Include_Success()
        {
            var transformer = new JTran.Transformer(_transformInclude, null, new Dictionary<string, string> { {"otherfile.json", _otherFile} });
            var result      = transformer.Transform(_dataForEachGroup1, null);
   
            Assert.AreNotEqual(_transformInclude, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob\", Year: 1965 }"), JObject.Parse(result)));
        }

        [TestMethod]
        public void Transformer_Transform_Include_function_Success()
        {
            var transformer = new JTran.Transformer(_transformIncludeFunction, null, new Dictionary<string, string> { {"otherfile.json", _otherFile} });
            var result      = transformer.Transform(_dataForEachGroup1, null);
   
            Assert.AreNotEqual(_transformIncludeFunction, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ name: \"BobJones\" }"), JObject.Parse(result)));
        }

        private static readonly string _transformInclude =
        @"{
            '#include':      'otherfile.json',

            '#calltemplate(DisplayName)':
            {
                name: 'bob'
            }
        }";

        private static readonly string _transformIncludeFunction =
        @"{
            '#include':      'otherfile.json',
             '#variable(person)':
             {
                FirstName: 'Bob',
                LastName: 'Jones'
             },

             name: '#(DisplayName($person))'
        }";

        private static readonly string _otherFile =
        @"{
             '#template(DisplayName, name)': 
             {
                'FirstName':  '#($name)',
                'Year':       1965
             },

             '#function(DisplayName, person2)':
             {
                return:  '#($person2.FirstName + $person2.LastName)'
             }
        }";

        #endregion

        #region Arrays

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_noexpressions_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayRaw, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArrayRaw, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual("JohnSmith", (json["Customers"] as JArray)[0].ToString());
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_noexpressions2_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayRaw2, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArrayRaw2, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual("JohnSmith", (json["Customers"] as JArray)[0]["Name"].ToString());
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_noexpressions3_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayRaw3, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArrayRaw3, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual("John", (json["Customers"] as JArray)[0]["Names"][0].ToString());
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_expressions_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArray, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArray, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual("JohnSmith", (json["Customers"] as JArray)[0].ToString());
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_expressions2_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArray2, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArray2, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual("John", (json["Customers"] as JArray)[0]["Names"][0].ToString());
            Assert.AreEqual("Smith", (json["Customers"] as JArray)[0]["Names"][1].ToString());
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_expressions3_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformNamedArray, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformNamedArray, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual("John", (json["Customers"] as JArray)[0]["bob"][0].ToString());
            Assert.AreEqual("Smith", (json["Customers"] as JArray)[0]["bob"][1].ToString());
        }

        private static readonly string _transformArrayRaw =
        @"{
            'Customers':
            [
                'JohnSmith'
            ]
        }";

        private static readonly string _transformArrayRaw2 =
        @"{
            'Customers':
            [
                {
                    'Name': 'JohnSmith'
                }
            ]
        }";

        private static readonly string _transformArrayRaw3 =
        @"{
            'Customers':
            [
                {
                    'Names': 
                    [
                        'John',
                        'Smith'
                    ]
                }
            ]
        }";

        private static readonly string _transformArray =
        @"{
            'Customers':
            [
                '#(FirstName + LastName)'
            ]
        }";

        private static readonly string _transformArray2 =
        @"{
            'Customers':
            [
                {
                    'Names': 
                    [
                        '#(FirstName)',
                        '#(LastName)'
                    ]
                }
            ]
        }";

        private static readonly string _transformNamedArray =
        @"{
            'Customers':
            [
                {
                    '#(ArrayName)': 
                    [
                        '#(FirstName)',
                        '#(LastName)'
                    ]
                }
            ]
        }";

        #endregion

        #region Array Element

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_element_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformEmptyArray, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformEmptyArray, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual(0, (json["Customers"] as JArray).Count());
        }

        private static readonly string _transformEmptyArray =
        @"{
            '#array(Customers)':
            {
            }
        }";

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_ArrayItem_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayItem, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArrayItem, _data1);

            var json = JObject.Parse(result);

            Assert.AreEqual(2,     (json["Customers"]  as JArray).Count());
            Assert.AreEqual("bob", (json["Customers"]  as JArray)[0]["Name"]);
            Assert.AreEqual("fred", (json["Customers"] as JArray)[1]["Name"]);
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_ArrayItem2_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayItem2, null);
            var result      = transformer.Transform(_data1);
   
            Assert.AreNotEqual(_transformArrayItem2, _data1);

            var json = JObject.Parse(result);
            var array = json["Customers"] as JArray;

            Assert.AreEqual(2,      array.Count());
            Assert.AreEqual("Fred", array[0]);
            Assert.AreEqual("John",array[1]);
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_foreach_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayForEach, null);
            var context     = new TransformerContext { Arguments = (new { Fred = "Fred", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_data4, context);
   
            Assert.AreNotEqual(_transformArrayForEach, _data4);

            var json  = JObject.Parse(result);
            var array = json["Persons"]  as JArray;

            Assert.AreEqual(5,              array.Count());
            Assert.AreEqual("JohnSmith",    array[0]["Name"]);
            Assert.AreEqual("King Jalusa",  array[4]["Name"]);
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_foreach_emptyarray_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayForEachEmptyArray, null);
            var result      = transformer.Transform(_dataNoEmployees);
   
            Assert.AreNotEqual(_transformArrayForEachEmptyArray, _data4);

            var json  = JObject.Parse(result);
            var array = json["Persons"]  as JArray;

            Assert.AreEqual(2,              array.Count());
            Assert.AreEqual("JohnSmith",    array[0]["Name"]);
            Assert.AreEqual("BobJones",     array[1]["Name"]);
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_2dim_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArray2dim, null);
            var context     = new TransformerContext { Arguments = (new { Fred = "Fred", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_data4, context);
   
            Assert.AreNotEqual(_transformArray2dim, _data4);

            var json  = JObject.Parse(result);
            var array = json["Drivers"]  as JArray;
            var array2 = array[0] as JArray;

            Assert.AreEqual(4, array2.Count);
            Assert.AreEqual("John", array2[0]["Name"].ToString());
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_2dim2_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArray2dim2, null);
            var context     = new TransformerContext { };
            var result      = transformer.Transform(_data2dim2, context);
   
            Assert.AreNotEqual(_transformArray2dim, _data2dim2);

            var json  = JObject.Parse(result);
            var array = json["Cells"]  as JArray;
            var array2 = array[0] as JArray;

            Assert.AreEqual(3, array2.Count);
            Assert.AreEqual(1, array2[0]);
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_2dim_foreach_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArray2dimforeach, null);
            var context     = new TransformerContext { };
            var result      = transformer.Transform(_data2dimforeach, context);
   
            Assert.AreNotEqual(_transformArray2dimforeach, _data2dimforeach);

            var json  = JObject.Parse(result);
            var array = json["Cells"]  as JArray;
            var array2 = array[0] as JArray;

            Assert.AreEqual(3, array2.Count);
            Assert.AreEqual(1, array2[0]);
        }

        [TestMethod]
        [TestCategory("Array")]
        public void Transformer_Transform_Array_foreach_sequence_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformArrayForEachSequence, null);
            var context     = new TransformerContext { Arguments = (new { prefix = "item", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_data4, context);
   
            Assert.AreNotEqual(_transformArrayForEachSequence, _data4);

            var json  = JObject.Parse(result);
            var array = json["Items"]  as JArray;

            Assert.AreEqual(5,        array.Count());
            Assert.AreEqual("item1",  array[0]);
            Assert.AreEqual("item2",  array[1]);
            Assert.AreEqual("item3",  array[2]);
            Assert.AreEqual("item4",  array[3]);
            Assert.AreEqual("item5",  array[4]);
        }

        private static readonly string _transformArrayItem =
        @"{
            '#array(Customers)':
            {
                '#arrayitem(1)':
                {
                   'Name': 'bob'
                },
                '#arrayitem(2)':
                {
                   'Name': 'fred'
                }
            }
        }";

        private static readonly string _transformArrayItem2 =
        @"{
            '#array(Customers)':
            {
                '#arrayitem(1)':    'Fred',
                '#arrayitem(2)':    '#(FirstName)'
            }
        }";

        private static readonly string _transformArrayForEach =
        @"{
            '#array(Persons)':
            {
                '#foreach(Customers, {})':
                {
                   'Name': '#(Name + Surname)'
                },
                '#if(any(Customers[Name == $Fred]))':
                {
                    '#arrayitem':
                    {
                        'Name': 'King Jalusa'
                    }
                },
                '#if(any(Customers[Name == $Dude]))':
                {
                    '#arrayitem':
                    {
                        'Name': 'King Krakatoa'
                    }
                }
            }
        }";

        private static readonly string _transformArrayForEachEmptyArray =
        @"{
            '#variable(Find)': 'Fred',
            '#array(Persons)':
            {
                '#foreach(Customers, {})':
                {
                   'Name': '#(Name + Surname)'
                },
                '#foreach(Employees[Name == $Find], {})':
                {
                   'Name': '#(Name + Surname)'
                }
            }
        }";

        private static readonly string _transformArrayForEachSequence =
        @"{
            '#array(Items)':
            {
                '#foreach(sequence(1, 5))':
                {
                   '#arrayitem':    '#($prefix + @)'
                }
            }
        }";

       private static readonly string _transformArray2dim =
        @"{
            'Drivers':
            [
                '#(Customers)',
                '#($Fred)'
            ]
        }";

       private static readonly string _transformEmptyObject =
        @"{
            'Drivers': {},
            'Cars': []
          }";

       private static readonly string _transformArray2dim2 =
        @"{
            '#array(Cells)':
            {
                '#arrayitem':    '#(Columns)'
            }
        }";

        private static readonly string _transformArray2dimforeach =
        @"{
            '#array(Cells)':
            {
                '#foreach(Rows)':
                {
                    '#arrayitem':    '#(@)'
                }
            }
        }";

        private static readonly string _data2dim2 =
        @"{
            Columns:  [1, 2, 3]
          }";

        private static readonly string _data2dimforeach =
        @"{
            Rows: 
            [
                [1, 2, 3],
                [4, 5, 6],
                [7, 8, 9]
           ]
          }";

        #endregion

        #region Sort

        [TestMethod]
        public void Transformer_Transform_Sort_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformSort, null);
            var context     = new TransformerContext { Arguments = (new { Fred = "Fred", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_data4, context);
   
            Assert.AreNotEqual(_transformSort, _data4);

            var json  = JObject.Parse(result);
            var array = json["Persons"]  as JArray;

            Assert.AreEqual(4,                  array.Count());
            Assert.AreEqual("FredAnderson",     array[0]["Name"]);
            Assert.AreEqual("JohnSmith",        array[1]["Name"]);
            Assert.AreEqual("LindaAnderson",    array[2]["Name"]);
            Assert.AreEqual("MarySmith",        array[3]["Name"]);
        }

        [TestMethod]
        public void Transformer_Transform_Sort_desc_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformSortDesc, null);
            var context     = new TransformerContext { Arguments = (new { Fred = "Fred", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_data4, context);
   
            Assert.AreNotEqual(_transformSortDesc, _data4);

            var json  = JObject.Parse(result);
            var array = json["Persons"]  as JArray;

            Assert.AreEqual(4,                  array.Count());
            Assert.AreEqual("FredAnderson",     array[3]["Name"]);
            Assert.AreEqual("JohnSmith",        array[2]["Name"]);
            Assert.AreEqual("LindaAnderson",    array[1]["Name"]);
            Assert.AreEqual("MarySmith",        array[0]["Name"]);
        }

        [TestMethod]
        public void Transformer_Transform_Sort_2fields_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformSort2fields, null);
            var context     = new TransformerContext { Arguments = (new { Fred = "Fred", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_data4, context);
   
            Assert.AreNotEqual(_transformSort2fields, _data4);

            var json  = JObject.Parse(result);
            var array = json["Persons"]  as JArray;

            Assert.AreEqual(4,                  array.Count());
            Assert.AreEqual("FredAnderson",     array[0]["Name"]);
            Assert.AreEqual("LindaAnderson",    array[1]["Name"]);
            Assert.AreEqual("JohnSmith",        array[2]["Name"]);
            Assert.AreEqual("MarySmith",        array[3]["Name"]);
        }

        [TestMethod]
        public void Transformer_Transform_Sort_3fields_Succeeds()
        {
            var transformer = new JTran.Transformer(_transformSort3fields, null);
            var context     = new TransformerContext { Arguments = (new { Fred = "Fred", Dude = "Jabberwocky" }).ToDictionary()};
            var result      = transformer.Transform(_dataSort3fields, context);
   
            Assert.AreNotEqual(_transformSort3fields, _dataSort3fields);

            var json  = JObject.Parse(result);
            var array = json["Persons"]  as JArray;

            Assert.AreEqual(5,                  array.Count());
            Assert.AreEqual("FredAnderson41",     array[0]["Name"]);
            Assert.AreEqual("LindaAnderson39",    array[1]["Name"]);
            Assert.AreEqual("JohnSmith34",        array[2]["Name"]);
            Assert.AreEqual("MarySmith32",        array[3]["Name"]);
            Assert.AreEqual("MarySmith51",        array[4]["Name"]);
        }

        private static readonly string _transformSort =
        "{ '#foreach(sort(Customers, \"Name\"), Persons)': { 'Name': '#(Name + Surname)' }  }";

        private static readonly string _transformSortDesc =
        "{ '#foreach(sort(Customers, \"Name\", \"desc\"), Persons)': { 'Name': '#(Name + Surname)' }  }";

        private static readonly string _transformSort2fields =
        "{ '#foreach(sort(Customers, \"Surname\", \"asc\", \"Name\", \"asc\"), Persons)': { 'Name': '#(Name + Surname)' }  }";

        private static readonly string _transformSort3fields =
        "{ '#foreach(sort(Customers, \"Surname\", \"asc\", \"Name\", \"asc\", \"Age\", \"asc\"), Persons)': { 'Name': '#(Name + Surname + Age)' }  }";

        #endregion

        #region name() function

        [TestMethod]
        public void Transformer_Transform_Bind_name_Success()
        {
            var transformer = new JTran.Transformer(_transform6, new object[] { new ExtFunctions2() } );
            var result      = transformer.Transform(_data6);

            Assert.AreNotEqual(_transform6, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<Driver>(result);

            Assert.AreEqual("Driver",      driver.FieldName);
            Assert.AreEqual("Driver",      driver.Car.FieldName);
        }

        #endregion

        #region Content transforms
        
        [TestMethod]
        [TestCategory("Content")]
        [DataRow("CA", "West")]
        [DataRow("WA", "PNW")]
        [DataRow("OR", "PNW")]
        [DataRow("NY", "East")]
        [DataRow("MO", "Midwest")]
        [DataRow("AR", "South")]
        public void Transformer_Transform_variable_content_if(string state, string region)
        {
            var transformSource = LoadTransform("variablecontent");
            var transformer     = new JTran.Transformer(transformSource, null);
            var result          = transformer.Transform(_address.Replace("{0}", state));
            var json            = JObject.Parse(result);
   
            Assert.AreNotEqual(transformSource, _dataProducts);
            Assert.AreEqual(region, json["Region"].ToString());
        }        
        
        [TestMethod]
        [TestCategory("Content")]
        [DataRow("CA", "West")]
        [DataRow("WA", "PNW")]
        [DataRow("OR", "PNW")]
        [DataRow("NY", "East")]
        [DataRow("MO", "Midwest")]
        [DataRow("AR", "South")]
        public void Transformer_Transform_variable_content_map(string state, string region)
        {
            var transformSource = LoadTransform("map");
            var transformer     = new JTran.Transformer(transformSource, null);
            var result          = transformer.Transform(_address.Replace("{0}", state));
            var json            = JObject.Parse(result);
   
            Assert.AreNotEqual(transformSource, _dataProducts);
            Assert.AreEqual(region, json["Region"].ToString());
        }

        [TestMethod]
        [TestCategory("Content")]
        [DataRow("CA", "West", "map_expressions")]
        [DataRow("WA", "Puget Sound", "map_expressions")]
        [DataRow("OR", "PNW", "map_expressions")]
        [DataRow("NY", "East", "map_expressions")]
        [DataRow("MO", "Midwest", "map_expressions")]
        [DataRow("AR", "South", "map_expressions")]

        [DataRow("CA", "West", "map_expressions2")]
        [DataRow("WA", "Puget Sound", "map_expressions2")]
        [DataRow("OR", "PNW", "map_expressions2")]
        [DataRow("NY", "East", "map_expressions2")]
        [DataRow("MO", "Midwest", "map_expressions2")]
        [DataRow("AR", "South", "map_expressions2")]

        [DataRow("WA", "Puget Sound", "map_property")]
        public void Transformer_Transform_variable_content_map_wExpressions(string state, string region, string transform)
        {
            var transformSource = LoadTransform(transform);
            var transformer     = new JTran.Transformer(transformSource, null);
            var data            = _address.Replace("{0}", state);
            var result          = transformer.Transform(data);
            var json            = JObject.Parse(result);
   
            Assert.AreNotEqual(result, transformSource);
            Assert.AreEqual(region, json["Region"].ToString());
        }

        #endregion

        #region Bugs

        [TestMethod]
        public void Transformer_Transform_empty_object()
        {
            var transformer = new JTran.Transformer(_transformEmptyObject, new object[] { new ExtFunctions2() } );
            var result      = transformer.Transform(_data6);

            Assert.AreNotEqual(_transform6, result);

            var jobj = JObject.Parse(result);

            Assert.IsNotNull(jobj);
            Assert.IsNotNull(jobj["Drivers"]);
            Assert.IsNotNull(jobj["Cars"]);
        }

        #endregion
        
        #region Private 

        private class ExtFunctions
        {
            public string addx(string val)  { return "x" + val; }
            public string addy(string val)  { return "y" + val; }
        }

        private class ExtFunctions2
        {
            public string CarModel(Automobile val)  { return val.Model; }
        }

        private static string ForComparison(string str)
        {
            return str.Replace("\"", "").Replace("'", "").Replace(" ", "").Replace("\r", "").Replace("\n", "");
        }

        #region Transforms 

        private static readonly string _transform1 =
        @"{
            Make:   'Chevy',
            Model:  'Corvette',
            Year:   1964,
            Color:  'Blue'
        }";       
        
        private static readonly string _source58 =
        @"{
            Car:
            {
              Make:   'Chevy',
              Model:   'Corvette'
            }
        }";       
        
        private static readonly string _transform58 =
        @"{
                Make: '#(Car.Make)'
        }";

       private static readonly string _transformForEach1 =
        @"{
            'SpecialCustomer':    '#(last(Customers.Residents).Name)',
            '#foreach(Customers.Residents, Customers)':
            {
                LastName:    '#(//Surname)',
                FirstName:   '#(Name)',
                Age:         '#(Age)',
                Address:     '#(//Address)'
            }
        }";

        private static readonly string _transformForEach2 =
        @"{
            '#foreach(Customers.Residents.Humans, Customers)':
            {
                LastName:    '#(///Surname)',
                FirstName:   '#(Name)',
                Age:         '#(Age)',
                Address:     '#(///Address)'
            }
        }";

        private static readonly string _transformForEach3 =
        "{" + 
         "    '#foreach(Customers[Surname == \"Anderson\"], Customers)':" + 
         "    {" + 
         "        LastName:    \"#(required(Surname, 'Surname is required'))\"," + 
         "        FirstName:   '#(Name)'," + 
         "        Age:         '#(Age)'," + 
         "        Address:     '#(Address)'" + 
         "    }" + 
         "}";

        private static readonly string _transformList = 
        @"{
            'Name':        'Fred',
            '#foreach(Automobiles, Cars)':
            {
                Brand:    '#(Make))',
                Model:    '#(Model)',
                Year:     '#(Year)',
                Color:    '#(Color)',
                Sponsor:  'Jimbo'
            }
        }";

        private static readonly string _transformForEachNoArray = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#foreach(Automobiles)':
                    {
                        '#(Make)':
                        {
                            Model: '#(Model)',
                            Color:  '#(Color)'
                        }
                    }
                }
            }
        }";

        private static readonly string _transformNestedForEach = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                '#foreach(Automobiles, Cars)':
                {
                    Make:  '#(Make)',
                    Model:  '#(Model)',
                    Color:  '#(Color)',
                    '#foreach(Drivers, Mechanics)':
                    {
                        FirstName:  '#(FirstName)',
                        LastName:  '#(LastName)'
                    }
                }
            }
        }";

        private static readonly string _transformExtFunction = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#foreach(Automobiles)':
                    {
                        '#(Make)':
                        {
                            Model: '#(addx(Model))',
                            Color:  '#(addy(Color))'
                        }
                    }
                }
            }
        }";

        private static readonly string _transformExtFunction2 = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#foreach(Automobiles)':
                    {
                        '#(Make)':
                        {
                            Model: '#(CarModel(@))',
                            Color:  '#(Color)'
                        }
                    }
                }
            }
        }";

        private static readonly string _transformNullReference = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#if(State != null)':
                    {
                        '#foreach(Automobiles)':
                        {
                            '#(Make)':
                            {
                                Model: '#(CarModel(@))',
                                Color:  '#(Color)'
                            }
                        }
                    }
                }
            }
        }";

        private static readonly string _transformNullReference2 = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#if(Address.State != null)':
                    {
                        '#foreach(Automobiles)':
                        {
                            '#(Make)':
                            {
                                Model: '#(CarModel(@))',
                                Color:  '#(Color)'
                            }
                        }
                    }
                }
            }
        }";

        private static readonly string _transformNullReference3 = 
        @"{
             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#if(Address.State != null)':
                    {
                        '#foreach(Automobiles)':
                        {
                            '#(Make)':
                            {
                                Model: '#(CarModel(@))',
                                Color:  '#(Color)'
                            }
                        }
                    }
                }
            }
        }";

        
        private static readonly string _mapError = 
        @"{
            'from':                 'noreply@noreply.com',
            'subject':              '#(Subject)',
            'body':                 '#(Body)',
            'Recipients':        
            {
                '#mapitem($dude)': 'bob'
            }
        }";

        private static readonly string _transformEmail = 
        @"{
            'from':                 'noreply@noreply.com',
            'subject':              '#(Subject)',
            'body':                 '#(Body)',
            'Recipients':        
            {
                '#foreach(Recipients)':
                {
                    '#(Email)':
                    {
                        'FirstName': '#(FirstName)',
                        'LastName':  '#(LastName)'
                    }
                }
            }
        }";

        private static readonly string _transform6 =
        @"{
            '#bind(Driver)':
            {
               FieldName:             '#(name())',

                Car:
                {
                    FieldName:        '#(name())',
                    '#(MakeField)':   '#(Car.Make)',
                    '#(ModelField)':  '#(Car.Model)',
                    Year:             '#(Car.Year)',
                    Color:            '#(Car.Color)',
                    Engine:
                    {
                        Displacement:   375
                    }
                }
             }
        }";
     
        private static readonly string _transformForEachGroup1 =
        @"{
            '#foreachgroup(Drivers, Make, Makes)':
            {
                Make:             '#(Make)',
                '#foreach(currentgroup(), Drivers)':
                {
                    Name:  '#(Name)',
                    Model: '#(Model)'
                }
             }
        }";
     
        private static readonly string _transformForEachGroup2 =
        @"{
            '#variable(Pontiac)':    'Pontiac',
            '#variable(Drivers)':    '#(Drivers[Make != $Pontiac])',

            '#foreachgroup($Drivers, Make, Makes)':
            {
                'Make': '#(Make)',

                '#foreach(currentgroup(), Drivers)':
                {
                    Name:  '#(Name)',
                    Model: '#(Model)'
                }
             }
        }";

        private static readonly string _resultForEachGroup1 = 
        @"{
            Makes:
            [
                {
                    Make:      'Chevy',
                    Drivers:   
                    [
                        {
                            Name:      'John Smith',
                            Model:     'Corvette',
                        },
                        {
                            Name:      'Mary Anderson',
                            Model:     'Camaro',
                        }
                    ]
                },
                {
                    Make:      'Pontiac',
                    Drivers:   
                    [
                        {
                            Name:      'Fred Jones',
                            Model:     'Firebird',
                        },
                        {
                            Name:      'Amanda Ramirez',
                            Model:     'GTO',
                        }
                    ]
                },
                {
                    Make:      'Audi',
                    Drivers:   
                    [
                        {
                            Name:      'William Lee',
                            Model:     'RS5',
                        }
                    ]
                }
            ]
        }";

        private static readonly string _resultForEachGroup2 = 
        @"{
            Makes:
            [
                {
                    Make:      'Chevy',
                    Drivers:   
                    [
                        {
                            Name:      'John Smith',
                            Model:     'Corvette',
                        },
                        {
                            Name:      'Mary Anderson',
                            Model:     'Camaro',
                        }
                    ]
                },
                {
                    Make:      'Audi',
                    Drivers:   
                    [
                        {
                            Name:      'William Lee',
                            Model:     'RS5',
                        }
                    ]
                }
            ]
        }";

        private static readonly string _resultForEachGroup3 = 
        @"{
            Makes:
            [
                {
                    Make:      'Chevy',
                    Drivers:   
                    [
                        {
                            'Name':  'Joan Baez',
                            'Make':  'Chevy',
                            'Model': 'Corvette',
                            'Year':  1956
                        }
                    ]
                }
            ]
        }";

        private static readonly string _transformBool = 
        @"{
            '#variable(isValidState)':  '#(Address.State != null)',

             'Owner':
             {
                'Name':        '#(Owner)',
                'Cars':       
                {
                    '#if($isValidState)':
                    {
                        '#foreach(Automobiles)':
                        {
                            '#(Make)':
                            {
                                Model: '#(Model)',
                                Color:  '#(Color)'
                            }
                        }
                    }
                }
            }
        }";

        private static readonly string _transformTertiary = 
        @"{
            '#variable(var1)': 'abc',
            '#variable(var2)': 'def',

            MyProp: '#((0 < 1 && 1 == 1) ? $var1 : $var2)'
          }";

        #endregion

        #region Data
        
        private static readonly string _data1 =
        @"{
            FirstName: 'John',
            LastName:  'Smith',
            ArrayName: 'bob'
        }";

        private static readonly string _data2 =
        @"{
              Customers:
              [
                  {
                      Surname:     'Smith',
                      Address:     '123 Elm St',   
                      Residents:
                     [
                        {
                             Name:   'John',
                             Age:     34
                        },
                        {
                             Name:   'Mary',
                             Age:     32
                        }
                     ]
                 },
                  {
                      Surname:     'Anderson',
                      Address:     '375 Maple Ave',   
                      Residents:
                     [
                        {
                             Name:   'Fred',
                             Age:     41
                        },
                        {
                             Name:   'Linda',
                             Age:     39
                        }
                     ]
                 }
              ]
            }";

        private static readonly string _data3 =
        @"{
              Customers:
              [
                  {
                      Surname:     'Smith',
                      Address:     '123 Elm St', 
                      Residents:
                      {
                          Humans:
                         [
                            {
                                 Name:   'John',
                                 Age:     34
                            },
                            {
                                 Name:   'Mary',
                                 Age:     32
                            }
                         ]
                      }
                 },
                  {
                      Surname:     'Anderson',
                      Address:     '375 Maple Ave',   
                      Residents:
                      {
                          Humans:
                         [
                            {
                                 Name:   'Fred',
                                 Age:     41
                            },
                            {
                                 Name:   'Linda',
                                 Age:     39
                            }
                        ]
                     }
                 }
              ]
            }";

            private static readonly string _dataNoEmployees =
            @"{
                Customers:
                [
                    {
                        Surname: 'Smith',
                        Name:    'John'
                    },
                    {
                        Surname: 'Jones',
                        Name:    'Bob'
                    }
               ],
               Employees:
               [
                 {
                     Surname: 'Anderson',
                     Name:    'Linda'
                 },
                 {
                     Surname:   'Gonzales',
                     Name:      'Pedro'
                 }
               ]
            }";

        private static readonly string _data4 =
        @"{
            Customers:
            [
                {
                    Surname:     'Smith',
                    Address:     '123 Elm St', 
                    Name:   'John',
                    Age:     34
                },
                {
                    Surname:     'Smith',
                    Address:     '123 Elm St', 
                    Name:   'Mary',
                    Age:     32
                },
                {
                    Surname:     'Anderson',
                    Address:     '375 Maple Ave',   
                    Name:   'Fred',
                    Age:     41
                },
                {
                    Surname:     'Anderson',
                    Address:     '375 Maple Ave',   
                    Name:   'Linda',
                    Age:     39
                }
            ]
        }";

        private static readonly string _dataSort3fields =
        @"{
            Customers:
            [
                {
                    Surname:     'Smith',
                    Address:     '4892 Acorn Ave', 
                    Name:   'Mary',
                    Age:     51
                },
                {
                    Surname:     'Smith',
                    Address:     '123 Elm St', 
                    Name:   'John',
                    Age:     34
                },
                {
                    Surname:     'Smith',
                    Address:     '123 Elm St', 
                    Name:   'Mary',
                    Age:     32
                },
                {
                    Surname:     'Anderson',
                    Address:     '375 Maple Ave',   
                    Name:   'Fred',
                    Age:     41
                },
                {
                    Surname:     'Anderson',
                    Address:     '375 Maple Ave',   
                    Name:   'Linda',
                    Age:     39
                }
            ]
        }";

        private static readonly string _data5 = 
        @"{
            Owner:           'Bob Smith',   
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green'
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue'
                },
                {
                    Make:     'Dodge',
                    Model:    'Charger',  
                    Color:    'Black'
                }
            ]
        }";

        private static readonly string _dataNested = 
        @"{
            Owner:           'Bob Smith',   
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green',
                    Drivers:
                    [
                        {   
                            FirstName:  'Bob',
                            LastName:   'Mendez'
                        },
                        {   
                            FirstName:  'Shirley',
                            LastName:   'Jones'
                        },
                        {   
                            FirstName:  'Jackie',
                            LastName:   'Chan'
                        }
                    ]
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue',
                    Drivers:
                    [
                        {   
                            FirstName:  'Jackie',
                            LastName:   'Mendez'
                        },
                        {   
                            FirstName:  'Bob',
                            LastName:   'Jones'
                        },
                        {   
                            FirstName:  'Bob',
                            LastName:   'Chan'
                        }
                    ]
                },
                {
                    Make:     'Dodge',
                    Model:    'Charger',  
                    Color:    'Black',
                    Drivers:
                    [
                        {   
                            FirstName:  'Robert',
                            LastName:   'Kawasaki'
                        },
                        {   
                            FirstName:  'Muriel',
                            LastName:   'Hernandez'
                        },
                        {   
                            FirstName:  'Sergei',
                            LastName:   'Korpoff'
                        }
                    ]
                }
            ]
        }";

        private static readonly string _dataNested2 = 
        @"{
            Owner:           'Bob Smith',   
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green',
                    Drivers:
                    [
                        {   
                            FirstName:  'Bob',
                            LastName:   'Mendez'
                        }
                    ]
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue',
                    Drivers:
                    [
                        {   
                            FirstName:  'Jackie',
                            LastName:   'Mendez'
                        }
                    ]
                },
                {
                    Make:     'Dodge',
                    Model:    'Charger',  
                    Color:    'Black',
                    Drivers:
                    [
                        {   
                            FirstName:  'Robert',
                            LastName:   'Kawasaki'
                        }
                    ]
                }
            ]
        }";

        private static readonly string _dataNull = 
        @"{
            Owner:           'Bob Smith',   
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green'
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue'
                },
                {
                    Make:     'Dodge',
                    Model:    null,  
                    Color:    'Black'
                }
            ]
        }";

        private static readonly string _dataNullReference = 
        @"{
            Owner:           'Bob Smith',  
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green'
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue'
                },
                {
                    Make:     'Dodge',
                    Model:    'Charger',  
                    Color:    'Black'
                }
            ]
        }";

        private static readonly string _dataNullReference3 = 
        @"{
            Owner:           'Bob Smith',  
            Address:
            {   
                State:       null,
            },
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green'
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue'
                },
                {
                    Make:     'Dodge',
                    Model:    'Charger',  
                    Color:    'Black'
                }
            ]
        }";

        private static readonly string _address = 
        @"{
            Owner:           'Bob Smith',  
            Address:
            {   
                State:       '{0}',
            },
          }";

         private static readonly string _dataBool = 
        @"{
            Owner:           'Bob Smith',  
            Address:
            {   
                State:       'Washington',
            },
            Automobiles:
            [
                {
                    Make:     'Chevy',
                    Model:    'Camaro',  
                    Color:    'Green'
                },
                {
                    Make:     'Pontiac',
                    Model:    'Firebird',  
                    Color:    'Blue'
                },
                {
                    Make:     'Dodge',
                    Model:    'Charger',  
                    Color:    'Black'
                }
            ]
        }";

        private static readonly string _dataEmail = 
        @"{
              'Body':       '<html><body>Your library book is overdue</body></html>',
              'Subject':    'Your library book is overdue',
              'Recipients': 
              [
                {
                  'FirstName':    'Fred',
                  'LastName':     'Flintstone',
                  'EmailAddress': 'fred.flintstone@bedrock.com'
                },
                {
                  'FirstName':    'Barney',
                  'LastName':     'Rubble',
                  'EmailAddress': 'barney.rubble@bedrock.com'
                }
              ]

        }";

        private static readonly string _data6 =
        @"{
            Driver:
            {
                FirstName: 'John',
                LastName:  'Smith',
                Car:   
                {
                   Make:   'Chevy',
                   Model:  'Corvette',
                   Year:   1964,
                   Color:  'Blue'
                },
                MakeField: 'Brand',
                ModelField: 'Model'
            }
        }";

        private static readonly string _dataForEachGroup1 = 
        @"{
            Drivers:
            [
                {
                    Name:      'John Smith',
                    Make:      'Chevy',
                    Model:     'Corvette',
                    Year:      1964,
                    Color:     'Blue'
                },
                {
                    Name:      'Fred Jones',
                    Make:      'Pontiac',
                    Model:     'Firebird',
                    Year:      1964,
                    Color:     'Blue'
                },
                {
                    Name:      'Mary Anderson',
                    Make:      'Chevy',
                    Model:     'Camaro',
                    Year:      1969,
                    Color:     'Green'
                },
                {
                    Name:      'Amanda Ramirez',
                    Make:      'Pontiac',
                    Model:     'GTO',
                    Year:      1971,
                    Color:     'Black'
                },
                {
                    Name:      'William Lee',
                    Make:      'Audi',
                    Model:     'RS5',
                    Year:      2023,
                    Color:     'Red'
                }
            ]
        }";

        #endregion

        #region Models

        public class Customer
        {
            public string FirstName   { get; set; }
            public string LastName    { get; set; }
            public int    Age         { get; set; }
            public string Address     { get; set; }
        }        
        
        public class Automobile
        {
            public string        Make     { get; set; }
            public string        Model    { get; set; }
            public int           Year     { get; set; }
            public string        Color    { get; set; }
        }

        public class CustomerContainer
        {
            public string SpecialCustomer     { get; set; }
            public List<Customer> Customers   { get; set; }
        }     
        
        public class Driver
        {
            public string       FieldName   { get; set; }
            public string       FirstName   { get; set; }
            public string       LastName    { get; set; }
            public Automobile2  Car         { get; set; }
        }
        
        public class Automobile2
        {
            public string        FieldName { get; set; }
            public string        Brand     { get; set; }
            public string        Model     { get; set; }
            public int           Year      { get; set; }
            public string        Color     { get; set; }
        }
        
        public class Automobile3
        {
            public string        Make       { get; set; }
            public string        Model      { get; set; }
            public int           Year       { get; set; }
            public string        Color      { get; set; }
            public IList<Driver> Mechanics  { get; set; }
        }

        public class Owner
        {
            public string            Name   { get; set; }
            public List<Automobile3> Cars   { get; set; }
        }     

        public class Owner2
        {
            public string            Name   { get; set; }
            public List<Automobile2> Cars   { get; set; }
        }     

        public class Roster
        {
            public Owner Owner  { get; set; }
        }

        #endregion

        #region Helpers

        public static string LoadTransform(string name)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(TransformerTests)).Location).SubstringBefore("\\bin");
            var path = Path.Combine(assemblyPath, "Transforms", name + ".jtran");

            return File.ReadAllText(path);
        }

        public static string LoadSample(string name)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(TransformerTests)).Location).SubstringBefore("\\bin");
            var path = Path.Combine(assemblyPath, "Samples", name);

            return File.ReadAllText(path);
        }

        #endregion

        #endregion
   
    }
}
