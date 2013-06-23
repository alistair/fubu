using Fubu.Generation;
using FubuCsProjFile.Templating;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace fubu.Testing.Generation
{
    [TestFixture]
    public class NewCommandTester
    {
        [Test]
        public void default_ripple_is_public_only()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
            };

            var request = NewCommand.BuildTemplateRequest(input);

            request.Templates.ShouldContain("public-ripple");
            request.Templates.ShouldNotContain("edge-ripple");
            request.Templates.ShouldNotContain("floating-ripple");
        }

        [Test]
        public void choose_the_float_ripple()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                RippleFlag = FeedChoice.FloatingEdge
            };

            var request = NewCommand.BuildTemplateRequest(input);

            request.Templates.ShouldNotContain("public-ripple");
            request.Templates.ShouldNotContain("edge-ripple");
            request.Templates.ShouldContain("floating-ripple");
        }

        [Test]
        public void choose_the_edge_ripple()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                RippleFlag = FeedChoice.Edge
            };

            var request = NewCommand.BuildTemplateRequest(input);

            request.Templates.ShouldNotContain("public-ripple");
            request.Templates.ShouldContain("edge-ripple");
            request.Templates.ShouldNotContain("floating-ripple");
        }

        [Test]
        public void no_project_if_app_flag_is_false()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                RippleFlag = FeedChoice.Edge
            };

            input.AppFlag.ShouldBeFalse();

            var request = NewCommand.BuildTemplateRequest(input);
        
            request.Projects.Any().ShouldBeFalse();
        }
    }

    [TestFixture]
    public class when_an_app_is_requested_within_the_new_request
    {
        private ProjectRequest project;

        [SetUp]
        public void SetUp()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                AppFlag = true
            };

            var request = NewCommand.BuildTemplateRequest(input);

            project = request.Projects.Single();
        }

        [Test]
        public void is_a_project()
        {
            project.ShouldNotBeNull();
        }

        [Test]
        public void project_has_the_same_name_as_the_solution()
        {
            project.Name.ShouldEqual("NewThing");
        }

        [Test]
        public void is_build_from_empty_fubumvc_app()
        {
            project.Template.ShouldEqual("fubumvc-empty");
        }

        [Test]
        public void defaults_to_structuremap()
        {
            project.Alterations.ShouldContain("structuremap");
        }
    }
}