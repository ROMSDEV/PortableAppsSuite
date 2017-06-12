namespace mpTrimPortable
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Win32;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                var appPath = PathEx.Combine(PathEx.LocalDir, "App\\mpTrim\\mpTrim.exe");
                if (!File.Exists(appPath))
                    return;

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                const string regKey = "HKCU\\Software\\mpTrim";
                Helper.RegForwarding(Helper.Options.Start, regKey);

                var regPath = PathEx.Combine(PathEx.LocalDir, "Data\\settings.reg");
                if (!File.Exists(regPath))
                {
                    var screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                    const int windowWidth = 335;
                    var windowLeft = (int)Math.Round(screenWidth / 2d - windowWidth / 2d);
                    Reg.Write(regKey, "MainFormLeft", windowLeft, RegistryValueKind.DWord);

                    var screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
                    const int windowHeight = 410;
                    var windowTop = (int)Math.Round(screenHeight / 2d - windowHeight / 2d);
                    Reg.Write(regKey, "MainFormTop", windowTop, RegistryValueKind.DWord);
                }

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.RegForwarding(Helper.Options.Exit, regKey);
            }
        }
    }
}
