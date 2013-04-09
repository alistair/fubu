using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Bottles.Services.Messaging;
using FubuCore;
using FubuCore.CommandLine;
using System.Linq;
using System.Collections.Generic;

namespace Fubu.Running
{
    [CommandDescription("Run a fubumvc application hosted in Katana")]
    public class RunCommand : FubuCommand<ApplicationRequest>, IListener<ApplicationStarted>, IListener<InvalidApplication>, IApplicationObserver
    {
        public static readonly FileMatcher FileMatcher;

        static RunCommand()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            if (Directory.Exists(directory))
            {
                FileMatcher = FileMatcher.ReadFromFile(directory.AppendPath(FileMatcher.File));
            }
            else
            {
                FileMatcher = FileMatcher.ReadFromFile(FileMatcher.File);
            }

            
        }

        private readonly ManualResetEvent _reset = new ManualResetEvent(false);
        private RemoteFubuMvcProxy _proxy;
        private ApplicationRequest _input;
        private bool _opened;
        private FubuMvcApplicationFileWatcher _watcher;

        public override bool Execute(ApplicationRequest input)
        {
            _input = input;

            _watcher = new FubuMvcApplicationFileWatcher(this, FileMatcher);

            start();


            Console.WriteLine("Press 'r' to recycle the application, anything else to quit");
            var key = Console.ReadKey();
            while (key.Key == ConsoleKey.R)
            {
                _reset.Reset();
                _proxy.Recycle();
                _reset.WaitOne();

                key = Console.ReadKey();
            }


            return true;
        }

        private void start()
        {
            _reset.Reset();
            _proxy = new RemoteFubuMvcProxy(_input);
            _proxy.Start(this);

            _reset.WaitOne();
        }

        public void Receive(ApplicationStarted message)
        {
            Console.WriteLine("Started application {0} at url {1} at {2}", message.ApplicationName, message.HomeAddress, message.Timestamp);

            if (_input.OpenFlag && !_opened)
            {
                _opened = true;
                Process.Start(message.HomeAddress);
            }

            _watcher.StartWatching(_input.DirectoryFlag, message.BottleContentFolders);

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

        public void RefreshContent()
        {
            // TODO -- do something
        }

        public void RecycleAppDomain()
        {
            _watcher.StopWatching();
            if (_proxy != null)
            {
                _proxy.SafeDispose();
            }

            start();
        }

        public void RecycleApplication()
        {
            _watcher.StopWatching();
            _proxy.Recycle();
        }
    }
}