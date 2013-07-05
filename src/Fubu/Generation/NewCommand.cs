using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;
using FubuCore;
using System.Linq;

namespace Fubu.Generation
{
    [CommandDescription("Lays out and creates a new code tree with the idiomatic fubu project layout", Name = "new")]
    public class NewCommand : FubuCommand<NewCommandInput>
    {
        private static readonly IDictionary<FeedChoice, string> _rippleTemplates = new Dictionary<FeedChoice, string>{{FeedChoice.Edge, "edge-ripple"}, {FeedChoice.FloatingEdge, "floating-ripple"}, {FeedChoice.PublicOnly, "public-ripple"}};

        public NewCommand()
        {
            Usage("default").Arguments(x => x.SolutionName);
        }

        public override bool Execute(NewCommandInput input)
        {
            var request = BuildTemplateRequest(input);
            var plan = BuildTemplatePlan(request);

            if (input.PreviewFlag)
            {
                Console.WriteLine("To solution directory " + input.SolutionDirectory());
                Console.WriteLine();

                plan.WritePreview();
            }
            else
            {
                prepareTargetDirectory(input, request);

                plan.Execute();

                new RakeStep().Alter(plan);

                plan.WriteInstructions();
            }
            



            return true;
        }

        public static TemplatePlan BuildTemplatePlan(TemplateRequest request)
        {
            var library = LoadTemplates();
            var planner = new TemplatePlanBuilder(library);

            var plan = planner.BuildPlan(request);
            if (plan.Steps.OfType<GemReference>().Any())
            {
                plan.Add(new BundlerStep());
            }

            return plan;
        }

        private static void prepareTargetDirectory(NewCommandInput input, TemplateRequest request)
        {
            if (input.CleanFlag)
            {
                new FileSystem().ForceClean(request.RootDirectory);
            }
            else
            {
                AssertEmpty(request.RootDirectory);
            }
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
                var project = new ProjectRequest(input.SolutionName, "fubumvc-empty");
                project.AddAlteration("structuremap");
                project.AddAlteration("baseline");

                if (input.ShortNameFlag.IsNotEmpty())
                {
                    project.Substitutions.Set(ProjectPlan.SHORT_NAME, input.ShortNameFlag);
                }

                // TODO -- hard-coded for now, but needs to change later when spark & razor are available
                project.AddAlteration("no-views");

                request.AddProjectRequest(project);
            }

            request.AddTemplate("baseline");

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