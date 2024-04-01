using Newtonsoft.Json.Linq;

namespace Rota.Transform.Test
{
    [TestClass]
    public class ShipGeneratorTests
    {
        [TestMethod]
        [DataRow(1415)]
        public async Task ShipGenerator_success(int numShips)
        {
            var result = await Test("shipgenerator", "{ 'bob': 'bob'}", new { NumShips = numShips });

            Assert.IsNotNull(result);
        }
        
        #region Private

        public async Task<string> Test(string transform, string data, object? args = null)
        {
            var result = await TransformerTest.TestStaticData(transform, data, args: args);           
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