using System;
using System.Diagnostics;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;

namespace Fubu.Generation
{
    public class RakeStep : ITemplateStep
    {
        public void Alter(TemplatePlan plan)
        {
            var rake = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "rake",
                CreateNoWindow = true,
                WorkingDirectory = plan.Root
            };

            var process = Process.Start(rake);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                plan.Logger.WriteWarning("rake script failed!");
            }
            else
            {
                plan.Logger.WriteSuccess("rake succeeded");
            }
        }

        public override string ToString()
        {
            return "Run the rake script";
        }
    }
}