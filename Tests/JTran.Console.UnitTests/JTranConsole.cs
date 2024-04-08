using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using JTran.Project;

namespace JTran.Console.UnitTests
{
    [TestClass]
    public class JTranConsoleTests
    {
        [TestMethod]
        [DataRow("Project1")]
        public void JTranConsole(string projectName)
        {
            var projectPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Projects\\{projectName}.json";
            
            JTran.Console.Program.Main(new string[] {"-p", projectPath});

            Assert.IsTrue(true);
        } 
        
        [TestMethod]
        public void JTranConsole_test_something()
        {
            var project = new JTran.Project.Project
            {
                DocumentPaths = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "docs", "doc1" }
                }
            };

            var json = JsonConvert.SerializeObject(project);

            Assert.IsNotNull(json);
        }
    }
}
