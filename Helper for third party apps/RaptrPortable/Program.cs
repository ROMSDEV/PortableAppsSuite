using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RaptrPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();

                    string defaultAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Raptr");
                    string appDir = Path.Combine(Application.StartupPath, "App\\Raptr"); ;
                    if (isRunning(appDir, "raptr"))
                        killAll(appDir, "raptr");
                    DirLinkHelper(defaultAppPath, appDir);

                    string defaultProfilePath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Raptr");
                    string profilePath = Path.Combine(Application.StartupPath, "Data\\Profile");
                    DirLinkHelper(defaultProfilePath, profilePath);

                    string defaultScreensPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Raptr Screenshots");
                    string screensPath = Path.Combine(Application.StartupPath, "Data\\Screenshots");
                    DirLinkHelper(defaultScreensPath, screensPath);

                    string defaultVidsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Raptr");
                    string vidsPath = Path.Combine(Application.StartupPath, "Data\\Videos");
                    DirLinkHelper(defaultVidsPath, vidsPath);

                    string regKey = Path.Combine(Application.StartupPath, "Data\\settings.reg");
                    SilDev.Reg.ImportFile(regKey);
                    SilDev.Reg.WriteValue("HKCU\\Software\\Raptr\\Overlay", "InstallationDirectory", appDir);
                    SilDev.Reg.WriteValue("HKLM\\SOFTWARE\\Raptr", "Install_Dir", appDir);

                    SilDev.Run.App(Application.StartupPath, "App\\Raptr\\raptrstub.exe", 0);
                    while (isRunning(appDir, null)) 
                        Thread.Sleep(200);
                    Thread.Sleep(15000);
                    while (isRunning(appDir, null))
                        Thread.Sleep(200);

                    SilDev.Reg.ExportFile("HKCU\\Software\\Raptr", regKey);
                    SilDev.Reg.RemoveExistSubKey("HKCU\\Software\\Raptr");
                    SilDev.Reg.RemoveExistSubKey("HKLM\\SOFTWARE\\Raptr");
                    SilDev.Reg.RemoveExistSubKey("HKCR\\raptr");

                    DirUnLinkHelper(defaultAppPath);
                    DirUnLinkHelper(defaultProfilePath);
                    DirUnLinkHelper(defaultScreensPath);
                    DirUnLinkHelper(defaultVidsPath);
                }
            }
        }

        #region HELPER

        static bool isRunning(string _path, string _match)
        {
            try
            {
                foreach (string f in Directory.GetFiles(_path, "*.exe", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    if ((string.IsNullOrEmpty(_match) || !string.IsNullOrEmpty(_match) && name.Contains(_match)) && Process.GetProcessesByName(name).Length > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "RaptrPortable.Program.isRunning");
                return false;
            }
        }

        static void killAll(string _path, string _match)
        {
            try
            {
                foreach (string f in Directory.GetFiles(_path, "*.exe", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    if (!name.Contains(_match))
                        continue;
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(f)))
                    {
                        p.CloseMainWindow();
                        p.WaitForExit(100);
                        if (!p.HasExited)
                            p.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "RaptrPortable.Program.killAll");
            }
        }

        static void DirLinkHelper(string _src, string _dest)
        {
            if (!Directory.Exists(_dest))
                Directory.CreateDirectory(_dest);
            if (Directory.Exists(_dest))
            {
                if (Directory.Exists(_src))
                {
                    if (!SilDev.Data.DirIsLink(_src))
                    {
                        Directory.Move(_src, string.Format("{0}.SI13N7-BACKUP", _src));
                        if (Directory.Exists(_src))
                            Directory.Delete(_src, true);
                    }
                    else
                        SilDev.Data.DirUnLink(_src);
                }
                SilDev.Data.DirLink(_dest, _src);
            }
        }

        static void DirUnLinkHelper(string _path)
        {
            if (Directory.Exists(string.Format("{0}.SI13N7-BACKUP", _path)))
            {
                if (Directory.Exists(_path))
                    Directory.Delete(_path, true);
                Directory.Move(string.Format("{0}.SI13N7-BACKUP", _path), _path);
            }
            SilDev.Data.DirUnLink(_path);
        }

        #endregion
    }
}
