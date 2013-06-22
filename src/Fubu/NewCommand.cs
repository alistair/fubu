using System;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;

namespace Fubu
{
    [CommandDescription("Creates a new FubuMVC solution", Name = "new")]
    public class NewCommand : FubuCommand<NewCommandInput>
    {
        public NewCommand()
        {
            FileSystem = new FileSystem();
            ZipService = new ZipFileService(FileSystem);
            ProcessFactory = new ProcessFactory();
        }

        public IFileSystem FileSystem { get; set; }
        public IZipFileService ZipService { get; set; }
        public IProcessFactory ProcessFactory { get; set; }

        public override bool Execute(NewCommandInput input)
        {
            throw new NotImplementedException();

//            Console.WriteLine("Solution {0} created", input.ProjectName);
//            return true;
        }


    }
}