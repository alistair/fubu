using System;
using Fubu.Generation;
using FubuCore;
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
        public void new_project_request_gets_the_assembly_version_alteration()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                AppFlag = true
            };

            var request = NewCommand.BuildTemplateRequest(input);
            request.Projects.Single().Alterations.ShouldContain("common-assembly");
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

        [Test]
        public void assert_folder_is_empty_does_not_blow_for_empty_target()
        {
            new FileSystem().DeleteDirectory("target");
            new FileSystem().CreateDirectory("target");

            NewCommand.AssertEmpty("target");
        }

        [Test]
        public void assert_folder_is_empty_is_fine_when_directory_does_not_exist()
        {
            new FileSystem().DeleteDirectory("nonexistent");
            NewCommand.AssertEmpty("nonexistent");
        }

        [Test]
        public void assert_folder_blows_up_when_directory_is_not_empty()
        {
            new FileSystem().CreateDirectory("not-empty");
            new FileSystem().WriteStringToFile("not-empty".AppendPath("foo.txt"), "anything");

            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                NewCommand.AssertEmpty("not-empty");
            });
        }

        [Test]
        public void adds_a_rake_step_to_the_plan()
        {
            NewCommand.BuildTemplatePlan(new TemplateRequest())
                      .Steps.Last().ShouldBeOfType<RakeStep>();
        }

        [Test]
        public void should_add_a_bundler_step_if_there_are_any_gem_references()
        {
            var request = new TemplateRequest
            {
                SolutionName = "Foo",
                RootDirectory = "Foo"
            };

            request.AddTemplate("baseline");

            NewCommand.BuildTemplatePlan(request)
                      .Steps.OfType<BundlerStep>()
                      .Count().ShouldEqual(1);
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