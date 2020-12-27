using System;
using System.IO;
using System.Net;
using System.Threading;
using WebServer.HelpfulStaff;
using WebServer.Network;

namespace WebServer.WebServer_Test
{
   public class PortInitialisation_Test
   {
      public void StartTest()
      {
         ConsoleColorPrinter.WriteLine("<--- PortInitialisation_Test - Start --->", ConsoleColor.DarkYellow);
         int port;
         port = 80;
         TestPortAfterCreation(port);

         port = 8150;
         TestPortAfterCreation(port);
         ConsoleColorPrinter.WriteLine("<--- PortInitialisation_Test - End --->\n", ConsoleColor.DarkYellow);
      }

      /// <summary>
      /// Тест проверяет, правильно ли присвоился порт серверу, и поднялся ли сервер с таким портом.
      /// </summary>
      /// <param name="port"></param>
      private void TestPortAfterCreation(int port)
      {
         ConsoleColorPrinter.WriteLine($"Test with data port = {port}\n", ConsoleColor.DarkYellow);

         // Создадим параметры для сервера
         string ip = DefaultArguments.LocalIp;
         string directory = DefaultArguments.DefaultDirectory;

         // Поднимем сервер и проверим, удалось ли нам это
         HttpServer server = new HttpServer(ip, port, new DirectoryInfo(directory));
         server.StartAsync();

         if (port != server.Port)
         {
            ConsoleColorPrinter.WriteLine("Ports conflict, test failed", ConsoleColor.DarkYellow);
            return;
         }

         if (!TestHelper.IsHttpServerUp(ip, port, 5000)) return;
         ConsoleColorPrinter.WriteLine("Test success!", ConsoleColor.DarkYellow);

         // Выключим сервер
         server.Shutdown();
      }
   }
}
 
 
 