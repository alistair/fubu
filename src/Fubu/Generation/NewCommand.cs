using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;
using FubuCore;
using System.Linq;

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
            if (input.CleanFlag)
            {
                new FileSystem().ForceClean(request.RootDirectory);
            }
            else
            {
                AssertEmpty(request.RootDirectory);
            }

            request.AddTemplate("baseline");

            var planner = new TemplatePlanBuilder(library);

            var plan = planner.BuildPlan(request);

            plan.Execute();

            // TODO -- gonna prolly have to run rake

            return true;

//            Console.WriteLine("Solution {0} created", input.ProjectName);
//            return true;
        }

        public static void AssertEmpty(string directory)
        {
            if (Directory.Exists(directory))
            {
                if (new FileSystem().FindFiles(directory, FileSet.Everything()).Any())
                {
                    throw new InvalidOperationException("Directory {0} is not empty!  Use the --clean flag to override this validation check to overwrite the contents of the solution".ToFormat(directory));
                }
            }
        }

        public static TemplateRequest BuildTemplateRequest(NewCommandInput input)
        {
            var request = new TemplateRequest
            {
                RootDirectory = input.SolutionDirectory(),
                SolutionName = input.SolutionName
            };

            request.AddTemplate(_rippleTemplates[input.RippleFlag]);

            if (input.AppFlag)
            {
                var project = new ProjectRequest {Name = input.SolutionName, Template = "fubumvc-empty"};
                project.AddAlteration("structuremap");

                request.AddProjectRequest(project);
            }

            return request;
        }

        public static TemplateLibrary LoadTemplates()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory.AppendPath("templates");
            if (Directory.Exists(path))
            {
                return new TemplateLibrary(path);
            }

            // Testing mode.
            path = ".".ToFullPath()
                          .ParentDirectory().ParentDirectory().ParentDirectory()
                          .ParentDirectory()
                          .AppendPath("templates");

            return new TemplateLibrary(path);
        }


    }
}