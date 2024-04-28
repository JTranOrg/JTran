using System.Collections;
using System.Reflection;
using System.Text;
using JTran.Common;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JTran.Transform.UnitTests
{
    public static class TransformerTest
    {
        public static async Task<string> Test(string transformName, string sampleName, IEnumerable extFunctions = null, Func<string, string>? dataTransform = null, IDictionary<string, string>? includeSource = null, TransformerContext? context = null)
        {
            var data = await LoadSample(sampleName);

            return await TestData(transformName, data, extFunctions, dataTransform, includeSource, context);
        }

        public static async Task TestData(string transformName, Stream input, Stream output, IEnumerable extFunctions = null, IDictionary<string, string>? includeSource = null, TransformerContext? context = null)
        {
            var transform   = await LoadTransform(transformName);
            var transformer = new JTran.Transformer(transform, extFunctions, includeSource: includeSource);

            transformer.Transform(input, output, context);

            return;
        }

        public static async Task TestData(string transformName, string data, Stream output, IEnumerable extFunctions = null, Func<string, string>? dataTransform = null, IDictionary<string, string>? includeSource = null, TransformerContext? context = null)
        {
            var transform   = await LoadTransform(transformName);
            var transformer = new JTran.Transformer(transform, extFunctions, includeSource: includeSource);

            if(dataTransform != null) 
                data = dataTransform(data);

            using var input = new MemoryStream(UTF8Encoding.Default.GetBytes(data));

            transformer.Transform(input, output, context);

            return;
        }

        public static async Task<string> TestData(string transformName, string data, IEnumerable extFunctions = null, Func<string, string>? dataTransform = null, IDictionary<string, string>? includeSource = null, TransformerContext? context = null)
        {
            using var output = new MemoryStream();

            await TestData(transformName, data, output, extFunctions, dataTransform, includeSource, context);

            var result = await output.ReadStringAsync();

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.AreNotEqual(result, data);

            return result;
        }

        public static async Task<string> TestList(string transformName, IEnumerable list, string? listName = null, TransformerContext? context = null)
        {
            var transform   = await LoadTransform(transformName);
            var transformer = new JTran.Transformer(transform, null);
            var result = "";

            using var output = new MemoryStream();

            transformer.Transform(list, listName, output, context: context);

            result = await output.ReadStringAsync();

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));

            if( listName != null)
            { 
                var jobj = JObject.Parse(result);

                Assert.IsNotNull(jobj);
            }
            else
            {
                var array = JArray.Parse(result);

                Assert.IsNotNull(array);
            }

            return result;
        }

        internal static Task<string> LoadTransform(string name)
        {
            var files = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var path = $"JTran.Transform.UnitTests.Test_Transforms.{name}.jtran";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            
            if(stream == null)
                throw new FileNotFoundException(name);

            return stream!.ReadStringAsync();
        }

        internal static Task<string> LoadSample(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.Transform.UnitTests.Sample_Data.{name}.json");
                       
            if(stream == null)
                throw new FileNotFoundException(name);

            return stream!.ReadStringAsync();
        }

        internal static Stream LoadSampleStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.Transform.UnitTests.Sample_Data.{name}.json");
        }
    }
}