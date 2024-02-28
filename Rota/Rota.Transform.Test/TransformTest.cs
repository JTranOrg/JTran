using System.Collections;
using System.Reflection;

using JTran;
using JTran.Common;
using Newtonsoft.Json.Linq;

namespace Rota.Transform.Test
{
    public static class TransformerTest
    {
        public static async Task<string> Test(string transformName, string sampleName, IEnumerable extFunctions = null, Func<string, string>? dataTransform = null, IDictionary<string, string>? includeSource = null)
        {
            var data = await LoadSample(sampleName);

            return await TestStaticData(transformName, data, extFunctions, dataTransform, includeSource);
        }

        public static async Task<string> TestStaticData(string transformName, string data, IEnumerable extFunctions = null, Func<string, string>? dataTransform = null, IDictionary<string, string>? includeSource = null)
        {
            var transform   = await LoadTransform(transformName);
            var transformer = new JTran.Transformer(transform, new List<object> { new JTran.Random.RandomExtensions() }, includeSource: includeSource);

            if(dataTransform != null) 
                data = dataTransform(data);

            var context = new TransformerContext() { DocumentRepositories = new Dictionary<string, IDocumentRepository> { { "docs", new DocumentRepository() } }, Arguments = null};
            var result = transformer.Transform(data, context);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.AreNotEqual(result, transform);
            Assert.AreNotEqual(result, data);

            return result;
        }

        internal static Task<string> LoadTransform(string name)
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("Rota.Transform.Test\\bin\\Debug\\net8.0", "");
            
            return File.ReadAllTextAsync(Path.Combine(location, "Transforms", name + ".jtran"));
        }

        internal static Task<string> LoadSample(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.Transform.UnitTests.Sample_Data.{name}.json");
                       
            if(stream == null)
                throw new FileNotFoundException(name);

            return stream!.ReadStringAsync();
        }

        public class DocumentRepository : IDocumentRepository2
        { 
            private static string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("Rota.Transform.Test\\bin\\Debug\\net8.0", "")), "Transforms\\Documents");

            public DocumentRepository()
            {
            }

            public string GetDocument(string name)
            {
                return File.ReadAllText(Path.Combine(_path, name + ".json"));
            }

            public Stream GetDocumentStream(string name)
            {
                return File.OpenRead(Path.Combine(_path, name + ".json"));
            }
        }

    }
}