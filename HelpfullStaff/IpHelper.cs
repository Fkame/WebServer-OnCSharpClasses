using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace WebServer.HelpfulStaff
{
    static class IpHelper
    {
        public static string GetLocalIp()
        {
            string result = string.Empty;
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                    {
                        result = ip.ToString();
                        break;
                    } 
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Message={ex.Message}\nStackTrace={ex.StackTrace}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Message={e.Message}\nStackTrace={e.StackTrace}");
            }
           
            return result;    
        }

        public static bool IsIPv4(string maybeIp)
        {
            string[] oktets = maybeIp.Split('.');
            if (oktets.Length != 4) return false;

            foreach(string oktet in oktets)
            {
                int value = 0;
                if (!Int32.TryParse(oktet, out value)) return false;
                if (value < 0 || value > 255) return false;
            }

            return true;
        }
    }
}