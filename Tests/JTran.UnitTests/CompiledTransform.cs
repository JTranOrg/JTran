using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Newtonsoft.Json;

using JTran;
using JTran.Expressions;

namespace JTran.UnitTests
{
    [TestClass]
    public class CompiledTransformTests
    {
        [TestMethod]
        public void CompiledTransform_Transform_simple_Success()
        {
            var transformer = CompiledTransform.Compile(_transform1);
            var result      = transformer.Transform(_data1, null, null);

            Assert.AreNotEqual(_transform1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var car = JsonConvert.DeserializeObject<Automobile>(result);

            Assert.AreEqual("Chevy",    car.Make);
            Assert.AreEqual("Corvette", car.Model);
            Assert.AreEqual(1964,       car.Year);
            Assert.AreEqual("Blue",     car.Color);
            Assert.IsNotNull(car.Engine);
            Assert.AreEqual("375",      car.Engine.Displacement);
        }

        [TestMethod]
        public void CompiledTransform_Transform_propnames_Success()
        {
            var transformer = CompiledTransform.Compile(_transform2);
            var result      = transformer.Transform(_data2, null, null);

            Assert.AreNotEqual(_transform2, result);
            Assert.IsNotNull(JObject.Parse(result));

            var car = JsonConvert.DeserializeObject<Automobile2>(result);

            Assert.AreEqual("Chevy",    car.Brand);
            Assert.AreEqual("Corvette", car.Model);
            Assert.AreEqual(1964,       car.Year);
            Assert.AreEqual("Blue",     car.Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_propnames2_Success()
        {
            var transformer = CompiledTransform.Compile(_transform2a);
            var result      = transformer.Transform(_data2, null, null);

            Assert.AreNotEqual(_transform2a, result);
            Assert.IsNotNull(JObject.Parse(result));

            var car = JsonConvert.DeserializeObject<Automobile2>(result);

            Assert.AreEqual("Chevy",    car.Brand);
            Assert.AreEqual("Corvette", car.Model);
            Assert.AreEqual(1964,       car.Year);
            Assert.AreEqual("Blue",     car.Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_ChildObject_Success()
        {
            var transformer =  CompiledTransform.Compile(_transform3);
            var result      = transformer.Transform(_data2, null, null);

            Assert.AreNotEqual(_transform3, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<Driver>(result);

            Assert.AreEqual("Chevy",    driver.Car.Brand);
            Assert.AreEqual("Corvette", driver.Car.Model);
            Assert.AreEqual("1964",     driver.Car.Year.ToString());
            Assert.AreEqual("Blue",     driver.Car.Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_Bind_Success()
        {
            var transformer = CompiledTransform.Compile(_transform4);
            var result      = transformer.Transform(_data3, null, null);

            Assert.AreNotEqual(_transform4, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<Driver>(result);

            Assert.AreEqual("Chevy",    driver.Car.Brand);
            Assert.AreEqual("Corvette", driver.Car.Model);
            Assert.AreEqual(1964,       driver.Car.Year);
            Assert.AreEqual("Blue",     driver.Car.Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_wArguments_Success()
        {
            var transformer = CompiledTransform.Compile(_transform5);
            var result      = transformer.Transform(_data3, new TransformerContext { Arguments = new Dictionary<string, object> { { "CarMake", "Aston-Martin" }}}, null);

            Assert.AreNotEqual(_transform5, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<Driver>(result);

            Assert.AreEqual("Aston-Martin",  driver.Car.Brand);
            Assert.AreEqual("Corvette",      driver.Car.Model);
            Assert.AreEqual("1964",          driver.Car.Year.ToString());
            Assert.AreEqual("Blue",          driver.Car.Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_ForEach_Success()
        {
            var transformer = CompiledTransform.Compile(_transformForEach1);
            var result      = transformer.Transform(_dataCarList, null, null);

            Assert.AreNotEqual(_transformForEach1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer2>(result);

            Assert.AreEqual(3, driver.Driver.Vehicles.Count);
            Assert.AreEqual("Chevy",    driver.Driver.Vehicles[0].Brand);
            Assert.AreEqual("Corvette", driver.Driver.Vehicles[0].Model);
            Assert.AreEqual(1964,       driver.Driver.Vehicles[0].Year);
            Assert.AreEqual("Blue",     driver.Driver.Vehicles[0].Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_If_Success()
        {
            var transformer = CompiledTransform.Compile(_transformIf1);
            var result      = transformer.Transform(_dataCarList, null, null);

            Assert.AreNotEqual(_transformIf1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer2>(result);

            Assert.AreEqual(1, driver.Driver.Vehicles.Count);
            Assert.AreEqual("Chevy",    driver.Driver.Vehicles[0].Brand);
            Assert.AreEqual("Corvette", driver.Driver.Vehicles[0].Model);
            Assert.AreEqual(1964,       driver.Driver.Vehicles[0].Year);
            Assert.AreEqual("Blue",     driver.Driver.Vehicles[0].Color);
        }

        [TestMethod]
        public void CompiledTransform_Transform_IfElsifElse1_Success()
        {
            var transformer = CompiledTransform.Compile(_transformElseIf1);
            var result      = transformer.Transform("{ Region: 'WA', CurrentRegion: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformIf1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Fred",       driver.Driver.FirstName);
            Assert.AreEqual("Flintstone", driver.Driver.LastName);
        }

        #region Variable

        [TestMethod]
        [TestCategory("Variable")]
        public void CompiledTransform_Transform_Variable_Success()
        {
            var transformer = CompiledTransform.Compile(_transformVariable);
            var result      = transformer.Transform("{ Region: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformVariable, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Fred",       driver.Driver.FirstName);
            Assert.AreEqual("Flintstone", driver.Driver.LastName);
        }

        [TestMethod]
        [TestCategory("Variable")]
        public void CompiledTransform_Transform_Variable2_Success()
        {
            var transformer = CompiledTransform.Compile(_transformVariable2);
            var result      = transformer.Transform("{ Region: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformVariable2, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Fred",       driver.Driver.FirstName);
            Assert.AreEqual("Flintstone", driver.Driver.LastName);
        }

        [TestMethod]
        [TestCategory("Variable")]
        public void CompiledTransform_Transform_Variable_Object_Success()
        {
            var transformer = CompiledTransform.Compile(_transformVariableObj);
            var result      = transformer.Transform("{ Region: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformVariableObj, result);

            var json = JObject.Parse(result);

            Assert.IsNotNull(json);

            Assert.AreEqual("RodrigoGuiterrez", json["Driver"]["Name"].ToString());
        }

        [TestMethod]
        [TestCategory("Variable")]
        public void CompiledTransform_Transform_Variable_Object2_Success()
        {
            var transformer = CompiledTransform.Compile(_transformVariableObj2);
            var result      = transformer.Transform("{ }", null, null);

            Assert.AreNotEqual(_transformVariableObj2, result);

            var json = JObject.Parse(result);

            Assert.IsNotNull(json);

            Assert.AreEqual("RodrigoGuiterrez", json["Driver"]["Name"].ToString());
        }

        private static readonly string _transformVariableObj =
        @"{
           '#variable(Customer)':   
           {
                FirstName:  'Rodrigo',
                LastName:   'Guiterrez',
           },

            Driver:  
            {
                'Name':      '#($Customer.FirstName + $Customer.LastName)'
            }
        }";

        private static readonly string _transformVariableObj2 =
        @"{
           '#variable(Customer)':   
           {
                Drivers:
                [
                    {
                        FirstName:  'Rodrigo',
                        LastName:   'Guiterrez'
                    }
                ]
           },

            Driver:  
            {
                'Name':      '#($Customer.Drivers[0].FirstName + $Customer.Drivers[0].LastName)'
            }
        }";

        #endregion

        [TestMethod]
        public void CompiledTransform_Transform_IfElsifElse2_Success()
        {
            var transformer = CompiledTransform.Compile(_transformElseIf1);
            var result      = transformer.Transform("{ Region: 'OR', CurrentRegion: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformIf1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Barney", driver.Driver.FirstName);
            Assert.AreEqual("Rubble", driver.Driver.LastName);
        }

        [TestMethod]
        public void CompiledTransform_Transform_IfElsifElse3_Success()
        {
            var transformer = CompiledTransform.Compile(_transformElseIf1);
            var result      = transformer.Transform("{ Region: 'CA', CurrentRegion: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformIf1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("John",   driver.Driver.FirstName);
            Assert.AreEqual("Pebble", driver.Driver.LastName);
        }

        [TestMethod]
        public void CompiledTransform_Transform_IfElsifElse4_Success()
        {
            var transformer = CompiledTransform.Compile(_transformElseIf1);
            var result      = transformer.Transform("{ Region: 'ID', CurrentRegion: 'WA', PastRegion: 'OR', FutureRegion: 'CA' }", null, null);

            Assert.AreNotEqual(_transformIf1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Frank", driver.Driver.FirstName);
            Assert.AreEqual("Sands", driver.Driver.LastName);
        }

        [TestMethod]
        public void CompiledTransform_Transform_IfElsifElse1_2Levels_Success()
        {
            var transformer = CompiledTransform.Compile(_transformElseIf2);
            var result      = transformer.Transform("{ Region: 'WA', CurrentRegion: 'WA', PastRegion: 'OR', FutureRegion: 'CA', Subregion: 'Seattle', CurrentSubregion: 'Seattle',  }", null, null);

            Assert.AreNotEqual(_transformElseIf2, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Fred",       driver.Driver.FirstName);
            Assert.AreEqual("Flintstone", driver.Driver.LastName);
        }

        [TestMethod]
        public void CompiledTransform_Transform_IfElsifElse2_2Levels_Success()
        {
            var transformer = CompiledTransform.Compile(_transformElseIf2);
            var result      = transformer.Transform("{ Region: 'WA', CurrentRegion: 'WA', PastRegion: 'OR', FutureRegion: 'CA', Subregion: 'Seattle', CurrentSubregion: 'Bellevue'  }", null, null);

            Assert.AreNotEqual(_transformElseIf2, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer>(result);

            Assert.AreEqual("Lance",   driver.Driver.FirstName);
            Assert.AreEqual("Boulder", driver.Driver.LastName);
        }

        #region CopyOf

        [TestMethod]
        [TestCategory("CopyOf")]
        public void CompiledTransform_Transform_CopyOf_Success()
        {
            var transformer = CompiledTransform.Compile(_transformCopyOf1);
            var result      = transformer.Transform(_dataCopyOf, null, null);

            Assert.AreNotEqual(_transformCopyOf1, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["Invoice"]);
            Assert.AreEqual(945, jresult["Invoice"]["Muffler"]);
            Assert.AreEqual(123, jresult["Invoice"]["Sparkplugs"]);
            Assert.AreEqual(77,  jresult["Invoice"]["Solenoid"]);
        }

        [TestMethod]
        [TestCategory("CopyOf")]
        public void CompiledTransform_Transform_CopyOf2_Success()
        {
            var transformer = CompiledTransform.Compile(_transformCopyOf2);
            var result      = transformer.Transform(_dataCopyOf2, null, null);

            Assert.AreNotEqual(_transformCopyOf2, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["Vehicles"]);
            Assert.AreEqual("Chevy", jresult["Vehicles"][0]["Stuff"]["Make"]);
        }

        [TestMethod]
        [TestCategory("CopyOf")]
        public void CompiledTransform_Transform_CopyOf3_Success()
        {
            var transformer = CompiledTransform.Compile(_transformCopyOf3);
            var result      = transformer.Transform(_dataCopyOf3, null, null);

            Assert.AreNotEqual(_transformCopyOf3, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["output"]);

            var val = jresult["output"]["modified"].Value<DateTime>();

            Assert.AreEqual("2016-07-04T17:00:00.0000000-07:00", val.ToString("o"));
        }

        private static readonly string _transformCopyOf3 =
        @"{
            'output': '#copyof(Input)'
          }";

        private static readonly string _dataCopyOf3 =
        @"{
             'Input': { 'modified': '2016-07-04T17:00:00-07:00' }
          }";

        [TestMethod]
        [TestCategory("CopyOf")]
        public void CompiledTransform_Transform_CopyOf4_Success()
        {
            var transformer = CompiledTransform.Compile(_transformCopyOf4);
            var result      = transformer.Transform(_dataCopyOf4, null, null);

            Assert.AreNotEqual(_transformCopyOf4, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["output"]);
            Assert.AreEqual("true", jresult["output"]["modified"].ToString().ToLower());
        }

        [TestMethod]
        [TestCategory("CopyOf")]
        public void CompiledTransform_Transform_CopyOf_array_Success()
        {
            var transformer = CompiledTransform.Compile(_transformCopyOf4);
            var result      = transformer.Transform(_dataCopyOf5, null, null);

            Assert.AreNotEqual(_transformCopyOf4, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["output"]);
            Assert.IsTrue(jresult["output"] is JArray);
            Assert.AreEqual("red", (jresult["output"] as JArray)[0].ToString());
        }

        [TestMethod]
        [TestCategory("CopyOf")]
        public void CompiledTransform_Transform_CopyOf_array2_Success()
        {
            var transformer = CompiledTransform.Compile(_transformCopyOf4);
            var result      = transformer.Transform(_dataCopyOf6, null, null);

            Assert.AreNotEqual(_transformCopyOf4, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["output"]);
            Assert.IsTrue(jresult["output"] is JArray);
            Assert.AreEqual("Tomato", ((jresult["output"] as JArray)[0]["RedShades"] as JArray)[2].ToString());
        }

        private static readonly string _transformCopyOf4 =
        @"{
            'output': '#copyof(Input)'
          }";

        private static readonly string _dataCopyOf4 =
        @"{
             'Input': { 'modified': true }
          }";

        private static readonly string _dataCopyOf5 =
        @"{
             'Input': 
             [
                'red',
                'green',
                'blue'
             ]
          }";

        private static readonly string _dataCopyOf6 =
        @"{
             'Input': 
             [
                {
                    'RedShades': 
                    [
                        'Pink',
                        'Bisque',
                        'Tomato'
                    ]
                 },
                {
                    'GreenShades': 
                    [
                        'Light Green',
                        'Lime',
                        'Hunter'
                    ]
                 },
                {
                    'BlueShades': 
                    [
                        'Turqoise',
                        'Light Blue',
                        'Slate'
                    ]
                 },
                
             ]
          }";

        #endregion

        [TestMethod]
        public void CompiledTransform_Transform_if_wbool_Success()
        {
            var transformer = CompiledTransform.Compile(_transformIfBool);
            var result      = transformer.Transform(_dataIfBool, new TransformerContext { Arguments = new Dictionary<string, object> { {"Licensed", true }, {"YearsDriven", 10 }, {"LicenseState", 7 }} } , null);

            Assert.AreNotEqual(_transformIfBool, result);

            var jresult = JObject.Parse(result);

            Assert.IsNotNull(jresult);

            Assert.IsNotNull(jresult["CanDrive"]);
            Assert.AreEqual("true", jresult["CanDrive"].ToString().ToLower());
        }

        private static readonly string _transformIfBool =
        @"{
            'name': 'bob',

            '#variable(Local)': true,
            '#variable(ShortHaul)': 2,
            '#variable(LongHaul)': 3,
            '#variable(MinYears)': 10,

              '#variable(Type3)':       2,
              
              '#variable(Status_Published)':    0,
              '#variable(Status_Active)':       1,
              '#variable(Status_Expired)':      2,
              
              '#variable(CA)':  1,
              '#variable(WA)':  2,
              '#variable(Unknown)':  0,
              '#variable(isactive)':  true,

                 '#if($isactive && Type == $Type3 && $LicenseState != $CA && $LicenseState != $Unknown)':
                {
                    'CanDrive': true
                }

          }";

        private static readonly string _dataIfBool =
        @"{
            
                 'Age': 42,
                 'Type': 2
               
          }";

        [TestMethod]
        public void CompiledTransform_Transform_function_position_Success()
        {
            var transformer = CompiledTransform.Compile(_transformForEach2);
            var result      = transformer.Transform(_dataCarList, null, Transformer.CompileFunctions(null));

            Assert.AreNotEqual(_transformForEach1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var driver = JsonConvert.DeserializeObject<DriverContainer2>(result);

            Assert.AreEqual(3, driver.Driver.Vehicles.Count);
            Assert.AreEqual("Chevy",    driver.Driver.Vehicles[0].Brand);
            Assert.AreEqual("Corvette", driver.Driver.Vehicles[0].Model);
            Assert.AreEqual(1964,       driver.Driver.Vehicles[0].Year);
            Assert.AreEqual("Blue",     driver.Driver.Vehicles[0].Color);

            Assert.AreEqual(0,          driver.Driver.Vehicles[0].Index);
            Assert.AreEqual(1,          driver.Driver.Vehicles[1].Index);
            Assert.AreEqual(2,          driver.Driver.Vehicles[2].Index);
        }

        [TestMethod]
        public void CompiledTransform_Transform_document_Success()
        {
            var transformer = CompiledTransform.Compile(_transformDocument1);
            var context     = new TransformerContext();
            var locations   = "{ City: 'Granitesville', Region: 'Bedrock' }";
            var repo        = new DocumentRepository();

            repo.Documents.Add("Default", locations);

            context.DocumentRepositories.Add("Locations", repo);

            var result      = transformer.Transform("{ FirstName: 'Fred', LastName: 'Flintstone' }", context, Transformer.CompileFunctions(null));

            Assert.AreNotEqual(_transformDocument1, result);
            Assert.IsNotNull(JObject.Parse(result));

            var person = JsonConvert.DeserializeObject<Person>(result);

            Assert.AreEqual("Fred",          person.FirstName);
            Assert.AreEqual("Flintstone",    person.LastName);
            Assert.AreEqual("Granitesville", person.City);
            Assert.AreEqual("Bedrock",       person.Region);
        }

        public class DocumentRepository : IDocumentRepository
        { 
            public DocumentRepository()
            {

            }
            public string GetDocument(string name)
            {
                return this.Documents[name];
            }

            public IDictionary<string, string> Documents { get; set;  } = new Dictionary<string, string>();
        }

        #region Private 

        #region Transforms 

        private static readonly string _transform1 =
        @"{
            Make:   'Chevy',
            Model:  'Corvette',
            Year:   1964,
            Color:  'Blue',
            Engine:
            {
                Displacement:   375
            }
        }";

        private static readonly string _transform2 =
        @"{
            '#(MakeField)':   '#(Car.Make)',
            '#(ModelField)':  'Corvette',
            Year:             1964,
            Color:            'Blue',
            Engine:
            {
                Displacement:   375
            }
        }";

        private static readonly string _transform2a =
        @"{
            Brand:            '#(Car.Make)',
            Model:           'Corvette',
            Year:             1964,
            Color:            'Blue',
            Engine:
            {
                Displacement:   375
            }
        }";

        private static readonly string _transform3 =
        @"{
            Car:
            {
                '#(MakeField)':   '#(Car.Make)',
                '#(ModelField)':  '#(Car.Model)',
                Year:             '#(Car.Year)',
                Color:            '#(Car.Color)',
                Engine:
                {
                    Displacement:   375
                }
             }
        }";

        private static readonly string _transform4 =
        @"{
            '#bind(Driver)':
            {
                Car:
                {
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

        private static readonly string _transform5 =
        @"{
            '#bind(Driver)':
            {
                Car:
                {
                    '#(MakeField)':   '#($CarMake)',
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

        private static readonly string _transformForEach1 =
        @"{
            Driver:
            {
                FirstName: 'Bob',
                LastName:  'Jones',
                '#foreach(Cars, Vehicles)':
                {
                    Brand:   '#(Make)',
                    Model:   '#(Model)',
                    Year:    '#(Year)',
                    Color:   '#(Color)',
                    Engine:
                    {
                        Displacement:   375
                    }
                }
            }
        }";

        private static readonly string _transformForEach2 =
        "{\r\n" + 
        "    Driver:\r\n" + 
        "    {\r\n" + 
        "        FirstName: \"Bob\",\r\n" + 
        "        LastName:  \"Jones\",\r\n" + 
        "        \"#foreach(Cars, Vehicles)\":\r\n" + 
        "        {\r\n" + 
        "             Brand:   \"#(Make)\",\r\n" + 
        "             Model:   \"#(Model)\",\r\n" + 
        "             Year:    \"#(Year)\",\r\n" + 
        "             Color:   \"#(Color)\",\r\n" + 
        "             Index:   \"#(position())\",\r\n" + 
        "             Engine:\r\n" + 
        "             {\r\n" + 
        "                 Displacement:   375\r\n" + 
        "             }\r\n" + 
        "        }\r\n" + 
        "    }\r\n" + 
        "}";

        private static readonly string _transformIf1 =
        "{\r\n" + 
        "    Driver:\r\n" + 
        "    {\r\n" + 
        "        FirstName: \"Bob\",\r\n" + 
        "        LastName:  \"Jones\",\r\n" + 
        "        \"#foreach(Cars, Vehicles)\":\r\n" + 
        "        {\r\n" + 
        "            \"#if(Make == 'Chevy')\":\r\n" + 
        "            {\r\n" + 
        "                Brand:   \"#(Make)\",\r\n" + 
        "                Model:   \"#(Model)\",\r\n" + 
        "                Year:    \"#(Year)\",\r\n" + 
        "                Color:   \"#(Color)\",\r\n" + 
        "                Engine:\r\n" + 
        "                {\r\n" + 
        "                    Displacement:   375\r\n" + 
        "                }\r\n" + 
        "            }\r\n" + 
        "        }\r\n" + 
        "    }\r\n" + 
        "}";

        private static readonly string _transformElseIf1 =
        @"{
            Driver:  
            {
                '#if(Region == CurrentRegion)':
                {
                    FirstName:   'Fred',
                    LastName:    'Flintstone'
                },
                '#elseif(Region == PastRegion)':
                {
                    FirstName:   'Barney',
                    LastName:    'Rubble'
                },
                '#elseif(Region == FutureRegion)':
                {
                    FirstName:   'John',
                    LastName:    'Pebble'
                },
                '#else':
                {
                    FirstName:   'Frank',
                    LastName:    'Sands'
                }
            }
        }";

        private static readonly string _transformVariable =
        @"{
            Driver:  
            {
                '#variable(CurrentRegion)':    'WA',

                '#if(Region == $CurrentRegion)':
                {
                    FirstName:   'Fred',
                    LastName:    'Flintstone'
                },
                '#elseif(Region == PastRegion)':
                {
                    FirstName:   'Barney',
                    LastName:    'Rubble'
                },
                '#elseif(Region == FutureRegion)':
                {
                    FirstName:   'John',
                    LastName:    'Pebble'
                },
                '#else':
                {
                    FirstName:   'Frank',
                    LastName:    'Sands'
                }
            }
        }";

        private static readonly string _transformVariable2 =
        @"{
           '#variable(CurrentRegion)':    'WA',

            Driver:  
            {
                '#if(Region == $CurrentRegion)':
                {
                    FirstName:   'Fred',
                    LastName:    'Flintstone'
                },
                '#elseif(Region == PastRegion)':
                {
                    FirstName:   'Barney',
                    LastName:    'Rubble'
                },
                '#elseif(Region == FutureRegion)':
                {
                    FirstName:   'John',
                    LastName:    'Pebble'
                },
                '#else':
                {
                    FirstName:   'Frank',
                    LastName:    'Sands'
                }
            }
        }";

        private static readonly string _transformElseIf2 =
        @"{
            Driver:  
            {
                '#if(Region == CurrentRegion)':
                {
                    '#if(Subregion == CurrentSubregion)':
                    {
                        FirstName:   'Fred',
                        LastName:    'Flintstone'
                    },
                    '#else':
                    {
                        FirstName:   'Lance',
                        LastName:    'Boulder'
                    }
                },
                '#elseif(Region == PastRegion)':
                {
                    FirstName:   'Barney',
                    LastName:    'Rubble'
                },
                '#elseif(Region == FutureRegion)':
                {
                    FirstName:   'John',
                    LastName:    'Pebble'
                },
                '#else':
                {
                    FirstName:   'Frank',
                    LastName:    'Sands'
                }
            }
        }";

        private static readonly string _transformCopyOf1 =
        @"{
            Brand:   '#(Make)',
            Model:   '#(Model)',
            Year:    '#(Year)',
            Color:   '#(Color)',
            Invoice: '#copyof(Service.Parts)'
        }";

        private static readonly string _transformCopyOf2 =
        @"{
            '#foreach(Cars, Vehicles)':
            {
                Stuff: '#copyof(@)'
            }
        }";

        private static readonly string _transformDocument1 =
        @"{
            '#variable(Locations)':   '#(document(Locations, Default))',

            FirstName:  '#(FirstName)',
            LastName:   '#(LastName)',
            City:       '#($Locations.City)',
            Region:     '#($Locations.Region)'
        }";

        #endregion

        #region Data
        
        private static readonly string _data1 =
        @"{
            FirstName: 'John',
            LastName:  'Smith',
            Car:   
            {
               Make:   'Chevy',
               Model:  'Corvette',
               Year:   1964,
               Color:  'Blue'
            }
        }";

        private static readonly string _data2 =
        @"{
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
        }";

        private static readonly string _data3 =
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

        private static readonly string _dataCarList =
        @"{
            Cars:
            [
                {
                   Make:   'Chevy',
                   Model:  'Corvette',
                   Year:   1964,
                   Color:  'Blue'
                },
                {
                   Make:   'Pontiac',
                   Model:  'Firebird',
                   Year:   1969,
                   Color:  'Green'
                },
                {
                   Make:   'Audi',
                   Model:  'S8',
                   Year:   2020,
                   Color:  'Black'
                }
            ],
            MakeField: 'Brand',
            ModelField: 'Model'
        }";

        private static readonly string _dataCopyOf =
        @"{
             Make:   'Chevy',
             Model:  'Corvette',
             Year:   1964,
             Color:  'Blue',
             Service:
             {
                Parts:
                {
                    Muffler:    945,
                    Sparkplugs: 123,
                    Solenoid:   77
                }
             }
        }";

        private static readonly string _dataCopyOf2 =
        @"{
            Cars:
            [
                {
                    Make:   'Chevy',
                    Model:  'Corvette',
                    Year:   1964,
                    Parts:
                    {
                        Muffler:    945,
                        Sparkplugs: 123,
                        Solenoid:   77
                    }
                },
                {
                   Make:   'Pontiac',
                   Model:  'Firebird',
                   Year:   1969,
                   Color:  'Green',
                    Parts:
                    {
                        Muffler:    945,
                        Sparkplugs: 123,
                        Solenoid:   77
                    }
                },
                {
                   Make:   'Audi',
                   Model:  'S8',
                   Year:   2020,
                   Color:  'Black',
                    Parts:
                    {
                        Muffler:    945,
                        Sparkplugs: 123,
                        Solenoid:   77
                    }
                }
            ],
        }";

        #endregion

        public class DriverContainer
        {
            public Driver  Driver         { get; set; }
        }

        public class DriverContainer2
        {
            public Driver2  Driver         { get; set; }
        }

        public class Driver
        {
            public string       FieldName   { get; set; }
            public string       FirstName   { get; set; }
            public string       LastName    { get; set; }
            public Automobile2  Car         { get; set; }
        }

        public class Person
        {
            public string  FirstName   { get; set; }
            public string  LastName    { get; set; }
            public string  City        { get; set; }
            public string  Region      { get; set; }
        }

        public class Driver2
        {
            public string             FirstName   { get; set; }
            public string             LastName    { get; set; }
            public List<Automobile2>  Vehicles    { get; set; }
        }

        public class Automobile
        {
            public string        Make     { get; set; }
            public string        Model    { get; set; }
            public int           Year     { get; set; }
            public string        Color    { get; set; }
            public Engine        Engine   { get; set; }
            public List<Service> Services { get; set; }
        }

        public class Automobile2
        {
            public string        FieldName { get; set; }
            public string        Brand     { get; set; }
            public string        Model     { get; set; }
            public int           Year      { get; set; }
            public string        Color     { get; set; }
            public Engine        Engine    { get; set; }
            public int?          Index     { get; set; }
        }

        public class Engine
        {
            public string  Displacement  { get; set; }
        }

        public class Service
        {
            public string  Description { get; set; }
            public string  Schedule    { get; set; }
            public decimal Cost        { get; set; }
        }

        #endregion
    }
}
