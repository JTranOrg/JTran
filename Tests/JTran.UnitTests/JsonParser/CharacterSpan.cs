
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Common;
using System.Linq;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonParser")]
    public class CharacterSpanTests
    {
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
        public void CharacterSpan_PeekChar()
        {
            var source = "abc123def456".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);
            var c4 = new CharacterSpan(source, 9, 3);

            Assert.AreEqual('a', c1.PeekChar());
            Assert.AreEqual('1', c2.PeekChar());
            Assert.AreEqual('d', c3.PeekChar());
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
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 3);
            var c4 = new CharacterSpan();

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
            var c1 = new CharacterSpan(source, 3, 8);
            var c2 = new CharacterSpan(source, 5, 3);
            var c3 = new CharacterSpan(source, 0, 3);

            Assert.IsTrue(c1.Contains(c2));
            Assert.IsFalse(c1.Contains(c3));
        }


        [TestMethod]
        public void CharacterSpan_IndexOf_ch()
        {
            var source = "abc12345678def".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 8);
            var c3 = new CharacterSpan(source, 11, 3);

            Assert.AreEqual(0, c1.IndexOf('a'));
            Assert.AreEqual(1, c1.IndexOf('b'));
            Assert.AreEqual(2, c1.IndexOf('c'));
            Assert.AreEqual(4, c2.IndexOf('5'));
            Assert.AreEqual(2, c3.IndexOf('f'));
        }

        [TestMethod]
        public void CharacterSpan_FormatForJsonOutput()
        {
            var source = "a\rc1234\"56".ToArray();
            var c1 = new CharacterSpan(source, 0, 3);
            var c2 = new CharacterSpan(source, 3, 3);
            var c3 = new CharacterSpan(source, 6, 4);
            var c4 = new CharacterSpan();

            Assert.AreEqual("a\\rc",  c1.FormatForJsonOutput().ToString());
            Assert.AreEqual("123",     c2.FormatForJsonOutput().ToString());
            Assert.AreEqual("",        c4.FormatForJsonOutput().ToString());
            Assert.AreEqual("4\\\"56", c3.FormatForJsonOutput().ToString());
        }

        [TestMethod]
        [DataRow(4096)]
        [DataRow(16)]
        public void CharacterSpanFactory_ToString(int bufferSize)
        {
            var factory = new CharacterSpanFactory(bufferSize);

            factory.Append('a');
            factory.Append('b');
            factory.Append('c');

            var c1 = factory.Current;

            factory.Append('1');
            factory.Append('2');
            factory.Append('3');

            var c2 = factory.Current;

            factory.Append('d');
            factory.Append('e');
            factory.Append('f');

            var c3 = factory.Current;

            factory.Append('4');
            factory.Append('5');
            factory.Append('6');
            factory.Append('0');
            factory.Append('.');
            factory.Append('1');

            var c4 = factory.Current;

            factory.Append('-');
            factory.Append('4');
            factory.Append('5');
            factory.Append('6');
            factory.Append('0');
            factory.Append('5');
            factory.Append('.');
            factory.Append('1');
            factory.Append('2');

            var c5 = factory.Current;

            Assert.AreEqual("abc", c1.ToString());
            Assert.AreEqual("123", c2.ToString());
            Assert.AreEqual("def", c3.ToString());
            Assert.AreEqual("4560.1", c4.ToString());
            Assert.AreEqual("-45605.12", c5.ToString());

            Assert.IsFalse(c1.TryParseNumber(out double _));
            Assert.IsTrue(c2.TryParseNumber(out double c2n));
            Assert.IsFalse(c3.TryParseNumber(out double _));
            Assert.IsTrue(c4.TryParseNumber(out double c4n));
            Assert.IsTrue(c5.TryParseNumber(out double c5n));

            Assert.AreEqual(123d, c2n);
            Assert.AreEqual(4560.1d, c4n);
            Assert.AreEqual(-45605.12d, c5n, 2);
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

            var d = new Dictionary<CharacterSpan, string>();

            d.Add(c1, "bob");
            d.Add(c2, "fred");
            d.Add(c3, "wilma");

            Assert.AreEqual("bob",   d[c4]);
            Assert.AreEqual("fred",  d[c5]);
            Assert.AreEqual("wilma", d[c6]);
        }
    }
}
