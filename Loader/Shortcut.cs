using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Loader
{
    public static class Shortcut
    {
        /// <summary>
        /// Создания ярлыка
        /// </summary>
        public static void CreateShortcut()
        {
            if (CheckIfShortсutExists()) return;

            GetIcon();

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{GetShortcutName()}.lnk");
            var shell = new IWshRuntimeLibrary.WshShell();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(path);
            shortcut.Description = $"Расположение: Loader ({Utils.LocalPath})";
            shortcut.TargetPath = Path.Combine(Utils.LocalPath, "Loader.exe");
            shortcut.IconLocation = Path.Combine(Utils.LocalPath, "Loader.ico");
            shortcut.Save();
            Utils.WriteLog($"Shortcut created {shortcut.FullName}");
        }

        /// <summary>
        /// Проверка на наличие ярлыка нашего лоадера
        /// </summary>
        /// <returns></returns>
        private static bool CheckIfShortсutExists()
        {
            var list = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)).GetFiles()
               .Concat(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)).GetFiles())
               .Distinct().Where(n => n.Extension == ".lnk");
            var shell = new IWshRuntimeLibrary.WshShell();

            foreach (var file in list)
            {
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(file.FullName);
                if (shortcut.TargetPath.Contains(Assembly.GetEntryAssembly().Location))
                {
                    //Utils.WriteLog($"Shortcut exists: {shortcut.FullName}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Получение иконки exe-файла в нормальном качестве
        /// </summary>
        private static void GetIcon()
        {
            try
            {
                var source = Path.Combine(Utils.LocalPath, Utils.ExeFile);
                using (var s = File.Create(Path.Combine(Utils.LocalPath, "Loader.ico")))
                    IconExtractor.Extract1stIconTo(source, s);
                Utils.WriteLog($"Icon created from {Utils.ExeFile}");
            }
            catch (Exception)
            {
                Utils.WriteLog($"Unable to create icon from {Utils.ExeFile}");
            }
        }

        /// <summary>
        /// Имя ярылка(Assembly - Title exe-файла)
        /// </summary>
        /// <returns></returns>
        private static string GetShortcutName()
        {
            var assembly = Assembly.LoadFile(Path.Combine(Utils.LocalPath, Utils.ExeFile));
            var appTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false))?.Title;
            return appTitle ?? (Utils.ExeFile.Substring(Utils.ExeFile.Length - 3));
        }
    }
}
