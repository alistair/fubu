using System;
using System.Diagnostics;
using System.Threading;
using Bottles.Services.Messaging;
using FubuCore;
using FubuCore.CommandLine;
using System.Linq;
using System.Collections.Generic;

namespace Fubu.Running
{
    [CommandDescription("Run a fubumvc application hosted in Katana")]
    public class RunCommand : FubuCommand<ApplicationRequest>, IListener<ApplicationStarted>, IListener<InvalidApplication>
    {
        private readonly ManualResetEvent _reset = new ManualResetEvent(false);
        private RemoteFubuMvcProxy _proxy;
        private ApplicationRequest _input;

        public override bool Execute(ApplicationRequest input)
        {
            _input = input;

            _proxy = new RemoteFubuMvcProxy(input);
            _proxy.Start(this);

            _reset.WaitOne();



            Console.WriteLine("Press 'r' to recycle the application, anything else to quit");
            var key = Console.ReadKey();
            while (key.Key == ConsoleKey.R)
            {
                _proxy.Recycle();

                key = Console.ReadKey();
            }


            return true;
        }

        public void Receive(ApplicationStarted message)
        {
            Console.WriteLine("Started application {0} at url {1} at {2}", message.ApplicationName, message.HomeAddress, message.Timestamp);

            if (_input.OpenFlag)
            {
                Process.Start(message.HomeAddress);
            }

            _reset.Set();
        }

        public void Receive(InvalidApplication message)
        {
            ConsoleWriter.Write(ConsoleColor.Red, message.Message);

            if (message.Applications != null && message.Applications.Any())
            {
                Console.WriteLine("Found applications:  " + message.Applications.Join(", "));
            }

            if (message.ExceptionText.IsNotEmpty())
            {
                ConsoleWriter.Write(ConsoleColor.Yellow, message.ExceptionText);
            }

            throw new Exception("Application Failed!");
        }
    }
}