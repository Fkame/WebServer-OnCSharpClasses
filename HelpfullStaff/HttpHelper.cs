using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace WebServer.HelpfulStaff
{
    public static class HttpHelper
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

            #region Старая реализация
            /*
            ConsoleColorPrinter.Write($"IsSecureConnection={request.IsSecureConnection}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"UserHostAddress={request.UserHostAddress}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"UserAgent={request.UserAgent}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"Url={request.Url.OriginalString}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"TransportContext={request.TransportContext}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"ServiceName={request.ServiceName}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"RawUrl={request.RawUrl.ToString()}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"ProtocolVersion={request.ProtocolVersion}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"UserHostName={request.UserHostName}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"QueryString={request.QueryString}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"UserLanguages={string.Join(' ', request.UserLanguages)}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"HttpMethod={request.HttpMethod}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"UrlReferrer={request.UrlReferrer}\n", ConsoleColor.Red);
            ConsoleColorPrinter.Write($"Headers=\n", ConsoleColor.Red);
            */
            #endregion

            HttpHelper.PrintHeaders(request);
        }

        public static void PrintHeaders(HttpListenerRequest request)
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
    }
}