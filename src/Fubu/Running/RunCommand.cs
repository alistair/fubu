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

            tellUsersWhatToDo();
            ConsoleKeyInfo key = Console.ReadKey();
            while (key.Key != ConsoleKey.Q)
            {
                if (key.Key == ConsoleKey.R)
                {
                    _application.RecycleAppDomain();

                    tellUsersWhatToDo();
                    key = Console.ReadKey();
                }
            }

            _application.Shutdown();

            return true;
        }

        private static void tellUsersWhatToDo()
        {
            Console.WriteLine("Press 'q' to quit or 'r' to recycle the application");
        }
    }
}