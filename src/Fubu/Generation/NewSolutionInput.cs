using System;
using System.ComponentModel;
using FubuCore;

namespace Fubu.Generation
{
    public class NewSolutionInput
    {
        [Description("Name of the solution and the root folder without an extension")]
        public string SolutionName { get; set; }

        [Description("Only list a preview of the template plan, but do not execute the plan")]
        public bool PreviewFlag { get; set; }

        [Description("Choose a ripple configuration for only public Nuget feeds, including the Fubu TeamCity feed, or 'floating' on the Fubu edge")]
        public FeedChoice RippleFlag { get; set; }

        [Description("Used in many templates as a prefix for generted classes")]
        public string ShortNameFlag { get; set; }

        [Description("Clean out any existing contents of the target folder before running the templates")]
        public bool CleanFlag { get; set; }

        public string SolutionDirectory()
        {
            return Environment.CurrentDirectory.AppendPath(SolutionName);
        }
    }
}