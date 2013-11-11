using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using Newtonsoft.Json.Bson;
using StoryTeller;
using StoryTeller.Assertions;
using StoryTeller.Engine;

namespace fubu.Testing.Fixtures
{
    public class TemplatingFixture : Fixture
    {
        public static FileSystem FileSystem = new FileSystem();
        private string _root;
        private string _original;

        private string _processPath;
        private string _folder;

        public override void SetUp(ITestContext context)
        {
            _root = AppDomain.CurrentDomain.BaseDirectory.AppendPath("Templating").ToFullPath();

            FileSystem.DeleteDirectory(_root);
            FileSystem.CreateDirectory(_root);


            var compile = AppDomain.CurrentDomain.BaseDirectory.ToLower().EndsWith("debug")
                ? "debug"
                : "release";

            _processPath =
                        AppDomain.CurrentDomain.BaseDirectory.ParentDirectory()
                            .ParentDirectory()
                            .ParentDirectory()
                            .AppendPath("Fubu", "bin", compile, "Fubu.exe");

            Debug.WriteLine("The process path is " + _processPath);


            _original = Environment.CurrentDirectory;

            Environment.CurrentDirectory = _root;

            Debug.WriteLine("The root directory is " + _root);
        }

        public override void TearDown()
        {
            Environment.CurrentDirectory = _original;
        }

        [FormatAs("Run fubu {command}")]
        public void Execute(string command)
        {
            var fubu = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = _processPath,
                CreateNoWindow = true,
                WorkingDirectory = _root,
                Arguments = command,
                RedirectStandardOutput = true
            };

            var process = Process.Start(fubu);
            process.WaitForExit();

            StoryTellerAssert.Fail(process.ExitCode != 0, "Command failed!" + process.StandardOutput.ReadToEnd());

            Debug.WriteLine(process.StandardOutput.ReadToEnd());
        }

        [FormatAs("For folder {folder}")]
        public void ForFolder(string folder)
        {
            _folder = folder;
        }

        [FormatAs("The rake script can run successfully")]
        public bool RakeSucceeds()
        {
            var workingDirectory = _root.AppendPath(_folder);
            Debug.WriteLine("Trying to run the rake script at " + workingDirectory);

            
            var rake = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "rake",
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };

            var process = Process.Start(rake);
            process.WaitForExit();

            StoryTellerAssert.Fail(process.ExitCode != 0, "Rake failed at directory {0}!".ToFormat(workingDirectory));

            return true;
        }

        [ExposeAsTable("Files should exist")]
        public bool FileExists(string Name)
        {
            return FileSystem.FileExists(_root, Name);
        }

        public IGrammar AllTheFilesShouldBe()
        {
            return VerifyStringList(allFiles)
                .Titled("All the files generated are")
                .Grammar();
        }

        private IEnumerable<string> allFiles()
        {
            var path = _root.AppendPath(_folder);

            var searchSpecification = FileSet.Everything();
            searchSpecification.Exclude = "logs/*.*";

            return
                FileSystem.FindFiles(path, searchSpecification)
                    .Select(x => x.PathRelativeTo(path).Replace("\\", "/"));
        }

        [FormatAs("File {File} should contain {Contents}")]
        public bool FileContains(string File, string Contents)
        {
            var contents = FileSystem.ReadStringFromFile(_root.AppendPath(File));

            StoryTellerAssert.Fail(!contents.Contains(Contents), contents);

            return true;
        }

        [FormatAs("File {File} should not contain {Contents}")]
        public bool FileDoesNotContain(string File, string Contents)
        {
            var contents = FileSystem.ReadStringFromFile(_root.AppendPath(File));

            StoryTellerAssert.Fail(contents.Contains(Contents), contents);

            return true;
        }


    }
}