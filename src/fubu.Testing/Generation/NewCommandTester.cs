using System;
using Fubu.Generation;
using FubuCore;
using FubuCsProjFile.Templating;
using FubuCsProjFile.Templating.Graph;
using FubuCsProjFile.Templating.Runtime;
using FubuMVC.Core;
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
            request.Projects.Single().Template.ShouldContain("baseline");
        }

        [Test]
        public void supports_the_shortname_flag()
        {
            var input = new NewCommandInput
            {
                SolutionName = "FubuMVC.Scenarios",
                AppFlag = true,
                ShortNameFlag = "Foo"
            };

            var request = NewCommand.BuildTemplateRequest(input);

            request.Projects.Single().Substitutions.ValueFor(ProjectPlan.SHORT_NAME).ShouldEqual("Foo");
        }

        [Test]
        public void add_no_views_home_page_if_there_are_no_views()
        {
            var input = new NewCommandInput
            {
                SolutionName = "FubuMVC.Scenarios",
                AppFlag = true,
            };

            var request = NewCommand.BuildTemplateRequest(input);
            request.Projects.Single().Alterations.ShouldContain("no-views");
        }

        [Test]
        public void add_spark_but_not_no_views_if_spark_option_is_requested()
        {
            var input = new NewCommandInput
            {
                SolutionName = "FubuMVC.Scenarios",
                AppFlag = true,
                OptionsFlag = new string[]{"spark"}
            };

            var request = NewCommand.BuildTemplateRequest(input);
            request.Projects.Single().Alterations.ShouldNotContain("no-views");
            request.Projects.Single().Alterations.ShouldContain("spark");
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
        public void no_tests_if_no_tests_flag()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                AppFlag = true,
                TestsFlag = false
            };

            var request = NewCommand.BuildTemplateRequest(input);
            request.TestingProjects.Any().ShouldBeFalse();
        }

        [Test]
        public void adds_in_the_testing_request_if_app_and_tests_are_selected()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                AppFlag = true,
                TestsFlag = true
            };

            var request = NewCommand.BuildTemplateRequest(input);
            var testingRequest = request.TestingProjects.Single();

            testingRequest.ShouldNotBeNull();
            testingRequest.OriginalProject.ShouldEqual("NewThing");
            testingRequest.Name.ShouldEqual("NewThing.Testing");
            testingRequest.Template.ShouldEqual("baseline");

            testingRequest.Alterations.Single().ShouldEqual("unit-testing");
        }

        [Test]
        public void license_file_is_ignored()
        {
            NewCommand.IsBaselineFile("license.txt")
                .ShouldBeTrue();
        }

        [Test]
        public void dot_git_files_are_ignored()
        {
            NewCommand.IsBaselineFile(".git/something")
                .ShouldBeTrue();
        }

        [Test]
        public void readme_files_are_ignored()
        {
            NewCommand.IsBaselineFile("readme").ShouldBeTrue();
            NewCommand.IsBaselineFile("readme.txt").ShouldBeTrue();
            NewCommand.IsBaselineFile("README.txt").ShouldBeTrue();
            NewCommand.IsBaselineFile("README.markdown").ShouldBeTrue();
            NewCommand.IsBaselineFile("README.md").ShouldBeTrue();
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
            project.Template.ShouldEqual("baseline");
        }

        [Test]
        public void defaults_to_structuremap()
        {
            project.Alterations.ShouldContain("structuremap");
        }

        [Test]
        public void has_empty_fubumvc()
        {
            project.Alterations.ShouldContain("fubumvc-empty");
        }


    }
}