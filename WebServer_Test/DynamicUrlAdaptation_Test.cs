using System;
using System.IO;
using System.Net;
using System.Threading;
using WebServer.HelpfulStaff;
using WebServer.Network;

namespace WebServer.WebServer_Test
{
   public class DynamicUrlAdaptation_Test
   {
        public void StartTest()
        {
            ConsoleColorPrinter.WriteLine("<--- DirectoryInitialisation_Test - Start --->", ConsoleColor.DarkYellow);

            DirectoryInfo siteDir = new DirectoryInfo(Path.Combine(System.Environment.CurrentDirectory, "WebServer_Test", "TestWebsite"));
            int port = 8250;
            string ip = DefaultArguments.LocalIp;
            TestDynamicAdaptation(siteDir, port, ip);
            ConsoleColorPrinter.WriteLine("<--- DirectoryInitialisation_Test - End --->\n", ConsoleColor.DarkYellow);
        }

        private void TestDynamicAdaptation(DirectoryInfo directory, int port, string ip)
        {
            // Поднимем сервер и проверим, удалось ли нам это
            HttpServer server = TestHelper.UpServer(directory.FullName, port, ip);
            if (server == null)
            {
                ConsoleColorPrinter.WriteLine("Test failed - Server is still down!", ConsoleColor.DarkYellow);
                return;
            }

            // Создадим файл и папку в директории с сайтом
            (string folder, string html) fileAndFolder = TestHelper.CreateFolderAndHtmlFileWithTestText(directory);
            if (fileAndFolder.html.Equals(string.Empty))
            {
                ConsoleColorPrinter.WriteLine("Test failed - cannot create test file", ConsoleColor.DarkYellow);
            }  

            // Подождём, пока адаптируется url
            Thread.Sleep(2000);

            // Проверим, доступен ли теперь новый url
            DirectoryInfo path = new DirectoryInfo(fileAndFolder.folder);
            if (!TestHelper.IsHttpServerUp(ip, port, 5000, path.Name)) return;

            // Удалим всё то, что создали
            Directory.Delete(fileAndFolder.folder, true);

            // Выключим сервер 
            server.Shutdown();
        }

       
   }
}