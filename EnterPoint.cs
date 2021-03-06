﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

using WebServer.HelpfulStaff;
using WebServer.Network;
using WebServer.FileSystem;
using WebServer.FileSystem.HelpfulStaff;
using WebServer.Network.HelpfulStaff;
using WebServer.WebServer_Test;

namespace WebServer
{
    /// <summary>
    /// Точка входа в программу. В точке входа проверяются аргументы на валидность, посл
    /// </summary>
    class EnterPoint
    {
        static void Main(string[] args)
        {   
            // Проверка тестового режима
            if (args.Length == 1)
            {
               if (args[0].ToLower().Equals("test"))
                {
                    EnterPoint.RunTests();
                    return;
                } 
            }
                
            // Сперва присвоим переменным значения по умолчанию.
            string ip = DefaultArguments.LocalIp;
            int port = DefaultArguments.HttpPort;
            DirectoryInfo directory = new DirectoryInfo(DefaultArguments.DefaultDirectory);

            // Создание директории по умолчанию на случай, если с ней что-то случилось
            if (!directory.Exists) directory.Create();
            
            // Первый передаваемый аргумент - ip-адрес
            if (args.Length > 0) 
            {
                if (IpHelper.IsIPv4(args[0])) ip = args[0];
                else Console.WriteLine("Incorrect ip ddress! - Changed for default");
            }

            // Второй передаваемый аргумент - порт
            if (args.Length > 1) 
            {
                int value = 0;
                if (Int32.TryParse(args[1], out value) & value > 0) port = value;
                else Console.WriteLine("Incorrect port! - Changed for default");
            }

            // Третий передаваемый аргумент - папка, где располагается сайт
            if (args.Length > 2)
            {
                DirectoryInfo dir = null;
                if (DirectoryHelper.TryDirectory(args[2], out dir) & dir.Exists) directory = dir;
                else Console.WriteLine("Incorrect path to folder or path does not exits! - Changed for default");
            }

            // Создаём и запускаем HttpServer асинхронно
            HttpServer ws = new HttpServer(ip, port, directory);
            ws.StartAsync();

            ConsoleColorPrinter.WriteLine("Tip: Press Ctrl + C to finish program or write \"Stop\" to stop server", ConsoleColor.Magenta); 
            while (Console.ReadLine().ToLower() != "stop") 
            { 
                ConsoleColorPrinter.WriteLine("Unknown command!", ConsoleColor.DarkRed);
            };

            ws.Shutdown();
            ConsoleColorPrinter.WriteLine(">> Server was stopped by command from terminal", ConsoleColor.Magenta); 

        }


        static void RunTests()
        {
            WebServer_Test.PortInitialisation_Test portTest = new WebServer_Test.PortInitialisation_Test();
            //portTest.StartTest();

            WebServer_Test.CreatedUrls_Test urlsTest = new WebServer_Test.CreatedUrls_Test();
            //urlsTest.StartTest();

            WebServer_Test.DirectoryInitialisation_Test directoryTest = new WebServer_Test.DirectoryInitialisation_Test();
            //directoryTest.StartTest();

            WebServer_Test.DynamicUrlAdaptation_Test uriTest = new WebServer_Test.DynamicUrlAdaptation_Test();
            uriTest.StartTest();
        }

        static void Test_IpHelper_IsIPv4()
        {
            string[] ips = new string[6];
            ips[0] = "abs";
            ips[1] = "a.b.s.s.s";
            ips[2] = "255.255.255";
            ips[3] = "0.1.2.255";
            ips[4] = "0.2.-5.100";
            ips[5] = "0.2.5.100.156";
            foreach (string ip in ips)
            {
                Console.WriteLine($"{ip} is {IpHelper.IsIPv4(ip)}");
            }
        }
    }
}
