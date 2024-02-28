using Newtonsoft.Json.Linq;

namespace Rota.Transform.Test
{
    [TestClass]
    public class ShipGeneratorTests
    {
        [TestMethod]
        public async Task ShipGenerator_success()
        {
            var result = await Test("shipgenerator", "{ 'bob': 'bob'}");

            Assert.IsNotNull(result);
        }
        
        #region Private

        public async Task<string> Test(string transform, string data)
        {
            var result = await TransformerTest.TestStaticData(transform, data);           
            var array   = JArray.Parse(result)!;

            Assert.IsNotNull(array);

            #if DEBUG
            await File.WriteAllTextAsync($"C:\\Development\\Projects\\JTranOrg\\JTran\\Rota\\Transforms\\Documents\\ships.json", result);
            #endif

            return result;
        }

        #endregion
    }
}