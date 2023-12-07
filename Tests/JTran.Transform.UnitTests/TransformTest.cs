using System.Collections;
using System.Reflection;

using JTran.Common;
using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    public static class TransformerTest
    {
        public static async Task<string> Test(string transformName, string sampleName, IEnumerable extFunctions = null, Func<string, string>? dataTransform = null)
        {
            var transform   = await LoadTransform(transformName);
            var data        = await LoadSample(sampleName);
            var transformer = new JTran.Transformer(transform, extFunctions);

            if(dataTransform != null) 
                data = dataTransform(data);

            var result = transformer.Transform(data);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.AreNotEqual(result, transform);
            Assert.AreNotEqual(result, data);

            var jobj = JObject.Parse(result);

            Assert.IsNotNull(jobj);

            return result;
        }

        public static async Task<string> TestList(string transformName, IEnumerable list, string listName)
        {
            var transform   = await LoadTransform(transformName);
            var transformer = new JTran.Transformer(transform, null);
            var result = "";

            using(var output = new MemoryStream())
            { 
                transformer.Transform(list, listName, output);

                result = await output.ReadStringAsync();
            }

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));

            var jobj = JObject.Parse(result);

            Assert.IsNotNull(jobj);

            return result;
        }

        #region Private

        private static Task<string> LoadTransform(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.Transform.UnitTests.Test_Transforms.{name}.jtran");
            
            return stream!.ReadStringAsync();
        }

        private static Task<string> LoadSample(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.Transform.UnitTests.Sample_Data.{name}.json");
            
            return stream!.ReadStringAsync();
        }

        #endregion
    }
}