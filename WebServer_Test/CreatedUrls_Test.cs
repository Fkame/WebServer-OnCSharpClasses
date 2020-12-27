using System;
using System.IO;
using System.Net;
using System.Threading;
using WebServer.HelpfulStaff;
using WebServer.Network;

namespace WebServer.WebServer_Test
{
   public class CreatedUrls_Test
   {
        public void StartTest()
        {
            ConsoleColorPrinter.WriteLine("<--- CreatedUrls_Test - Start --->", ConsoleColor.DarkYellow);
            this.TestUrls();
            ConsoleColorPrinter.WriteLine("<--- CreatedUrls_Test - End --->\n", ConsoleColor.DarkYellow);
        }

        private void TestUrls()
        {
            // Создадим параметры для сервера
            int port = 8180;
            string ip = DefaultArguments.LocalIp;
            string directory = DefaultArguments.DefaultDirectory;

            // Поднимем сервер и проверим, удалось ли нам это
            HttpServer server = TestHelper.UpServer(directory, port, ip);
            if (server == null)
            {
                ConsoleColorPrinter.WriteLine("Test failed - Server is still down!", ConsoleColor.DarkYellow);
                return;
            }

            // Получим список всех адресов, которые обслуживает сервер и проверим каждый
            string[] urls = server.GetActiveUrls();
            for (int i = 0; i < urls.Length; i++)
            {
                if (!TestHelper.IsHttpServerUp(new Uri(urls[i]), 5000))
                {
                    ConsoleColorPrinter.WriteLine($"Error: {urls[i]} is not supporting but must beign", ConsoleColor.DarkYellow);
                }
                else
                {
                    ConsoleColorPrinter.WriteLine($"OK: {urls[i]} is supporting", ConsoleColor.DarkYellow);
                }
            }

            // Выключаем сервер
            server.Shutdown();
        }
   }
}
