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
            #region Old code
            /*
            // Создание объекта, следящим за указанной директорией и управляющим буфером файлов; загружает файлы в буффер.
            DirectoryWorker.Start();

            // Вывод служебной информации
            ConsoleColorPrinter.WriteLineWithTime($"Server on [{Dns.GetHostName()}] Started!", ConsoleColor.Green, ConsoleColor.Yellow); 
            //logger.Debug($"Server on [{Dns.GetHostName()}] Started!");     

            // Настройка слушателя http запросов и его запуск.
            httpListener = this.GetConfiguredListener();
            httpListener.Start();
            ConsoleColorPrinter.WriteLineWithTime("Start listening requests...\n", ConsoleColor.Green, ConsoleColor.Yellow);
            //logger.Debug("Start listening requests...\n"); 
            */
            #endregion

            // Инициализация всех компонентов и их запуск
            this.DoInitialisingOfAllComponents();

            // Запуск главного цикла
            this.DoMainCycle();
        }

        /// <summary>
        /// Запуск асинхронной работы сервера.
        /// </summary>
        /// <returns></returns>
        public async void StartAsync()
        {
            // Инициализация всех компонентов и их запуск
            this.DoInitialisingOfAllComponents();

            // Запуск главного цикла
            await Task.Run(this.DoMainCycle);
        }

        /// <summary>
        /// Метод инициализирует запуск работы всех модулей программы.
        /// </summary>
        private void DoInitialisingOfAllComponents()
        {
            // Создание объекта, следящим за указанной директорией и управляющим буфером файлов; загружает файлы в буффер.
            DirectoryWorker.Start();

            // Вывод служебной информации
            ConsoleColorPrinter.WriteLineWithTime($"Server on [{Dns.GetHostName()}] Started!", ConsoleColor.Green, ConsoleColor.Yellow);    

            // Настройка слушателя http запросов и его запуск.
            httpListener = this.GetConfiguredListener();
            httpListener.Start();
            ConsoleColorPrinter.WriteLineWithTime("Start listening requests...\n", ConsoleColor.Green, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Основной цикл обслуживания запросов клиентов.
        /// </summary>
        private void DoMainCycle()
        {
            try
            {
                while (httpListener.IsListening)
                {
                    IAsyncResult result = httpListener.BeginGetContext(new AsyncCallback(ClientHandler), httpListener);
                    result.AsyncWaitHandle.WaitOne();
                }  
            }
            catch (Exception ex)
            {
                logger.Error("Some troubles with main listener cycle. This can be because of finishing programm.\nMessage: {0}\nStackTrace:\n{1}",
                    ex.Message, ex.StackTrace);
            }   
        }

        /// <summary>
        /// Очистка ресурсов, которые использует объект. Прерываются асинхронные операции, которые делаются на фоне.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (httpListener.IsListening)
                {
                    httpListener.Stop();
                    //logger.Debug("Listener stopped");
                    ConsoleColorPrinter.WriteLineWithTime("Listener stopped", ConsoleColor.DarkGreen, ConsoleColor.Yellow);
                }
                DirectoryWorker.Shutdown();
                //logger.Debug("File system wathcer stopped");
                ConsoleColorPrinter.WriteLineWithTime("File system watcher stopped", ConsoleColor.DarkGreen, ConsoleColor.Yellow);
                NLog.LogManager.Shutdown();
                //logger.Debug("Logger stopped");
                ConsoleColorPrinter.WriteLineWithTime("Logger stopped", ConsoleColor.DarkGreen, ConsoleColor.Yellow);
            }
            catch (Exception) { }
           
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
                //ConsoleColorPrinter.WriteLine(ex.Message, ConsoleColor.White);
                //ConsoleColorPrinter.WriteLine(ex.StackTrace, ConsoleColor.White);
                logger.Warn($"Message:{ex.Message}\nStackTrace{ex.StackTrace}\n");
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

            //ConsoleColorPrinter.WriteLineWithTime("Input request: ", ConsoleColor.Green, ConsoleColor.Yellow);
            logger.Info("Input request: ");
            logger.Info(HttpPrinterHelper.GetMinimalHttpInfo(request));
            //HttpPrinterHelper.PrintRequestInfoByType(request, ShowInfoByType.ShowMinimalInfo);

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
                //ConsoleColorPrinter.WriteLine($"Can not find file for responce by way [{needPath}]", ConsoleColor.DarkGreen);
                logger.Warn($"Can not find file for responce by way [{needPath}]");
                return;
            }

            // Отправка запроса
            //ConsoleColorPrinter.WriteLine($"Responcing file by way [{needPath}]", ConsoleColor.DarkGreen);
            logger.Info($"Responcing file by way [{needPath}]");
            HttpListenerResponse response = context.Response;
            Stream output = response.OutputStream;
            output.Write(file, 0, file.Length);
            output.Close();
            response.Close();
        }

        /// <summary>
        /// Выводит в консоль доступные для подключения URL адреса.
        /// </summary>
        /// <param name="httpListener"></param>
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
