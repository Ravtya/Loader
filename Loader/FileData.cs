using System;

namespace Loader
{
    public class FileData
    {
        public FileData(string fullPath, DateTime date)
        {
            FullPath = fullPath;
            ModifiedDate = date;
        }

        /// <summary>
        /// Полный путь
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Имя с родительскими директориями
        /// </summary>
        public string Name => FullPath.Replace(FullPath.Contains(Utils.ServerPath) ? Utils.ServerPath : Utils.LocalPath, string.Empty).Replace('/', '\\');

        /// <summary>
        /// Дата изменения
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}