using Fubu.Running;
using NUnit.Framework;
using FubuTestingSupport;

namespace fubu.Testing.Running
{
    [TestFixture]
    public class FileMatcherTester
    {
        private FileMatcher theMatcher;

        [SetUp]
        public void SetUp()
        {
            theMatcher = new FileMatcher();

            theMatcher.Add(new EndsWithPatternMatch(FileChangeCategory.Application, "*.asset.config"));
            theMatcher.Add(new ExtensionMatch(FileChangeCategory.Content, "*.css"));
            theMatcher.Add(new ExtensionMatch(FileChangeCategory.Content, "*.spark"));

        }

        [Test]
        public void always_return_app_domain_for_files_in_appdomain()
        {
            theMatcher.CategoryFor("bin\\foo").ShouldEqual(FileChangeCategory.AppDomain);
            theMatcher.CategoryFor("bin\\innocuous.txt").ShouldEqual(FileChangeCategory.AppDomain);
        }

        [Test]
        public void can_return_application()
        {
            theMatcher.CategoryFor("diagnostics.asset.config").ShouldEqual(FileChangeCategory.Application);
        }

        [Test]
        public void web_config_is_app_domain()
        {
            theMatcher.CategoryFor("web.config").ShouldEqual(FileChangeCategory.AppDomain);
        }

        [Test]
        public void dll_is_app_domain()
        {
            theMatcher.CategoryFor("something.dll").ShouldEqual(FileChangeCategory.AppDomain);
        }

        [Test]
        public void exe_is_app_domain()
        {
            theMatcher.CategoryFor("something.exe").ShouldEqual(FileChangeCategory.AppDomain);
        }

        [Test]
        public void match_on_extension()
        {
            theMatcher.CategoryFor("something.spark").ShouldEqual(FileChangeCategory.Content);
            theMatcher.CategoryFor("something.css").ShouldEqual(FileChangeCategory.Content);
        }

        [Test]
        public void match_on_nothing_is_nothing()
        {
            theMatcher.CategoryFor("foo.txt").ShouldEqual(FileChangeCategory.Nothing);
        }
    }
}