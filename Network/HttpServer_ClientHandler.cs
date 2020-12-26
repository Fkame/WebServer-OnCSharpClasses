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
    /// Часть класса _ClientHandler, содежрит основной цикл прослушивания и методы отправки ответа на запрос.
    /// </summary>
    public partial class HttpServer
    {
        /// <summary>
        /// Запуск алгоритма прослушиваний запросов - отправки ответов.
        /// Сперва проиходит запуск анализатора директории, после этого настроивается сам веб-сервер.
        /// После этого происходит вход в бесконечный цикл ожидания запросов и ответы на них.
        /// </summary>
        public void Start()
        {
            // Создание объекта, следящим за указанной директорией и управляющим буфером файлов; загружает файлы в буффер.
            DirectoryWorker.Start();

            // Вывод служебной информации
            ConsoleColorPrinter.WriteLineWithTime($"Server on [{Dns.GetHostName()}] Started!", ConsoleColor.Green, ConsoleColor.Yellow);      
            //ConsoleColorPrinter.WriteLine("Tip: Press Ctrl + C to finish program", ConsoleColor.Magenta); 

            // Настройка слушателя http запросов и его запуск.
            httpListener = this.GetConfiguredListener();
            httpListener.Start();
            ConsoleColorPrinter.WriteLineWithTime("Start listening requests...\n", ConsoleColor.Green, ConsoleColor.Yellow);

            // Бесконечный цикл создания ассинхронных обработчиков входящих запросов. Каждый последующий ждёт пока предыдущий получит запрос.
            // Таким образом они не хаотично появляются каждую микросекунду.
            while (httpListener.IsListening)
            {
                IAsyncResult result = httpListener.BeginGetContext(new AsyncCallback(ClientHandler), httpListener);
                result.AsyncWaitHandle.WaitOne();
            }       
        }

        /// <summary>
        /// Запуск асинхронной работы сервера.
        /// </summary>
        /// <returns></returns>
        public async void StartAsync()
        {
            await Task.Run(this.Start);
        }

        public void Shutdown()
        {
            if (httpListener.IsListening)
            {
                httpListener.Stop();
                //Console.WriteLine("Listener stopped");
            }
            DirectoryWorker.Shutdown();
            //Console.WriteLine("File system wathcer stopped");
            NLog.LogManager.Shutdown();
            //Console.WriteLine("Logger stopped");
        }

        /// <summary>
        /// Запускает "Настоящий" (потому что что-то делает) обработчик запросов клиента, и обрабатывает его ошибки.
        /// </summary>
        /// <param name="result"></param>
        private void ClientHandler(IAsyncResult result)
        {
            try
            { 
                RealClientHandler(result); 
            }
            catch (Exception ex) 
            {
                ConsoleColorPrinter.WriteLine(ex.Message, ConsoleColor.White);
                ConsoleColorPrinter.WriteLine(ex.StackTrace, ConsoleColor.White);
            }
        }

        /// <summary>
        /// Асинхронно принимает запрос, после чего завершает асинхронную операцию и начинает обратывать входящий http запрос
        /// </summary>
        /// <param name="result"></param>
        private void RealClientHandler(IAsyncResult result)
        {
            HttpListener listener = (HttpListener) result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            
            HttpListenerRequest request = context.Request;

            ConsoleColorPrinter.WriteLineWithTime("Input request: ", ConsoleColor.Green, ConsoleColor.Yellow);
            HttpPrinterHelper.PrintRequestInfoByType(request, LogRequestTextes);

            HttpListenerResponse response = context.Response;

            // создаем ответ в виде кода html
            this.SendResponce(context);
        }

        /// <summary>
        /// Отправка ответа на запрос; это может быть что угодно: картинка, html-файл, js-файл, css-файл.
        /// </summary>
        /// <param name="context"></param>
        private void SendResponce(HttpListenerContext context)
        {
            string needPath = context.Request.RawUrl;

            // Частный случай запроса - когда не пишется конкретный html файл.
            if (needPath.EndsWith(HttpUriHelper.UrlPathChar)) 
                needPath = String.Format("{0}index.html", needPath);

            needPath = HttpUriHelper.ChangeUrlLikeToSeparator(needPath);

            // Получение и проверка данных по указанной дирректории.
            byte[] file = DirectoryWorker.filebuffer.GetValueByKey(needPath);
            if (file == null) 
            {
                ConsoleColorPrinter.WriteLine($"Can not find file for responce by way [{needPath}]", ConsoleColor.DarkGreen);
                return;
            }

            // Отправка запроса
            ConsoleColorPrinter.WriteLine($"Responcing file by way [{needPath}]", ConsoleColor.DarkGreen);
            HttpListenerResponse response = context.Response;
            Stream output = response.OutputStream;
            output.Write(file, 0, file.Length);
            output.Close();
            response.Close();
        }

        private void PrintPrefixes(HttpListener httpListener)
        {
            ConsoleColorPrinter.WriteLineWithTime("Set next Prefixes:", ConsoleColor.Green, ConsoleColor.Yellow);
            foreach (string url in httpListener.Prefixes)
            {
                ConsoleColorPrinter.WriteLine($"\t{url}", ConsoleColor.Green);
            }
        }
        
        /// <summary>
        /// Возвращает путь, по которому хранится "поднятый" сайт.
        /// </summary>
        /// <returns></returns>
        public string GetWebSiteDirectory()
        {
            return this.DirectoryWorker.DirectoryPath.ToString();
        }

        /// <summary>
        /// Возвращает список прослушиваемых URL адресов.
        /// </summary>
        /// <returns></returns>
        public string[] GetActiveUrls()
        {
            string[] array = new string[this.httpListener.Prefixes.Count];
            httpListener.Prefixes.CopyTo(array, 0);
            return array;
        }
    }
}
