using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace WebServer.HelpfulStaff
{
    /// <summary>
    /// Статический класс, содержаий агрументы по умолчанию.
    /// Под аргументами в данном случае имеются ввиду не столько агрументы, передаваемые при запуске программы,
    /// сколько аргументы, передаваемые в конструктор HttpServer.
    /// </summary>
    public static class DefaultArguments
    {
        /// <summary>
        /// Порт по умолчанию
        /// </summary>
        public readonly static int HttpPort = 80;

        /// <summary>
        /// IP-адрес по умолчанию
        /// </summary>
        /// <returns></returns>
        public static string LocalIp {get { return DefaultArguments.GetLocalIp(); }}

        /// <summary>
        /// Папка с сайтом по умолчанию
        /// </summary>
        /// <returns></returns>
        public static readonly string DefaultDirectory = Path.Join(Environment.CurrentDirectory, "WebSite");

        /// <summary>
        /// Метод получает из класса Dns локальный ip адрес компьютера, на котором запускается сервер
        /// </summary>
        /// <returns></returns>
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
    }
}
