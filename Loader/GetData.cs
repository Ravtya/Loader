using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace Loader
{
    public static class GetData
    {
        public static string RootPath = new Uri(Utils.ServerPath.TrimEnd('/')).GetLeftPart(UriPartial.Authority);

        /// <summary>
        /// Файлы на сервере
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static IEnumerable<FileData> GetServerFilesData(string baseUrl)
        {
            var htmlText = GetHtmlText(baseUrl);

            //html разделяется по тегу <br>, пропускаются заголовки и последняя строка
            var list = htmlText.Split(new string[] { "<br>" }, StringSplitOptions.None).Skip(2).Where(n => !n.Contains("</pre>"));

            foreach (var item in list)
            {
                //Путь без имени сервера
                var relativePath = item.Cut("HREF=\"", "\">");

                //Директории
                if (item.Contains("&lt;dir&gt;"))
                {
                    var folderFiles = GetServerFilesData(RootPath + relativePath);
                    foreach (var file in folderFiles)
                        yield return file;
                    continue;
                }

                //Дата изменения
                var dateText = item.Cut(endString: "M", includeLastSymbol: true);
                var date = DateTime.Parse(dateText, new CultureInfo("en-US"));

                yield return new FileData(RootPath + relativePath, date.RecreateDate());
            }
        }

        /// <summary>
        /// HTML текст на сервере
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetHtmlText(string path)
        {
            var request = WebRequest.Create(path);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
                return reader.ReadToEnd();
        }

        /// <summary>
        /// Локальные файлы
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FileData> GetLocalFilesData()
        {
            var dir = new DirectoryInfo(Utils.LocalPath);
            var files = dir.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var file in files)
                yield return new FileData(file.FullName, file.LastWriteTime.RecreateDate());
        }
    }
}
