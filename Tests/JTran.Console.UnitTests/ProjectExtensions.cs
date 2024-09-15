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
            project.AddArguments("-arg3 \"george\"");

            Assert.AreEqual("george", project.ArgumentProviders[0]["arg3"]);
            Assert.AreEqual("prod", project.ArgumentProviders[2]["environment"]);
            Assert.AreEqual("prod baby", project.ArgumentProviders[1]["environment2"]);
        } 
    }
}
