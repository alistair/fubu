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

        [Description("Add a testing library for the project using the default FubuTestingSupport w/ NUnit")]
        public bool TestsFlag { get; set; }

    }
    
    [CommandDescription("Creates a new project as a FubuMVC Bottle", Name = "new-bottle")]
    public class BottleCommand : FubuCommand<BottleInput>
    {
        public override bool Execute(BottleInput input)
        {
            string solutionFile = SolutionFinder.FindSolutionFile();
            if (solutionFile == null)
            {
                return false;
            }

            var request = BuildTemplateRequest(input, solutionFile);

            var plan = NewCommand.BuildTemplatePlan(request);

            plan.Solution = Solution.LoadFrom(solutionFile);

            // TODO -- try to add CopyReferences from the parent
            NewCommand.ExecutePlan(plan, () => initializeTheBottle(input, plan));

            return true;
        }

        public static TemplateRequest BuildTemplateRequest(BottleInput input, string solutionFile)
        {
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

            // TODO -- some duplication here.
            if (input.TestsFlag)
            {
                var testing = new TestProjectRequest(projectRequest.Name + ".Testing", "baseline",
                                                     projectRequest.Name);

                testing.AddAlteration("unit-testing");

                request.AddTestingRequest(testing);
            }

            if (input.OptionsFlag != null)
            {
                input.OptionsFlag.Each(projectRequest.AddAlteration);
            }

            return request;
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