using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;

using WebServer.HelpfulStaff;

namespace WebServer
{
    class WebServer
    {
        public string IP {get; private set;}
        public int Port {get; private set;} = 80;

        public bool ShowRequestTextes {get; set;} = true;
        public bool ShowResponseTextes {get; set;} = true;

        //public string[] SiteWays {get; set;} = null;

        public WebServer(string ip)
        {
           this.IP = ip;
        }

        public WebServer(string ip, int port)
        {
           this.IP = ip;
           this.Port = port;
        }

        public void Start()
        {
            ConsoleColorPrinter.WriteLineWithTime($"Server on [{Dns.GetHostName()}] Started!", ConsoleColor.Green, ConsoleColor.Yellow);      
            ConsoleColorPrinter.WriteLine("Tip: Press Ctrl + C to finish program", ConsoleColor.Magenta); 

            HttpListener httpListener = this.GetConfiguredListener();
            httpListener.Start();
            ConsoleColorPrinter.WriteLineWithTime("Start listening requests...", ConsoleColor.Green, ConsoleColor.Yellow);

            while (true)
            {
                HttpListenerContext context = httpListener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                Console.WriteLine();
                ConsoleColorPrinter.WriteLineWithTime("There is input request!", ConsoleColor.Green, ConsoleColor.Yellow);
                ConsoleColorPrinter.WriteLine($"Client Ip = [{request.RemoteEndPoint.Address}], port = [{request.RemoteEndPoint.Port}]", ConsoleColor.Green);

                if (ShowRequestTextes) 
                {
                   HttpHelper.PrintFullHttpRequestText(request);
                }
                
                // создаем ответ в виде кода html

                string responseStr = "<html><head><meta charset='utf8'></head><body>Как же я блять ненавижу программирование!</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // закрываем поток
                output.Close();
            }
            
            httpListener.Close();
        }

        private HttpListener GetConfiguredListener()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(String.Format("http://{0}:{1}/", this.IP, this.Port));
            httpListener.Prefixes.Add("http://localhost:8888/connection/");

            ConsoleColorPrinter.WriteLineWithTime("Set next Prefixes:", ConsoleColor.Green, ConsoleColor.Yellow);
            foreach (string url in httpListener.Prefixes)
            {
                ConsoleColorPrinter.WriteLine($"\t{url}", ConsoleColor.Green);
            }

            return httpListener;
        }
    }
}
