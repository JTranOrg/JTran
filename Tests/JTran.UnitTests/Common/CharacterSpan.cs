
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
        [DataRow("    bob ", 0, 8, "bob")]
        [DataRow("    bob ", 1, 7, "bob")]
        [DataRow("    bob .   fred", 1, 7, "bob")]
        [DataRow("    bob .   fred   ", 9, 9, "fred")]
        [DataRow("    bob . \r\tfred \n ", 9, 9, "fred")]
        public void CharacterSpan_Trim(string src, int offset, int length, string result)
        {
            Assert.AreEqual(result,  CharacterSpan.Trim(src.ToArray(), offset, length).ToString());
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
