using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Bottles;
using FubuCore;
using FubuCore.CommandLine;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.SelfHost;

namespace Fubu.Applications
{
    [CommandDescription("Run a FubuMVC application with Web API Self Hosting")]
    public class SelfHostCommand : FubuCommand<AppInput>
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
            var domain = new SelfHostApplicationDomain();
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
                Console.WriteLine("Press any key to stop the SelfHost listener");
                Console.ReadLine();
            }

            return true;
        }
    }

    public class SelfHostApplicationDomain : IApplicationDomain
    {
        private AppDomain _domain;
        private SelfHostApplicationRunner _runner;
        private readonly FubuMvcApplicationFileWatcher _watcher;
        private ApplicationSettings _currentSettings;

        public SelfHostApplicationDomain()
        {
            _watcher = new FubuMvcApplicationFileWatcher(this);
        }

        public ApplicationStartResponse Start(ApplicationSettings settings)
        {
            _currentSettings = settings;


            return createAppDomain(settings);
        }

        private ApplicationStartResponse createAppDomain(ApplicationSettings settings)
        {
            var setup = new AppDomainSetup
                        {
                            ApplicationName = "FubuMVC-SelfHost-" + settings.Name + "-" + Guid.NewGuid(),
                            ConfigurationFile = "Web.config",
                            ShadowCopyFiles = "true",
                            PrivateBinPath = "bin",
                            ApplicationBase = settings.PhysicalPath
                            
                        };

            copyAssembly<SelfHostApplicationRunner>(setup);
            copyAssembly<SelfHostHttpServer>(setup);
            copyAssembly("System.Net.Http", setup);
            copyAssembly("System.Net.Http.Formatting", setup);
            copyAssembly("System.Net.Http.WebRequest", setup);
            copyAssembly("System.Web.Http", setup);
            copyAssembly("System.Web.Http.SelfHost", setup);
            copyAssembly("Newtonsoft.Json", setup);

            Console.WriteLine("Starting a new AppDomain at " + setup.ApplicationBase);

            _domain = AppDomain.CreateDomain(setup.ApplicationName, null, setup);

            Type proxyType = typeof(SelfHostApplicationRunner);
            _runner =
                (SelfHostApplicationRunner)
                _domain.CreateInstanceAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);

            var resetEvent = new ManualResetEvent(false);
            var response = _runner.StartApplication(settings, resetEvent);

            if (response.Status == ApplicationStartStatus.Started)
            {
                setupWatchers(settings, response);
            }

            return response;
        }

        private static void copyAssembly<T>(AppDomainSetup setup)
        {
            var assembly = typeof (T).Assembly;
            copyAssembly(assembly, setup);
        }

        private static void copyAssembly(string name, AppDomainSetup setup)
        {
            var assembly = Assembly.Load(name);
            copyAssembly(assembly, setup);
        }

        private static void copyAssembly(Assembly assembly, AppDomainSetup setup)
        {
            var assemblyLocation = assembly.Location;
            new FileSystem().Copy(assemblyLocation, setup.ApplicationBase.AppendPath("bin"));
        }


        private void setupWatchers(ApplicationSettings settings, ApplicationStartResponse response)
        {
            _watcher.StartWatching(settings, response.BottleDirectories);
        }

        public void RecycleContent()
        {
            Console.WriteLine("Restarting the FubuMVC application");
            var response =  _runner.Recycle();
        
            explainResponse(response);

        }

        private static void explainResponse(RecycleResponse response)
        {
            if (response.Success)
            {
                Console.WriteLine("  Success!");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("  Failed!");
                Console.WriteLine(response.Message);
            }
        }

        public void RecycleDomain()
        {
            Teardown();
            var response = createAppDomain(_currentSettings);
            response.WriteReport(_currentSettings);
        }

        public void Teardown()
        {
            try
            {
                if (_runner != null) _runner.Dispose();
            }
            catch (Exception)
            {
            }

            _runner = null;
            if (_domain != null)
            {
                AppDomain.Unload(_domain);
                _domain = null;
            }
        }
    }


    public class SelfHostApplicationRunner : MarshalByRefObject, IDisposable
    {
        private readonly IApplicationSourceFinder _sourceFinder;
        private SelfHostApplication _application;

        public SelfHostApplicationRunner()
            : this(new ApplicationSourceFinder(new ApplicationSourceTypeFinder()))
        {
        }

        public SelfHostApplicationRunner(IApplicationSourceFinder sourceFinder)
        {
            _sourceFinder = sourceFinder;
        }

        public ApplicationStartResponse StartApplication(ApplicationSettings settings, ManualResetEvent reset)
        {
            var response = new ApplicationStartResponse();

            try
            {
                var source = _sourceFinder.FindSource(settings, response);
                if (source == null)
                {
                    response.Status = ApplicationStartStatus.CouldNotResolveApplicationSource;
                }
                else
                {
                    StartApplication(source, settings, reset);
                    response.ApplicationSourceName = source.GetType().AssemblyQualifiedName;

                    reset.WaitOne();
                    determineBottleFolders(response);
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.ToString();
                response.Status = ApplicationStartStatus.ApplicationSourceFailure;
            }

            return response;
        }

        public virtual void StartApplication(IApplicationSource source, ApplicationSettings settings, ManualResetEvent reset)
        {
            FubuMvcPackageFacility.PhysicalRootPath = settings.GetApplicationFolder();
            _application = new SelfHostApplication(settings, source);
            // Put a thread here

            ThreadPool.QueueUserWorkItem(o =>
            {
                // Need to make this capture the package registry failures cleanly
                _application.RunApplication(settings.Port, r => reset.Set());
            });


        }

        private static void determineBottleFolders(ApplicationStartResponse response)
        {
            var list = new List<string>();
            PackageRegistry.Packages.Each(pak =>
            {
                pak.ForFolder(BottleFiles.WebContentFolder, list.Add);
                pak.ForFolder(BottleFiles.BinaryFolder, list.Add);
                pak.ForFolder(BottleFiles.DataFolder, list.Add);
            });

            response.BottleDirectories = list.ToArray();
        }

        public RecycleResponse Recycle()
        {
            try
            {
                _application.Recycle(r => { });
                return new RecycleResponse
                {
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new RecycleResponse
                {
                    Success = false,
                    Message = e.ToString()
                };
            }
        }




        public void Dispose()
        {
            _application.Stop();
        }
    }

    public class SelfHostApplication
    {
        private readonly ApplicationSettings _settings;
        private readonly IApplicationSource _source;
        private SelfHostHttpServer _server;

        public SelfHostApplication(ApplicationSettings settings, IApplicationSource source)
        {
            _settings = settings;
            _source = source;
        }

        public void RunApplication(int port, Action<FubuRuntime> onstartup)
        {
            _server = new SelfHostHttpServer(port, _settings.GetApplicationFolder());
            Recycle(onstartup);
        }

        public void Stop()
        {
            _server.SafeDispose();
        }

        public void Recycle(Action<FubuRuntime> action)
        {
            var runtime = _source.BuildApplication().Bootstrap();
            _server.Start(runtime);

            action(runtime);
        }
    }
}