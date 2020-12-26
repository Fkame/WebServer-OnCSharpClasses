using System;
using NLog;

namespace WebServer.Logging
{
    /// <summary>
    /// Класс содержит код ручной настройки параметров для NLog.
    /// </summary>
    public static class NLogHelper
    {
        public static void SetLoggerConfigurationManually()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Создадим Targets для async обёртки
            var logRequests = new NLog.Targets.FileTarget("logRequests");
            logRequests.FileName = "${basedir}/logs/httpRequests.txt";
            logRequests.ArchiveFileName = "${basedir}/logs/archive/httpRequests.{####}.txt";
            logRequests.ArchiveAboveSize = 10240;

            var logTroubles = new NLog.Targets.FileTarget("logTroubles");
            logTroubles.FileName = "${basedir}/logs/troubles.txt";
            logTroubles.ArchiveFileName = "${basedir}/logs/archive/troubles.{####}.txt";
            logTroubles.ArchiveAboveSize = 10240;

            var logConsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            logConsole.UseDefaultRowHighlightingRules = true;

            // Создадим обёртки для асинхронных записей
            var asyncTargetWrapper1 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logRequests);
            var asyncTargetWrapper2 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logTroubles);
            var asyncTargetWrapper3 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logConsole);

            // Создадим sync targets
            var logBigTroubles = new NLog.Targets.FileTarget("logBigTroubles");
            logBigTroubles.FileName = "${basedir}/logs/bigTroubles.txt";
            logBigTroubles.ArchiveFileName = "${basedir}/logs/archive/bigTroubles.{####}.txt";
            logBigTroubles.ArchiveAboveSize = 10240;

            // Зададим Rules          
            config.AddRuleForOneLevel(LogLevel.Error, logBigTroubles);
            config.AddRuleForOneLevel(LogLevel.Warn, logTroubles);
            config.AddRuleForOneLevel(LogLevel.Info, logRequests);

            // Применим Configuration          
            NLog.LogManager.Configuration = config;
        }
    }
}