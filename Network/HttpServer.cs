using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebServer.HelpfulStaff;
using WebServer.FileSystem;
using WebServer.Network.HelpfulStaff;

namespace WebServer.Network
{
    /// <summary>
    /// Класс, реализующий веб-сервер, работающий на протоколе http, и соответственно принимающий http-запросы.
    /// Файл без приставки содержит состояния, контсруктор и метод настройки HttpListener.
    /// </summary>
    public partial class HttpServer
    {
        public string IP {get; private set;}
        public int Port {get; private set;}

        /// <summary>
        /// Данный объект выполняет ряд важных задач по работе с файловой системой:
        /// 1.Он инкапсулирует в себе контейнер, который хранит в себе буферезированные файлы (то есть загружает их в оперативную память).
        /// Эта штука нужна, чтобы при отправке файла по http запросу не читать его с диска, тем самым, скорее всего, производительность
        /// будет выше.
        /// 2. Объект запускает FileSystemWatcher, который отслеживает изменеия в переданной директории и сообщает о них.
        /// 3. Объект реагирует на изменения, и соответствующе обновляет буфер. Это нужно, чтобы можно было подправить сайт, не выключая сервер.
        /// 4. TODO--> Нужно настроить сихнронизацию доступа к буферу файлов, иначе возможна отправка устаревшей информации. И возникновение ошибок.
        /// Скорее всего будет достаточно что-то типа монитора при изменениии.
        /// </summary>
        private DirectoryAnalyser DirectoryWorker;

        private HttpListener httpListener;

        public ShowInfoType LogRequestTextes {get; set;} = ShowInfoType.ShowMinimalInfo;

        public ShowInfoType LogResponseTextes {get; set;} = ShowInfoType.ShowNothing;

        private HttpUriHelper uriHelper;

        public HttpServer(string ip, int port, DirectoryInfo directory)
        {
            this.IP = ip;
            this.Port = port;
            uriHelper = new HttpUriHelper(ip, port);

            this.DirectoryWorker = new DirectoryAnalyser(directory);

            // Добавим обработчик события создания нового файла
            this.DirectoryWorker.NotifyAboutCreate += OnNewFile;

            // Добавим обработчик события удаления файла
            this.DirectoryWorker.NotifyAboutDelete += OnDeleteFile;
        }

        /// <summary>
        /// Создаётся HttpListener, после чего заполняются его .Prefixes ссылками на директории.
        /// </summary>
        /// <returns></returns>
        private HttpListener GetConfiguredListener()
        {
            HttpListener httpListener = new HttpListener();

            // Добавляем url верхнего уровня в список префиксов.
            foreach (string url in uriHelper.UrlBeginings)
            {
                httpListener.Prefixes.Add(String.Format("{0}/",url));
            }

            // Ищем только директории, содержание файлы *.html, после чего меняем разделитель с '\' на '/'.
            List<string> prefixes = DirectoryWorker.GetDirectoriesWithDotHTML();
            prefixes = HttpUriHelper.ChangeSeparatorToUrlLike(prefixes);

            // Добавляем отобранные директории к url верхнего уровня и закидываем в список префиксов.
            foreach(string path in prefixes)
            {
                foreach (string startUrl in uriHelper.UrlBeginings)
                {
                    httpListener.Prefixes.Add(String.Format("{0}{1}",startUrl, path));
                }
            }

            // Вывод всех добавленных префиксов
            this.PrintPrefixes(httpListener);
            
            return httpListener;
        }
    }
}
