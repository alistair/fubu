using System;
using Fubu.Generation;
using NUnit.Framework;
using FubuCore;
using FubuTestingSupport;

namespace fubu.Testing.Generation
{
    [TestFixture]
    public class NewCommandInputTester
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
    }
}