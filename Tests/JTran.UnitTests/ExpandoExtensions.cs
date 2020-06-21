using System;
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran;
using JTran.Extensions;

namespace JTran.UnitTests
{
    [TestClass]
    public class ExpandoExtensionsTests
    {
        [TestMethod]
        public void ExpandoExtensions_ToJson_Success()
        {
            var exp = _data1.JsonToExpando() as ExpandoObject;
            var json = exp.ToJson().Replace(" ", "").Replace("\r\n", "");

            _data1 = _data1.Replace(" ", "").Replace("\r\n", "").Replace("'", "\"");

            Assert.AreEqual(_data1, json);
        }

        private static string _data1 =
        @"{
            FirstName: 'John',
            LastName:  'Smith',
            Car:   
            {
               Make:   'Chevy',
               Model:  'Corvette',
               Year:   1964,
               Color:  'Blue',
               Engine:
               {
                 Displacement:   375
               },
               ServiceCalls:
               [
                 {
                   Description: 'Change sparkplugs',
                   Estimate:    100
                 },
                 {
                   Description: 'Tune up',
                   Estimate:    200,
                   Invoice:     210.79
                 }
               ]
            }
        }";

    }
}
