using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using Operations.Classification.WpfUi.Properties;

namespace Operations.Classification.WpfUi
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(App));
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupLogging();
            SetupExceptionHandlers();
            SetupCulture();
        }

        private static void SetupLogging()
        {
            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%d [%t] %-5p %m%n";
            patternLayout.ActivateOptions();

            var debugAppender = new DebugAppender() { Layout = patternLayout };
            debugAppender.ActivateOptions();

            var root = hierarchy.Root;
            root.AddAppender(debugAppender);
            root.Level = Level.All;

            hierarchy.Configured = true;
        }

        private void SetupExceptionHandlers()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void SetupCulture()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Culture))
            {
                CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Culture);
            }

            if (!string.IsNullOrEmpty(Settings.Default.UiCulture))
            {
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.UiCulture);
            }
            
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            _log.Error("CurrentDomain_UnhandledException", exception);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _log.Error("OnDispatcherUnhandledException", e.Exception);
        }
    }
}