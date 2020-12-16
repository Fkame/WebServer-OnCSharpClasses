using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using WebServer.HelpfulStaff;

namespace WebServer.Network.HelpfulStaff
{
    /// <summary>
    /// Статический класс, облегчающий вывод информации из HTTP запроса
    /// </summary>
    public static class HttpPrinterHelper
    {
        public static ConsoleColor NameColor {get; set;} = ConsoleColor.Red;
        public static ConsoleColor ValueColor {get; set;} = ConsoleColor.DarkGreen;
        public static ConsoleColor HeadersNamesColor {get; set; } = ConsoleColor.DarkYellow;
        public static ConsoleColor HeadersValuesColor {get; set; } = ConsoleColor.DarkRed;

        public static void PrintFullHttpRequestText(HttpListenerRequest request)
        {
            string[] names = {
                "IsSecureConnection=",
                "UserHostAddress=",
                "UserAgent=",
                "Url=",
                "TransportContext=",
                "ServiceName=",
                "RawUrl=",
                "ProtocolVersion=",
                "UserHostName=",
                "QueryString=",
                "UserLanguages=",
                "HttpMethod=",
                "UrlReferrer=",
                "Content-Type=",
                "Content-Encoding="
            };

            string[] values = {
                $"{request.IsSecureConnection}",
                $"{request.UserHostAddress}",
                $"{request.UserAgent}",
                $"{request.Url.OriginalString}",
                $"{request.TransportContext}",
                $"{request.ServiceName}",
                $"{request.RawUrl}",
                $"{request.ProtocolVersion}",
                $"{request.UserHostName}",
                $"{request.QueryString}",
                $"{string.Join(' ', request.UserLanguages)}",
                $"{request.HttpMethod}",
                $"{request.UrlReferrer}",
                $"{request.ContentType}",
                $"{request.ContentEncoding}"
            };

            for (int i = 0; i < names.Length; i++)
            {
                ConsoleColorPrinter.Write(names[i], NameColor);
                ConsoleColorPrinter.WriteLine(values[i], ValueColor);
            }
            
            HttpPrinterHelper.PrintFullHeaders(request);
        }

        public static void PrintFullHeaders(HttpListenerRequest request)
        {
            ConsoleColorPrinter.Write($"Headers=\n", NameColor);
            System.Collections.Specialized.NameValueCollection headers = request.Headers;
            foreach (string key in headers.AllKeys)
            {
                ConsoleColorPrinter.WriteLine($"\t{key}:", HeadersNamesColor);
                string[] textOfKey = headers.GetValues(key);
                if (textOfKey.Length > 0)
                {
                    foreach (string text in textOfKey)
                    {
                        ConsoleColorPrinter.WriteLine($"\t\t{text}", HeadersValuesColor);
                    }
                }
                else 
                {
                    ConsoleColorPrinter.WriteLine("\t\tThere is no value associated with the header.", HeadersValuesColor);
                }
            }
        }

        public static void PrintMinimalHttpInfo(HttpListenerRequest request)
        {
            string[] names = {
                "UserHostAddress=",
                "Url=", 
                "RawUrl=",
                "ProtocolVersion=",      
                "HttpMethod=",  
                "Content-Type=",
                "Content-Encoding="
            };

            string[] values = {
                $"{request.UserHostAddress}",
                $"{request.Url.OriginalString}",
                $"{request.RawUrl}",
                $"{request.ProtocolVersion}",
                $"{request.HttpMethod}",
                $"{request.ContentType}",
                $"{request.ContentEncoding}"
            };

            for (int i = 0; i < names.Length; i++)
            {
                ConsoleColorPrinter.Write(names[i], NameColor);
                ConsoleColorPrinter.WriteLine(values[i], ValueColor);
            }
            Console.WriteLine();
        }

        public static void PrintRequestInfoByType(HttpListenerRequest request, ShowInfoType amountOfInfo)
        {
            switch(amountOfInfo)
            {
                case ShowInfoType.ShowFullInfo:
                    PrintFullHttpRequestText(request);
                    break;
                case ShowInfoType.ShowMinimalInfo:
                    PrintMinimalHttpInfo(request);
                    break;
                default:
                    break;
            }
        }
    }
}