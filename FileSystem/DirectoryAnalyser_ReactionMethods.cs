using System;
using System.IO;

namespace WebServer.FileSystem
{
    public partial class DirectoryAnalyser
    {
        private bool wasRenamed = false;

        /// <summary>
        /// 1. Ищет файл, с которым произошли изменения, если файл найден - переходим к шагу 2, если файл не найден, переходим к шагу 4.
        /// 2. Если файл найден, сверяется содержимое хранимого в буфере образа.
        /// 3. Если содержимое совпадает, алгоритм завершается, если содержимое не совпадает - в хранилице помещается более актуальное
        /// 4. Алгоритм завершается
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnChanged(object source, FileSystemEventArgs e) 
        {
            Console.WriteLine($"{DateTime.Now} File: {e.FullPath} -> {e.ChangeType}");

            if (wasRenamed)
            {
                wasRenamed = false;
                Console.WriteLine("Was renamed, not my territory.");
                return;
            }

            string path = e.FullPath;

            // Проверка: есть ли файл с таким путем в хранилище
            if (!File.Exists(path) | !filebuffer.IsExists(path)) 
            {
                Console.WriteLine($"No such file it buffer. Change func return;");
                return;
            }

            // Если пришло уведомление, это не обязательно значит, что запись в файл уже завершена.
            byte[] fileFromDisc = null;
            while (true)
            {
                try { fileFromDisc = File.ReadAllBytes(path); break; } 
                catch (Exception ex)
                { 
                    //Console.WriteLine(ex.Message);
                }
            }
           
            byte[] fileFromDict = filebuffer.GetValueByKey(path);

            // Проверка: совпадают ли размеры файлов. Нужно для того, чтобы не было исключения при следующей проверке
            if (fileFromDict.Length != fileFromDisc.Length) 
            {
                Console.WriteLine($"File on disc is not the same as in buffer - replacing");
                filebuffer.ReplaceValue(path, fileFromDisc);
                this.PrintFileBuffer(filebuffer, DirectoryPath);
                return;
            }

            // Проверка эквивалентности содержимого
            for (int i = 0; i < fileFromDisc.Length; i++) 
            {
                if (fileFromDict[i] != fileFromDisc[i])
                {
                    filebuffer.ReplaceValue(path, fileFromDisc);
                    Console.WriteLine($"File on disc is not the same as in buffer - replacing");
                    this.PrintFileBuffer(filebuffer, DirectoryPath);
                    break;
                }
            }
            
            Console.WriteLine("No changes in file");
        }

        /// <summary>
        /// 1. Заменить путь к файлу на более актуальный в хранилище. Хранимые данные не трогать.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnRenamed(object source, RenamedEventArgs e) 
        {
            wasRenamed = true;
            Console.WriteLine($"{DateTime.Now} File: {e.OldFullPath} -- to -- {e.FullPath} -> {e.ChangeType}");

            if(!filebuffer.ReplaceKey(e.OldFullPath, e.FullPath)) Console.WriteLine("Cannot replase key in dictionary!");
            this.PrintFileBuffer(filebuffer, DirectoryPath);
        }
        
        /// <summary>
        /// 1. Добавить файл с полученным путем в хранилище.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnCreated(object source, FileSystemEventArgs e) 
        {
            Console.WriteLine($"{DateTime.Now} File: {e.FullPath} -> {e.ChangeType}");

            byte[] file = null;
            while (true)
            {
                try { file = File.ReadAllBytes(e.FullPath); break; } 
                catch (Exception ex)
                { 
                    //Console.WriteLine(ex.Message);
                }
            }

            filebuffer.Add(e.FullPath, file);
            this.PrintFileBuffer(filebuffer, DirectoryPath);
        }

        /// <summary>
        /// 1. Удалить файл с таким путем из хранилища.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnDeleted(object source, FileSystemEventArgs e) 
        {
            Console.WriteLine($"{DateTime.Now} File: {e.FullPath} -> {e.ChangeType}");   
            filebuffer.Remove(e.FullPath);
            this.PrintFileBuffer(filebuffer, DirectoryPath);
        }   
    }
}
