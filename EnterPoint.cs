using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

using WebServer.HelpfulStaff;

namespace WebServer
{
    class EnterPoint
    {
        static void Main(string[] args)
        {
            string ip = IpHelper.GetLocalIp();
            int port = 80;
            if (args.Length == 2)
            {
                if (IpHelper.IsIPv4(args[0])) ip = args[0];
                int value = 0;
                if (Int32.TryParse(args[1], out value) & value > 0) port = value;
            }
            
            WebServer ws = new WebServer(ip, port);
            ws.Start();
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
