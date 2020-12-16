using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace WebServer.Network.HelpfulStaff

{
    /// <summary>
    /// Статический класс для проверки, является ли строка ipv4 адресом.
    /// </summary>
    static class IpHelper
    {
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