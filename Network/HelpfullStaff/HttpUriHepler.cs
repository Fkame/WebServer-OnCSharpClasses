using System;
using System.IO;
using System.Collections.Generic;

namespace WebServer.Network.HelpfulStaff
{
    /// <summary>
    /// Инкапсулирует операции с проверкой-адаптацией URL и разделителя для записи путей к каталогам в ОС.
    /// </summary>
    public class HttpUriHelper
    {
        /// <summary>
        /// Url верхнего уровня, то есть без каких-либо подкаталогов.
        /// Включают 2 вида: по заданному ip и localhost.
        /// </summary>
        /// <value></value>
        public string[] UrlBeginings { get; private set; }

        /// <summary>
        /// Константный разделитель, используемый в URL
        /// </summary>
        public static readonly char UrlPathChar = '/';

        public HttpUriHelper(string ip, int port)
        {
            UrlBeginings = new string[2];
            UrlBeginings[0] = String.Format("http://{0}:{1}", ip, port);
            UrlBeginings[1] = String.Format("http://localhost:{0}", port);
        }

        /// <summary>
        /// Присоединяет путь к осноному Url. Если путь использует не Url-разделители, метод произведёт замену.
        /// Также метод проверит, чтобы путь заканчивался на /.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string[] ConnectBeginigsWithPath(string path)
        {
            if (path.IndexOf(Path.DirectorySeparatorChar.ToString()) != -1)
            {
                path = ChangeSeparatorToUrlLike(path);
            }

            if (!path.EndsWith(UrlPathChar)) path = String.Format("{0}/", path);
            if (!path.StartsWith(UrlPathChar)) path = String.Format("/{0}", path);

            string[] paths = new string[UrlBeginings.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = String.Format("{0}{1}", UrlBeginings[i], path);
            }

            return paths;
        }

        /// <summary>
        /// В поданом на вход пути заменяет разделители стиля ОС на разделители, используемые в URL.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ChangeSeparatorToUrlLike(string path)
        {
            char s = Path.DirectorySeparatorChar;
            return path.Replace(s, UrlPathChar);
        }

        /// <summary>
        /// Меняет разделители, используемые в URL на разделители, используемые в текущей ОС.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ChangeUrlLikeToSeparator(string path)
        {
            char s = Path.DirectorySeparatorChar;
            return path.Replace(UrlPathChar, s);
        }

        /// <summary>
        /// В поданом на вход пути заменяет разделители стиля ОС на разделители, используемые в URL.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> ChangeSeparatorToUrlLike(List<string> paths)
        {
            List<string> pathUrlLike = new List<string>();
            for (int i = 0; i < paths.Count; i++)
            {
                pathUrlLike.Add(ChangeSeparatorToUrlLike(paths[i]));
            }
            return pathUrlLike;
        }

    }
}