using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using JTran.Project;
using System.Collections.Generic;
using System;

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
        [DataRow("Project_error")]
        public async Task JTranConsole_singlefile_error(string projectName)
        {
            var projectPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Projects\\{projectName}.json";
            
            var ex = await Assert.ThrowsExceptionAsync<Transformer.SyntaxException>( async ()=> await JTran.Console.Program.Main(new string[] {"-p", projectPath, "-se"}));

            Assert.IsTrue(ex.Message.Contains("at line 10"));
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

        [TestMethod]
        [DataRow("Project1")]
        public async Task JTranConsole_arguments_provider(string projectName)
        {
            var transformPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Transforms\\{projectName}b.jtran";
            var sourcePath    = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Sources\\{projectName}.json";
            var destination   = "C:\\Development\\Testing\\JTran\\Console Tests\\Project1b.json";
            var thisAssembly  = "C:\\Development\\Projects\\JTranOrg\\JTran\\Tests\\TestArgumentsProvider\\bin\\Debug\\net8.0\\TestArgumentsProvider.dll";
            
            await JTran.Console.Program.Main(new string[] {"-t", transformPath, "-s", sourcePath, "-o", destination, "-tp", "-environment 'prod'", "-a", thisAssembly + "::TestArgumentsProvider.MyArgs"});

            Assert.IsTrue(true);
        } 
    }

    public class MyArgs : Dictionary<string, object>
    {
        public MyArgs() 
        {
            this.Add("Phrase", "bobs your uncle");
        }
    }
}
