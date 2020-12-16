using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using WebServer.HelpfulStaff;
using WebServer.FileSystem;
using WebServer.Network.HelpfulStaff;

namespace WebServer.Network
{
    public class HttpServer
    {
        public string IP {get; private set;}
        public int Port {get; private set;}

        public ShowInfoType ShowRequestTextes {get; set;} = ShowInfoType.ShowMinimalInfo;

        public ShowInfoType ShowResponseTextes {get; set;} = ShowInfoType.ShowNothing;

        public HttpServer(string ip, int port, DirectoryInfo directory)
        {
           this.IP = ip;
           this.Port = port;
        }

        public void Start()
        {
            // Создание объекта, следящим за указанной директорией и управляющим буфером файлов

            ConsoleColorPrinter.WriteLineWithTime($"Server on [{Dns.GetHostName()}] Started!", ConsoleColor.Green, ConsoleColor.Yellow);      
            ConsoleColorPrinter.WriteLine("Tip: Press Ctrl + C to finish program", ConsoleColor.Magenta); 

            HttpListener httpListener = this.GetConfiguredListener();
            httpListener.Start();
            ConsoleColorPrinter.WriteLineWithTime("Start listening requests...\n", ConsoleColor.Green, ConsoleColor.Yellow);

            // Бесконечный цикл создания ассинхронных обработчиков входящих запросов
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
            HttpPrinterHelper.PrintRequestInfoByType(request, ShowRequestTextes);

            HttpListenerResponse response = context.Response;

            // создаем ответ в виде кода html
            // this.SendResponce();
            string path = Path.Join(System.Environment.CurrentDirectory, 
                "Resources", 
                "frontend-test-jQuery-master", 
                "src",
                "index.html");
            ConsoleColorPrinter.WriteLine($"Responce file by way [{path}]", ConsoleColor.DarkGreen);
            
            byte[] buffer = File.ReadAllBytes(path);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // закрываем поток
            output.Close();
        }


        #region Старый код
        private void MainCycleOfRequestsResponces(HttpListener listener) 
        {
            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    ConsoleColorPrinter.WriteLineWithTime("Input request: ", ConsoleColor.Green, ConsoleColor.Yellow);
                    Thread.Sleep(30000);

                    HttpPrinterHelper.PrintRequestInfoByType(request, ShowRequestTextes);

                    // создаем ответ в виде кода html
                    string path = Path.Join(System.Environment.CurrentDirectory, 
                        "Resources", 
                        "frontend-test-jQuery-master", 
                        "src",
                        "index.html");
                    ConsoleColorPrinter.WriteLine($"Responce file by way [{path}]", ConsoleColor.DarkGreen);
                    
                    byte[] buffer = File.ReadAllBytes(path);
                    // получаем поток ответа и пишем в него ответ
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // закрываем поток
                    output.Close();
                } 
                catch (Exception ex) 
                {
                    ConsoleColorPrinter.WriteLine(ex.Message, ConsoleColor.White);
                    ConsoleColorPrinter.WriteLine(ex.StackTrace, ConsoleColor.White);
                }
            }
            
            listener.Close();
        }

        #endregion

        /// <summary>
        /// Создаётся HttpListener, после чего заполняются его .Prefixes ссылками на директории, содержащие index.html файлы
        /// </summary>
        /// <returns></returns>
        private HttpListener GetConfiguredListener()
        {
            HttpListener httpListener = new HttpListener();
            string[] urlBeginings = { String.Format("http://{0}:{1}/", this.IP, this.Port),
                                    String.Format("http://localhost:{0}/", this.Port) 
            };
        
            foreach (string url in urlBeginings)
            {
                 httpListener.Prefixes.Add(url);
            }

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
    }
}
