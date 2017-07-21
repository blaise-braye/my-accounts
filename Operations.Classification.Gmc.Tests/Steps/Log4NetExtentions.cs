using System;
using log4net;

namespace Operations.Classification.Gmc.Tests.Steps
{
    public static class Log4NetExtentions
    {
        public static void Trace(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Trace(this ILog log, string message)
        {
            log.Trace(message, null);
        }

        public static bool IsTraceEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(log4net.Core.Level.Trace);
        }

        public static void Verbose(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Verbose, message, exception);
        }

        public static void Verbose(this ILog log, string message)
        {
            log.Verbose(message, null);
        }

        public static bool IsVerboseEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(log4net.Core.Level.Verbose);
        }
    }
}