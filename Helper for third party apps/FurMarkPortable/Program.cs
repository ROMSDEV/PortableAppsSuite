using System;
using System.Diagnostics;

namespace FurMarkPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\FurMark\\FurMark.exe" });
        }
    }
}
