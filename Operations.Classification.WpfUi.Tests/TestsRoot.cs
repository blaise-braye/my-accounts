using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace Operations.Classification.WpfUi.Tests
{
    [SetUpFixture]
    public class TestsRoot
    {
        [OneTimeSetUp]
        public void Setup()
        {
            
            SetupLogger();
        }
        
        private static void SetupLogger()
        {
            var hierarchy = (Hierarchy) LogManager.GetRepository();

            var patternLayout = new PatternLayout { ConversionPattern = "%d [%t] %-5p %m%n" };
            patternLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender { Layout = patternLayout };
            consoleAppender.ActivateOptions();

            var root = hierarchy.Root;
            root.AddAppender(consoleAppender);
            root.Level = Level.Debug;

            hierarchy.Configured = true;
        }
    }
}