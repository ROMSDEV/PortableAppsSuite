using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinampPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Initialization.File(Application.StartupPath, "winamp\\Winamp.ini");
            SilDev.Initialization.WriteValue("Winamp", "no_registry", 0);
            SilDev.Initialization.WriteValue("WinampReg", "NeedReg", 0);
            SilDev.Run.App(Application.StartupPath, "winamp\\winamp.exe", Environment.CommandLine.Replace(string.Format("\"{0}\"", Application.ExecutablePath), string.Empty));
        }
    }
}
