using System.Globalization;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.GererMesComptes;

namespace MyAccounts.Business.AccountOperations
{
    public class FileStructureMetadata
    {
        public SourceKind SourceKind { get; set; }

        public string Encoding { get; set; }

        public string Culture { get; set; }

        public string DecimalSeparator { get; set; }

        public static CultureInfo GetCultureInfo(string name, string decimalSeparator)
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo(name).Clone();
            var formatInfo = culture.NumberFormat;
            formatInfo.CurrencyDecimalSeparator = decimalSeparator;
            formatInfo.NumberDecimalSeparator = decimalSeparator;
            formatInfo.PercentDecimalSeparator = decimalSeparator;
            return CultureInfo.ReadOnly(culture);
        }

        public CultureInfo GetCultureInfo()
        {
            return GetCultureInfo(Culture, DecimalSeparator);
        }
    }
}