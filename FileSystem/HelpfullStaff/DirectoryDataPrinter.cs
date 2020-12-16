using System;
using System.IO;

namespace WebServer.FileSystem.HelpfulStaff
{
    public static class DirectoryDataPrinter
    {
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