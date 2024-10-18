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
            
            var rtnCode = await JTran.Console.Program.Main(new string[] {"-p", projectPath, "-se"});

            Assert.AreEqual(1, rtnCode);
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
        [DataRow("Project1", "MyArgs")]
        [DataRow("Project1", "MyArgs2")]
        [DataRow("Project1", "MyArgs3")]
        public async Task JTranConsole_arguments_provider(string projectName, string argsName)
        {
            var transformPath = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Transforms\\{projectName}b.jtran";
            var sourcePath    = Assembly.GetExecutingAssembly().Location.SubstringBefore("\\bin") + $"\\Sources\\{projectName}.json";
            var destination   = $"C:\\Development\\Testing\\JTran\\Console Tests\\Project1{Guid.NewGuid().ToString()}.json";
            var thisAssembly  = "C:\\Development\\Projects\\JTranOrg\\JTran\\Tests\\TestArgumentsProvider\\bin\\Debug\\net8.0\\TestArgumentsProvider.dll";
            
            var returnCode = await JTran.Console.Program.Main(new string[] {"-t", transformPath, "-s", sourcePath, "-o", destination, "-tp", "-environment 'prod' -arg2 \"bob\"", "-a", thisAssembly + $"::TestArgumentsProvider.{argsName}"});

            Assert.AreEqual(0, returnCode);

            var result = System.IO.File.ReadAllText(destination);
            var persons = JsonConvert.DeserializeObject<Container>(result);

            Assert.IsNotNull(persons.Persons);
            Assert.AreEqual(4, persons.Persons.Count);

            // Clean up
            System.IO.File.Delete(destination);
        } 
    }

    public class MyArgs : Dictionary<string, object>
    {
        public MyArgs() 
        {
            this.Add("Phrase", "bobs your uncle");
        }
    }

    internal class Container
    {
        public List<Person> Persons { get; set; }
    }

    internal class Person
    {
        public string  Name    { get; set; }
        public string  Surname { get; set; }
        public string  Address { get; set; }
        public int     Age     { get; set; }
    }
}
