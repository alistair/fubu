using FubuCore;
using System.Linq;

namespace Fubu.Generation
{
    // TODO -- make this fancier later.
    // TODO -- detect Spark or Razor
    // TODO -- let you define view template by codebase
    public static class SparkViewBuilder
    {
        public static readonly string _template = "<viewdata model=\"%MODEL%\" />";

        public static void Write(string folder, string inputModel)
        {
            var path = folder.AppendPath(inputModel.Split('.').Last() + ".spark");
            var contents = _template.Replace("%MODEL%", inputModel);

            new FileSystem().WriteStringToFile(path, contents);
        }
    }
}