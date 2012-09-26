using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuMVC.Core;

namespace Fubu.Applications
{
    public class ApplicationSourceTypeFinder : IApplicationSourceTypeFinder
    {
        public IEnumerable<Type> FindApplicationSourceTypes(ApplicationSettings settings)
        {
            var assemblies = AssembliesFromApplicationBaseDirectory(AssemblyMatches);
            if (!assemblies.Any())
            {
                var assemblyName = Path.GetFileName(settings.GetApplicationFolder());
                assemblies = AssembliesFromApplicationBaseDirectory(x => x.GetName().Name == assemblyName);
            }

            return assemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.CanBeCastTo<IApplicationSource>() && x.IsConcreteWithDefaultCtor());
        }

        public static bool AssemblyMatches(Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof (FubuAppAttribute), true).Any();
        }

        // TODO -- this is all ripped from StructureMap, but put in FubuCore
        public IEnumerable<Assembly> AssembliesFromApplicationBaseDirectory(Predicate<Assembly> assemblyFilter)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory.AppendPath(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);

            return AssembliesFromPath(baseDirectory, assemblyFilter);
        }


        public IEnumerable<Assembly> AssembliesFromPath(string path, Predicate<Assembly> assemblyFilter)
        {
            IEnumerable<string> assemblyPaths = Directory.GetFiles(path)
                .Where(file =>
                       Path.GetExtension(file).Equals(
                           ".exe",
                           StringComparison.OrdinalIgnoreCase)
                       ||
                       Path.GetExtension(file).Equals(
                           ".dll",
                           StringComparison.OrdinalIgnoreCase));

            foreach (string assemblyPath in assemblyPaths)
            {
                Assembly assembly = null;
                try
                {
                    assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
                }
                catch
                {
                }
                if (assembly != null && assemblyFilter(assembly)) yield return assembly;
            }
        }
    }
}