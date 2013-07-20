using System;
using System.Collections.Generic;
using System.ComponentModel;
using FubuCore;

namespace Fubu.Generation
{

    public class NewCommandInput
    {
        [Description("Used in many templates as a prefix for generted classes")]
        public string ShortNameFlag { get; set; }

        public NewCommandInput()
        {
            RippleFlag = FeedChoice.PublicOnly;
        }


        // If this is blank, use this folder as the solution name too
        [Description("Name of the solution and the root folder without an extension")]
        public string SolutionName { get; set; }

        [Description("If chosen, fubu new will also create a single, empty FubuMVC application project with the same name")]
        public bool AppFlag { get; set; }

        [Description("Clean out any existing contents of the target folder before running the templates")]
        public bool CleanFlag { get; set; }

        [Description("Only list a preview of the template plan, but do not execute the plan")]
        public bool PreviewFlag { get; set; }

        [Description("Add a testing library for the project using the default FubuTestingSupport w/ NUnit")]
        public bool TestsFlag { get; set; }

        [Description("Ignore the presence of existing files")]
        public bool IgnoreFlag { get; set; }

        [Description("Choose a ripple configuration for only public Nuget feeds, including the Fubu TeamCity feed, or 'floating' on the Fubu edge")]
        public FeedChoice RippleFlag { get; set; }

        [Description("Extra options for the new application")]
        public IEnumerable<string> OptionsFlag { get; set; }

        public string SolutionDirectory()
        {
            return Environment.CurrentDirectory.AppendPath(SolutionName);
        }
    }

    public enum FeedChoice
    {
        FloatingEdge,
        Edge,
        PublicOnly
    }
}