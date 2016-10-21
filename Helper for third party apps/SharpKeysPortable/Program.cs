namespace SharpKeysPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
                Ini.File($"%CurDir%\\{Path.GetFileNameWithoutExtension(PathEx.LocalPath)}.ini");
                if (Ini.ReadBoolean("Settings", "ShowWarning"))
                {
                    Reg.CreateNewSubKey(Reg.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys");
                    Reg.WriteValue(Reg.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys", "ShowWarning", "1", Reg.RegValueKind.DWord);
                }
                using (var p = ProcessEx.Start("%CurDir%\\SharpKeys\\SharpKeys.exe", true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                Ini.Write("Settings", "ShowWarning", true);
                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys");
                if (Reg.GetSubKeys(Reg.RegKey.CurrentUser, "Software\\RandyRants").Count <= 0)
                    Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\RandyRants");
            }
        }
    }
}
