using System.Text;

using Newtonsoft.Json;

using JTran.Common;
using JTran.Json;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JTran.PerformanceTests
{
    [TestClass]
    public class RotaTests
    {
        [TestMethod]
        public void Rota_flights()
        {
            var transform = LoadFile("flightgenerator.jtran");
            var transformer = CreateTransformer(transform);

            using var output = File.OpenWrite($"c:\\Documents\\Testing\\JTran\\flights.json");
            using var input = new MemoryStream(UTF8Encoding.Default.GetBytes(LoadFile("ships.json")));

            transformer.Transform(input, output);
        }

        #region Private

        internal static string LoadFile(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.PerformanceTests.Resources.{name}");
                       
            if(stream == null)
                throw new FileNotFoundException(name);

            return stream!.ReadString();
        }

        internal static Stream LoadStream(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.PerformanceTests.Resources.{name}");
                       
            if(stream == null)
                throw new FileNotFoundException(name);

            return stream;
        }

        private static ITransformer<string> CreateTransformer(string transform)
        {
            return TransformerBuilder.FromString(transform)
                                     .AddDocumentRepository("docs", new DocumentRepository())
                                     .AddExtension(new JTran.Random.RandomExtensions())
                                     .Build<string>();
        }

        public class DocumentRepository : IDocumentRepository2
        { 
            public string GetDocument(string name)
            {
                return LoadFile(name + ".json");
            }

            public Stream GetDocumentStream(string name)
            {
                return new MemoryStream(UTF8Encoding.Default.GetBytes(GetDocument(name)));
            }
        }

        #endregion
    }
}