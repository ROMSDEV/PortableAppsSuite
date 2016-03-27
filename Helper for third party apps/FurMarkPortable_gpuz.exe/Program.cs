using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace gpuz.exe
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string file = Path.GetFullPath($"{Application.StartupPath}\\..\\..\\..\\GPU-ZPortable\\GPU-ZPortable.exe");
            if (File.Exists(file))
                Process.Start(file);
        }
    }
}
