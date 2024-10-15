using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Json;

using JTranParser = JTran.Parser.ExpressionParser;
using JTran.Common;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Diagnostics.Metrics;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("Builtin Functions")]
    public class BuiltinFunctionsTests
    {
        private readonly BuiltinFunctions _fn = new();

        public BuiltinFunctionsTests()
        {            
        }

        [TestMethod]
        [DataRow("franklin", 0, 5,      "frank")]
        [DataRow("franklin", 5, -1000,  "lin")]
        [DataRow("franklin", 5, 11,     "lin")]
        [DataRow("franklin", 3, 3,      "nkl")]
        [DataRow(null,       3, 3,      null)]
        [DataRow("",         3,3 ,      "")]
        public void BuiltinFunctions_substring(string? val, int start, int length, string? expected)
        {
            TestFunction(val, expected, (s)=> _fn.substring(s, start, length));
        }

        [TestMethod]
        [DataRow("franklin", "ran",   "bob",     "fbobklin")]
        [DataRow("franklin", "frank", "john",    "johnlin")]
        [DataRow("franklin", "lin",   "enstein", "frankenstein")]
        [DataRow(null,       "",      "",        null)]
        [DataRow("",         null,    null,      "")]
        public void BuiltinFunctions_replace(string? val, string? replace, string? with, string? expected)
        {
            TestFunction(val, expected, (s)=> _fn.replace(s, replace, with));
        }

        [TestMethod]
        [DataRow("franklin", "ran",      "bob",     "franklin")]
        [DataRow("franklin", "frank",    "john",    "franklin")]
        [DataRow("franklin", "lin",      "enstein", "frankenstein")]
        [DataRow("franklin", "franklin", "george",  "george")]
        [DataRow(null,       "",      "",        null)]
        [DataRow("",         null,    null,      "")]
        public void BuiltinFunctions_replaceending(string? val, string? replace, string? with, string? expected)
        {
            TestFunction(val, expected, (s)=> _fn.replaceending(s, replace, with));
        }

        [TestMethod]
        [DataRow("franklin", "ran",   "f")]
        [DataRow("franklin", "lin",   "frank")]
        [DataRow("franklin", "john",  "franklin")]
        [DataRow("franklin", "johnsyouruncle",   "franklin")]
        [DataRow(null,       "",        null)]
        [DataRow("",         null,      "")]
        public void BuiltinFunctions_substringbefore(string? val, string? before, string? expected)
        {
            TestFunction(val, expected, (s)=> _fn.substringbefore(s, before));
        }

        [TestMethod]
        [DataRow("franklin", "ran", "klin")]
        [DataRow("franklin", "f",   "ranklin")]
        [DataRow("franklin", "john",    "")]
        [DataRow("franklin", "johnsyouruncle",    "")]
        [DataRow(null,       "",    null)]
        [DataRow("",         null, "")]
        public void BuiltinFunctions_substringafter(string? val, string? replace, string? expected)
        {
            TestFunction(val, expected, (s)=> _fn.substringafter(s, replace));
        }

        [TestMethod]
        [DataRow("FRANK", "frank")]
        [DataRow("frank", "frank")]
        [DataRow("",      "")]
        public void BuiltinFunctions_lowercase(string val, string expected)
        {
            TestFunction(val, expected, (s)=> _fn.lowercase(s));
        }

        [TestMethod]
        [DataRow("FRANK", "FRANK")]
        [DataRow("frank", "FRANK")]
        [DataRow("",      "")]
        public void BuiltinFunctions_uppercase(string val, string expected)
        {
            TestFunction(val, expected, (s)=> _fn.uppercase(s));
        }

        [TestMethod]
        [DataRow("",            "aaaaaaaa")]
        [DataRow("b",           "aaaaaaab")]
        [DataRow("12345678",    "12345678")]
        [DataRow("1234567890",  "1234567890")]
        [DataRow("bcde",        "aaaabcde")]
        public void BuiltinFunctions_padleft(string val, string expected)
        {
            TestFunction(val, expected, (s)=> _fn.padleft(s, "a", 8));
        }

        [TestMethod]
        [DataRow("",            "aaaaaaaa")]
        [DataRow("b",            "baaaaaaa")]
        [DataRow("12345678",    "12345678")]
        [DataRow("1234567890",  "1234567890")]
        [DataRow("bcde",        "bcdeaaaa")]
        public void BuiltinFunctions_padright(string val, string expected)
        {
            TestFunction(val, expected, (s)=> _fn.padright(s, "a", 8));
        }

        [TestMethod]
        [DataRow("", "")]
        [DataRow(null, null)]
        [DataRow("abcdef", "abcdef")]
        [DataRow("  abc  def  ", "abc def")]
        [DataRow("  a        b   c  de f  ", "a b c de f")]
        public void BuiltinFunctions_normalizespace(string? val, string? expected)
        {
            TestFunction(val, expected, (s)=> _fn.normalizespace(s));
        }

        [TestMethod]
        [DataRow("bobfred",          "",     "bobfred")]
        [DataRow("",                 "",     "")]
        [DataRow("bobfred",          "fred", "bob")]
        [DataRow("bobfred",          "bob",  "fred")]
        [DataRow("fredtedred",       "ted",  "fredred")]
        [DataRow("fredtedred",       "ted",  "fredred")]
        [DataRow("tedfredtedredted", "ted",  "fredred")]
        [DataRow("john",             "ted",  "john")]
        [DataRow("bobfred",          "",     "bobfred")]
        [DataRow("",                 "",     "")]
        public void BuiltinFunctions_remove(string val, string remove, string expected)
        {
            TestFunction(val, expected, (s)=> _fn.remove(s, remove));
        }

        [TestMethod]
        [DataRow("bobfred",          "",     "bobfred")]
        [DataRow("",                 "",     "")]
        [DataRow("bobfred",          "fred", "bob")]
        [DataRow("bobfred",          "bob",  "bobfred")]
        [DataRow("fredtedred",       "ted",  "fredtedred")]
        [DataRow("fredtedredred",    "red",  "fredtedred")]
        [DataRow("tedfredtedredted", "ted",  "tedfredtedred")]
        [DataRow("john",             "ted",  "john")]
        [DataRow("bobfred",          "",     "bobfred")]
        [DataRow("",                 "",     "")]
        public void BuiltinFunctions_removeending(string val, string remove, string expected)
        {
            TestFunction(val, expected, (s)=> _fn.removeending(s, remove));
        }

        [TestMethod]
        public void BuiltinFunctions_startswith()
        {
            TestFunctionTrue("franklin",  (s)=> _fn.startswith(s, "frank"));
            TestFunctionTrue("john",      (s)=> _fn.startswith(s, "j"));
                                                                
            TestFunctionFalse("franklin", (s)=> _fn.startswith(s, "john"));
            TestFunctionFalse("john",     (s)=> _fn.startswith(s, ""));
            TestFunctionFalse("frank",    (s)=> _fn.startswith(s, null));
            TestFunctionFalse(null,       (s)=> _fn.startswith(s, ""));
        }

        [TestMethod]
        public void BuiltinFunctions_endswith()
        {
            TestFunctionTrue("franklin", (s)=> _fn.endswith(s, "lin"));
            TestFunctionTrue("john",     (s)=> _fn.endswith(s, "n"));
                               
            TestFunctionFalse("franklin", (s)=> _fn.endswith(s, "john"));
            TestFunctionFalse("john",     (s)=> _fn.endswith(s, ""));
            TestFunctionFalse("frank",    (s)=> _fn.endswith(s, null));
            TestFunctionFalse(null,       (s)=> _fn.endswith(s, ""));
        }

        [TestMethod]
        [DataRow("red,blue,green", "green,red,blue",        true)]
        [DataRow("red,blue,green", "green,brown,blue",      false)]
        [DataRow("red,blue,green", "green,red,blue,orange", false)]
        [DataRow("red,blue,green", "",                      false)]
        [DataRow("",               "",                      true)]
        public void BuiltinFunctions_containsall(string list1, string list2, bool result)
        {
            Assert.AreEqual(result, _fn.containsall(list1.Split(",").Select(i=> i as object), list2.Split(",").Select(i=> i as object)));
            Assert.AreEqual(result, _fn.containsall(list2.Split(",").Select(i=> i as object), list1.Split(",").Select(i=> i as object)));
        }

        [TestMethod]
        [DataRow("red,blue,green", "green,red,blue",        true)]
        [DataRow("red,blue,green", "green,brown,blue",      false)]
        [DataRow("red,blue,green", "green,red,blue,orange", false)]
        [DataRow("red,blue,green", "",                      false)]
        [DataRow("",               "",                      true)]
        public void BuiltinFunctions_containsall_characterspan(string list1, string list2, bool result)
        {
            Assert.AreEqual(result, _fn.containsall(list1.Split(",").Select(i=> CharacterSpan.FromString(i)), list2.Split(",").Select(i=> CharacterSpan.FromString(i))));
            Assert.AreEqual(result, _fn.containsall(list2.Split(",").Select(i=> CharacterSpan.FromString(i)), list1.Split(",").Select(i=> CharacterSpan.FromString(i))));
        }

        [TestMethod]
        [DataRow("1,2,3", "3,2,1", true)]
        [DataRow("1,2,3", "3,2,1,4", false)]
        [DataRow("1,2,3", "2,1,55", false)]
        public void BuiltinFunctions_containsall_ints(string list1, string list2, bool result)
        {
            Assert.AreEqual(result, _fn.containsall(list1.Split(",").Select( i=> int.Parse(i) as object), list2.Split(",").Select( i=> int.Parse(i) as object)));
            Assert.AreEqual(result, _fn.containsall(list2.Split(",").Select( i=> int.Parse(i) as object), list1.Split(",").Select( i=> int.Parse(i) as object)));
            Assert.AreEqual(result, _fn.containsall(list1.Split(",").Select( i=> int.Parse(i)), list2.Split(",").Select( i=> int.Parse(i))));
            Assert.AreEqual(result, _fn.containsall(list2.Split(",").Select( i=> int.Parse(i)), list1.Split(",").Select( i=> int.Parse(i))));
        }

        private void TestFunction(string? val, string? expected, Func<object?, object?> fn)
        {
            Assert.AreEqual(expected, fn(val)?.ToString());

            if(val != null)
            {
                var valSpan = CharacterSpan.FromString(val);

                Assert.AreEqual(expected, fn(valSpan)?.ToString());

                valSpan.ExpressionResult = true;

                Assert.AreEqual(expected, fn(valSpan)?.ToString());
            }
        }

        private void TestFunctionBool(string? val, bool expected, Func<object?, bool> fn)
        {
            Assert.AreEqual(expected, fn(val));

            if(val != null)
            {
                var valSpan = CharacterSpan.FromString(val);

                if(expected)
                    Assert.AreEqual(expected, fn(valSpan));

                valSpan.ExpressionResult = true;

                Assert.AreEqual(expected, fn(valSpan));
            }
        }

        private void TestFunctionTrue(string? val, Func<object?, bool> fn)
        {
            Assert.IsTrue(fn(val));

            if(val != null)
            {
                var valSpan = CharacterSpan.FromString(val);

                Assert.IsTrue(fn(valSpan));

                valSpan.ExpressionResult = true;

                Assert.IsTrue( fn(valSpan));
            }
        }

        private void TestFunctionFalse(string? val, Func<object?, bool> fn)
        {
            Assert.IsFalse(fn(val));

            if(val != null)
            {
                var valSpan = CharacterSpan.FromString(val);

                Assert.IsFalse(fn(valSpan));

                valSpan.ExpressionResult = true;

                Assert.IsFalse( fn(valSpan));
            }
        }
    }
}
