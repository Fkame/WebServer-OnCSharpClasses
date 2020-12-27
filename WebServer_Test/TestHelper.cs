using System;
using System.IO;
using System.Net;
using System.Threading;
using WebServer.HelpfulStaff;
using WebServer.Network;

namespace WebServer.WebServer_Test
{
    public static class TestHelper
    {
        public static bool IsHttpServerUp(Uri uri, int waitTimeOut)
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = waitTimeOut;
                WebResponse response = request.GetResponse();
                ConsoleColorPrinter.WriteLine($"Server by {uri} is up!", ConsoleColor.DarkYellow);

                return true;
            }
            catch (Exception)
            {
                ConsoleColorPrinter.WriteLine($"Server by {uri} is down!", ConsoleColor.DarkYellow);
                return false;
            }
        }

        public static bool IsHttpServerUp(string ip, int port, int waitTimeOut, params string[] subUrls)
        {
            Uri uri = TestHelper.FormHttpAddrress(ip, port, subUrls);
            if (uri == null) return false;
            return IsHttpServerUp(uri, waitTimeOut);
        }

        public static Uri FormHttpAddrress(string ip, int port, params string[] subUrls)
        {
            char uriSep = '/';
            Uri uri = null;
            try
            {
                if (subUrls.Length == 0)
                {
                    uri = new Uri(String.Format("http://{0}:{1}/", ip, port));
                }
                else
                {
                    uri = new Uri(String.Format("http://{0}:{1}/{2}/", ip, port, string.Join(uriSep, subUrls)));
                }
            }
            catch (Exception) { }

            return uri;
        }

        public static HttpServer UpServer(string directoryWithSite, int port, string ip)
        {
            ConsoleColorPrinter.WriteLine($"Test with data port={port}, ip={ip}, directory={directoryWithSite}\n", ConsoleColor.DarkYellow);
            HttpServer server = new HttpServer(ip, port, new DirectoryInfo(directoryWithSite));
            server.StartAsync();
            return server;
        }

        public static (string folder, string file) CreateFolderAndHtmlFileWithTestText(DirectoryInfo whereToCreate)
        {
            DirectoryInfo tempFolder = null;
            FileInfo tempHtml = null;
            try
            {
                // Создание папки
                tempFolder = new DirectoryInfo(Path.Combine(whereToCreate.FullName, "tempFolder"));
                if (!tempFolder.Exists) tempFolder.Create();

                // Создание пути к файлу index.html
                tempHtml = new FileInfo(Path.Combine(tempFolder.FullName, "index.html"));

                // Небольшая задержка - система наверняка начала обрабатывать появление нового файла
                Thread.Sleep(500);

                // Текст для файла
                string textToFile = "<html><head>Test page</head><body><h1>Bruh<br><h2>Mini bruh</body></html>";

                // Запись небольшого текста в файл
                int magicNum = 0;
                while (magicNum != 43)
                {
                    try 
                    { 
                        File.WriteAllText(tempHtml.FullName, textToFile); 
                        magicNum = 43; 
                    }
                    catch (Exception) 
                    { 
                        ConsoleColorPrinter.WriteLineWithTime("File is busy...", ConsoleColor.White, ConsoleColor.Yellow); 
                        Thread.Sleep(500);
                    }
                }

                return (tempFolder.FullName, tempHtml.FullName);
            }
            catch (Exception) 
            {
                if (tempFolder != null)
                {
                    if (tempFolder.Exists) tempFolder.Delete(true);
                }

                return (string.Empty, string.Empty);
            }
        }
    }
}