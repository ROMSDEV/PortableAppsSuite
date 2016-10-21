namespace HeidiSQLPortable // BETA
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var settingsPath = PathEx.Combine("%CurDir%\\HeidiSQL\\portable_settings.txt");
                    if (!File.Exists(settingsPath))
                        File.Create(settingsPath).Close();
                    var portableProfile = PathEx.Combine("%CurDir%\\Data");
                    if (!Directory.Exists(portableProfile))
                        Directory.CreateDirectory(portableProfile);
                    var userProfile = PathEx.Combine("%AppData%\\HeidiSQL");
                    Data.DirLink(userProfile, portableProfile, true);
                    using (var p = ProcessEx.Start("%CurDir%\\HeidiSQL\\heidisql.exe", true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Data.DirUnLink(userProfile, true);
                }
        }
    }
}
