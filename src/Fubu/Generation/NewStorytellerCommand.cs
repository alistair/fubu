using System;
using System.ComponentModel;
using FubuCore.CommandLine;
using FubuCore.Descriptions;
using FubuCsProjFile.Templating;
using FubuCsProjFile.Templating.Graph;

namespace Fubu.Generation
{
    public class NewStorytellerInput
    {
        [Description("Name of a new or existing project to make into a Storyteller testing project")]
        public string Name { get; set; }
    }

    [CommandDescription("Sets up a new or existing project to be a Storyteller2 testing project", Name = "new-storyteller")]
    public class NewStorytellerCommand : FubuCommand<NewStorytellerInput>
    {
        public override bool Execute(NewStorytellerInput input)
        {
            // TODO -- duplication here
            string solutionFile = SolutionFinder.FindSolutionFile();
            if (solutionFile == null)
            {
                return false;
            }

            var projectRequest = new ProjectRequest(input.Name, "baseline");
            projectRequest.Alterations.Add("storyteller");

            var request = new TemplateRequest
            {
                RootDirectory = Environment.CurrentDirectory,
                SolutionName = solutionFile
            };

            request.AddProjectRequest(projectRequest);


            // TODO -- add an option to copy references from the parent project
            var plan = NewCommand.BuildTemplatePlan(request);

            NewCommand.ExecutePlan(plan);

            return true;
        }
    }
}