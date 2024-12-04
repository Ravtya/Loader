using System.IO;
using System.Linq;
using System.Net;

namespace Loader
{
    public static class Updater
    {
        /// <summary>
        /// Главный метод
        /// </summary>
        public static void Update()
        {
            var serverFiles = GetData.GetServerFilesData(Utils.ServerPath).ToList();
            var localFiles = GetData.GetLocalFilesData().ToList();

            //Измененные и недостающие файлы на сервере
            var toDownload = serverFiles
                .Where(y => localFiles.Any(z => z.Name == y.Name && z.ModifiedDate != y.ModifiedDate) || !localFiles.Select(n => n.Name).Contains(y.Name))
                .ToList();

            foreach (var file in toDownload)
            {
                //Если файл в подкаталоге, создаем этот каталог
                if (file.Name.Contains('\\'))
                    Utils.CreateFolder(file.Name);

                //Загрузка файла
                DownloadFile(file);

                //Проставление верной даты последнего изменения
                File.SetLastWriteTime(Path.Combine(Utils.LocalPath, file.Name), file.ModifiedDate);

                Utils.WriteLog($"{file.Name} downloaded");
            }

            //Удаление лишних файлов и папок
            var toRemove = localFiles.Select(n => n.Name).Except(serverFiles.Select(n => n.Name)).ToList();
            Utils.DeleteUnusedFiles(toRemove);
        }

        /// <summary>
        /// Загрузка файла с сервера
        /// </summary>
        /// <param name="file"></param>
        private static void DownloadFile(FileData file)
        {
            var request = WebRequest.Create(file.FullPath);
            using (var response = request.GetResponse())
            using (var q = response.GetResponseStream())
            using (var destStream = File.Create(Utils.LocalPath + file.Name))
                q.CopyTo(destStream);
        }
    }
}
