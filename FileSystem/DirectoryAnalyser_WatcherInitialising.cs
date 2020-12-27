using System;
using System.IO;
using System.Threading;
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

        /// <summary>
        /// Асихнронный метод, который постоянно следит за изменениями в дирректории
        /// </summary>
        private void StartWatching()
        {
            // Создаётся новый FileSystemWatcher и прозводится его настройка
            using (watcher = new FileSystemWatcher())
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

                //Console.WriteLine("\nWatching Events creater");
                logger.Trace("Watching Events created\n");

                // Начало наблюдения
                watcher.EnableRaisingEvents = true;
                //Console.WriteLine($"\nWatching for directory [{DirectoryPath}] and it subdirectories start!");
                logger.Trace($"Watching for directory [{DirectoryPath}] and it subdirectories start!\n");

                while (true)
                {
                    try { watcher.WaitForChanged(WatcherChangeTypes.All); }
                    catch (Exception ex) 
                    {
                        if (watcher == null) logger.Trace("FileSystemWatcher == null -> Serves seems to shutdown");  
                        else logger.Error($"FileSystemWatcher stop because: {ex.Message}\nStackTrace:\n{ex.StackTrace}\n");
                        break;
                    }
                }
            }
        }
    }
}
