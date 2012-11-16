using System;
using System.IO;
using Bottles;
using Bottles.Commands;
using FubuCore;
using FubuCore.CommandLine;
using FubuMVC.Core.Packaging;

namespace Fubu
{
    [CommandDescription("Install a package zip file to the specified application", Name = "install-pak")]
    public class InstallPackageCommand : FubuCommand<InstallPackageInput>
    {
        public InstallPackageCommand()
        {
            Usage("Install a package zip file to an application").Arguments(x => x.PackageFile, x => x.AppFolder).ValidFlags();
            Usage("Remove a package zip file from an application").Arguments(x => x.PackageFile, x => x.AppFolder).ValidFlags(x => x.UninstallFlag);
        }

        public override bool Execute(InstallPackageInput input)
        {
            var applicationFolder = new AliasService().GetFolderForAlias(input.AppFolder);
            var uninstallFlag = input.UninstallFlag;
            var packageFileName = input.PackageFile;

            var service = new PackageService(new FileSystem());
            service.InstallPackage(applicationFolder, packageFileName, uninstallFlag);

            return true;
        }
    }
}