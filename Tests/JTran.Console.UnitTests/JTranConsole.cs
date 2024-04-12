using System.Reflection;
using System.Threading.Tasks;

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
        [DataRow("Project1a")]
        public async Task JTranConsole_singlefile(string projectName)
        {
            var projectPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Projects\\{projectName}.json";
            
            await JTran.Console.Program.Main(new string[] {"-p", projectPath});

            Assert.IsTrue(true);
        } 

        [TestMethod]
        [DataRow("Project_Split")]
        public async Task JTranConsole_singlesrc_splitoutput(string projectName)
        {
            var projectPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Projects\\{projectName}.json";
            
            await JTran.Console.Program.Main(new string[] {"-p", projectPath});

            Assert.IsTrue(true);
        } 

        [TestMethod]
        [DataRow("Project2")]
        public async Task JTranConsole_wildcards(string projectName)
        {
            var projectPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Projects\\{projectName}.json";
            
            await JTran.Console.Program.Main(new string[] {"-p", projectPath});

            Assert.IsTrue(true);
        } 

        [TestMethod]
        [DataRow("Project3")]
        public async Task JTranConsole_wildcards_nofilename(string projectName)
        {
            var projectPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Projects\\{projectName}.json";
            
            await JTran.Console.Program.Main(new string[] {"-p", projectPath});

            Assert.IsTrue(true);
        } 
    }
}
