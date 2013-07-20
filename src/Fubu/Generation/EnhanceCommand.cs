using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FubuCore.CommandLine;
using FubuCore;
using FubuCsProjFile;
using FubuCsProjFile.Templating;

namespace Fubu.Generation
{
    public class EnhanceInput
    {
        [Description("Name of the project to enhance")]
        public string Project { get; set; }

        [Description("The options to apply")]
        public IEnumerable<string> Options { get; set; }

        [Description("List all the valid options")]
        public bool ListFlag { get; set; }
    }

    [CommandDescription("Enhances an existing project by applying infrastructure templates to an existing project")]
    public class EnhanceCommand : FubuCommand<EnhanceInput>
    {


        public override bool Execute(EnhanceInput input)
        {
            var solutionFile = SolutionFinder.FindSolutionFile();


            // TODO -- check validity of the solutionFile

            var request = new TemplateRequest
            {
                RootDirectory = Environment.CurrentDirectory.ToFullPath(),
                SolutionName = Path.GetFileNameWithoutExtension(solutionFile)
            };

            var projectRequest = new ProjectRequest(input.Project, "baseline");
            request.AddProjectRequest(projectRequest);
            input.Options.Each(projectRequest.AddAlteration);

            var plan = NewCommand.BuildTemplatePlan(request);
            plan.Solution = Solution.LoadFrom(solutionFile);

            // TODO -- duplication
            plan.Execute();

            new RakeStep().Alter(plan);

            plan.WriteInstructions();

//            if (solutionFile.IsEmpty())
//            {
//                ConsoleWriter.Write(ConsoleColor.Yellow, "Could not find a single solution.  Try specifying the ");
//            }

            return true;
        }
    }
}