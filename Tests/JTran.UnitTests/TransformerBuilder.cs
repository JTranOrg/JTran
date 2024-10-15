
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JTran.Common;
using System.Text;

namespace JTran.UnitTests
{
    [TestClass]
    public class TransformerBuilderTests
    {
        [TestMethod]
        public void TransformerBuilder_Build_wInclude_success()
        {
            var transformer = TransformerBuilder                               
                                .FromString(_transformInclude)
                                .AddInclude("otherfile.json", _otherFile)
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

           var result  = transformer.Transform(_dataForEachGroup1, null);
   
            Assert.AreNotEqual(_transformInclude, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob\", Year: 1965 }"), JObject.Parse(result)));
        }     
        
        [TestMethod]
        public void TransformerBuilder_Build_FromStream_wInclude_success()
        {
            using var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(_transformInclude));
            var transformer = TransformerBuilder                               
                                .FromStream(stream)
                                .AddInclude("otherfile.json", _otherFile)
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

           var result  = transformer.Transform(_dataForEachGroup1, null);
   
            Assert.AreNotEqual(_transformInclude, _dataForEachGroup1);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ FirstName: \"bob\", Year: 1965 }"), JObject.Parse(result)));
        }     
        
        [TestMethod]
        public void TransformerBuilder_Build_wExtension_success()
        {
            var transformer = TransformerBuilder                               
                                .FromString(_transformExtFunction)
                                .AddExtension(new ExtFunctions())
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

            var result  = transformer.Transform(_data5, null);
   
            Assert.AreNotEqual(_transformExtFunction, _data5);

            var json = JObject.Parse(result)!;

            Assert.AreEqual("xCamaro",   json["Owner"]!["Cars"]!["Chevy"]!["Model"]!.ToString());
            Assert.AreEqual("xFirebird", json["Owner"]!["Cars"]!["Pontiac"]!["Model"]!.ToString());
            Assert.AreEqual("xCharger",  json["Owner"]!["Cars"]!["Dodge"]!["Model"]!.ToString());
            Assert.AreEqual("yGreen",    json["Owner"]!["Cars"]!["Chevy"]!["Color"]!.ToString());
            Assert.AreEqual("yBlue",     json["Owner"]!["Cars"]!["Pontiac"]!["Color"]!.ToString());
            Assert.AreEqual("yBlack",    json["Owner"]!["Cars"]!["Dodge"]!["Color"]!.ToString());
        }     
        
        [TestMethod]
        public void TransformerBuilder_Build_wDocument_success()
        {
            var transformer = TransformerBuilder                               
                                .FromString(_transformDocument)
                                .AddDocumentRepository("test1", new DocumentRepository())
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

           var result  = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformDocument, result);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Name: \"John Smith\" }"), JObject.Parse(result)));
        }     
        
        [TestMethod]
        public void TransformerBuilder_Build_wArgs_success()
        {
            var transformer = TransformerBuilder                               
                                .FromString(_transformDocument2)
                                .AddDocumentRepository("test1", new DocumentRepository())
                                .AddArguments(new Dictionary<string, object> { { "Hometown", "Whoville" }})
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

           var result  = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformDocument2, result);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Name: \"John Smith\", Hometown: \"Whoville\" }"), JObject.Parse(result)));
        }     
        
        [TestMethod]
        public void TransformerBuilder_Build_wArgs2_success()
        {
            var transformer = TransformerBuilder                               
                                .FromString(_transformDocument4)
                                .AddDocumentRepository("test1", new DocumentRepository())
                                .AddArguments(new Dictionary<string, object> { { "Hometown", "Whoville" }})
                                .AddArguments(new Dictionary<string, object> { { "Age", 36 }, { "Hometown", "Whereville" }})
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

           var result  = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformDocument2, result);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Name: \"John Smith\", Hometown: \"Whoville\", Age: 36 }"), JObject.Parse(result)));
        }     
        
        [TestMethod]
        public void TransformerBuilder_Build_wOutputVars_success()
        {
            var transformer = TransformerBuilder                               
                                .FromString(_transformDocument4)
                                .AddDocumentRepository("test1", new DocumentRepository())
                                .AddArguments(new Dictionary<string, object> { { "Hometown", "Whoville" }})
                                .AddArguments(new Dictionary<string, object> { { "Age", 36 }, { "Hometown", "Whereville" }})
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

            var context = new TransformerContext();
            var result  = transformer.Transform(_dataForEachGroup1, context);
   
            Assert.AreNotEqual(_transformDocument2, result);
            Assert.AreEqual("Bob Jones", context.OutputArguments["Winner"]);

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse("{ Name: \"John Smith\", Hometown: \"Whoville\", Age: 36 }"), JObject.Parse(result)));
        }     
                
        [TestMethod]
        public void TransformerBuilder_Build_wOutputFunction_success()
        {
            var outputName = "";
            var outputVal = "";
            var transformer = TransformerBuilder                               
                                .FromString(_transformDocument3)
                                .AddDocumentRepository("test1", new DocumentRepository())
                                .AddArguments(new Dictionary<string, object> { { "Hometown", "Whoville" }})
                                .OnOutputArgument( (string name, object val)=> 
                                {
                                    outputName = name;
                                    outputVal = val?.ToString() ?? "";
                                })
                                .Build<string>();
 
            Assert.IsNotNull(transformer);

            var result  = transformer.Transform(_dataForEachGroup1);
   
            Assert.AreNotEqual(_transformDocument3, result);
            Assert.AreEqual("Winner", outputName);
            Assert.AreEqual("Bob Jones", outputVal);
        }             

        #region Transforms and Data

        public class DocumentRepository : IDocumentRepository2
        { 
            public DocumentRepository()
            {
            }

            public string GetDocument(string name)
            {
                return _doc1;
            }

            public Stream GetDocumentStream(string name)
            {
                return new MemoryStream(UTF8Encoding.Default.GetBytes(_doc1));
            }
        }

        // "#variable(origins)":   "#(document(docs, origins))",
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

        private static readonly string _transformDocument =
        @"{
            '#variable(drivers)':      '#(document(test1, test1))',
            '#variable(DriverName)':   'John Smith',

            '#bind($drivers.Drivers[Name == $DriverName][0])':
            {
                Name: '#(Name)'
            }
        }";


        private static readonly string _transformDocument2 =
        @"{
            '#variable(drivers)':      '#(document(test1, test1))',
            '#variable(DriverName)':   'John Smith',

            '#bind($drivers.Drivers[Name == $DriverName][0])':
            {
                Name:     '#(Name)',
                Hometown: '#($Hometown)'
            }
        }";

        private static readonly string _transformDocument3 =
        @"{
            '#variable(drivers)':      '#(document(test1, test1))',
            '#variable(DriverName)':   'John Smith',
            '#outputvariable(Winner)': 'Bob Jones',

            '#bind($drivers.Drivers[Name == $DriverName][0])':
            {
                Name:     '#(Name)',
                Hometown: '#($Hometown)'
            }
        }";

        private static readonly string _transformDocument4 =
        @"{
            '#variable(drivers)':      '#(document(test1, test1))',
            '#variable(DriverName)':   'John Smith',
            '#outputvariable(Winner)': 'Bob Jones',

            '#bind($drivers.Drivers[Name == $DriverName][0])':
            {
                Name:     '#(Name)',
                Hometown: '#($Hometown)',
                Age:      '#($Age)'
            }
        }";

        private static readonly string _transformInclude =
        @"{
            '#include':      'otherfile.json',

            '#calltemplate(DisplayName)':
            {
                name: 'bob'
            }
        }";

        private static readonly string _transform1 =
        @"{
            Make:   'Chevy',
            Model:  'Corvette',
            Year:   1964,
            Color:  'Blue'
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
        
        private static readonly string _doc1 = 
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

        private class ExtFunctions
        {
            public string addx(string val)  { return "x" + val; }
            public string addy(string val)  { return "y" + val; }
        }


        #endregion
    }
}