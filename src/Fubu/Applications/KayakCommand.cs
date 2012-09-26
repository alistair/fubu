using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;
using FubuMVC.Core;

namespace Fubu.Applications
{
    public class KayakCommand : FubuCommand<AppInput>
    {
        public override bool Execute(AppInput input)
        {
            var settings = input.FindSettings();
            if (settings == null)
            {
                Console.WriteLine("Unable to determine how to launch the application");
                return false;
            }

            settings.Port = input.PortFlag;
            var domain = new KayakApplicationDomain();
            var response = domain.Start(settings);

            response.WriteReport(settings);
            if (response.Status != ApplicationStartStatus.Started)
            {
                return false;
            }


            var url = "http://localhost:" + settings.Port;
            if (input.UrlFlag.IsNotEmpty())
            {
                url += input.UrlFlag;
                url = url.Replace("//", "/");
            }

            Console.WriteLine("Opening default browser to " + url);
            Process.Start(url);

            if (response.Status == ApplicationStartStatus.Started)
            {
                Console.WriteLine("Press any key to stop the Kayak listener");
                Console.ReadLine();
            }

            return true;
        }

    }
}