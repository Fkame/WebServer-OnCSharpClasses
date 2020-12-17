using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
        public DirectoryInfo DirectoryPath{ get; private set; }

        public FilesContainer filebuffer {get; private set; }

        public DirectoryAnalyser(DirectoryInfo directoryPath) 
        {
            this.DirectoryPath = directoryPath;
            filebuffer = new FilesContainer();
        }

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

        private void PrintFileBuffer(FilesContainer filebuffer, DirectoryInfo directoryPath)
        {
            Console.WriteLine("\n--- Writing Dictionaty ---");
            string[] keys = filebuffer.GetKeys();
            foreach (string key in keys)
            {
                StringBuilder shortPath = new StringBuilder(key);
                //shortPath.Remove(0, directoryPath.FullName.Length);
                Console.WriteLine($"[{shortPath}] -> somefile...(null={filebuffer.GetValueByKey(key)==null})");
            }
            Console.WriteLine("--- End Writing Dictionary ---\n");
        }

        /// <summary>
        /// Выполняет поиск директорий, содержащих Html файл - они станут префиксами URL.
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

        public string GetLocalPath(string path)
        {
            if (!path.StartsWith(this.DirectoryPath.FullName)) return string.Empty;
            int localPathStartIdx = this.DirectoryPath.FullName.Length;
            return path.Substring(localPathStartIdx);
        }
    }
}
