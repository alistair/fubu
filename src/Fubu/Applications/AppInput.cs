using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core;

namespace Fubu.Applications
{
    public class AppInput
    {
        public AppInput()
        {
            Location = ".".ToFullPath();
            PortFlag = 5500;
        }

        public string Location { get; set; }
        public string UrlFlag { get; set; }
        public int PortFlag { get; set; }


        public ApplicationSettings FindSettings()
        {
            if (Location.IsEmpty())
            {
                return new ApplicationSettings{
                    PhysicalPath = ".".ToFullPath(),
                    ParentFolder = ".".ToFullPath()
                };
            }

            var system = new FileSystem();
            if (system.FileExists(Location) && system.IsFile(Location))
            {
                Console.WriteLine("Reading application settings from " + Location);
                return ApplicationSettings.Read(Location);
            }

            if (system.DirectoryExists(Location))
            {
                return findFromDirectory(system);
            }

            return findByName(system);
        }

        private ApplicationSettings findByName(FileSystem system)
        {

            var files = system.FindFiles(".".ToFullPath(), ApplicationSettings.FileSearch(Location));
            if (!files.Any())
            {
                Console.WriteLine("Could not find any matching *.application.config files");
                return null;
            }

            if (files.Count() == 1)
            {
                var location = files.Single();
                Console.WriteLine("Using file " + location);
                return ApplicationSettings.Read(location);
            }

            Console.WriteLine("Found multiple *.application.settings files");
            files.Each(x => Console.WriteLine(" - " + x));

            return null;
        }

        private ApplicationSettings findFromDirectory(FileSystem system)
        {
            var files = system.FindFiles(Location, ApplicationSettings.FileSearch());


            if (!files.Any())
            {
                Console.WriteLine(
                    "Did not find any *.application.config file, \nwill try to determine the application source by scanning assemblies");
                return new ApplicationSettings{
                    PhysicalPath = Location.ToFullPath(),
                    ParentFolder = Location.ToFullPath()
                };
            }


            if (files.Count() == 1)
            {
                var location = files.Single();
                Console.WriteLine("Reading application settings from " + location);
                return ApplicationSettings.Read(location);
            }


            Console.WriteLine("Found multiple *.application.config files, cannot determine which to use:");
            files.Each(x => Console.WriteLine(" - " + x));

            return null;
        }
   
    }
}