using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HeidiSQLPortable  // BETA
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
                    string PortableProfile = SilDev.Run.EnvVarFilter("%CurrentDir%\\Data");
                    if (!Directory.Exists(PortableProfile))
                        Directory.CreateDirectory(PortableProfile);
                    string UserProfile = SilDev.Run.EnvVarFilter("%AppData%\\HeidiSQL");
                    SilDev.Data.DirLink(UserProfile, PortableProfile, true);
                    SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\HeidiSQL\\heidisql.exe" }, 0);
                    SilDev.Data.DirUnLink(UserProfile, true);
                }
            }
        }
    }
}
