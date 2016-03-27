using System;
using System.Diagnostics;

namespace FurMarkPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\FurMark\\FurMark.exe" });
        }
    }
}
