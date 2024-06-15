using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Project;

namespace JTran.Console.UnitTests
{
    [TestClass]
    public class ProjectExtensionsTests
    {
        [TestMethod]
        public void ProjectExtensions_AddArguments()
        {
            var project = new JTran.Project.Project();
            
            project.AddArguments("-environment prod");
            project.AddArguments("-environment2 'prod baby'");

            Assert.AreEqual("prod", project.ArgumentProviders[1]["environment"]);
            Assert.AreEqual("prod baby", project.ArgumentProviders[0]["environment2"]);
        } 
    }
}
