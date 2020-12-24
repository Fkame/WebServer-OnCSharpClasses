using System;
using System.IO;

namespace WebServer.FileSystem.HelpfulStaff
{
    /// <summary>
    /// Статический класс. Содержит методы для вывода информации о директории, и вывода содержимого директории
    /// </summary>
    public static class DirectoryDataPrinter
    {
        /// <summary>
        /// Метод для вывода содержимого указанной дирректории. При желании, выводит также содержимое и всех подкаталогов внутри директории.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="includeSubdirectories"></param>
        public static void PrintAllFilesInDirectory(DirectoryInfo directoryPath, bool includeSubdirectories)
        {
            if (includeSubdirectories)
            {
                Console.WriteLine("All files in directory and subdirectories:");
                Console.WriteLine(string.Join('\n', Directory.GetFiles(directoryPath.FullName, "*", SearchOption.AllDirectories)));
            }
            else
            {
                Console.WriteLine("All files in directory:");
                Console.WriteLine(string.Join('\n', Directory.GetFiles(directoryPath.FullName, "*", SearchOption.TopDirectoryOnly)));
            }
        }

        /// <summary>
        /// Метод для вывода в консоль общей информации о директории.
        /// </summary>
        /// <param name="directory"></param>
        public static void PrintInfoAboutDirectory(DirectoryInfo directory)
        {
            Console.WriteLine("***** Информация о каталоге *****\n");
            Console.WriteLine("Полный путь: {0}\nНазвание папки: {1}\nРодительский каталог: {2}\n" + 
                        "Время создания: {3}\nАтрибуты: {4}\nКорневой каталог: {5}",
                        directory.FullName, directory.Name, directory.Parent, 
                        directory.CreationTime, directory.Attributes, directory.Root
            );
        }
    
    }
}