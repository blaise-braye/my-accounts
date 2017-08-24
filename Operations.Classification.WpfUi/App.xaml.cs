using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MyAccounts.Business.IO.Caching;
using MyAccounts.Business.IO.Caching.InMemory;
using Operations.Classification.WpfUi.Properties;
using Operations.Classification.WpfUi.Technical.Caching.Redis;
using Operations.Classification.WpfUi.Technical.Localization;

namespace Operations.Classification.WpfUi
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(App));

        public static ICachemanager CacheManager { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupLogging();
            SetupExceptionHandlers();
            SetupCache();
            ApplicationCulture.ResetCulture();

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private static void SetupCache()
        {
            IRawCacheRepository repository;

            if (string.IsNullOrEmpty(Settings.Default.RedisConnectionString))
            {
                repository = new InMemoryRawCacheRepository();
            }
            else
            {
                repository = new RedisRawCacheRepository(Settings.Default.RedisConnectionString);
            }

            CacheManager = new CacheManager(repository);
            _log.Info($"Cache provider initialized, working with repository {repository.GetType()}");
        }

        private static void SetupLogging()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout { ConversionPattern = "%d [%t] %-5p %m%n" };
            patternLayout.ActivateOptions();

            var debugAppender = new DebugAppender { Layout = patternLayout };
            debugAppender.ActivateOptions();

            var root = hierarchy.Root;
            root.AddAppender(debugAppender);
            root.Level = Level.All;

            hierarchy.Configured = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            _log.Error("CurrentDomain_UnhandledException", exception);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _log.Error("OnDispatcherUnhandledException", e.Exception);
        }

        private void SetupExceptionHandlers()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
    }
}