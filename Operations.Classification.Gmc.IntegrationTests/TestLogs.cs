using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace Operations.Classification.Gmc.IntegrationTests
{
    [SetUpFixture]
    public class TestLogs
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%d [%t] %-5p %m%n";
            patternLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender { Layout = patternLayout };
            consoleAppender.ActivateOptions();

            var root = hierarchy.Root;
            root.AddAppender(consoleAppender);
            root.Level = Level.All;

            hierarchy.Configured = true;
        }
    }
}