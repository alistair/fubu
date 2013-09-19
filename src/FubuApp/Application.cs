using System;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.StructureMap;
using StructureMap;

namespace FubuApp
{
    public class HomeEndpoint
    {
        public static DateTime First = DateTime.Now;

        public string Index()
        {
            return "I was called!";
        }

        public string get_file()
        {
            var filename = FubuMvcPackageFacility.GetApplicationPath().AppendPath("Something.spark");
            return new FileSystem().ReadStringFromFile(filename);
        }

        public string get_time()
        {
            return "AppDomain Startup Time = " + First.ToLongTimeString() + "\n\nApplication Startup Time = " + FubuMvcPackageFacility.Restarted.Value.ToLongTimeString();
        }

    }

    public class SampleApplication : IApplicationSource
    {
        

        public FubuApplication BuildApplication()
        {
            return FubuApplication.DefaultPolicies().StructureMap(new Container());
        }


    }
}