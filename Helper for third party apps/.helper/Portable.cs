namespace Portable
{
#if ApplicationStart || ConfigOverwrite || DirectoryForwarding || FileForwarding || FileSecureForwarding || FindJava || RedistHandling || RegistryForwarding || RegistrySecureOverwrite
#if ApplicationStart || ConfigOverwrite || DirectoryForwarding || FileForwarding || FindJava || RedistHandling || RegistryForwarding || RegistrySecureOverwrite
    using System;
#endif
#if ConfigOverwrite || DirectoryForwarding || FileForwarding || FileSecureForwarding || RedistHandling || RegistrySecureOverwrite
    using System.Collections.Generic;
#endif
#if ApplicationStart || RedistHandling || FindJava
    using System.Diagnostics;
#endif
    using System.IO;
#if ConfigOverwrite || DirectoryForwarding || FileForwarding || FileSecureForwarding || RegistryForwarding || RegistrySecureOverwrite
    using System.Linq;
#endif
#if RegistrySecureOverwrite
    using System.Text;
#endif
#if ApplicationStart
    using System.Threading;
#endif
#if RedistHandling || FindJava
    using System.Windows.Forms;
#endif
    using SilDev;
#if RedistHandling
    using SilDev.Forms;
#endif
#endif

    public static class Helper
    {
#if DirectoryForwarding || FileForwarding || FileSecureForwarding || RedistHandling || RegistryForwarding
        public enum Options
        {
            Start,
            Exit
        }
#endif

#if ApplicationStart
        public static void ApplicationStart(string fileName, string workingDirectory, string arguments, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Normal, bool? full = true)
        {
            var path = PathEx.Combine(fileName);
            if (!File.Exists(path))
                return;
            using (var p = ProcessEx.Start(path, workingDirectory, arguments, Elevation.IsAdministrator, processWindowStyle, false))
                if (p?.HasExited == false)
                    p.WaitForExit();
            if (full == null)
                return;
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return;
            Recheck:
            foreach (var file in Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories))
            {
                var wasRunning = false;
                bool isRunning;
                do
                {
                    isRunning = ProcessEx.IsRunning(full == true ? file : Path.GetFileName(file));
                    if (!wasRunning && isRunning)
                        wasRunning = true;
                    Thread.Sleep(200);
                }
                while (isRunning);
                if (!wasRunning)
                    continue;
                Thread.Sleep(250);
                goto Recheck;
            }
        }

        public static void ApplicationStart(string fileName, string arguments, ProcessWindowStyle processWindowStyle, bool? full = true)
        {
            string workingDirectory = null;
            try
            {
                workingDirectory = Path.GetDirectoryName(fileName);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            ApplicationStart(fileName, workingDirectory, arguments, processWindowStyle, full);
        }

        public static void ApplicationStart(string fileName, string arguments, bool? full = true) =>
            ApplicationStart(fileName, arguments, ProcessWindowStyle.Normal, full);

        public static void ApplicationStart(string fileName, ProcessWindowStyle processWindowStyle, bool? full = true) =>
            ApplicationStart(fileName, null, ProcessWindowStyle.Normal, full);

        public static void ApplicationStart(string fileName, bool? full) =>
            ApplicationStart(fileName, null, ProcessWindowStyle.Normal, full);
#endif

#if ConfigOverwrite
        public static void ConfigOverwrite(Dictionary<string, Dictionary<string, string>> iniMap, string path)
        {
            if (iniMap?.Any() != true)
                return;
            var file = PathEx.Combine(path);
            try
            {
                var dir = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(dir))
                    throw new ArgumentNullException(dir);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(file))
                    File.Create(file).Close();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return;
            }
            foreach (var data in iniMap)
            {
                var section = data.Key;
                foreach (var pair in iniMap[section])
                {
                    var key = pair.Key;
                    var value = pair.Value;
                    Ini.WriteDirect(section, key, value, path);
                }
            }
        }
#endif

