using System;
using System.Globalization;
using System.Windows.Data;
using Operations.Classification.WpfUi.Properties;

namespace Operations.Classification.WpfUi.Technical.Converters
{
    public class DefaultAmountValueConverter : IValueConverter
    {
        public static readonly DefaultAmountValueConverter Instance = new DefaultAmountValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Settings.Default.HideAmounts)
            {
                return 0.01;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}