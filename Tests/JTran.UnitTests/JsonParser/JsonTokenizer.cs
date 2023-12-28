
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Json;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonParser")]
    public class JsonTokenizerTests
    {
        [TestMethod]
        public void JsonParser_ReadNextToken_success()
        {
            Assert.ThrowsException<JsonParseException>( ()=> Test("").Type);

            Assert.AreEqual(JsonToken.TokenType.BeginObject,    Test("  {").Type);
            Assert.AreEqual(JsonToken.TokenType.EndObject,      Test("\r\n  }").Type);
            Assert.AreEqual(JsonToken.TokenType.BeginArray,     Test("  [").Type);
            Assert.AreEqual(JsonToken.TokenType.EndArray,       Test("  ]").Type);
            Assert.AreEqual(JsonToken.TokenType.Comma,          Test("  ,  ").Type);
            Assert.AreEqual(JsonToken.TokenType.Property,       Test("  :").Type);

            Assert.AreEqual(JsonToken.TokenType.Number,         Test("  42").Type);
            Assert.AreEqual(42m,                                Test("  42").Value);

            Assert.AreEqual(JsonToken.TokenType.Number,         Test("  42.2").Type);
            Assert.AreEqual(42.2m,                              Test("  42.2").Value);

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \"bob\"").Type);
            Assert.AreEqual("bob",                              Test("  \"bob\"").Value);

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \"bo\\\"b\"").Type);
            Assert.AreEqual("bo\"b",                            Test("  \"bo\\\"b\"").Value);

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \'bob\'").Type);
            Assert.AreEqual("bob",                              Test("  \'bob\'").Value);

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \"123\"").Type);
            Assert.AreEqual("123",                              Test("  \"123\"").Value);

            Assert.AreEqual(JsonToken.TokenType.Null,           Test("  null}").Type);
            Assert.AreEqual("null",                             Test("  null}").Value);

            Assert.AreEqual(JsonToken.TokenType.Boolean,        Test("  true ").Type);
            Assert.AreEqual("true",                             Test("  true ").Value);
                                                                             
            Assert.AreEqual(JsonToken.TokenType.Boolean,        Test("  false,").Type);
            Assert.AreEqual("false",                            Test("  false,").Value);
        }

        private JsonToken Test(string text)
        {
            using var json = new MemoryStream(UTF8Encoding.Default.GetBytes(text));
            var reader     = new CharacterReader(json);
            var tokenizer  = new JsonTokenizer();
            var lineNumber = 0L;

            return tokenizer.ReadNextToken(reader, ref lineNumber);
        }
    }
}
