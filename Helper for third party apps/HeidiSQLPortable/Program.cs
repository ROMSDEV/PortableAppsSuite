using SilDev;
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
                    string settingsPath = PATH.Combine("%CurDir%\\HeidiSQL\\portable_settings.txt");
                    if (!File.Exists(settingsPath))
                        File.Create(settingsPath).Close();
                    string PortableProfile = PATH.Combine("%CurDir%\\Data");
                    if (!Directory.Exists(PortableProfile))
                        Directory.CreateDirectory(PortableProfile);
                    string UserProfile = PATH.Combine("%AppData%\\HeidiSQL");
                    DATA.DirLink(UserProfile, PortableProfile, true);
                    RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\HeidiSQL\\heidisql.exe" }, 0);
                    DATA.DirUnLink(UserProfile, true);
                }
            }
        }
    }
}
