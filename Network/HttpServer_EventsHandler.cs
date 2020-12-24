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
                ConsoleColorPrinter.WriteLineWithTime($"Find new HTML file in=[{localDirectory}/]. Adding it to .Prefixes", 
                                                ConsoleColor.Green, ConsoleColor.Yellow);
                
                foreach (string path in uriHelper.ConnectBeginigsWithPath(localDirectory))
                {
                    this.httpListener.Prefixes.Add(path);
                }
                ConsoleColorPrinter.WriteLine($"New prefix created!", ConsoleColor.DarkGreen);
                this.PrintPrefixes(httpListener);
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
                ConsoleColorPrinter.WriteLineWithTime($"HTML file deleted in=[{localDirectory}/]. Deleting it from .Prefixes", 
                                                ConsoleColor.Green, ConsoleColor.Yellow);

                foreach (string path in uriHelper.ConnectBeginigsWithPath(localDirectory))
                {
                    this.httpListener.Prefixes.Remove(path);
                }
                
                ConsoleColorPrinter.WriteLine($"Prefix deleted!", ConsoleColor.DarkGreen);
                this.PrintPrefixes(httpListener);
            }
        }
    }
}
