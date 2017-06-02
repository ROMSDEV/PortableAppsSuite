namespace SharpKeysPortable
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Win32;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;
                Ini.SetFile(Path.ChangeExtension(PathEx.LocalPath, ".ini"));
                if (Ini.Read("Settings", "ShowWarning", false))
                    Reg.Write("HKCU\\Software\\RandyRants\\SharpKeys", "ShowWarning", 1, RegistryValueKind.DWord);
                using (var p = ProcessEx.Start("%CurDir%\\SharpKeys\\SharpKeys.exe", true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                Ini.Write("Settings", "ShowWarning", true);
                Reg.RemoveSubKey("HKCU\\Software\\RandyRants\\SharpKeys");
                if (Reg.GetSubKeys("HKCU\\Software\\RandyRants").Any())
                    Reg.RemoveSubKey("HKCU\\Software\\RandyRants");
            }
        }
    }
}
