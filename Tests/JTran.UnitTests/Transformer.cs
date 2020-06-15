using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

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
            var transformer = new JTran.Transformer(_transform1);
            var result      = transformer.Transform(_data1, null);
   
            Assert.AreNotEqual(_transform1, _data1);
        }

        [TestMethod]
        public void Transformer_Transform_ForEach_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach1);
            var result      = transformer.Transform(_data2);
   
            Assert.AreNotEqual(_transformForEach1, _data2);

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
        public void Transformer_Transform_ForEach2_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach2);
            var result      = transformer.Transform(_data3);
   
            Assert.AreNotEqual(_transformForEach1, _data3);

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
        public void Transformer_Transform_ForEach3_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach3);
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
        public void Transformer_Transform_copyof_Success()
        {
            var transformer = new JTran.Transformer(_transformForEach2);
            var result      = transformer.Transform(_data3);
   
            Assert.AreNotEqual(_transformForEach1, _data3);

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
        public void Transformer_Transform_ForEach_noarray_Success()
        {
            var transformer = new JTran.Transformer(_transformForEachNoArray);
            var result      = transformer.Transform(_data5);
   
            Assert.AreNotEqual(_transformForEachNoArray, _data5);

            var json = JObject.Parse(result);

            Assert.AreEqual("Camaro", json["Owner"]["Cars"]["Chevy"]["Model"].ToString());
            Assert.AreEqual("Firebird", json["Owner"]["Cars"]["Pontiac"]["Model"].ToString());
            Assert.AreEqual("Charger", json["Owner"]["Cars"]["Dodge"]["Model"].ToString());
        }

        [TestMethod]
        public void Transformer_Transform_Email_Fails()
        {
            var transformer = new JTran.Transformer(_transformEmail);

             Assert.ThrowsException<Transformer.SyntaxException>( ()=> transformer.Transform(_dataEmail) );
        }

        #region Private 

        #region Transforms 

        private static readonly string _transform1 =
        @"{
            Make:   'Chevy',
            Model:  'Corvette',
            Year:   1964,
            Color:  'Blue'
        }";

       private static readonly string _transformForEach1 =
        @"{
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
         "        LastName:    '#(Surname)'," + 
         "        FirstName:   '#(Name)'," + 
         "        Age:         '#(Age)'," + 
         "        Address:     '#(Address)'" + 
         "    }" + 
         "}";

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
     
        #endregion

        #region Data
        
        private static readonly string _data1 =
        @"{
            FirstName: 'John',
            LastName:  'Smith'
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

        #endregion

        #region Models

        public class Customer
        {
            public string FirstName   { get; set; }
            public string LastName    { get; set; }
            public int    Age         { get; set; }
            public string Address     { get; set; }
        }        
        
        public class CustomerContainer
        {
            public List<Customer> Customers   { get; set; }
        }        
        
        #endregion

        #endregion
    }
}
