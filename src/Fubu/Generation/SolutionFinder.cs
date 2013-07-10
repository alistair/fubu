using System;
using System.Collections.Generic;
using FubuCore;
using FubuCsProjFile.Templating;
using System.Linq;

namespace Fubu.Generation
{
    public static class SolutionFinder
    {
         public static string FindSolutionFile()
         {
             var currentDirectory = Environment.CurrentDirectory.ToFullPath();
             var files = new FileSystem().FindFiles(currentDirectory, FileSet.Deep("*.sln"));
             if (files.Count() == 1)
             {
                 return files.Single().ToFullPath().PathRelativeTo(currentDirectory);
             }

             return null;
         }

        public static IEnumerable<string> FindSolutions()
        {
            var currentDirectory = Environment.CurrentDirectory.ToFullPath();
            var files = new FileSystem().FindFiles(currentDirectory, FileSet.Deep("*.sln"));

            return files.Select(x => x.ToFullPath().PathRelativeTo(currentDirectory));
        }
    }
}