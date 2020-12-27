using System;
using System.IO;
using System.Net;
using System.Threading;
using WebServer.HelpfulStaff;
using WebServer.Network;

namespace WebServer.WebServer_Test
{
   public class DirectoryInitialisation_Test
   {
        public void StartTest()
        {
            ConsoleColorPrinter.WriteLine("<--- DirectoryInitialisation_Test - Start --->", ConsoleColor.DarkYellow);

            DirectoryInfo siteDir = new DirectoryInfo(Path.Combine(System.Environment.CurrentDirectory, "WebServer_Test", "TestWebsite"));
            TestDirectory(siteDir);
            ConsoleColorPrinter.WriteLine("<--- DirectoryInitialisation_Test - End --->\n", ConsoleColor.DarkYellow);
        }

        private void TestDirectory(DirectoryInfo directory)
        {
            // Создадим параметры для сервера
            int port = 8180;
            string ip = DefaultArguments.LocalIp;

            // Поднимем сервер и проверим, удалось ли нам это
            HttpServer server = TestHelper.UpServer(directory.FullName, port, ip);
            if (server == null)
            {
                ConsoleColorPrinter.WriteLine("Test failed - Server is still down!", ConsoleColor.DarkYellow);
                return;
            }

            if (!directory.FullName.Equals(server.GetWebSiteDirectory()))
            {
                ConsoleColorPrinter.WriteLine($"Directories conflict, test failed.\nSetDirectory={directory.FullName}, ServerDirectory={server.GetWebSiteDirectory()}", 
                    ConsoleColor.DarkYellow);
                return;
            }

            if (!TestHelper.IsHttpServerUp(ip, port, 5000)) return;
            ConsoleColorPrinter.WriteLine("Test success!", ConsoleColor.DarkYellow);

            // Выключим сервер
            server.Shutdown();

        }
   }
}