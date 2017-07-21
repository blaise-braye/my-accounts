using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace Operations.Classification.Tests
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

            var debugAppender = new DebugAppender { Layout = patternLayout };
            debugAppender.ActivateOptions();

            var root = hierarchy.Root;
            root.AddAppender(consoleAppender);
            root.AddAppender(debugAppender);
            root.Level = Level.Debug;

            hierarchy.Configured = true;
        }
    }
}