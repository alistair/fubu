using System;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;
using FubuCore;

namespace Fubu.Generation
{
    [CommandDescription("Creates a new FubuMVC solution", Name = "new")]
    public class NewCommand : FubuCommand<NewCommandInput>
    {
        public override bool Execute(NewCommandInput input)
        {
            // Only supporting the "path is right underneath where I am right now"
            var library = LoadTemplates();
            var request = new TemplateRequest
            {
                RootDirectory = ".".ToFullPath().AppendPath(input.SolutionName),
                SolutionName = input.SolutionName
            };

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