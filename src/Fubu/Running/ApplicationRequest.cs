using System;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using FubuCore;

namespace Fubu.Running
{
    public class ApplicationRequest
    {
        public ApplicationRequest()
        {
            PortFlag = 5500;
            DirectoryFlag = Environment.CurrentDirectory;
            BuildFlag = "Debug";
        }

        [Description("If you are running a class library, sets the preference for the profile to load.  As in bin/[BuildFlag].  Default is debug")]
        public string BuildFlag { get; set; }

        [Description("IP Port.  Default is 5500")]
        public int PortFlag { get; set; }

        [Description("Specific name of an IApplicationSource class that builds this application")]
        public string ApplicationFlag { get; set; } // this is optional

        [Description("Overrides the directory that is the physical path of the running fubumvc application")]
        public string DirectoryFlag { get; set; } // This is mandatory
        public string DetermineBinPath()
        {
            var buildPath = DirectoryFlag.AppendPath("bin", BuildFlag);
            if (Directory.Exists(buildPath))
            {
                return buildPath;
            }

            var binPath = DirectoryFlag.AppendPath("bin");
            if (Directory.Exists(binPath))
            {
                return binPath;
            }

            return null;
        }
    }
}