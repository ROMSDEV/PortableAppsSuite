using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace FurMarkPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Run.App(Application.StartupPath, "FurMark\\FurMark.exe");
        }
    }
}
