using System.Linq;
using Fubu.Generation;
using FubuCsProjFile.Templating.Graph;
using FubuTestingSupport;
using NUnit.Framework;

namespace fubu.Testing.Generation
{
    [TestFixture]
    public class TemplatingTester
    {
        [Test]
        public void should_add_a_bundler_step_if_there_are_any_gem_references()
        {
            var request = new TemplateRequest
            {
                SolutionName = "Foo",
                RootDirectory = "Foo"
            };

            request.AddTemplate("baseline");

            Templating.BuildPlan(request)
                      .Steps.OfType<BundlerStep>()
                      .Count().ShouldEqual(1);
        }
    }
}