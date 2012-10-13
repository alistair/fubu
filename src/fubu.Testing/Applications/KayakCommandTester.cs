using System;
using System.Collections.Generic;
using Fubu.Applications;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using NUnit.Framework;
using StructureMap;

namespace fubu.Testing.Applications
{
    [TestFixture]
    public class KayakCommandTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            theFileSystem = new FileSystem();

            theFileSystem.DeleteDirectory("fake-app");
            theFileSystem.CreateDirectory("fake-app");

            theFileSystem.FindFiles(".".ToFullPath(), ApplicationSettings.FileSearch())
                .Each(x => theFileSystem.DeleteFile(x));

            theOriginalSettings = ApplicationSettings.For<KayakApplication>();
            theOriginalSettings.ParentFolder = "fake-app".ToFullPath();
            theOriginalSettings.Write();

            aSpecificLocation = theOriginalSettings.GetFileName();
        }

        #endregion

        private FileSystem theFileSystem;
        private string aSpecificLocation;
        private ApplicationSettings theOriginalSettings;


        [Test]
        public void find_settings_for_a_folder_if_there_is_only_one_settings_file()
        {
            var input = new AppInput
            {
                Location = theOriginalSettings.ParentFolder
            };

            ApplicationSettings settings = input.FindSettings();

            settings.ApplicationSourceName.ShouldEqual(theOriginalSettings.ApplicationSourceName);
            settings.Name.ShouldEqual(theOriginalSettings.Name);
        }

        [Test]
        public void find_settings_for_a_specific_app_settings_file_that_exists()
        {
            var input = new AppInput
            {
                Location = aSpecificLocation
            };

            ApplicationSettings settings = input.FindSettings();

            settings.ApplicationSourceName.ShouldEqual(theOriginalSettings.ApplicationSourceName);
            settings.Name.ShouldEqual(theOriginalSettings.Name);
        }

        [Test]
        public void location_is_app_name()
        {
            var input = new AppInput
            {
                Location = theOriginalSettings.Name
            };

            ApplicationSettings settings = input.FindSettings();

            settings.ApplicationSourceName.ShouldEqual(theOriginalSettings.ApplicationSourceName);
            settings.Name.ShouldEqual(theOriginalSettings.Name);
        }

        [Test]
        public void location_is_null_try_to_use_the_current_directory()
        {
            theFileSystem.FindFiles(".".ToFullPath(), ApplicationSettings.FileSearch())
                .Each(x => theFileSystem.DeleteFile(x));

            var appInput = new AppInput
            {
                Location = null
            };

            ApplicationSettings settings = appInput.FindSettings();
            settings.PhysicalPath.ShouldEqual(".".ToFullPath());
            settings.ParentFolder.ShouldEqual(".".ToFullPath());
        }

        [Test]
        public void no_application_files_exist_so_try_settings_with_just_the_physical_path()
        {
            theFileSystem.DeleteFile(theOriginalSettings.GetFileName());

            var input = new AppInput
            {
                Location = theOriginalSettings.ParentFolder
            };

            ApplicationSettings settings = input.FindSettings();
            settings.ApplicationSourceName.ShouldBeNull();
            settings.Name.ShouldBeNull();
            settings.PhysicalPath.ShouldEqual(input.Location);
            settings.ParentFolder.ShouldEqual(input.Location);
        }

        [Test]
        public void undeterministic_application_with_a_directory_and_multiple_settings()
        {
            ApplicationSettings additionalSettings = ApplicationSettings.For<KayakApplication>();
            additionalSettings.ParentFolder = "fake-app".ToFullPath();
            additionalSettings.Name = "SomethingElse";

            additionalSettings.Write();

            var input = new AppInput
            {
                Location = theOriginalSettings.ParentFolder
            };

            input.FindSettings().ShouldBeNull();
        }
    }

    public class KayakApplication : IApplicationSource
    {
        #region IApplicationSource Members

        public FubuApplication BuildApplication()
        {
            return FubuApplication
                .For<KayakRegistry>()
                .StructureMap(new Container());
        }

        #endregion
    }

    public class KayakRegistry : FubuRegistry
    {
        public KayakRegistry()
        {
            Routes.HomeIs<SayHelloController>(x => x.Hello());

            Actions.IncludeClassesSuffixedWithController();
        }
    }

    public class NameModel
    {
        public string Name { get; set; }
    }

    public class SayHelloController
    {
        public string Hello()
        {
            return "Hello, it's " + DateTime.Now;
        }

        public NameModel get_say_Name(NameModel model)
        {
            return model;
        }

        public IDictionary<string, object> post_name(NameModel model)
        {
            return new Dictionary<string, object> {{"name", model.Name}};
        }
    }
}