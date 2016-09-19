using SilDev;
using System;
using System.Diagnostics;
using System.IO;

namespace gpuz.exe
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string path = PATH.Combine("%CurDir%\\..\\..\\..\\GPU-ZPortable\\GPU-ZPortable.exe");
            if (File.Exists(path))
                Process.Start(path);
        }
    }
}
