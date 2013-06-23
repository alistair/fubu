using Fubu.Generation;
using NUnit.Framework;
using FubuTestingSupport;

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
    }
}