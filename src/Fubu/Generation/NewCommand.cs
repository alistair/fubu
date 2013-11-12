using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating.Graph;
using FubuCsProjFile.Templating.Planning;
using FubuCsProjFile.Templating.Runtime;

namespace Fubu.Generation
{
    [CommandDescription("Lays out and creates a new code tree with the idiomatic fubu project layout", Name = "new")]
    public class NewCommand : FubuCommand<NewCommandInput>
    {


        public NewCommand()
        {
            Usage("default").Arguments(x => x.SolutionName);
            Usage("profile").Arguments(x => x.SolutionName, x => x.Profile);
        }

        public override bool Execute(NewCommandInput input)
        {
            TemplateRequest request = input.CreateRequestForSolution();
            TemplatePlan plan = Templating.BuildPlan(request);

            if (input.PreviewFlag)
            {
                Console.WriteLine("To solution directory " + input.SolutionDirectory());
                Console.WriteLine();

                plan.WritePreview();
            }
            else
            {
                prepareTargetDirectory(input, request);

                Templating.ExecutePlan(plan);
                // TODO -- open the solution if you can
            }


            return true;
        }


        private static void prepareTargetDirectory(NewCommandInput input, TemplateRequest request)
        {
            if (input.CleanFlag)
            {
                new FileSystem().ForceClean(request.RootDirectory);
            }
            else if (!input.IgnoreFlag)
            {
                AssertEmpty(request.RootDirectory);
            }

            new FileSystem().CreateDirectory(request.RootDirectory);
        }


        public static void AssertEmpty(string directory)
        {
            if (Directory.Exists(directory))
            {
                IEnumerable<string> files = new FileSystem().FindFiles(directory, FileSet.Everything());
                string[] notBaseline = files.Where(x => !IsBaselineFile(x)).ToArray();
                if (notBaseline.Any())
                {
                    throw new InvalidOperationException(
                        "Directory {0} is not empty!  Use the --clean flag to override this validation check to overwrite the contents of the solution"
                            .ToFormat(directory));
                }
            }
        }

        public static bool IsBaselineFile(string file)
        {
            if (file.StartsWith(".git")) return true;

            if (Path.GetFileNameWithoutExtension(file).StartsWith("readme", StringComparison.OrdinalIgnoreCase))
                return true;

            if (file.Contains(".git")) return true;

            if (Path.GetFileName(file).EqualsIgnoreCase("license.txt")) return true;

            return false;
        }
    }
}