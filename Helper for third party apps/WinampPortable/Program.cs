using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinampPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            SilDev.Initialization.File(Application.StartupPath, "winamp\\Winamp.ini");
            SilDev.Initialization.WriteValue("Winamp", "no_registry", 0);
            SilDev.Initialization.WriteValue("WinampReg", "NeedReg", 0);
            SilDev.Run.App(new ProcessStartInfo()
            {
                Arguments = Environment.CommandLine.Replace($"\"{Application.ExecutablePath}\"", string.Empty),
                FileName = "%CurrentDir%\\winamp\\winamp.exe"
            });
        }
    }
}
