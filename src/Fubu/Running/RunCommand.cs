using System;
using FubuCore.CommandLine;

namespace Fubu.Running
{
    [CommandDescription("Run a fubumvc application hosted in Katana")]
    public class RunCommand : FubuCommand<ApplicationRequest>
    {
        private RemoteApplication _application;


        public override bool Execute(ApplicationRequest input)
        {
            _application = new RemoteApplication();
            _application.Start(input);

            Console.WriteLine("Press 'r' to recycle the application, anything else to quit");
            ConsoleKeyInfo key = Console.ReadKey();
            while (key.Key == ConsoleKey.R)
            {
                _application.RecycleAppDomain();

                key = Console.ReadKey();
            }


            return true;
        }
    }
}