namespace ProcessRun
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
                var file = PathEx.Combine("%CurDir%\\WinSize2.EXE");
                if (!File.Exists(file))
                    return;
                try
                {
                    for (var i = 0; i < 100; i++)
                    {
                        using (var p = ProcessEx.Start(PathEx.Combine("%CurDir%", file), true, false))
                            if (!p?.HasExited == true)
                                p?.WaitForExit(100);
                        Thread.Sleep(100);
                        foreach (var p in Process.GetProcessesByName("WinSize2"))
                        {
                            if (p.MainWindowHandle == IntPtr.Zero)
                                continue;
                            var title = new StringBuilder(32);
                            IntPtr result;
                            if ((int)WinApi.UnsafeNativeMethods.SendMessageTimeoutText(p.MainWindowHandle, 0xd, (UIntPtr)32, title, 0x2, 0xc8, out result) <= 0)
                                continue;
                            if (!title.ToString().ContainsEx("WinSize2"))
                                continue;
                            p.CloseMainWindow();
                            p.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}
