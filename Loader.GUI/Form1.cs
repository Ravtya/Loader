using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Loader.GUI
{
    public partial class Form1 : Form
    {
        private string DestPath;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void BtnRun_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DestPath) || string.IsNullOrEmpty(textBoxExeFile.Text) || string.IsNullOrEmpty(textBoxSource.Text))
                throw new Exception("Заполните все поля");

            ChangeConfigFile();
            CopyLoader(DestPath);

            Application.ApplicationExit += Application_ApplicationExit;

            Application.Exit();
        }

        /// <summary>
        /// Выбор папки назначения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDest.Text = folderBrowserDialog1.SelectedPath;
                DestPath = folderBrowserDialog1.SelectedPath;
            }
        }

        /// <summary>
        /// Запуск лоадера из папки назначения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            var process = new ProcessStartInfo(Path.Combine(DestPath, "Loader.exe"));
            Process.Start(process);
        }

        /// <summary>
        /// Копирование самого лоадера в путь назначения
        /// </summary>
        /// <param name="destFolder"></param>
        private void CopyLoader(string destFolder)
        {
            File.Copy("Loader.exe", Path.Combine(destFolder, "Loader.exe"), true);
            File.Copy("Loader.exe.config", Path.Combine(destFolder, "Loader.exe.config"), true);
        }

        /// <summary>
        /// Запись параметров в файл настроек
        /// </summary>
        private void ChangeConfigFile()
        {
            var doc = XDocument.Load("Loader.exe.config");

            var exeNameElement = doc.Descendants().Where(x => (string)x.Parent?.Attribute("name") == "ExeName").FirstOrDefault();
            var serverPathElement = doc.Descendants().Where(x => (string)x.Parent?.Attribute("name") == "ServerPath").FirstOrDefault();

            if (exeNameElement != null)
                exeNameElement.Value = textBoxExeFile.Text;

            if (serverPathElement != null)
                serverPathElement.Value = textBoxSource.Text;

            doc.Save("Loader.exe.config");
        }
    }
}
