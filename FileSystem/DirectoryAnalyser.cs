﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace WebServer.FileSystem
{
    /// <summary>
    /// Данный объект выполняет ряд важных задач по работе с файловой системой:
    /// 1.Он инкапсулирует в себе контейнер, который хранит в себе буферезированные файлы (то есть загружает их в оперативную память).
    /// Эта штука нужна, чтобы при отправке файла по http запросу не читать его с диска, тем самым, скорее всего, производительность
    /// будет выше.
    /// 2. Объект запускает FileSystemWatcher, который отслеживает изменеия в переданной директории и сообщает о них.
    /// 3. Объект реагирует на изменения, и соответствующе обновляет буфер. Это нужно, чтобы можно было подправить сайт, не выключая сервер.
    /// 4. TODO--> Нужно настроить сихнронизацию доступа к буферу файлов, иначе возможна отправка устаревшей информации. И возникновение ошибок.
    /// Скорее всего будет достаточно что-то типа монитора при изменениии.
    /// </summary>
    /// <remarks>
    /// Объект не предполагает запуск в отдельном потоке (или асинхронного запуска) непосредственно его. Механизм для слежки за файлами он самостоятельно запускает
    /// в виде отдельной задачи.
    /// </remarks>
    public partial class DirectoryAnalyser
    {
        /// <summary>
        /// Логгер из библиотеки Nlog.
        /// </summary>
        /// <returns></returns>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Дирректория, за которой изменениями в которой нужно следить. И файлы которой (и файлы подкаталогов этой дирректории) будут
        /// помещены в буффер.
        /// </summary>
        /// <value></value>
        public DirectoryInfo DirectoryPath{ get; private set; }

        /// <summary>
        /// Экземпляр класса FilesContainer. Буффер файлов. По сути это обёртка над словарём. Чтобы упростить ряд специфических операций с ним.
        /// </summary>
        /// <value></value>
        public FilesContainer filebuffer {get; private set; }
        
        /// <summary>
        /// Экзмепляр класса FileSystemWatcher. Данный объект реализует слежку за изменениями в дирректории и уведомляет о них.
        /// </summary>
        private FileSystemWatcher watcher;

        public DirectoryAnalyser(DirectoryInfo directoryPath) 
        {
            this.DirectoryPath = directoryPath;
            filebuffer = new FilesContainer();
        }

        /// <summary>
        /// Запуск всех важных задач, которые выполняет объект. Как только он их все запустит, так сразу вернёт управление обратно.
        /// </summary>
        public void Start()
        { 
            // Загрузка файлов с диска в буффер
            this.LoadFilesToBuffer();

            // Просмотр буфера после загрузки туда файлов
            this.PrintFileBuffer(filebuffer, DirectoryPath);
            
            // Запуск отслеживателя изменений в директории и поддиректориях. Осторожно: работает асинхронно!
            this.StartWatchingForChangesInFiles();
        }

        /// <summary>
        /// Очистка ресурсов, которые использует объект. Прерываются асинхронные операции, которые делаются на фоне.
        /// </summary>
        public void Shutdown()
        {
            filebuffer.Clear();
            DirectoryPath = null;

            watcher.Changed -= OnChanged;
            watcher.Created -= OnCreated;
            watcher.Deleted -= OnDeleted;
            watcher.Renamed -= OnRenamed;
            watcher = null;     
        }

        /// <summary>
        /// В методе ищутся все файлы, которые находятся в указанной при создании объекта папке, в том числе и в подпапках.
        /// После чего пути к файлам обрезаются - из них удаляются DirectoryPath. То есть остается локальный путь в рабочей директории.
        /// После этого файл и путь у файлу помещаются в буфер.
        /// </summary>
        private void LoadFilesToBuffer()
        {
            filebuffer.Clear();
            string[] allFiles = Directory.GetFiles(DirectoryPath.FullName, "*", SearchOption.AllDirectories);
            foreach (string path in allFiles)
            {
                byte[] file = File.ReadAllBytes(path);
                string localPath = GetLocalPath(path);
                filebuffer.Add(localPath, file);
            }
        }

        /// <summary>
        /// Выводит всё содержимое буффера в виде: ключ(путь) -> содержимое(null/not null).
        /// </summary>
        /// <param name="filebuffer"></param>
        /// <param name="directoryPath"></param>
        private void PrintFileBuffer(FilesContainer filebuffer, DirectoryInfo directoryPath)
        {
            //Console.WriteLine($"--- Files in {DirectoryPath} and it subdirectories ---");
            logger.Trace($"--- Files in {DirectoryPath} and it subdirectories ---");
            string[] keys = filebuffer.GetKeys();
            foreach (string key in keys)
            {
                //Console.WriteLine($"[{key}] -> somefile...(null={filebuffer.GetValueByKey(key)==null})");
                logger.Trace($"[{key}] -> somefile...(null={filebuffer.GetValueByKey(key)==null})");
            }
            //Console.WriteLine("--- End Writing Dictionary ---\n");
            logger.Trace("---End of files ---\n");
        }

        /// <summary>
        /// Выполняет поиск директорий (то есть папок), содержащих Html файл - они станут префиксами URL.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDirectoriesWithDotHTML()
        {
            string template = ".html";
            char separator = Path.DirectorySeparatorChar;

            List<string> directoriesWithDotHTML = new List<string>();
            string[] keys = filebuffer.GetKeys();
            
            foreach (string path in keys)
            {
                if (path.EndsWith(template))
                {
                    int endIdx = path.LastIndexOf(separator);
                    string directory = path.Substring(0, endIdx + 1);
                    directoriesWithDotHTML.Add(directory);
                }
            }
            return directoriesWithDotHTML;
        }

        /// <summary>
        /// Позволяет получить из полного пути к файлу/папке сокращённый - отрезает приставку DirectoryPath от пути.
        /// Если входной путь не содержит приставки DirectoryPath - метод возвращает пустую строку.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetLocalPath(string path)
        {
            if (!path.StartsWith(this.DirectoryPath.FullName)) return string.Empty;
            int localPathStartIdx = this.DirectoryPath.FullName.Length;
            return path.Substring(localPathStartIdx);
        }
    }
}
