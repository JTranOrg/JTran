using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

using JTran.Common;
using JTran.UnitTests;

namespace JTran.Project.UnitTests
{
    [TestClass]
    [TestCategory("Project")]
    public class CompiledProjectTests
    {
        [TestMethod]
        public async Task CompiledProject_Load()
        { 
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).SubstringBefore("bin");
            var project = new JTran.Project.Project
            {
                Name            = "Test Project",
                TransformPath   = Path.Combine(path, "Transforms\\nestedfunction.jtran"),
                SourcePath      = Path.Combine(path, "Sources\\source1.json"),
                DestinationPath = Path.Combine(path, "destination1.json"),
                IncludePaths    = new Dictionary<string, string> { { "default", Path.Combine(path, "Include") } },
                DocumentPaths   = new Dictionary<string, string> { { "default", Path.Combine(path, "Documents") } },
                ExtensionPaths  = new List<string> { Path.Combine(path, "Extensions") }
            };

            var proj = JTran.Project.CompiledProject.Load(project, (ex)=> 
            {
                throw ex;
            });

            Assert.AreEqual(1, proj.Extensions.Count);
            Assert.AreEqual(1, proj.Documents.Count);
            Assert.IsTrue(proj.Documents["default.manufacturers"].Contains("manufacturers"));

            using var stream = new MemoryStream();

            proj.Run(stream);

            var result = await stream.ReadStringAsync();

            Assert.IsNotNull(result);

            var json = JObject.Parse(result);

            Assert.IsNotNull(json);
            Assert.IsNotNull(json["Products"]);
            Assert.AreEqual(2, (json["Products"] as JArray).Count);
            Assert.AreEqual("Ajax Cleanser", (json["Products"] as JArray)[0]["Name"]);
            Assert.AreEqual("bottle", (json["Products"] as JArray)[0]["UOM"]);
        }
    }
}
