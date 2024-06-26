
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
            Assert.AreEqual(JsonToken.TokenType.EOF,            Test("").Type);
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
            Assert.AreEqual("bob",                              Test("  \"bob\"").Value!.ToString());

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \"bo\\\"b\"").Type);
            Assert.AreEqual("bo\"b",                            Test("  \"bo\\\"b\"").Value!.ToString());

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \'bob\'").Type);
            Assert.AreEqual("bob",                              Test("  \'bob\'").Value!.ToString());

            Assert.AreEqual(JsonToken.TokenType.Text,           Test("  \"123\"").Type);
            Assert.AreEqual("123",                              Test("  \"123\"").Value!.ToString());

            Assert.AreEqual(JsonToken.TokenType.Null,           Test("  null}").Type);
            Assert.AreEqual(JsonToken.TokenType.Null,           Test("  null}\r\nfred:").Type);
            Assert.AreEqual(JsonToken.TokenType.Null,           Test("  null  \r\nfred:").Type);
            Assert.AreEqual(JsonToken.TokenType.Null,           Test("  null fred:").Type);
            Assert.AreEqual(JsonToken.TokenType.Null,           Test("  null}").Type);

            Assert.AreEqual(JsonToken.TokenType.Boolean,        Test("  true ").Type);
            Assert.IsTrue((bool)Test("  true ").Value!);
                                                                             
            Assert.AreEqual(JsonToken.TokenType.Boolean,        Test("  false,").Type);
            Assert.IsFalse((bool)Test("  false,").Value!);

            Assert.AreEqual("bob",                              Test(" bob, // fred").Value.ToString());
            Assert.AreEqual("bob",                              Test(" bob// fred").Value.ToString());
            Assert.AreEqual("end of file",                      Test(" // fred").Value.ToString());
            Assert.AreEqual("//",                                 Test(" '//' fred").Value.ToString());
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
