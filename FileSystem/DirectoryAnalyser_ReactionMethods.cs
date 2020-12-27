using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebServer.FileSystem
{
    public partial class DirectoryAnalyser
    {
        /// <summary>
        /// Флаг для определения того, был ли файл переименован. Используется потому, как переименовывание файла генериует 2 события: 
        /// Change и Rename. Флаг помогает определить в обработчике Change, что содержимое файла не было изменено, не проверяя это самому.
        /// </summary>
        private bool wasRenamed = false;

        /// <summary>
        /// Делегат для вызова методов, желающих подписаться на событие NotifyAboutCreate.
        /// </summary>
        /// <param name="file"></param>
        public delegate void AddedNewFile(FileInfo file);

        /// <summary>
        /// Событие, уведомляющее о появлении нового файла в папке.
        /// </summary>
        public event AddedNewFile NotifyAboutCreate;

        /// <summary>
        /// Делегат для вызова методов, желающих подписаться на событие NotifyAboutDelete.
        /// </summary>
        /// <param name="file"></param>
        public delegate void DeleteFile(FileInfo file);

        /// <summary>
        /// Событие, уведомляющее об удалении существующего файла из папки.
        /// </summary>
        public event DeleteFile NotifyAboutDelete;

        /// <summary>
        /// Обработчик события Change от FileSystemWatcher.
        /// 1. Ищет файл, с которым произошли изменения, если файл найден - переходим к шагу 2, если файл не найден, переходим к шагу 4.
        /// 2. Если файл найден, сверяется содержимое хранимого в буфере образа.
        /// 3. Если содержимое совпадает, алгоритм завершается, если содержимое не совпадает - в хранилице помещается более актуальное
        /// 4. Алгоритм завершается
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnChanged(object source, FileSystemEventArgs e) 
        {
            //Console.WriteLine($"{DateTime.Now} File: {e.FullPath} -> {e.ChangeType}");
            logger.Trace($"{e.ChangeType} | {DateTime.Now} -> File: {e.FullPath}");

            if (wasRenamed)
            {
                wasRenamed = false;
                //Console.WriteLine("Was renamed, not change func territory.");
                logger.Trace("Change handler found that file was renamed - stop next checks\n");
                return;
            }

            string path = e.FullPath;

            // Проверка: есть ли файл с таким путем в хранилище
            string localPartOfPath = GetLocalPath(path);
            if (!File.Exists(path) | !filebuffer.IsExists(localPartOfPath)) 
            {
                //Console.WriteLine($"No such file in buffer. Change func return;");
                logger.Trace("No such file in buffer. Change handler stops\n");
                return;
            }

            // Если пришло уведомление, это не обязательно значит, что запись в файл уже завершена.
            byte[] fileFromDisc = null;
            while (true)
            {
                try { fileFromDisc = File.ReadAllBytes(path); break; } 
                catch (Exception) {  }
            }
           
            byte[] fileFromDict = filebuffer.GetValueByKey(localPartOfPath);

            // Проверка: совпадают ли размеры файлов. Нужно для того, чтобы не было исключения при следующей проверке
            if (fileFromDict.Length != fileFromDisc.Length) 
            {
                //Console.WriteLine($"File on disc is not the same length as in buffer - replacing");
                logger.Trace("File on disc is not the same length as in buffer - replacing\n");
                filebuffer.ReplaceValue(localPartOfPath, fileFromDisc);
                return;
            }

            // Проверка эквивалентности содержимого
            for (int i = 0; i < fileFromDisc.Length; i++) 
            {
                if (fileFromDict[i] != fileFromDisc[i])
                {
                    filebuffer.ReplaceValue(localPartOfPath, fileFromDisc);
                    //Console.WriteLine($"File on disc is not the same as in buffer - replacing");
                    logger.Trace("File on disc is not the same length as in buffer - replacing\n");
                    return;
                }
            }
            
            //Console.WriteLine("No changes in file");
            logger.Trace("No changes in file\n");
        }

        /// <summary>
        /// Обработчик события Rename от FileSystemWatcher.
        /// Заменяет путь к файлу в буффере на более актуальный . Хранимые данные не трогает.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnRenamed(object source, RenamedEventArgs e) 
        {
            wasRenamed = true;
            //Console.WriteLine($"{DateTime.Now} File: {e.OldFullPath} -- to -- {e.FullPath} -> {e.ChangeType}");
            logger.Trace($"{e.ChangeType} | {DateTime.Now} -> File: {this.GetLocalPath(e.OldFullPath)} -- to -- {this.GetLocalPath(e.FullPath)}");
            
            string oldLocalPath = GetLocalPath(e.OldFullPath);
            string newLocalPath = GetLocalPath(e.FullPath);
            if(!filebuffer.ReplaceKey(oldLocalPath, newLocalPath)) logger.Trace("Cannot replase key in dictionary!\n");
            else logger.Trace("Key replased\n");
        }
        
        /// <summary>
        /// Обработчик события Create от FileSystemWatcher.
        /// Добавляет новый файл в буффер.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnCreated(object source, FileSystemEventArgs e) 
        {
            //Console.WriteLine($"{DateTime.Now} File: {e.FullPath} -> {e.ChangeType}");
            logger.Trace($"{e.ChangeType} | {DateTime.Now} -> File: {e.FullPath}");

            byte[] file = null;
            
            int count = 10;
            while (count-- > 0)
            {
                try { file = File.ReadAllBytes(e.FullPath); break; } 
                catch (Exception) {  Thread.Sleep(10); }
            }
            if (count <= 0) return;

            string localPath = GetLocalPath(e.FullPath);
            filebuffer.Add(localPath, file);

            logger.Trace("Calling event about creating!");
            // Уведомим httpServer о добавленном файле
            NotifyAboutCreate?.Invoke(new FileInfo(e.FullPath));
        }

        /// <summary>
        /// Обработчик события Delete от FileSystemWatcher.
        /// Удаляеть файл из буффера.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnDeleted(object source, FileSystemEventArgs e) 
        {
            //Console.WriteLine($"{DateTime.Now} File: {e.FullPath} -> {e.ChangeType}");   
            logger.Trace($"{e.ChangeType} | {DateTime.Now} -> File: {e.FullPath}");
            string localPath = GetLocalPath(e.FullPath);
            filebuffer.Remove(localPath);
            //this.PrintFileBuffer(filebuffer, DirectoryPath);

            // Уведомим httpServer об удалённом файле
            NotifyAboutDelete?.Invoke(new FileInfo(e.FullPath));
        }   
    }
}
