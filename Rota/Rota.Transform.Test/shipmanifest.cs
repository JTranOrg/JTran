using Newtonsoft.Json.Linq;

namespace Rota.Transform.Test
{
    [TestClass]
    public class ShipManifestTests
    {
        [TestMethod]
        public async Task ShipManifest_success()
        {
            var result = await Test("shipmanifest", "{ 'bob': 'bob'}");

            Assert.IsNotNull(result);

            var jobj = JObject.Parse(result);
            var pods = jobj["pods"] as JArray;

            Assert.IsNotNull(pods);
        }
        
        #region Private

        public async Task<string> Test(string transform, string data)
        {
            var result = await TransformerTest.TestStaticData(transform, data);           
            var jobj   = JObject.Parse(result)!;

            Assert.IsNotNull(jobj);

            #if DEBUG
            await File.WriteAllTextAsync($"c:\\Development\\Testing\\Rota\\{transform}.json", result);
            #endif

            return result;
        }

        #endregion
    }
}