using System.Text.Json;

namespace JTran.Transform.UnitTests
{
    [TestClass]
    [TestCategory("Operators")]
    public class OperatorTests
    {
        [TestMethod]
        [DataRow("Operators.dotattributes",  "car1")]
        public async Task Operator_dotattributes(string transform, string data)
        {
            await TransformerTest.Test(transform, data);
        }

        [TestMethod]
        [DataRow("Operators.ancestor1",  "customers5")]
        public async Task Operator_ancestor1(string transform, string data)
        {
            var result = await TransformerTest.Test(transform, data);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));

            var residents = JsonSerializer.Deserialize<List<Resident>>(result);

            Assert.AreEqual(4, residents.Count);
            Assert.AreEqual("Mr. Smith, John", residents[0].name);
            Assert.AreEqual("Mr. Smith, Mary", residents[1].name);
            Assert.AreEqual("Mr. Anderson, Fred", residents[2].name);
            Assert.AreEqual("Mr. Anderson, Linda", residents[3].name);
        }

        [TestMethod]
        [DataRow("Operators.ancestor2",  "profile")]
        public async Task Operator_ancestor2(string transform, string data)
        {
            var result = await TransformerTest.Test(transform, data);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));

            var profile = JsonSerializer.Deserialize<Profile>(result);

            Assert.AreEqual("John",     profile.Name);
            Assert.AreEqual("Chicago",   profile.City);
            Assert.AreEqual("Address",   profile.ParentName);
        }
    }

    file class Resident
    {
        public string name { get; set; } = string.Empty;
        public string field_name { get; set; } = string.Empty;
    }

    file class Profile
    {
        public string Name           { get; set; } = string.Empty;
        public string City           { get; set; } = string.Empty;
        public string ParentName     { get; set; } = string.Empty;
        public string FirstNameName  { get; set; } = string.Empty;
        public List<EmailEntry> Emails  { get; set; } = new();
    }

    file class EmailEntry
    {
        public string Email           { get; set; } = string.Empty;
        public int    Index           { get; set; }
        public string Previous        { get; set; } = string.Empty;
    }
}