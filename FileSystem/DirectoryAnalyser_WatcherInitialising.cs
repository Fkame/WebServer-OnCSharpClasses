using System;
using System.IO;
using System.Threading.Tasks;

namespace WebServer.FileSystem
{
    public partial class DirectoryAnalyser
    {
        /// <summary>
        /// Метод, запускающий в асинхронном режиме к основному алгоритму ещё и проверку на изменения в файлах сайта.
        /// Также, этот метод в последствии вызывает специальные методы, которые выполняют специальные действия при определённом событии.
        /// </summary>
        /// <returns></returns>
        public async void StartWatchingForChangesInFiles()
        {
            await Task.Run(StartWatching);
        }

        private Task StartWatching()
        {
             // Создаётся новый FileSystemWatcher и прозводится его настройка
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = DirectoryPath.FullName;
                watcher.IncludeSubdirectories = true;

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.FileName
                                    | NotifyFilters.DirectoryName;

                // Наблюдение за всеми файлами в дирректории
                watcher.Filter = "*.*";

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                Console.WriteLine("\nWatching Events creater");

                // Начало наблюдения
                watcher.EnableRaisingEvents = true;
                Console.WriteLine($"\nWatching for directory [{DirectoryPath}] and it subdirectories start!");

                while (true)
                {
                    watcher.WaitForChanged(WatcherChangeTypes.All);
                }
            }
        }
    }
}
