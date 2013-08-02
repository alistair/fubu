using System;
using System.ComponentModel;
using FubuCore.CommandLine;

namespace Fubu.Generation
{
    // TODO -- needs to be smart enough to know spark v. razor
    // TODO --
    public class ViewInput
    {
        [Description("Name of the view and matching model without file extension")]
        public string Name { get; set; }

        [Description("If specified, will make this actionless view applied to the given url pattern")]
        public string UrlFlag { get; set; }

        [Description("open the view model and view after generation")]
        public bool OpenFlag { get; set; }
    }

    [CommandDescription("Creates and attaches a view model/view pair to the project in the current folder")]
    public class ViewCommand : FubuCommand<ViewInput>
    {
        public override bool Execute(ViewInput input)
        {
            Location location = ProjectFinder.DetermineLocation(Environment.CurrentDirectory);

            ViewModelBuilder.BuildCodeFile(input, location);

            var modelName = location.Namespace + "." + input.Name;
            var path = SparkViewBuilder.Write(Environment.CurrentDirectory, modelName);

            if (input.OpenFlag)
            {
                EditorLauncher.LaunchFile(path);
            }

            return true;
        }
    }
}