#if DirectoryForwarding
        public static void DirectoryForwarding(Options option, Dictionary<string, string> dirMap)
        {
            if (dirMap?.Any() != true)
                return;
            foreach (var data in dirMap)
            {
                if (string.IsNullOrWhiteSpace(data.Key) || string.IsNullOrWhiteSpace(data.Value))
                    continue;
                var srcPath = PathEx.Combine(data.Key);
                var destPath = PathEx.Combine(data.Value);
                Data.SetAttributes(srcPath, FileAttributes.Normal);
                var backupPath = srcPath + ".SI13N7-BACKUP";
                switch (option)
                {
                    case Options.Exit:
                        Data.SetAttributes(backupPath, FileAttributes.Normal);
                        if (Data.DirUnLink(srcPath, true))
                            continue;
                        try
                        {
                            if (Directory.Exists(srcPath))
                            {
                                Data.SetAttributes(srcPath, FileAttributes.Normal);
                                Data.DirCopy(srcPath, destPath, true, true);
                                Directory.Delete(srcPath, true);
                            }
                            if (Directory.Exists(backupPath))
                            {
                                Data.DirCopy(backupPath, srcPath, true, true);
                                Directory.Delete(backupPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                    default:
                        if (Data.DirLink(srcPath, destPath, true))
                        {
                            Data.SetAttributes(backupPath, FileAttributes.Normal);
                            continue;
                        }
                        try
                        {
                            if (Directory.Exists(srcPath))
                            {
                                if (!Directory.Exists(backupPath))
                                {
                                    Data.DirSafeMove(srcPath, backupPath);
                                    Data.SetAttributes(backupPath, FileAttributes.Hidden);
                                }
                                if (Directory.Exists(srcPath))
                                    Directory.Delete(srcPath, true);
                            }
                            if (Directory.Exists(destPath))
                            {
                                Data.SetAttributes(destPath, FileAttributes.Normal);
                                Data.DirCopy(destPath, srcPath);
                            }
                            if (!Directory.Exists(srcPath))
                                Directory.CreateDirectory(srcPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                }
            }
        }
#endif

#if FileForwarding
        public static void FileForwarding(Options option, Dictionary<string, string> fileMap, bool simple = false)
        {
            if (fileMap?.Any() != true)
                return;
            foreach (var data in fileMap)
            {
                if (string.IsNullOrWhiteSpace(data.Key) || string.IsNullOrWhiteSpace(data.Value))
                    continue;
                var srcPath = PathEx.Combine(data.Key);
                var destPath = PathEx.Combine(data.Value);
                try
                {
                    if (!File.Exists(destPath))
                    {
                        var destDir = Path.GetDirectoryName(destPath);
                        if (string.IsNullOrEmpty(destDir))
                            throw new ArgumentNullException(nameof(destDir));
                        if (!Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);
                        File.Create(destPath).Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    continue;
                }
                var backupPath = srcPath + ".SI13N7-BACKUP";
                switch (option)
                {
                    case Options.Exit:
                        if (simple)
                        {
                            try
                            {
                                if (File.Exists(srcPath))
                                    File.Copy(srcPath, destPath, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                            continue;
                        }
                        Data.SetAttributes(backupPath, FileAttributes.Normal);
                        if (Elevation.IsAdministrator && Data.FileUnLink(srcPath, true))
                            continue;
                        try
                        {
                            if (File.Exists(srcPath))
                            {
                                Data.SetAttributes(srcPath, FileAttributes.Normal);
                                File.Copy(srcPath, destPath, true);
                                File.Delete(srcPath);
                            }
                            if (File.Exists(backupPath))
                            {
                                File.Copy(backupPath, srcPath, true);
                                File.Delete(backupPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                    default:
                        if (simple)
                        {
                            try
                            {
                                if (File.Exists(destPath) && (!File.Exists(srcPath) || File.GetLastWriteTime(destPath) > File.GetLastWriteTime(srcPath)))
                                    File.Copy(destPath, srcPath, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                            continue;
                        }
                        if (Elevation.IsAdministrator && Data.FileLink(srcPath, destPath, true))
                        {
                            Data.SetAttributes(backupPath, FileAttributes.Hidden);
                            continue;
                        }
                        try
                        {
                            if (File.Exists(srcPath))
                            {
                                if (!File.Exists(backupPath))
                                {
                                    File.Copy(srcPath, backupPath);
                                    Data.SetAttributes(backupPath, FileAttributes.Hidden);
                                }
                                File.Delete(srcPath);
                            }
                            if (!File.Exists(destPath))
                            {
                                var dir = Path.GetDirectoryName(destPath);
                                if (string.IsNullOrEmpty(dir))
                                    continue;
                                if (!Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);
                                File.Create(destPath).Close();
                            }
                            Data.SetAttributes(backupPath, FileAttributes.Normal);
                            File.Copy(destPath, srcPath, true);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                }
            }
        }
#endif

#if FileSecureForwarding
        public static void FileSecureForwarding(Options option, Dictionary<string, string> fileMap)
        {
            if (fileMap?.Any() != true)
                return;
            foreach (var data in fileMap)
            {
                if (string.IsNullOrWhiteSpace(data.Key) || string.IsNullOrWhiteSpace(data.Value))
                    continue;
                var srcPath = PathEx.Combine(data.Key);
                var destPath = PathEx.Combine(data.Value);
                if (!File.Exists(srcPath) || !PathEx.IsValidPath(srcPath) || !PathEx.IsValidPath(destPath))
                    continue;
                switch (option)
                {
                    case Options.Exit:
                        using (var p = ProcessEx.Send($"DEL /F /Q \"{destPath}\"", Elevation.IsAdministrator, false))
                            if (p?.HasExited == false)
                                p.WaitForExit();
                        break;
                    default:
                        using (var p = ProcessEx.Send($"COPY /Y \"{srcPath}\" \"{destPath}\"", Elevation.IsAdministrator, false))
                            if (p?.HasExited == false)
                                p.WaitForExit();
                        break;
                }
            }
        }
#endif

#if FindJava
        public static void FindJava(out string javaPath, string iniPath = null)
        {
            javaPath = null;
            if (!string.IsNullOrEmpty(iniPath))
            {
                javaPath = Ini.ReadDirect("Java", "Path", iniPath);
                if (File.Exists(javaPath))
                    return;
            }
            try
            {
                var envVar = PathEx.LocalDir;
                var drive = new DriveInfo(envVar).RootDirectory.Root.Name;
                var javaDir = drive;
                foreach (var dirName in envVar.Split('\\'))
                    try
                    {
                        if (drive.Contains(dirName))
                            continue;
                        javaDir = Path.Combine(javaDir, dirName);
                        string tmpDir;
                        if (Environment.Is64BitOperatingSystem)
                        {
                            tmpDir = Path.Combine(javaDir, "CommonFiles\\Java64");
                            if (Directory.Exists(tmpDir))
                                foreach (var file in Directory.GetFiles(tmpDir, "javaw.exe", SearchOption.AllDirectories))
                                {
                                    javaDir = tmpDir;
                                    javaPath = file;
                                    break;
                                }
                        }
                        tmpDir = Path.Combine(javaDir, "CommonFiles\\Java");
                        if (!Directory.Exists(tmpDir))
                            continue;
                        foreach (var file in Directory.GetFiles(tmpDir, "javaw.exe", SearchOption.AllDirectories))
                        {
                            javaDir = tmpDir;
                            javaPath = file;
                            break;
                        }
                        if (File.Exists(javaPath))
                            break;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                if (!File.Exists(javaPath))
                    throw new PathNotFoundException(javaPath);

                if (!string.IsNullOrEmpty(iniPath))
                    Ini.WriteDirect("Java", "Path", javaPath, iniPath);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                var info = FileVersionInfo.GetVersionInfo(PathEx.LocalPath);
                MessageBox.Show(@"Java Portable not found!", info.FileDescription, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
        }
#endif

#if RedistHandling
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static void RedistHandling(Options options, params EnvironmentEx.Redist.Flags[] versions)
        {
            var dict = new Dictionary<EnvironmentEx.Redist.Flags, Dictionary<int, List<int>>>();
            foreach (var version in versions)
            {
                if (!dict.ContainsKey(version))
                    dict.Add(version, new Dictionary<int, List<int>>());
                switch (version)
                {
                    case EnvironmentEx.Redist.Flags.VC2005X86:
                        if (!dict[version].ContainsKey(2005))
                            dict[version].Add(2005, new List<int>());
                        dict[version][2005].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2005X64:
                        if (!dict[version].ContainsKey(2005))
                            dict[version].Add(2005, new List<int>());
                        dict[version][2005].Add(64);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2008X86:
                        if (!dict[version].ContainsKey(2008))
                            dict[version].Add(2008, new List<int>());
                        dict[version][2008].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2008X64:
                        if (!dict[version].ContainsKey(2008))
                            dict[version].Add(2008, new List<int>());
                        dict[version][2008].Add(64);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2010X86:
                        if (!dict[version].ContainsKey(2010))
                            dict[version].Add(2010, new List<int>());
                        dict[version][2010].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2010X64:
                        if (!dict[version].ContainsKey(2010))
                            dict[version].Add(2010, new List<int>());
                        dict[version][2010].Add(64);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2012X86:
                        if (!dict[version].ContainsKey(2012))
                            dict[version].Add(2012, new List<int>());
                        dict[version][2012].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2012X64:
                        if (!dict[version].ContainsKey(2012))
                            dict[version].Add(2012, new List<int>());
                        dict[version][2012].Add(64);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2013X86:
                        if (!dict[version].ContainsKey(2013))
                            dict[version].Add(2013, new List<int>());
                        dict[version][2013].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2013X64:
                        if (!dict[version].ContainsKey(2013))
                            dict[version].Add(2013, new List<int>());
                        dict[version][2013].Add(64);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2015X86:
                        if (!dict[version].ContainsKey(2015))
                            dict[version].Add(2015, new List<int>());
                        dict[version][2015].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2015X64:
                        if (!dict[version].ContainsKey(2015))
                            dict[version].Add(2015, new List<int>());
                        dict[version][2015].Add(64);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2017X86:
                        if (!dict[version].ContainsKey(2017))
                            dict[version].Add(2017, new List<int>());
                        dict[version][2017].Add(86);
                        break;
                    case EnvironmentEx.Redist.Flags.VC2017X64:
                        if (!dict[version].ContainsKey(2017))
                            dict[version].Add(2017, new List<int>());
                        dict[version][2017].Add(64);
                        break;
                    default:
                        return;
                }
            }
            foreach (var data in dict)
            {
                var version = data.Key;
                foreach (var vars in data.Value)
                {
                    var year = vars.Key;
                    foreach (var arch in vars.Value)
                    {
                        var path = PathEx.Combine(PathEx.LocalDir, $"_CommonRedist\\vcredist\\{year}\\vcredist_x{arch}.exe");
                        if (!File.Exists(path))
                            return;
                        var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
                        switch (options)
                        {
                            case Options.Exit:
                                if (Ini.ReadDirect("Redist", version.ToString(), iniPath).EqualsEx("True"))
                                    return;
                                using (var p = ProcessEx.Start(path, "/q /uninstall /norestart", Elevation.IsAdministrator, false))
                                    if (p?.HasExited == false)
                                        p.WaitForExit();
                                break;
                            default:
                                if (Ini.ReadDirect("Redist", version.ToString(), iniPath).EqualsEx("False"))
                                    Elevation.RestartAsAdministrator(EnvironmentEx.CommandLine(false));
                                if (EnvironmentEx.Redist.IsInstalled(version))
                                {
                                    Ini.WriteDirect("Redist", version.ToString(), true, iniPath);
                                    break;
                                }
                                var info = FileVersionInfo.GetVersionInfo(PathEx.LocalPath);
                                if (!Ini.ReadDirect("Redist", version.ToString(), iniPath).EqualsEx("True", "False"))
                                {
                                    MessageBoxEx.TopMost = true;
                                    MessageBoxEx.ButtonText.OverrideEnabled = true;
                                    MessageBoxEx.ButtonText.Yes = "&Yes";
                                    MessageBoxEx.ButtonText.No = "&No";
                                    MessageBoxEx.ButtonText.Cancel = "&Cancel";
                                    var msg = $"Microsoft Visual C++ {year} Redistributable Package (x{arch}) is required to run this porgram.{Environment.NewLine}{Environment.NewLine}Would you like to permanently install this package (yes) or temporarily (no)?";
                                    var result = MessageBoxEx.Show(msg, info.FileDescription, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                                    if (result == DialogResult.Cancel)
                                    {
                                        RedistHandling(Options.Exit, versions);
                                        Environment.Exit(Environment.ExitCode);
                                    }
                                    Ini.WriteDirect("Redist", version.ToString(), result == DialogResult.Yes, iniPath);
                                    if (result != DialogResult.Yes)
                                        Elevation.RestartAsAdministrator(EnvironmentEx.CommandLine(false));
                                }
                                var notifyBox = new NotifyBox();
                                notifyBox.Show($"Microsoft Visual C++ {year} Redistributable Package (x{arch}) has been initialized . . .", info.FileDescription, NotifyBox.NotifyBoxStartPosition.Center);
                                using (var p = ProcessEx.Start(path, "/q /norestart", Elevation.IsAdministrator, false))
                                    if (p?.HasExited == false)
                                        p.WaitForExit();
                                notifyBox.Close();
                                if (!EnvironmentEx.Redist.IsInstalled(version))
                                {
                                    Environment.ExitCode = 1;
                                    Environment.Exit(Environment.ExitCode);
                                }
                                break;
                        }
                    }
                }
            }
        }
#endif

#if RegistryForwarding
        public static void RegForwarding(Options option, params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return;
            var backup = PathEx.Combine($"%TEMP%\\backup-{{{Math.Abs(PathEx.LocalPath.GetHashCode())}}}.reg");
            var file = PathEx.Combine(PathEx.LocalDir, "Data\\settings.reg");
            switch (option)
            {
                case Options.Exit:
                    if (keys.Length > 0)
                    {
                        Reg.ExportKeys(file, keys);
                        foreach (var key in keys)
                            Reg.RemoveSubKey(key);
                    }
                    if (!File.Exists(backup))
                        return;
                    Reg.ImportFile(backup);
                    File.Delete(backup);
                    break;
                default:
                    if (!keys.Any(x => Reg.EntryExists(x, "Portable App")))
                    {
                        if (!File.Exists(backup))
                            Reg.ExportKeys(backup, keys);
                        foreach (var key in keys)
                            Reg.RemoveSubKey(key);
                    }
                    foreach (var key in keys)
                        Reg.Write(key, "Portable App", "True");
                    if (File.Exists(file))
                        Reg.ImportFile(file);
                    break;
            }
        }
#endif

#if RegistrySecureOverwrite
        public static void RegSecureOverwrite(Dictionary<string, Dictionary<string, string>> regMap, bool elevated = false)
        {
            if (regMap?.Any() != true)
                return;
            var file = PathEx.Combine($"%TEMP%\\overwrite-{{{Math.Abs(PathEx.LocalPath.GetHashCode())}}}.reg");
            try
            {
                var dir = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(dir))
                    throw new ArgumentNullException(dir);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return;
            }
            using (var sw = new StreamWriter(file, true, Encoding.GetEncoding(1252)))
            {
                sw.WriteLine("Windows Registry Editor Version 5.00");
                sw.WriteLine();
                foreach (var data in regMap)
                {
                    var section = data.Key;
                    if (!section.ContainsEx('\\'))
                        continue;
                    var levels = section.Split('\\');
                    var first = levels[0];
                    switch (first.TrimStart('[', '-'))
                    {
                        case "HKEY_CLASSES_ROOT":
                        case "HKEY_CURRENT_CONFIG":
                        case "HKEY_CURRENT_USER":
                        case "HKEY_LOCAL_MACHINE":
                        case "HKEY_PERFORMANCE_DATA":
                        case "HKEY_USERS":
                            break;
                        case "HKCR":
                            levels[0] = "HKEY_CLASSES_ROOT";
                            break;
                        case "HKCC":
                            levels[0] = "HKEY_CURRENT_CONFIG";
                            break;
                        case "HKCU":
                            levels[0] = "HKEY_CURRENT_USER";
                            break;
                        case "HKLM":
                            levels[0] = "HKEY_LOCAL_MACHINE";
                            break;
                        case "HKPD":
                            levels[0] = "HKEY_PERFORMANCE_DATA";
                            break;
                        case "HKU":
                            levels[0] = "HKEY_USERS";
                            break;
                        default:
                            continue;
                    }
                    if (!first.Equals(levels[0]))
                    {
                        if (first.StartsWithEx("[-", "-"))
                            levels[0] = $"-{levels[0]}";
                        section = levels.Join('\\');
                    }
                    if (!section.StartsWith("["))
                        section = $"[{section}";
                    if (!section.EndsWith("]"))
                        section = $"{section}]";
                    sw.WriteLine(section);
                    if (regMap[data.Key]?.Any() != true)
                    {
                        sw.WriteLine();
                        continue;
                    }
                    foreach (var pair in regMap[data.Key])
                    {
                        var key = !string.IsNullOrWhiteSpace(pair.Key) ? $"\"{pair.Key}\"" : "@";
                        var value = pair.Value;
                        if (string.IsNullOrWhiteSpace(value))
                            value = "-";
                        sw.WriteLine($"{key}={value}");
                    }
                    sw.WriteLine();
                }
                sw.WriteLine();
            }
            Reg.ImportFile(file, elevated || Elevation.IsAdministrator);
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
#endif
    }
}
