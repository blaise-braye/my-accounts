using System.Configuration;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MyAccounts.Tests.Gmc
{
    [SetUpFixture]
    public class TestsRoot
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupLogger();
            SetupEnvironmentSettings();
        }

        private static void SetupLogger()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout { ConversionPattern = "%d [%t] %-5p %m%n" };
            patternLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender { Layout = patternLayout };
            consoleAppender.ActivateOptions();

            var root = hierarchy.Root;
            root.AddAppender(consoleAppender);
            root.Level = Level.Debug;

            hierarchy.Configured = true;
        }

        private void SetupEnvironmentSettings()
        {
            var settingsFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "EnvironmentSettings.json");
            if (File.Exists(settingsFile))
            {
                var rawSettings = File.ReadAllText(settingsFile);
                var jSettings = JObject.Parse(rawSettings);
                foreach (var jSetting in jSettings)
                {
                    var section = (ClientSettingsSection)ConfigurationManager.GetSection("applicationSettings/MyAccounts.Tests.Gmc.Properties.Settings");
                    section.Settings.Get(jSetting.Key).Value.ValueXml.InnerText = jSetting.Value.Value<string>();
                }
            }
        }
    }
}