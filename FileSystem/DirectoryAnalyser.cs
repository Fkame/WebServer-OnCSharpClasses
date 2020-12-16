using System;
using System.IO;
using System.Text;

namespace WebServer.FileSystem
{
    public partial class DirectoryAnalyser
    {
        public DirectoryInfo DirectoryPath{ get; private set; }

        public FilesContainer filebuffer {get; private set; }

        public DirectoryAnalyser(DirectoryInfo directoryPath) 
        {
            this.DirectoryPath = directoryPath;
            filebuffer = new FilesContainer();
        }

        public void StartSomeJob()
        { 
            // Загрузка файлов с диска в буффер
            this.LoadFilesToBuffer();

            // Просмотр буфера после загрузки туда файлов
            this.PrintFileBuffer(filebuffer, DirectoryPath);
            
            // Запуск отслеживателя изменений в директории и поддиректориях. Осторожно: работает асинхронно!
            this.StartWatchingForChangesInFiles();
        }

        private void LoadFilesToBuffer()
        {
            filebuffer.Clear();
            string[] allFiles = Directory.GetFiles(DirectoryPath.FullName, "*", SearchOption.AllDirectories);
            foreach (string path in allFiles)
            {
                byte[] file = File.ReadAllBytes(path);
                filebuffer.Add(path, file);
            }
        }

        private void PrintFileBuffer(FilesContainer filebuffer, DirectoryInfo directoryPath)
        {
            Console.WriteLine("\n--- Writing Dictionaty ---");
            string[] keys = filebuffer.GetKeys();
            foreach (string key in keys)
            {
                StringBuilder shortPath = new StringBuilder(key);
                shortPath.Remove(0, directoryPath.FullName.Length);
                Console.WriteLine($"[{shortPath}] -> somefile...(null={filebuffer.GetValueByKey(key)==null})");
            }
            Console.WriteLine("--- End Writing Dictionary ---\n");
        }
    }
}
