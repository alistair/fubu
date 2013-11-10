using System;
using Fubu.Generation;
using FubuCsProjFile.Templating.Graph;
using NUnit.Framework;
using FubuCore;
using FubuTestingSupport;

namespace fubu.Testing.Generation
{
    [TestFixture]
    public class NewSolutionInputTester
    {
        [Test]
        public void solution_path_is_just_current_plus_solution_name_by_default()
        {
            var input = new NewCommandInput
            {
                SolutionName = "MySolution"
            };

            input.SolutionDirectory().ToFullPath()
                 .ShouldEqual(Environment.CurrentDirectory.AppendPath("MySolution").ToFullPath());
        }

        [Test]
        public void public_only_is_the_default_ripple_setting()
        {
            new NewSolutionInput().RippleFlag.ShouldEqual(FeedChoice.PublicOnly);
        }

        [Test]
        public void choose_the_public_ripple()
        {
            var input = new NewCommandInput
            {
                SolutionName = "NewThing",
                RippleFlag = FeedChoice.PublicOnly
            };

            var request = input.CreateRequestForSolution();

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

            var request = input.CreateRequestForSolution();

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

            var request = input.CreateRequestForSolution();

            request.Templates.ShouldNotContain("public-ripple");
            request.Templates.ShouldContain("edge-ripple");
            request.Templates.ShouldNotContain("floating-ripple");
        }
    }
}