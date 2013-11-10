using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
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

        public override void SetUp(ITestContext context)
        {
            FileSystem.DeleteDirectory("Templating");
            FileSystem.CreateDirectory("Templating");

            _root = "Templating".ToFullPath();

            var compile = AppDomain.CurrentDomain.BaseDirectory.ToLower().EndsWith("debug")
                ? "debug"
                : "release";

            _processPath =
                        AppDomain.CurrentDomain.BaseDirectory.ParentDirectory()
                            .ParentDirectory()
                            .ParentDirectory()
                            .AppendPath("Fubu", "bin", compile, "Fubu.exe");


            _original = Environment.CurrentDirectory;

            Environment.CurrentDirectory = _root;
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
                UseShellExecute = true,
                FileName = _processPath,
                CreateNoWindow = true,
                WorkingDirectory = _root
            };

            var process = Process.Start(fubu);
            process.WaitForExit();

            StoryTellerAssert.Fail(process.ExitCode != 0, "Command failed!");
        }

        [FormatAs("Rake succeeds in folder {folder}")]
        public bool RakeSucceeds(string folder)
        {
            var rake = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "rake",
                CreateNoWindow = true,
                WorkingDirectory = _root.AppendPath(folder)
            };

            var process = Process.Start(rake);
            process.WaitForExit();

            StoryTellerAssert.Fail(process.ExitCode != 0, "Rake failed!");

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
            return
                FileSystem.FindFiles(_root, FileSet.Everything())
                    .Select(x => x.PathRelativeTo(_root).Replace("\\", "/"));
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