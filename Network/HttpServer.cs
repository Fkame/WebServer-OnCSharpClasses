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
    /// Класс, реализующий веб-сервер, работающий на протоколе http, и соответственно принимающий http-запросы.
    /// </summary>
    public class HttpServer
    {
        public string IP {get; private set;}
        public int Port {get; private set;}

        /// <summary>
        /// Данный объект выполняет ряд важных задач по работе с файловой системой:
        /// 1.Он инкапсулирует в себе контейнер, который хранит в себе буферезированные файлы (то есть загружает их в оперативную память).
        /// Эта штука нужна, чтобы при отправке файла по http запросу не читать его с диска, тем самым, скорее всего, производительность
        /// будет выше.
        /// 2. Объект запускает FileSystemWatcher, который отслеживает изменеия в переданной директории и сообщает о них.
        /// 3. Объект реагирует на изменения, и соответствующе обновляет буфер. Это нужно, чтобы можно было подправить сайт, не выключая сервер.
        /// 4. TODO--> Нужно настроить сихнронизацию доступа к буферу файлов, иначе возможна отправка устаревшей информации. И возникновение ошибок.
        /// Скорее всего будет достаточно что-то типа монитора при изменениии.
        /// </summary>
        private DirectoryAnalyser DirectoryWorker;

        private HttpListener httpListener;

        public ShowInfoType LogRequestTextes {get; set;} = ShowInfoType.ShowMinimalInfo;

        public ShowInfoType LogResponseTextes {get; set;} = ShowInfoType.ShowNothing;

        private HttpUriHelper uriHelper;

        public HttpServer(string ip, int port, DirectoryInfo directory)
        {
            this.IP = ip;
            this.Port = port;
            uriHelper = new HttpUriHelper(ip, port);

            this.DirectoryWorker = new DirectoryAnalyser(directory);

            // Добавим обработчик события создания нового файла
            this.DirectoryWorker.NotifyAboutCreate += OnNewFile;

            // Добавим обработчик события удаления файла
            this.DirectoryWorker.NotifyAboutDelete += OnDeleteFile;
        }

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
            ConsoleColorPrinter.WriteLine("Tip: Press Ctrl + C to finish program", ConsoleColor.Magenta); 

            // Настройка слушателя http запросов и его запуск.
            httpListener = this.GetConfiguredListener();
            httpListener.Start();
            ConsoleColorPrinter.WriteLineWithTime("Start listening requests...\n", ConsoleColor.Green, ConsoleColor.Yellow);

            // Бесконечный цикл создания ассинхронных обработчиков входящих запросов. Каждый последующий ждёт пока предыдущий получит запрос.
            // Таким образом они не хаотично появляются каждую микросекунду.
            while (true)
            {
                IAsyncResult result = httpListener.BeginGetContext(new AsyncCallback(ClientHandler), httpListener);
                result.AsyncWaitHandle.WaitOne();
            }       
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

        /// <summary>
        /// Создаётся HttpListener, после чего заполняются его .Prefixes ссылками на директории.
        /// </summary>
        /// <returns></returns>
        private HttpListener GetConfiguredListener()
        {
            HttpListener httpListener = new HttpListener();

            // Добавляем url верхнего уровня в список префиксов.
            foreach (string url in uriHelper.UrlBeginings)
            {
                httpListener.Prefixes.Add(String.Format("{0}/",url));
            }

            // Ищем только директории, содержание файлы *.html, после чего меняем разделитель с '\' на '/'.
            List<string> prefixes = DirectoryWorker.GetDirectoriesWithDotHTML();
            prefixes = HttpUriHelper.ChangeSeparatorToUrlLike(prefixes);

            // Добавляем отобранные директории к url верхнего уровня и закидываем в список префиксов.
            foreach(string path in prefixes)
            {
                foreach (string startUrl in uriHelper.UrlBeginings)
                {
                    httpListener.Prefixes.Add(String.Format("{0}{1}",startUrl, path));
                }
            }

            // Вывод всех добавленных префиксов
            this.PrintPrefixes(httpListener);
            
            return httpListener;
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
