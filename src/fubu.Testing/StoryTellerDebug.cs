using FubuCore;
using NUnit.Framework;
using StoryTeller.Execution;
using StoryTeller.Workspace;

namespace StoryTellerTestHarness
{
    [TestFixture, Explicit]
    public class Template
    {
        private ProjectTestRunner runner;

        [TestFixtureSetUp]
        public void SetupRunner()
        {
            runner = new ProjectTestRunner(@"");
        }

        [Test]
        public void Build_a_simple_library_solution()
        {
            var project = Project.ForDirectory(".".ToFullPath().ParentDirectory().ParentDirectory());
            using (var runner = new ProjectTestRunner(project))
            {
                runner.RunAndAssertTest("New/Build a simple library solution");
            }
            
        }

        [TestFixtureTearDown]
        public void TeardownRunner()
        {
            runner.Dispose();
        }
    }
}