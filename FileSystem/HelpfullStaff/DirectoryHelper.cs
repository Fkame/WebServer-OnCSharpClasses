using System;
using System.IO;

namespace WebServer.FileSystem.HelpfulStaff
{
    /// <summary>
    /// Несколько статических методов, выполняющие проверку валидности директории.
    /// </summary>
    public static class DirectoryHelper
    {
        /// <summary>
        /// Данный метод проверяет, валидный ли путь, и не защищена ли папка, путь в которую передается.
        /// </summary>
        /// <remarks>Существует такая папка или нет - это игнорируется. Гланое - валидность</remarks>
        /// <param name="path">Путь к папке (директория).</param>
        /// <param name="maybePath">Переменная в которую объект типа DirectoryInfo - это объект, представляющий собой абстракцию над
        /// указанной в path папкой.</param>
        /// <returns>Вернёт true, если имя директории указано правильно, даже если такой папки не существует на диске</returns>
        public static bool TryDirectory(string path, out DirectoryInfo maybePath) 
        {
            bool isError = false;
            maybePath = null;
            try 
            { 
                maybePath = new DirectoryInfo(path); 
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine("Not enough access to this directory!");
                isError = true;
            }
            catch (System.ArgumentException)
            {
                Console.WriteLine("Invalid path!");
                isError = true;
            }
            catch (System.IO.PathTooLongException) 
            {
                Console.WriteLine("PathTooLong!");
                isError = true;
            }

            return isError;
        }

        /// <summary>
        /// Простой метод-опросник. Принимает текст вопроса, выводит его, считывает ответ, возвращает ответ вызываемому символу.
        /// </summary>
        /// <param name="text">Текст вопроса</param>
        /// <returns>Символ Y или N</returns>
        public static char AskUserYesOrNo(string text)
        {
            char answer;
            Console.WriteLine(text);
            do {
                Console.Write("Press your answer: ");
                answer = Console.ReadKey().KeyChar;
            } while (answer != 'y' & answer != 'n');
            return answer;
        }
    }
}