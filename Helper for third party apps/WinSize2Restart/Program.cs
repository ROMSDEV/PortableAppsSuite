namespace ProcessRun
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using SilDev;

    internal static class Program
    {
        private static readonly string FilePath = PathEx.Combine("%CurDir%\\WinSize2.EXE");

        [STAThread]
        private static void Main(string[] args)
        {
            Log.AllowLogging();
            if (!File.Exists(FilePath) || args.Length <= 0)
                return;
            try
            {
                if (Process.GetProcessesByName("WinSize2").Length > 0)
                    foreach (var p in Process.GetProcessesByName("WinSize2"))
                        p.Kill();
                foreach (var arg in args)
                    foreach (var p in Process.GetProcessesByName(arg))
                        p.WaitForExit();
            }
            catch
            {
                // ignored
            }
            finally
            {
                Run();
            }
        }

        private static void Run()
        {
            if (Process.GetProcessesByName("WinSize2").Length > 0)
                return;
            ProcessEx.Start(FilePath);
        }
    }
}
