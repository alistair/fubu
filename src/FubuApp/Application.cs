using FubuMVC.Core;
using FubuMVC.StructureMap;
using StructureMap;

namespace FubuApp
{
    public class HomeEndpoint
    {
        public string Index()
        {
            return "I was called!";
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