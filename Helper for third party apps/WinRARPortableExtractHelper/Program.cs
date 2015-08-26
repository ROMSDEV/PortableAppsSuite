using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinRARPortableExtractHelper
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    string extractorPath = string.Empty;
                    foreach (string arg in Environment.GetCommandLineArgs())
                    {
                        if (arg.ToLower().Contains(Application.ExecutablePath.ToLower()) || !File.Exists(arg))
                            continue;
                        if (string.IsNullOrWhiteSpace(extractorPath))
                        {
                            foreach (string f in Directory.GetFiles(Application.StartupPath, "WinRAR.exe", SearchOption.AllDirectories))
                            {
                                extractorPath = f;
                                break;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(extractorPath))
                            return;
                        string file = arg;
                        string dir = Path.Combine(Path.GetDirectoryName(arg), Path.GetFileNameWithoutExtension(arg));
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        SilDev.Run.App(new ProcessStartInfo() { Arguments = string.Format("x \"{0}\"", arg), FileName = extractorPath, WorkingDirectory = dir });
                    }
                }
            }
        }
    }
}
