using System;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;

namespace Fubu.Generation
{
    public class NewSerenityInput : NewStorytellerInput
    {
        
    }

    [CommandDescription("Applies Serenity testing to a new or existing project", Name = "new-serenity")]
    public class NewSerenityCommand : FubuCommand<NewSerenityInput>
    {
        public override bool Execute(NewSerenityInput input)
        {
            // TODO -- duplication here
            string solutionFile = SolutionFinder.FindSolutionFile();
            if (solutionFile == null)
            {
                return false;
            }

            // TODO -- try to attach the IApplicationSource from the parent
            // project
            var projectRequest = new ProjectRequest(input.Name, "baseline");
            projectRequest.AddAlteration("storyteller");
            projectRequest.AddAlteration("serenity");

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