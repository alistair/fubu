using System;
using System.Collections.Generic;
using System.ComponentModel;
using FubuCore;
using FubuCsProjFile.Templating.Graph;

namespace Fubu.Generation
{
    public class NewCommandInput
    {
        private static readonly IDictionary<FeedChoice, string> _rippleTemplates = new Dictionary<FeedChoice, string>
        {
            {FeedChoice.Edge, "edge-ripple"},
            {FeedChoice.FloatingEdge, "floating-ripple"},
            {FeedChoice.PublicOnly, "public-ripple"}
        };

        public NewCommandInput()
        {
            RippleFlag = FeedChoice.PublicOnly;
            Profile = "web-app";
        }

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

        public TemplateRequest CreateRequestForSolution()
        {
            var request = new TemplateRequest
            {
                RootDirectory = SolutionDirectory(),
                SolutionName = SolutionName
            };

            request.AddTemplate("baseline");

            request.AddTemplate(_rippleTemplates[RippleFlag]);

            return request;
        }

        [Description("Project profile.  Use the --list flag to see the valid options")]
        public string Profile { get; set; }

        // TODO -- change this to --no-tests
        [Description("Add a testing library for the project using the default FubuTestingSupport w/ NUnit")]
        public bool TestsFlag { get; set; }

        [Description("Ignore the presence of existing files")]
        public bool IgnoreFlag { get; set; }


        [Description("Extra options for the new application")]
        public IEnumerable<string> OptionsFlag { get; set; }
    }

    public enum FeedChoice
    {
        FloatingEdge,
        Edge,
        PublicOnly
    }
}