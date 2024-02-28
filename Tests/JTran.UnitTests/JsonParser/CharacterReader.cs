
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonParser")]
    public class CharacterReaderTests
    {
        [TestMethod]
        [DataRow("{ 'bob': 'fred' }")]
        [DataRow("{ 'bob':\r\n{\r\n 'wilma': 'fred'\r\n}\r\n}")]
        public void CharacterReader_ReadNext(string data)
        {
            var check = data.Replace("\r\n", "");
            var reader = new CharacterReader(data);
            var i = -1;

            while(reader.ReadNext()) 
            {
                ++i;
                Assert.AreEqual(reader.Current, check[i]);
            }
        }

        private (JsonToken.TokenType Type, object? Value) Test(string text)
        {
            using var json = new MemoryStream(UTF8Encoding.Default.GetBytes(text));
            var reader     = new CharacterReader(json);
            var tokenizer  = new JsonTokenizer();
            var lineNumber = 0L;

            var type = tokenizer.ReadNextToken(reader, ref lineNumber);

            return(type, tokenizer.TokenValue);
        }
    }
}
