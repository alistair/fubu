using System;
using System.Collections.Generic;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Services.Messaging;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Katana;
using FubuMVC.OwinHost;

namespace Fubu.Running
{
    public class RemoteFubuMvcBootstrapper : IBootstrapper, IActivator, IDeactivator, IListener<StartApplication>, IListener<RecycleApplication>
    {
        private readonly IApplicationUnderTestFinder _typeFinder;
        private EmbeddedFubuMvcServer _server;
        private IApplicationSource _applicationSource;
        private int _port;
        private string _physicalPath;

        public RemoteFubuMvcBootstrapper() : this(new ApplicationUnderTestFinder())
        {
        }

        public RemoteFubuMvcBootstrapper(IApplicationUnderTestFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield return this;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            EventAggregator.Messaging.AddListener(this);
        }

        public void Deactivate(IPackageLog log)
        {
            _server.SafeDispose();
        }


        public void Receive(StartApplication message)
        {
            Console.WriteLine("Trying to start application " + message);

            _physicalPath = message.PhysicalPath;

            var applicationTypes = _typeFinder.Find();

            Type applicationType = null;


            if (!applicationTypes.Any())
            {
                EventAggregator.SendMessage(new InvalidApplication
                {
                    Message = "Could not find any instance of IApplicationSource in any assembly in this directory"
                });
                return;
            }

            if (message.ApplicationName.IsNotEmpty())
            {
                applicationType = applicationTypes.FirstOrDefault(x => x.Name.EqualsIgnoreCase(message.ApplicationName));
            }

            if (applicationType == null && applicationTypes.Count() == 1)
            {
                applicationType = applicationTypes.Single();
            }

            if (applicationType == null)
            {
                EventAggregator.SendMessage(new InvalidApplication
                {
                    Applications = applicationTypes.Select(x => x.Name).ToArray(),
                    Message = "Unable to determine the FubuMVC Application"
                });
            }
            else
            {
                _applicationSource = Activator.CreateInstance(applicationType).As<IApplicationSource>();
                _port = PortFinder.FindPort(message.PortNumber);

                startUp();


            }
        }

        private void startUp()
        {
            try
            {
                var application = _applicationSource.BuildApplication();
                _server = new EmbeddedFubuMvcServer(application.Bootstrap(),
                                                    _physicalPath, _port);

                var list = new List<string>();
                PackageRegistry.Packages.Each(pak => { pak.ForFolder(BottleFiles.WebContentFolder, list.Add); });

                EventAggregator.SendMessage(new ApplicationStarted
                {
                    ApplicationName = _applicationSource.GetType().Name,
                    HomeAddress = _server.BaseAddress,
                    Timestamp = DateTime.Now,
                    BottleContentFolders = list.ToArray()
                });
            }
            catch (Exception e)
            {
                EventAggregator.SendMessage(new InvalidApplication
                {
                    ExceptionText = e.ToString(),
                    Message = "Bootstrapping {0} Failed!".ToFormat(_applicationSource.GetType().Name)
                });
            }
        }

        public void Receive(RecycleApplication message)
        {
            _server.SafeDispose();
            startUp();
        }
    }
}