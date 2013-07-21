using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Bottles.Commands;
using FubuCore;
using FubuCore.CommandLine;
using FubuCsProjFile;
using FubuCsProjFile.Templating;

namespace Fubu.Generation
{
    public class BottleInput
    {
        [Description("The name of the new Bottle")]
        public string Name { get; set; }

//        [Description("For a feature bottle, setting the --application will copy all the system and nuget references from the original project")]
//        public string ApplicationFlag { get; set; }

        [Description("The options to apply")]
        public IEnumerable<string> OptionsFlag { get; set; }

        [Description("List all the valid options")]
        public bool ListFlag { get; set; }

        [Description("Used in many templates as a prefix for generted classes")]
        public string ShortNameFlag { get; set; }

    }

    [Description("Creates a new project as a FubuMVC Bottle")]
    public class BottleCommand : FubuCommand<BottleInput>
    {
        public override bool Execute(BottleInput input)
        {
            var solutionFile = SolutionFinder.FindSolutionFile();


            // TODO -- check validity of the solutionFile

            var request = new TemplateRequest
            {
                RootDirectory = Environment.CurrentDirectory.ToFullPath(),
                SolutionName = Path.GetFileNameWithoutExtension(solutionFile)
            };

            var projectRequest = new ProjectRequest(input.Name, "baseline");

            // TODO -- duplication!
            if (input.ShortNameFlag.IsNotEmpty())
            {
                projectRequest.Substitutions.Set(ProjectPlan.SHORT_NAME, input.ShortNameFlag);
            }


            request.AddProjectRequest(projectRequest);
            projectRequest.AddAlteration("fubu-bottle");
            if (input.OptionsFlag != null)
            {
                input.OptionsFlag.Each(projectRequest.AddAlteration);
            }

            var plan = NewCommand.BuildTemplatePlan(request);

            plan.Solution = Solution.LoadFrom(solutionFile);

            // TODO -- duplication
            plan.Execute();


            initializeTheBottle(input, plan);


            new RakeStep().Alter(plan);

            plan.WriteInstructions();

            return true;
        }

        private static void initializeTheBottle(BottleInput input, TemplatePlan plan)
        {
            new InitCommand().Execute(
                new InitInput
                {
                    Name = input.Name,
                    Path = plan.Solution.FindProject(input.Name).Project.ProjectDirectory
                }
                );
        }
    }
}