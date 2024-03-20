using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Json;

using JTranParser = JTran.Parser.ExpressionParser;
using JTran.Common;
using System.ComponentModel.DataAnnotations;

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
        public void BuiltinFunctions_substring()
        {
            Assert.AreEqual("frank", _fn.substring("franklin", 0, 5)!.ToString());
            Assert.AreEqual("lin",   _fn.substring("franklin", 5)!.ToString());
            Assert.AreEqual("lin",   _fn.substring("franklin", 5, 11)!.ToString());
            Assert.AreEqual("nkl",   _fn.substring("franklin", 3, 3)!.ToString());
            Assert.IsNull(null,      _fn.substring(null, 3, 3)?.ToString());
            Assert.AreEqual("",      _fn.substring("", 3, 3)?.ToString());
        }

        [TestMethod]
        public void BuiltinFunctions_substringafter()
        {
            Assert.AreEqual("klin",    _fn.substringafter("franklin", "ran")!.ToString());
            Assert.AreEqual("ranklin", _fn.substringafter("franklin", "f")!.ToString());
            Assert.AreEqual("",        _fn.substringafter("franklin", "john")!.ToString());
            Assert.AreEqual("",        _fn.substringafter("franklin", "johnsyouruncle")!.ToString());
            Assert.IsNull(null,        _fn.substringafter(null, "")?.ToString());
            Assert.AreEqual("",        _fn.substringafter("", null)?.ToString());
        }

        [TestMethod]
        public void BuiltinFunctions_substringbefore()
        {
            Assert.AreEqual("f",        _fn.substringbefore("franklin", "ran")!.ToString());
            Assert.AreEqual("frank",    _fn.substringbefore("franklin", "lin")!.ToString());
            Assert.AreEqual("franklin", _fn.substringbefore("franklin", "john")!.ToString());
            Assert.AreEqual("franklin", _fn.substringbefore("franklin", "johnsyouruncle")!.ToString());
            Assert.IsNull(null,         _fn.substringbefore(null, "")?.ToString());
            Assert.AreEqual("",         _fn.substringbefore("", null)?.ToString());
        }

        [TestMethod]
        public void BuiltinFunctions_lowercase()
        {
            Assert.AreEqual("frank", _fn.lowercase("FRANK")!.ToString());
            Assert.AreEqual("frank", _fn.lowercase("frank")!.ToString());
            Assert.AreEqual("",      _fn.lowercase("")?.ToString());
        }

        [TestMethod]
        public void BuiltinFunctions_padleft()
        {
            Assert.AreEqual("aaaaaaaa",   _fn.padleft("", "a", 8)!.ToString());
            Assert.AreEqual("aaaaaaab",   _fn.padleft("b", "a", 8)!.ToString());
            Assert.AreEqual("12345678",   _fn.padleft("12345678", "a", 8)!.ToString());
            Assert.AreEqual("1234567890", _fn.padleft("1234567890", "a", 8)!.ToString());
            Assert.AreEqual("aaaabcde",    _fn.padleft("bcde", "a", 8)!.ToString());
        }

        [TestMethod]
        public void BuiltinFunctions_padright()
        {
            Assert.AreEqual("aaaaaaaa",   _fn.padright("", "a", 8)!.ToString());
            Assert.AreEqual("baaaaaaa",   _fn.padright("b", "a", 8)!.ToString());
            Assert.AreEqual("12345678",   _fn.padright("12345678", "a", 8)!.ToString());
            Assert.AreEqual("1234567890", _fn.padright("1234567890", "a", 8)!.ToString());
            Assert.AreEqual("bcdeaaaa",   _fn.padright("bcde", "a", 8)!.ToString());
        }

        [TestMethod]
        [DataRow("", "")]
        [DataRow(null, null)]
        [DataRow("abcdef", "abcdef")]
        [DataRow("  abc  def  ", "abc def")]
        [DataRow("  a        b   c  de f  ", "a b c de f")]
        public void BuiltinFunctions_normalizespace(string? val, string? expected)
        {
            Assert.AreEqual(expected, _fn.normalizespace(val)?.ToString());

            if(val != null)
                Assert.AreEqual(expected, _fn.normalizespace(CharacterSpan.FromString(val))?.ToString());
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
        [DataRow("bobfred",          "fred", "bob")]
        [DataRow("bobfred",          "bob",  "fred")]
        [DataRow("fredtedred",       "ted",  "fredred")]
        [DataRow("fredtedred",       "ted",  "fredred")]
        [DataRow("tedfredtedredted", "ted",  "fredred")]
        [DataRow("john",             "ted",  "john")]
        public void BuiltinFunctions_remove(string val, string remove, string expected)
        {
            Assert.AreEqual(expected, _fn.remove(val, remove)?.ToString());

            if(val != null)
            {
                Assert.AreEqual(expected, _fn.remove(CharacterSpan.FromString(val, false), CharacterSpan.FromString(remove))?.ToString());
                Assert.AreEqual(expected, _fn.remove(CharacterSpan.FromString(val, true), CharacterSpan.FromString(remove))?.ToString());
            }
        }

        [TestMethod]
        public void BuiltinFunctions_startswith()
        {
            Assert.IsTrue(_fn.startswith("franklin", "frank"));
            Assert.IsTrue(_fn.startswith("john",     "j"));
            
            Assert.IsFalse(_fn.startswith("franklin", "john"));
            Assert.IsFalse(_fn.startswith("john",     ""));
            Assert.IsFalse(_fn.startswith("frank",    null));
            Assert.IsFalse(_fn.startswith(null,       ""));
        }

        [TestMethod]
        public void BuiltinFunctions_endswith()
        {
            Assert.IsTrue(_fn.endswith("franklin", "lin"));
            Assert.IsTrue(_fn.endswith("john",     "n"));
                               
            Assert.IsFalse(_fn.endswith("franklin", "john"));
            Assert.IsFalse(_fn.endswith("john",     ""));
            Assert.IsFalse(_fn.endswith("frank",    null));
            Assert.IsFalse(_fn.endswith(null,       ""));
        }
    }
}
