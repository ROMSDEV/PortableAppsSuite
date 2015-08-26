using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace gpuz.exe
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string file = Path.GetFullPath(string.Format("{0}\\..\\..\\..\\GPU-ZPortable\\GPU-ZPortable.exe", Application.StartupPath));
            if (File.Exists(file))
                Process.Start(file);
        }
    }
}
