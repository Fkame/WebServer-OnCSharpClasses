using System;
using System.IO;
using System.Collections.Generic;

namespace WebServer.Network.HelpfulStaff
{
    public class HttpUriHelper
    {
        public string[] UrlBeginings { get; private set; }

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

        public static string ChangeSeparatorToUrlLike(string path)
        {
            char s = Path.DirectorySeparatorChar;
            return path.Replace(s, UrlPathChar);
        }

        public static string ChangeUrlLikeToSeparator(string path)
        {
            char s = Path.DirectorySeparatorChar;
            return path.Replace(UrlPathChar, s);
        }

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