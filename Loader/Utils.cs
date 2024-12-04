using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Loader
{
    public static class Utils
    {
        /// <summary>
        /// Локальный путь
        /// </summary>
        public static string LocalPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Путь на сервере
        /// </summary>
        public static string ServerPath = Properties.Settings.Default.ServerPath;

        /// <summary>
        /// Исполняемый файл
        /// </summary>
        public static string ExeFile = Properties.Settings.Default.ExeName;


        /// <summary>
        /// Запись в лог
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(string msg)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InstalledUICulture);
            Trace.WriteLine($"{time}: {msg}");
            Trace.Flush();
        }

        /// <summary>
        /// Создание директории если не существует
        /// </summary>
        /// <param name="fileName"></param>
        public static void CreateFolder(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(Path.Combine(LocalPath, directory));
            }
        }

        /// <summary>
        /// Удаление ненужных файлов
        /// </summary>
        public static void DeleteUnusedFiles(List<string> filesForDel)
        {
            try
            {
                foreach (var file in filesForDel)
                {
                    //Лоадеровский конфиг, екзешник, иконку и логи не трогаем. Не file.Contains("Loader.") потому что мало ли софтина будет называться *.Loader.
                    if (file == "Loader.exe" || file == "Loader.exe.config" || file == "Loader.ico" || file == "Loader.log")
                        continue;

                    File.Delete(Path.Combine(LocalPath, file));
                    WriteLog($"{file} file deleted.");
                }
                DeleteDirectories(LocalPath);
            }
            catch (Exception)
            {
                WriteLog("Error occurred during deleting excess files");
            }
        }

        /// <summary>
        /// Удаление ненужных каталогов
        /// </summary>
        /// <param name="startFolder"></param>
        private static void DeleteDirectories(string startFolder)
        {
            foreach (var directory in Directory.GetDirectories(startFolder))
            {
                DeleteDirectories(directory);
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                    WriteLog($"{directory} directory deleted.");
                }
            }
        }
    }
}
