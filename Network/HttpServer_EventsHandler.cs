using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebServer.HelpfulStaff;
using WebServer.FileSystem;
using WebServer.Network.HelpfulStaff;

namespace WebServer.Network
{
    /// <summary>
    /// Часть класса _EventsHandler, содержит обработчики событий от DirectoryAnalyser
    /// </summary>
    public partial class HttpServer
    {   
        /// <summary>
        /// Обработчик события NotifyAboutCreate от DirectoryAnalyser.
        /// Если файл является HTML, то добавляет его директорию в список префиксов.
        /// </summary>
        /// <param name="file"></param>
        public void OnNewFile(FileInfo file)
        {
            if (file.Extension.EndsWith(".html"))
            {
                string localDirectory = DirectoryWorker.GetLocalPath(file.DirectoryName);      
                //ConsoleColorPrinter.WriteLineWithTime($"Find new HTML file in=[{localDirectory}/]. Adding it to .Prefixes", 
                //                                ConsoleColor.Green, ConsoleColor.Yellow);
                logger.Debug($"Find new HTML file in=[{localDirectory}/]. Adding it to .Prefixes");

                foreach (string path in uriHelper.ConnectBeginigsWithPath(localDirectory))
                {
                    try { this.httpListener.Prefixes.Add(path); }
                    catch (Exception ex) 
                    { 
                        logger.Warn($"Trouble with adding prefix!\nMessage: {ex.Message}\nStackTrace:\n{ex.StackTrace}"); 
                    }
                }
                //ConsoleColorPrinter.WriteLine($"New prefix created!", ConsoleColor.DarkGreen);
                logger.Debug("Prefix adding procedure finished\n");
                //this.PrintPrefixes(httpListener);
            }
        }

        /// <summary>
        /// Обработчик события NotifyAboutDelete от DirectoryAnalyser.
        /// Если файл является HTML, то удаляет его директорию в список префиксов
        /// </summary>
        /// <param name="file"></param>
        public void OnDeleteFile(FileInfo file) 
        {
            if (file.Extension.EndsWith(".html"))
            {
                string localDirectory = DirectoryWorker.GetLocalPath(file.DirectoryName); 
                //ConsoleColorPrinter.WriteLineWithTime($"HTML file deleted in=[{localDirectory}/]. Deleting it from .Prefixes", 
                //                                ConsoleColor.Green, ConsoleColor.Yellow);
                logger.Debug($"HTML file deleted in=[{localDirectory}/]. Deleting it from .Prefixes");

                foreach (string path in uriHelper.ConnectBeginigsWithPath(localDirectory))
                {
                    this.httpListener.Prefixes.Remove(path);
                }
                
                //ConsoleColorPrinter.WriteLine($"Prefix deleted!", ConsoleColor.DarkGreen);
                logger.Debug($"HTML file deleted in=[{localDirectory}/]. Deleting it from .Prefixes");
                //this.PrintPrefixes(httpListener);
            }
        }
    }
}
