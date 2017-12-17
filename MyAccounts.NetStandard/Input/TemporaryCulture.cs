using System;
using System.Globalization;

namespace MyAccounts.NetStandard.Input
{
    public class TemporaryCulture : IDisposable
    {
        private readonly CultureInfo _previousCulture;

        public TemporaryCulture(CultureInfo cultureInfo)
        {
            _previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = cultureInfo;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CultureInfo.CurrentCulture = _previousCulture;
            }
        }
    }
}