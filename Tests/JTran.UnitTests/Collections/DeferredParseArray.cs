
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Collections;
using JTran.Common;
using System.Text;
using System.Text.Unicode;
using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    public class DeferredParseArrayTests
    {
        [TestMethod]
        public void DeferredParseArray_Success()
        {
            using var source = new MemoryStream(UTF8Encoding.Default.GetBytes(_test1));
            using var parser = new Json.Parser(new JsonModelBuilder());

            parser.ArrayInnerParse(new CharacterReader(source));

            var array = new DeferredParseArray(parser);

            var list = array.ToList();

            Assert.AreEqual(3,       list.Count);
            Assert.AreEqual("red",   list[0].ToString());
            Assert.AreEqual("green", list[1].ToString());
            Assert.AreEqual("blue",  list[2].ToString());
        }

        private static string _test1 = "[ 'red', 'green', 'blue']";
    }
}
