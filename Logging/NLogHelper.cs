using System;
using System.IO;
using NLog;
using NLog.Config;

namespace WebServer.Logging
{
    /// <summary>
    /// Класс содержит код ручной настройки параметров для NLog и метод считывания файла.
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
            logRequests.ArchiveAboveSize = 10000240;

            var logTroubles = new NLog.Targets.FileTarget("logTroubles");
            logTroubles.FileName = "${basedir}/logs/troubles.txt";
            logTroubles.ArchiveFileName = "${basedir}/logs/archive/troubles.{####}.txt";
            logTroubles.ArchiveAboveSize = 10240;

            var logConsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            logConsole.UseDefaultRowHighlightingRules = true;

            var logDirectoryEvents = new NLog.Targets.FileTarget("logDirectoryEvents");
            logTroubles.FileName = "${basedir}/logs/directoryEvents.txt";
            logTroubles.ArchiveFileName = "${basedir}/logs/archive/directoryEvents.{####}.txt";
            logTroubles.ArchiveAboveSize = 10240;

            // Создадим обёртки для асинхронных записей
            var asyncTargetWrapper1 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logRequests);
            var asyncTargetWrapper2 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logTroubles);
            var asyncTargetWrapper3 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logConsole);
            var asyncTargetWrapper4 = new NLog.Targets.Wrappers.AsyncTargetWrapper(logDirectoryEvents);

            // Создадим sync targets
            var logBigTroubles = new NLog.Targets.FileTarget("logBigTroubles");
            logBigTroubles.FileName = "${basedir}/logs/bigTroubles.txt";
            logBigTroubles.ArchiveFileName = "${basedir}/logs/archive/bigTroubles.{####}.txt";
            logBigTroubles.ArchiveAboveSize = 10240;

            // Зададим Rules          
            config.AddRuleForOneLevel(LogLevel.Error, logBigTroubles);
            config.AddRuleForOneLevel(LogLevel.Warn, logTroubles);
            config.AddRuleForOneLevel(LogLevel.Info, logRequests);
            config.AddRuleForOneLevel(LogLevel.Debug, logConsole);
            config.AddRuleForOneLevel(LogLevel.Trace, logDirectoryEvents);

            // Применим Configuration          
            NLog.LogManager.Configuration = config;
        }

        /// <summary>
        /// Метод для загрузки файла с настройками для NLog.
        /// </summary>
        /// <param name="folderInsideProjectFolder">Путь к папке внутри проекта, где лежит файл с настройками.
        /// Если файл лежит в папке с csproj файлом, нужно передать String.Empty</param>
        /// <param name="fileName">Имя файла с настройками</param>
        /// <returns>true - если удалось загрузить файл с настройками, иначе - false.</returns>
        public static bool LoadLoggerConfigsFromFile(string folderInsideProjectFolder, string fileName)
        {
            string CurrentDirectory = System.Environment.CurrentDirectory;
            string path = Path.Combine(CurrentDirectory, folderInsideProjectFolder, fileName);
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(path);
            LoggingConfiguration configs = NLog.LogManager.Configuration;
            if (configs == null) return false;
            return true;
        }
    }
}