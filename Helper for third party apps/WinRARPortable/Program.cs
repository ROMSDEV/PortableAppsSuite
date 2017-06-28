namespace WinRARPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Win32;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();

#if x86
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "WinRAR64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                return;
            }
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\winrar");
            var updPath = PathEx.Combine(appDir, "WinRARUpdater.exe");
#else
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\winrar-x64");
            var updPath = PathEx.Combine(appDir, "WinRARUpdater64.exe");
#endif

            var appPath = PathEx.Combine(appDir, "WinRAR.exe");
            var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
            var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");

            if (!File.Exists(iniPath))
            {
                Ini.SetFile(iniPath);
                Ini.Write("ContextMenu", "EntriesAllowed", false);
                Ini.Write("Associations", "FileTypes", "001,7z,ace,arj,bz2,bzip2,cab,gz,gzip,iso,lha,lzh,lzma,rar,tar,taz,tbz,tbz2,tgz,tpz,txz,xy,z,zip");
                Ini.WriteAll();
            }
            else
                Ini.SetFile(iniPath);

            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance && !Environment.CommandLine.ContainsEx("{5C1A7995-36C7-4346-B549-D6392EFC86E4}"))
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                var entries = Ini.Read("ContextMenu", "EntriesAllowed", false);
                RegistryHelper(appDir, entries);

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%AppData%\\WinRAR",
                        PathEx.Combine(dataDir, "WinRAR")
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "WinRAR.ini"),
                        PathEx.Combine(dataDir, "WinRAR.ini")
                    }
                };

                Helper.ApplicationStart(updPath, "/archlock /silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                Helper.FileForwarding(Helper.Options.Start, fileMap, entries);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                Helper.FileForwarding(Helper.Options.Exit, fileMap, entries);

                var regKeys = new[]
                {
                    "HKCR\\WinRAR",
                    "HKCR\\WinRAR.REV",
                    "HKCR\\WinRAR.ZIP",
                    "HKCU\\Software\\Classes\\WinRAR",
                    "HKCU\\Software\\Classes\\WinRAR.REV",
                    "HKCU\\Software\\Classes\\WinRAR.ZIP",
                    "HKCU\\Software\\WinRAR"
                };

                if (!entries && regKeys.Any(Reg.SubKeyExists))
                    Elevation.RestartAsAdministrator("{CFFB1200-8B38-451D-80A0-BE187E64EC61}");
            }
        }

        private static bool RegExists(RegistryKey key, string subKey, string arg)
        {
            try
            {
                var exists = Reg.ReadString(key, subKey, null).ContainsEx(arg);
                if (!exists)
                    exists = Reg.ReadString(key, subKey, "FileName").ContainsEx(arg);
                return exists;
            }
            catch
            {
                return false;
            }
        }

        private static void RegistryHelper(string appDir, bool entries)
        {
            var notifyBox = new NotifyBox { TopMost = true };
            if (!entries)
            {
                if (!Elevation.IsAdministrator || !Environment.CommandLine.ContainsEx("{CFFB1200-8B38-451D-80A0-BE187E64EC61}"))
                    return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                notifyBox.Show("Registry entries are being removed . . .",
#if x86
                "WinRAR Portable", 
#else
                "WinRAR Portable (64-bit)",
#endif
                NotifyBox.NotifyBoxStartPosition.Center);

                foreach (var key in new[] { Registry.ClassesRoot, Registry.CurrentUser, Registry.LocalMachine })
                {
                    var subKeys = Reg.GetSubKeyTree(key, null, 20000)?.Where(x => x.ContainsEx("WinRAR") || RegExists(key, x, "WinRAR")).ToArray();
                    if (subKeys?.Any(x => RegExists(key, x, "Portable")) != true)
                        continue;
                    foreach (var subKey in subKeys)
                        Reg.RemoveSubKey(key, subKey);
                }
                notifyBox.Close();
                Environment.ExitCode = 0;
                Environment.Exit(Environment.ExitCode);
            }

            var oldDir = Ini.Read("History", "AppDirectory");
            if (oldDir.EqualsEx(appDir))
                return;

            if (!Elevation.IsAdministrator)
            {
                ProcessEx.Start(PathEx.LocalPath, PathEx.LocalDir, "{5C1A7995-36C7-4346-B549-D6392EFC86E4}", true, false)?.WaitForExit();
                return;
            }

            if (!Ini.Read("History", "FirstTime", false))
            {
#if x86
                oldDir = PathEx.Combine(PathEx.LocalDir, "winrar");
#else
                oldDir = PathEx.Combine(PathEx.LocalDir, "winrar-x64");
#endif
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            notifyBox.Show("Registry entries are being repaired . . .",
#if x86
                "WinRAR Portable", 
#else
                "WinRAR Portable (64-bit)",
#endif
                NotifyBox.NotifyBoxStartPosition.Center);

            try
            {
                foreach (var key in new[] { Registry.ClassesRoot, Registry.CurrentUser, Registry.LocalMachine })
                {
                    var subKeys = Reg.GetSubKeyTree(key, null, 20000)?.Where(x => RegExists(key, x, oldDir));
                    if (subKeys == null)
                        continue;
                    foreach (var subKey in subKeys)
                    {
                        var oldValue = Reg.ReadString(key, subKey, null);
                        if (oldValue.ContainsEx(oldDir))
                            Reg.Write(key, subKey, null, oldValue.Replace(oldDir, appDir));
                        oldValue = Reg.ReadString(key, subKey, "FileName");
                        if (!oldValue.ContainsEx(oldDir))
                            continue;
                        Reg.Write(key, subKey, "FileName", oldValue.Replace(oldDir, appDir));
                    }
                }
                Ini.Write("History", "AppDirectory", appDir);
                Ini.Write("History", "FirstTime", true);
                Ini.WriteAll();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            notifyBox.Close();
            if (!Environment.CommandLine.ContainsEx("{5C1A7995-36C7-4346-B549-D6392EFC86E4}"))
                return;
            Environment.ExitCode = 0;
            Environment.Exit(Environment.ExitCode);
        }
    }
}
