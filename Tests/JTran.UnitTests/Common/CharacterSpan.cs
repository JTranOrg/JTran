
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;
using System.Linq;
using JTran.Expressions;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using System.Reflection.Metadata;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonParser")]
    public class CharacterSpanTests
    {
        #region ICharacterSpan

        [TestMethod]
        public void CharacterSpan_ToString()
        {
            var source = "abc123def456".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);
            var c4 = new CharacterSpan(source, 9, 3);

            Assert.AreEqual("abc", c1.ToString());
            Assert.AreEqual("123", c2.ToString());
            Assert.AreEqual("def", c3.ToString());
            Assert.AreEqual("456", c4.ToString());
        }

        [TestMethod]
        public void CharacterSpan_bracket_operator()
        {
            var source = "abc123def456".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);
            var c4 = new CharacterSpan(source, 9, 3);

            Assert.AreEqual('a', c1[0]);
            Assert.AreEqual('3', c2[2]);
            Assert.AreEqual('e', c3[1]);
        }

        [TestMethod]
        public void CharacterSpan_IsNullOrWhiteSpace()
        {
            var source = "abc123def456".ToArray();
            ICharacterSpan c1 = new CharacterSpan(source, 0, 3);
            ICharacterSpan c2 = new CharacterSpan(source, 3, 3);
            ICharacterSpan c3 = new CharacterSpan(source, 6, 3);
            ICharacterSpan c4 = new CharacterSpan();

            Assert.IsFalse(c1.IsNullOrWhiteSpace());
            Assert.IsFalse(c2.IsNullOrWhiteSpace());
            Assert.IsFalse(c2.IsNullOrWhiteSpace(1, 1));
            Assert.IsTrue(c3.IsNullOrWhiteSpace(5));
            Assert.IsTrue(c3.IsNullOrWhiteSpace(8, 1));
            Assert.IsFalse(c3.IsNullOrWhiteSpace(0, 7));
            Assert.IsTrue(c4.IsNullOrWhiteSpace(1));
        }

        [TestMethod]
        public void CharacterSpan_Contains()
        {
            var source = "abc12345678def".ToArray();
            ICharacterSpan c1 = new CharacterSpan(source, 3, 8);
            ICharacterSpan c2 = new CharacterSpan(source, 5, 3);
            ICharacterSpan c3 = new CharacterSpan(source, 0, 3);

            Assert.IsTrue(c1.Contains(c2));
            Assert.IsFalse(c1.Contains(c3));
        }

        [TestMethod]
        public void CharacterSpan_EndsWith()
        {
            var source = "abc12345678def".ToArray();
            ICharacterSpan c1 = new CharacterSpan(source, 0, 3);
            ICharacterSpan c2 = new CharacterSpan(source, 3, 8);
            ICharacterSpan c3 = new CharacterSpan(source, 11, 3);

            Assert.IsFalse(c1.EndsWith(CharacterSpan.FromString("")));
            Assert.IsFalse(c1.EndsWith(null));
            Assert.IsFalse(c2.EndsWith(CharacterSpan.FromString("hashkjdsakhdjsadas")));
            Assert.IsFalse(c2.EndsWith(CharacterSpan.FromString("123")));

            Assert.IsTrue(c1.EndsWith(CharacterSpan.FromString("abc")));
            Assert.IsTrue(c1.EndsWith(CharacterSpan.FromString("c")));
            Assert.IsTrue(c1.EndsWith(CharacterSpan.FromString("bc")));
            Assert.IsTrue(c2.EndsWith(CharacterSpan.FromString("678")));
            Assert.IsTrue(c2.EndsWith(CharacterSpan.FromString("8")));
            Assert.IsTrue(c3.EndsWith(CharacterSpan.FromString("def")));
            Assert.IsTrue(c3.EndsWith(CharacterSpan.FromString("ef")));

            Assert.IsFalse(c1.EndsWith(CharacterSpan.FromString("fr")));
            Assert.IsFalse(c1.EndsWith(CharacterSpan.FromString("abcd")));
            Assert.IsFalse(c2.EndsWith(CharacterSpan.FromString("876")));
            Assert.IsFalse(c2.EndsWith(CharacterSpan.FromString("123")));
            Assert.IsFalse(c1.EndsWith(CharacterSpan.FromString("cdef")));
        }

        #region IndexOf

        [TestMethod]
        public void CharacterSpan_IndexOf_cspan()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abc12345678def");
            ICharacterSpan c2 = new CharacterSpan(c1, 4, 9);
            ICharacterSpan find = CharacterSpan.FromString("456");

            Assert.AreEqual(6,  c1.IndexOf(find));
            Assert.AreEqual(6,  c1.IndexOf(find, 5));
            Assert.AreEqual(-1, c1.IndexOf(CharacterSpan.FromString("xyz")));
            Assert.AreEqual(2,  c2.IndexOf(find));
            Assert.AreEqual(2,  c2.IndexOf(find, 2));
            Assert.AreEqual(-1, c2.IndexOf(CharacterSpan.FromString("xyz")));
        }

        #endregion
        #region IndexOf

        [TestMethod]
        public void CharacterSpan_LastIndexOf_cspan()
        {
            Assert.AreEqual(2,   CharacterSpan.FromString("bob").LastIndexOf(CharacterSpan.FromString("b")));
            Assert.AreEqual(1,   CharacterSpan.FromString("bob").LastIndexOf(CharacterSpan.FromString("ob")));
            Assert.AreEqual(0,   CharacterSpan.FromString("bosh").LastIndexOf(CharacterSpan.FromString("bo")));
            Assert.AreEqual(-1,  CharacterSpan.FromString("bosh").LastIndexOf(CharacterSpan.FromString("mary")));
            Assert.AreEqual(-1,  CharacterSpan.FromString("bosh").LastIndexOf(CharacterSpan.FromString("")));
            Assert.AreEqual(-1,  CharacterSpan.FromString("").LastIndexOf(CharacterSpan.FromString("mary")));
        }

        [TestMethod]
        public void CharacterSpan_IndexOf_ch()
        {
            var source = "abc12345678def".ToArray();
            ICharacterSpan c1 = new CharacterSpan(source, 0, 3);
            ICharacterSpan c2 = new CharacterSpan(source, 3, 8);
            ICharacterSpan c3 = new CharacterSpan(source, 11, 3);

            Assert.AreEqual(0, c1.IndexOf('a'));
            Assert.AreEqual(1, c1.IndexOf('b'));
            Assert.AreEqual(2, c1.IndexOf('c'));
            Assert.AreEqual(4, c2.IndexOf('5'));
            Assert.AreEqual(2, c3.IndexOf('f'));
        }

        [TestMethod]
        public void CharacterSpan_IndexOf_ch_wstart()
        {
            var source = "abc.1234.5678.def".ToArray();
            ICharacterSpan c1 = new CharacterSpan(source!, 0);

            Assert.AreEqual(13, c1.IndexOf('.', 9));
        }

        #endregion

        [TestMethod]
        public void CharacterSpan_FormatForJsonOutput()
        {
            var source = "a\rc1234\"56".ToArray();
            ICharacterSpan c1 = new CharacterSpan(source, 0, 3, hasEscapeCharacters: true);
            ICharacterSpan c2 = new CharacterSpan(source, 3, 3);
            ICharacterSpan c3 = new CharacterSpan(source, 6, 4, hasEscapeCharacters: true);
            ICharacterSpan c4 = new CharacterSpan();

            Assert.AreEqual("a\\rc",   c1.FormatForJsonOutput().ToString());
            Assert.AreEqual("123",     c2.FormatForJsonOutput().ToString());
            Assert.AreEqual("",        c4.FormatForJsonOutput().ToString());
            Assert.AreEqual("4\\\"56", c3.FormatForJsonOutput().ToString());
        }

        [TestMethod]
        public void CharacterSpan_FormatForJsonOutput2()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abcdefghijk");

            Assert.AreEqual("abcdefghijk",     c1.FormatForJsonOutput().ToString());
        }

        private const decimal Zero = 0m;

        [TestMethod]   
        public void CharacterSpan_TryParseNumber()
        {
            TestNumber("16",                16m);
            TestNumber("16.3",              16.3m);
            TestNumber("16.3456",           16.3456m);
            TestNumber("-16.3456",          -16.3456m);
            TestNumber("0.3",               .3m);
            TestNumber(".3",                .3m);
            TestNumber("-.03",              -.03m);
            TestNumber("-0.3",              -0.3m);
            TestNumber("1234546.789012",    1234546.789012m);
            TestNumber("-1234546.789012",   -1234546.789012m);
            TestNumber("0",                 0m);
            TestNumber("750.87299999999998", 750.87299999999998m);
        }

        [TestMethod]   
        public void CharacterSpan_Split_no_separator()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abc");
            var parts = c1.Split('.').ToList();

            Assert.AreEqual(1, parts.Count());
            Assert.AreEqual("abc", parts![0].ToString());
        }

        [TestMethod]
        [DataRow("    bob ", 0, 8, "bob", true, true)]
        [DataRow("    bob ", 1, 7, "bob", true, true)]
        [DataRow("    bob .   fred", 1, 7, "bob", true, true)]
        [DataRow("    bob .   fred   ", 9, 9, "fred", true, true)]
        [DataRow("    bob . \r\tfred \n ", 9, 9, "fred", true, true)]
        [DataRow("    bob .  fred  ", 9, 9, "fred", true, true)]
        public void CharacterSpan_Trim_static(string src, int offset, int length, string result, bool start, bool end)
        {
            Assert.AreEqual(result,  CharacterSpan.Trim(src.ToArray(), offset, length).ToString());
        }

        [TestMethod]
        public void CharacterSpan_Trim()
        {
            Assert.AreEqual("",     CharacterSpan.FromString("", false).Trim(true, true).ToString());
            Assert.AreEqual("",     CharacterSpan.FromString(" \r\n\t ", false).Trim(true, true).ToString());
            Assert.AreEqual("bob",  CharacterSpan.FromString(" bob ", false).Trim(true, true).ToString());
            Assert.AreEqual("bob ", CharacterSpan.FromString(" bob ", false).Trim(true, false).ToString());
            Assert.AreEqual(" bob", CharacterSpan.FromString(" bob ", false).Trim(false, true).ToString());

            Assert.AreEqual("",     CharacterSpan.FromString("", true).Trim(true, true).ToString());
            Assert.AreEqual("",     CharacterSpan.FromString(" \r\n\t ", true).Trim(true, true).ToString());
            Assert.AreEqual("bob",  CharacterSpan.FromString(" bob ", true).Trim(true, true).ToString());
            Assert.AreEqual("bob ", CharacterSpan.FromString(" bob ", true).Trim(true, false).ToString());
            Assert.AreEqual(" bob", CharacterSpan.FromString(" bob ", true).Trim(false, true).ToString());
        }

        [TestMethod]
        public void CharacterSpan_Transform()
        {
            TransformTest("",      "",     (ch)=> (true, char.ToLower(ch)),  false);
            TransformTest("TED",   "ted",  (ch)=> (true, char.ToLower(ch)),  false);
            TransformTest("john",  "JOHN", (ch)=> (true, char.ToUpper(ch)),  false);
            TransformTest("fair",  "fir",   (ch)=> (ch != 'a', ch),          false);
            TransformTest("linda", "linda", (ch)=> (true, char.ToLower(ch)), false);

            TransformTest("",      "",     (ch)=> (true, char.ToLower(ch)),  true);
            TransformTest("BOB",   "bob",  (ch)=> (true, char.ToLower(ch)),  true);
            TransformTest("bob",   "BOB",  (ch)=> (true, char.ToUpper(ch)),  true);
            TransformTest("bread", "bred", (ch)=> (ch != 'a', ch),           true);
            TransformTest("fred",  "fred", (ch)=> (true, char.ToLower(ch)),  true);
        }

        private void TransformTest(string? input, string? output, Func<char, (bool use, char newVal)> transform, bool cache)
        {
            Assert.AreEqual(output, CharacterSpan.FromString(input, cache).Transform(transform).ToString());
        }

        [TestMethod]   
        public void CharacterSpan_Split_trim()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abc. def.\rghi.\tjk");
            var parts = c1.Split('.').ToList();

            Assert.AreEqual("abc", parts[0].ToString());
            Assert.AreEqual("def", parts[1].ToString());
            Assert.AreEqual("ghi", parts[2].ToString());
            Assert.AreEqual("jk",  parts[3].ToString());
        }

        [TestMethod]   
        public void CharacterSpan_Split_trim2()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abc. def.\rghi.jklm");
            var parts = c1.Split('.').ToList();

            Assert.AreEqual("abc",  parts[0].ToString());
            Assert.AreEqual("def",  parts[1].ToString());
            Assert.AreEqual("ghi",  parts[2].ToString());
            Assert.AreEqual("jklm", parts[3].ToString());
        }

        [TestMethod]   
        public void CharacterSpan_Split()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abc.def.ghi.jk");
            var parts = c1.Split('.').ToList();

            Assert.AreEqual("abc", parts[0].ToString());
            Assert.AreEqual("def", parts[1].ToString());
            Assert.AreEqual("ghi", parts[2].ToString());
            Assert.AreEqual("jk",  parts[3].ToString());
        }

        [TestMethod]   
        public void CharacterSpan_Substring()
        {
            ICharacterSpan c1 = CharacterSpan.FromString("abc.def.ghi.jk");
            ICharacterSpan c2 = CharacterSpan.FromString(" abc . def .ghi . jk  ");

            Assert.AreEqual("abc", c1.Substring(0, 3).ToString());
            Assert.AreEqual("def", c1.Substring(4, 3).ToString());
            Assert.AreEqual("ghi", c1.Substring(8, 3).ToString());
            Assert.AreEqual("jk",  c1.Substring(12, 2).ToString());
            Assert.AreEqual("jk",  c1.Substring(12).ToString());
            Assert.AreEqual("jk",  c1.Substring(12, 5).ToString());

            Assert.AreEqual("abc", c2.Substring(0, 5, true).ToString());
            Assert.AreEqual("def", c2.Substring(6, 5, true).ToString());
            Assert.AreEqual("ghi", c2.Substring(12, 4, true).ToString());
            Assert.AreEqual("jk",  c2.Substring(17, 5, true).ToString());
            Assert.AreEqual("jk",  c2.Substring(17, 9, true).ToString());
            Assert.AreEqual("jk",  c2.Substring(17, trim: true).ToString());
        }

        [TestMethod]   
        [DataRow("",         'a', 8, true,  "aaaaaaaa")]
        [DataRow("fred",     'a', 8, true,  "aaaafred")]
        [DataRow("12345678", 'a', 8, true,  "12345678")]
        [DataRow("1234567",  'a', 8, true,  "a1234567")]
        [DataRow("b",        'a', 8, true,  "aaaaaaab")]
        [DataRow("bc",       'a', 8, true,  "aaaaaabc")]
        [DataRow("",         'a', 8, false, "aaaaaaaa")]
        [DataRow("fred",     'a', 8, false, "fredaaaa")]
        [DataRow("12345678", 'a', 8, false, "12345678")]
        [DataRow("1234567",  'a', 8, false, "1234567a")]
        [DataRow("b",        'a', 8, false, "baaaaaaa")]
        [DataRow("bc",       'a', 8, false, "bcaaaaaa")]
        public void CharacterSpan_Pad(string val, char pad, int len, bool left, string expected)
        {
            Assert.AreEqual(expected, CharacterSpan.FromString(val).Pad(pad, len,left).ToString());
        }

        [TestMethod]   
        [DataRow("bobfred",          "",     "",        "bobfred",     false)]
        [DataRow("",                 "",     "",        "",            false)]
        [DataRow("bobfred",          "fred", "john",    "bobjohn",     false)]
        [DataRow("bobfred",          "bob",  "john",    "johnfred",    false)]
        [DataRow("fredtedred",       "ted",  "john",    "fredjohnred", false)]
        [DataRow("fredtedred",       "john", "george",  "fredtedred",     false)]
        [DataRow("tedfredtedredted", "ted",  "john",    "johnfredjohnredjohn", false)]
        [DataRow("john",             "ted",  "led",     "john",        false)]

        [DataRow("bobfred",          "",     "",        "bobfred",     true)]
        [DataRow("",                 "",     "",        "",            true)]
        [DataRow("bobfred",          "fred", "john",    "bobjohn",     true)]
        [DataRow("bobfred",          "bob",  "john",    "johnfred",    true)]
        [DataRow("fredtedred",       "ted",  "john",    "fredjohnred", true)]
        [DataRow("fredtedred",       "john", "george",  "fredtedred",     true)]
        [DataRow("tedfredtedredted", "ted",  "john",    "johnfredjohnredjohn", true)]
        [DataRow("john",             "ted",  "led",     "john",        true)]
        public void CharacterSpan_Replace(string val, string find, string replace, string expected, bool expression)
        {
            var ival = CharacterSpan.FromString(val);

            ival.ExpressionResult = expression;

            Assert.AreEqual(expected, ival.Replace(CharacterSpan.FromString(find), CharacterSpan.FromString(replace)).ToString());
        }

        [TestMethod]   
        [DataRow("",                 'a', "")]
        [DataRow("bobfred",          'f', "bob")]
        [DataRow("bobfred",          'x', "bobfred")]
        [DataRow("fred.ted.red",     '.', "fred")]
        public void CharacterSpan_SubstringBefore(string val, char find, string expected)
        {
            var ival = CharacterSpan.FromString(val);

            Assert.AreEqual(expected, ival.SubstringBefore(find).ToString());

            ival.ExpressionResult = true;

            Assert.AreEqual(expected, ival.SubstringBefore(find).ToString());
        }

        [TestMethod]    
        [DataRow("",        "",         "")]
        [DataRow("bobfred", "",         "bobfred")]
        [DataRow("bob",     "fred",     "bobfred")]
        [DataRow("",        "bobfred",  "bobfred")]
        public void CharacterSpan_Concat(string str1, string str2, string expected)
        {
            var i1 = CharacterSpan.FromString(str1);
            var i2 = CharacterSpan.FromString(str2);

            Assert.AreEqual(expected, i1.Concat(i2).ToString());

            i1.ExpressionResult = true;

            Assert.AreEqual(expected, i1.Concat(i2).ToString());
        }

        [TestMethod]   
        [DataRow("",                 'a', "")]
        [DataRow("bobfred",          'f', "red")]
        [DataRow("bobfred",          'x', "")]
        [DataRow("fred.ted.red",     '.', "ted.red")]
        public void CharacterSpan_SubstringAfter(string val, char find, string expected)
        {
            var ival = CharacterSpan.FromString(val);

            Assert.AreEqual(expected, ival.SubstringAfter(find).ToString());

            ival.ExpressionResult = true;

            Assert.AreEqual(expected, ival.SubstringAfter(find).ToString());
        }

        [TestMethod]   
        [DataRow("bobfred",          "",     "",        "bobfred",     false)]
        [DataRow("",                 "",     "",        "",            false)]
        [DataRow("bobfred",          "fred", "john",    "bobjohn",     false)]
        [DataRow("bobfred",          "bob",  "john",    "bobfred",     false)]
        [DataRow("fredtedted",       "ted",  "john",    "fredtedjohn", false)]
        [DataRow("fredtedred",       "john", "george",  "fredtedred",  false)]
        [DataRow("john",             "ted",  "led",     "john",        false)]

        [DataRow("bobfred",          "",     "",        "bobfred",     true)]
        [DataRow("",                 "",     "",        "",            true)]
        [DataRow("bobfred",          "fred", "john",    "bobjohn",     true)]
        [DataRow("bobfred",          "bob",  "john",    "bobfred",     true)]
        [DataRow("fredtedted",       "ted",  "john",    "fredtedjohn", true)]
        [DataRow("fredtedred",       "john", "george",  "fredtedred",  true)]
        [DataRow("john",             "ted",  "led",     "john",        true)]
        public void CharacterSpan_ReplaceEnding(string val, string find, string replace, string expected, bool expression)
        {
            var ival = CharacterSpan.FromString(val);

            ival.ExpressionResult = expression;

            Assert.AreEqual(expected, ival.ReplaceEnding(CharacterSpan.FromString(find), CharacterSpan.FromString(replace)).ToString());
        }

        [TestMethod]   
        [DataRow("bobfred",          "",     "bobfred", false)]
        [DataRow("",                 "",     "",        false)]
        [DataRow("bobfred",          "fred", "bob",     false)]
        [DataRow("bobfred",          "bob",  "fred",    false)]
        [DataRow("fredtedred",       "ted",  "fredred", false)]
        [DataRow("fredtedred",       "ted",  "fredred", false)]
        [DataRow("tedfredtedredted", "ted",  "fredred", false)]
        [DataRow("john",             "ted",  "john",    false)]
        [DataRow("bobfred",          "",     "bobfred", true)]
        [DataRow("",                 "",     "",        true)]
        [DataRow("bobfred",          "fred", "bob",     true)]
        [DataRow("bobfred",          "bob",  "fred",    true)]
        [DataRow("fredtedred",       "ted",  "fredred", true)]
        [DataRow("fredtedred",       "ted",  "fredred", true)]
        [DataRow("tedfredtedredted", "ted",  "fredred", true)]
        [DataRow("fredtedtedtedred", "ted",  "fredred", true)]
        [DataRow("john",             "ted",  "john",    true)]
        public void CharacterSpan_Remove(string val, string remove, string expected, bool expression)
        {
            var ival = CharacterSpan.FromString(val);

            ival.ExpressionResult = expression;

            Assert.AreEqual(expected, ival.Remove(CharacterSpan.FromString(remove)).ToString());
        }

        [TestMethod]   
        [DataRow("bob.fred",     "fred")]
        [DataRow("bob",          "bob")]
        [DataRow("bob.fred.ted", "ted")]
        [DataRow("bob,fred,ted", "bob,fred,ted")]
        [DataRow("",             "")]
        public void CharacterSpan_LastItemIn(string? val, string? expected)
        {
            var ival = CharacterSpan.FromString(val);

            Assert.AreEqual(expected, ival.LastItemIn('.')?.ToString());

            ival.ExpressionResult = true;

            Assert.AreEqual(expected, ival.LastItemIn('.')?.ToString());
        }

        private void TestNumber(string val, decimal expected)
        {
            var c1 = CharacterSpan.FromString(val);

            Assert.IsTrue(c1.TryParseNumber(out decimal dval));
            Assert.AreEqual(expected, dval);
        }

        #endregion

        [TestMethod]
        [DataRow(4096)]
        [DataRow(16)]
        public void CharacterSpanBuilder_ToString(int bufferSize)
        {
            var factory = new CharacterSpanBuilder(bufferSize);

            factory.Append('a');
            factory.Append('b');
            factory.Append('c');

            ICharacterSpan c1 = CharacterSpan.Clone(factory.Current);

            factory.Append('1');
            factory.Append('2');
            factory.Append('3');

            ICharacterSpan c2 = CharacterSpan.Clone(factory.Current);

            factory.Append('d');
            factory.Append('e');
            factory.Append('f');

            ICharacterSpan c3 = CharacterSpan.Clone(factory.Current);

            factory.Append('4');
            factory.Append('5');
            factory.Append('6');
            factory.Append('0');
            factory.Append('.');
            factory.Append('1');

            ICharacterSpan c4 = CharacterSpan.Clone(factory.Current);

            factory.Append('-');
            factory.Append('4');
            factory.Append('5');
            factory.Append('6');
            factory.Append('0');
            factory.Append('5');
            factory.Append('.');
            factory.Append('1');
            factory.Append('2');

            ICharacterSpan c5 = CharacterSpan.Clone(factory.Current);

            Assert.AreEqual("abc", c1.ToString());
            Assert.AreEqual("123", c2.ToString());
            Assert.AreEqual("def", c3.ToString());
            Assert.AreEqual("4560.1", c4.ToString());
            Assert.AreEqual("-45605.12", c5.ToString());

            Assert.IsFalse(c1.TryParseNumber(out decimal _));
            Assert.IsTrue(c2.TryParseNumber(out decimal c2n));
            Assert.IsFalse(c3.TryParseNumber(out decimal _));
            Assert.IsTrue(c4.TryParseNumber(out decimal c4n));
            Assert.IsTrue(c5.TryParseNumber(out decimal c5n));

            Assert.AreEqual(123m, c2n);
            Assert.AreEqual(4560.1m, c4n);
            Assert.AreEqual(-45605.12m, c5n, 2);
        }

        [TestMethod]
        public void CharacterSpan_GetHashCode()
        {
            var source = "abc123def456".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);

            var c4 = new CharacterSpan(source, 0, 3);
            var c5 = new CharacterSpan(source, 3, 3);
            var c6 = new CharacterSpan(source, 6, 3);

            var d = new Dictionary<ICharacterSpan, string>();

            d.Add(c1, "bob");
            d.Add(c2, "fred");
            d.Add(c3, "wilma");

            Assert.AreEqual("bob",   d[c4]);
            Assert.AreEqual("fred",  d[c5]);
            Assert.AreEqual("wilma", d[c6]);
        }

        [TestMethod]
        public void CharacterSpan_GetHashCode2()
        {
            var source = "abc123def456".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);

            var c1h = c1.GetHashCode();
            var c1s = CharacterSpan.FromString("abc").GetHashCode();

            Assert.AreEqual(c1h, c1s);
            Assert.AreEqual(c2.GetHashCode(), CharacterSpan.FromString("123").GetHashCode());
            Assert.AreEqual(c3.GetHashCode(), CharacterSpan.FromString("def").GetHashCode());
        }

        [TestMethod]
        public void CharacterSpan_Join()
        {
            var source = "abc123def456".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);

            Assert.AreEqual("abc.def.123", CharacterSpan.Join(new List<ICharacterSpan> { c1, c3, c2}, '.').ToString());
        }
    }
}
