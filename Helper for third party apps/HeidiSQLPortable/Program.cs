using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HeidiSQLPortable
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
                    string settingsPath = Path.Combine(Application.StartupPath, "HeidiSQL\\portable_settings.txt");
                    if (!File.Exists(settingsPath))
                        File.Create(settingsPath).Close();
                    string appdataPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "HeidiSQL");
                    string dataPath = Path.Combine(Application.StartupPath, "HeidiSQL\\data");
                    if (Directory.Exists(dataPath))
                    {
                        if (Directory.Exists(appdataPath))
                            Directory.Delete(appdataPath);
                        Directory.Move(dataPath, appdataPath);
                    }
                    SilDev.Run.App(Application.StartupPath, "HeidiSQL\\heidisql.exe", 0);
                    if (Directory.Exists(appdataPath))
                    {
                        if (Directory.GetFiles(appdataPath, "*", SearchOption.AllDirectories).Length > 0)
                        {
                            if (Directory.Exists(dataPath))
                            {
                                string backupPath = string.Format("{0}.BACKUP", dataPath);
                                if (Directory.Exists(backupPath))
                                    Directory.Delete(backupPath);
                                Directory.Move(dataPath, backupPath);
                            }
                            Directory.Move(appdataPath, dataPath);
                        }
                        else
                            Directory.Delete(appdataPath, true);
                    }
                }
                else
                    Environment.Exit(2);
            }
        }
    }
}
