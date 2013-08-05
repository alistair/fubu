using System;
using System.Threading;
using Fubu.Generation;
using FubuCore;
using FubuCsProjFile;
using FubuCsProjFile.Templating;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace fubu.Testing.Generation
{
    [TestFixture]
    public class BottleCommandTester
    {
        [Test]
        public void solution_does_not_exist()
        {
            var original = Environment.CurrentDirectory;

            var directory = Guid.NewGuid().ToString();
            new FileSystem().CreateDirectory(directory);

            Environment.CurrentDirectory = directory.ToFullPath();

            try
            {
                new BottleCommand().Execute(new BottleInput()).ShouldBeFalse();
            }
            finally
            {
                Environment.CurrentDirectory = original;
            }
        }

        [Test]
        public void too_many_solutions_and_none_specified()
        {
            var original = Environment.CurrentDirectory;

            var directory = Guid.NewGuid().ToString();
            new FileSystem().CreateDirectory(directory);
            Solution.CreateNew(directory, "foo").Save();
            Solution.CreateNew(directory, "bar").Save();

            Environment.CurrentDirectory = directory.ToFullPath();

            try
            {
                new BottleCommand().Execute(new BottleInput()).ShouldBeFalse();
            }
            finally
            {
                Environment.CurrentDirectory = original;
            }
        }
    }

    [TestFixture]
    public class when_building_the_template_request
    {
        private BottleInput theInput;
        private TemplateRequest theRequest;

        [SetUp]
        public void SetUp()
        {
            theInput = new BottleInput
            {
                Name = "MyBottle",
                OptionsFlag = new string[]{"raven", "spark"},
                ShortNameFlag = "Mine"
            };

            theRequest = BottleCommand.BuildTemplateRequest(theInput, "MySolution.sln");
        }

        [Test]
        public void project_does_need_to_be_the_baseline()
        {
            theRequest.Projects.Single().Template.ShouldEqual("baseline");
        }

        [Test]
        public void reads_the_options()
        {
            var theBottle = theRequest.Projects.Single();
            theBottle.Alterations.ShouldContain("spark");
            theBottle.Alterations.ShouldContain("raven");
        }

        [Test]
        public void adds_the_short_name_substitution()
        {
            theRequest.Projects.Single().Substitutions.ValueFor(ProjectPlan.SHORT_NAME)
                      .ShouldEqual("Mine");
        }
    }

   
    [TestFixture]
    public class when_executing_the_bottle_command_against_an_existing_solution : GenerationContext
    {
        // .package-manifest, gets the [FubuModule]
        // 2nd project

        protected override void theContext()
        {
            new QuickstartCommand().Execute(new QuickstartInput
            {
                SolutionName = "MyNewProject"
            });

            Environment.CurrentDirectory = Environment.CurrentDirectory.AppendPath("MyNewProject");

            new BottleCommand().Execute(new BottleInput
            {
                Name = "MyBottle"
            });
        }

        [Test]
        public void should_have_created_a_new_project()
        {
            csprojFor("MyBottle").ShouldNotBeNull();
        }

        [Test]
        public void should_have_added_FubuModule_attribute()
        {
            var contents = readFile("src", "MyBottle", "Properties", "AssemblyInfo.cs").ToArray();

            contents.ShouldContain("[assembly: FubuModule]");
            contents.ShouldContain("using FubuMVC.Core;");
        }
    }
}