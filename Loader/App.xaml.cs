using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Loader
{
    public partial class App : Application
    {
        private BackgroundWorker bgWorker;
        private LoaderWindow lWin;
        private bool startApp = true;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //Новый лог для каждого запуска
                var logPath = Path.Combine(Utils.LocalPath, "Loader.log");
                if (File.Exists(logPath))
                    File.Delete(logPath);

                Utils.WriteLog("Loader started.");

                lWin = new LoaderWindow();
                lWin.Closed += new EventHandler(LWin_Closed);
                lWin.Topmost = true;
                lWin.Show();

                bgWorker = new BackgroundWorker();
                bgWorker.DoWork += new DoWorkEventHandler(BgWorker_DoWork);
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BgWorker_RunWorkerCompleted);
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                HandleTerminalError(ex);
            }
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lWin.Close();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Updater.Update();
                Shortcut.CreateShortcut();
            }
            catch (Exception ex)
            {
                HandleTerminalError(ex);
            }
        }

        private void LWin_Closed(object sender, EventArgs e)
        {
            Shutdown();
            Utils.WriteLog("Loader finished.");

            if (startApp)
                StartApplication();
        }

        private void StartApplication()
        {
            try
            {
                Process.Start(Path.Combine(Utils.LocalPath, Utils.ExeFile));
            }
            catch (Exception e)
            {
                HandleTerminalError(e);
            }
        }

        private void HandleTerminalError(Exception ex)
        {
            startApp = false;
            var errMsg = ex.Message + Environment.NewLine
                + (ex.InnerException != null ? ex.InnerException.Message + Environment.NewLine : "")
                + ex.StackTrace;

            Utils.WriteLog(errMsg);
            MessageBox.Show(ex.Message);
            Shutdown();
        }
    }
}
