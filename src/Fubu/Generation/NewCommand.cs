using System;
using System.Collections.Generic;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;
using FubuCore;

namespace Fubu.Generation
{
    [CommandDescription("Creates a new FubuMVC solution", Name = "new")]
    public class NewCommand : FubuCommand<NewCommandInput>
    {
        private static readonly IDictionary<FeedChoice, string> _rippleTemplates = new Dictionary<FeedChoice, string>{{FeedChoice.Edge, "edge-ripple"}, {FeedChoice.FloatingEdge, "floating-ripple"}, {FeedChoice.PublicOnly, "public-ripple"}}; 

        public override bool Execute(NewCommandInput input)
        {
            // Only supporting the "path is right underneath where I am right now"
            var library = LoadTemplates();
            var request = BuildTemplateRequest(input);

            // TODO -- try to make clean work.  Use the ripple way
//            if (input.CleanFlag)
//            {
//                new FileSystem().CleanDirectory(request.RootDirectory);
//            }

            request.AddTemplate("baseline");

            var planner = new TemplatePlanBuilder(library);

            var plan = planner.BuildPlan(request);

            plan.Execute();

            // TODO -- gonna prolly have to run rake

            return true;

//            Console.WriteLine("Solution {0} created", input.ProjectName);
//            return true;
        }

        public static TemplateRequest BuildTemplateRequest(NewCommandInput input)
        {
            var request = new TemplateRequest
            {
                RootDirectory = ".".ToFullPath().AppendPath(input.SolutionName),
                SolutionName = input.SolutionName
            };

            request.AddTemplate(_rippleTemplates[input.RippleFlag]);

            return request;
        }

        public static TemplateLibrary LoadTemplates()
        {
            var path = ".".ToFullPath()
                          .ParentDirectory().ParentDirectory().ParentDirectory()
                          .ParentDirectory()
                          .AppendPath("templates");

            return new TemplateLibrary(path);
        }


    }
}