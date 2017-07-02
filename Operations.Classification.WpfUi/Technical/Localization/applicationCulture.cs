using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Operations.Classification.WpfUi.Properties;

namespace Operations.Classification.WpfUi.Technical.Localization
{
    public class ApplicationCulture
    {
        public static void ResetCulture()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Culture))
            {
                CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Culture);
            }

            if (!string.IsNullOrEmpty(Settings.Default.UiCulture))
            {
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.UiCulture);
            }
        }
    }
}
