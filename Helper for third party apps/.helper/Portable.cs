namespace Portable
{
#if ApplicationStart || ConfigOverwrite || DirectoryForwarding || FileForwarding || FileSecureForwarding || RedistHandling || RegistryForwarding || RegistrySecureOverwrite
#if ApplicationStart || ConfigOverwrite || DirectoryForwarding || FileForwarding || RedistHandling || RegistryForwarding || RegistrySecureOverwrite
    using System;
#endif
#if ConfigOverwrite || DirectoryForwarding || FileForwarding || FileSecureForwarding || RegistrySecureOverwrite
    using System.Collections.Generic;
#endif
#if RedistHandling
    using System.Diagnostics;
#endif
    using System.IO;
#if ApplicationStart || ConfigOverwrite || DirectoryForwarding || FileForwarding || FileSecureForwarding || RegistryForwarding || RegistrySecureOverwrite
    using System.Linq;
#endif
#if RegistrySecureOverwrite
    using System.Text;
#endif
#if ApplicationStart
    using System.Threading;
#endif
#if RedistHandling
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
        public static void ApplicationStart(string filePath, string commandLine = null, bool? full = true)
        {
            var path = PathEx.Combine(filePath);
            if (!File.Exists(path))
                return;
            using (var p = ProcessEx.Start(path, commandLine, Elevation.IsAdministrator, false))
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

#if RedistHandling
        public static void RedistHandling(Options options, EnvironmentEx.RedistPack version)
        {
            string arch, year;
            switch (version)
            {
                case EnvironmentEx.RedistPack.VC2005_x86:
                    arch = "x86";
                    year = "2005";
                    break;
                case EnvironmentEx.RedistPack.VC2005_x64:
                    arch = "x64";
                    year = "2005";
                    break;
                case EnvironmentEx.RedistPack.VC2008_x86:
                    arch = "x86";
                    year = "2008";
                    break;
                case EnvironmentEx.RedistPack.VC2008_x64:
                    arch = "x64";
                    year = "2008";
                    break;
                case EnvironmentEx.RedistPack.VC2010_x86:
                    arch = "x86";
                    year = "2010";
                    break;
                case EnvironmentEx.RedistPack.VC2010_x64:
                    arch = "x64";
                    year = "2010";
                    break;
                case EnvironmentEx.RedistPack.VC2012_x86:
                    arch = "x86";
                    year = "2012";
                    break;
                case EnvironmentEx.RedistPack.VC2012_x64:
                    arch = "x64";
                    year = "2012";
                    break;
                case EnvironmentEx.RedistPack.VC2013_x86:
                    arch = "x86";
                    year = "2013";
                    break;
                case EnvironmentEx.RedistPack.VC2013_x64:
                    arch = "x64";
                    year = "2013";
                    break;
                case EnvironmentEx.RedistPack.VC2015_x86:
                    arch = "x86";
                    year = "2015";
                    break;
                case EnvironmentEx.RedistPack.VC2015_x64:
                    arch = "x64";
                    year = "2015";
                    break;
                default:
                    return;
            }
            var path = PathEx.Combine(PathEx.LocalDir, $"_CommonRedist\\vcredist\\{year}\\vcredist_{arch}.exe");
            if (!File.Exists(path))
                return;
            var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
            switch (options)
            {
                case Options.Exit:
                    if (Ini.ReadDirect("Redist", "Installed", iniPath).EqualsEx("True"))
                        return;
                    using (var p = ProcessEx.Start(path, "/q /uninstall /norestart", Elevation.IsAdministrator, false))
                        if (p?.HasExited == false)
                            p.WaitForExit();
                    break;
                default:
                    if (Ini.ReadDirect("Redist", "Installed", iniPath).EqualsEx("False"))
                        Elevation.RestartAsAdministrator(EnvironmentEx.CommandLine(false));
                    if (EnvironmentEx.RedistPackIsInstalled(version))
                    {
                        Ini.WriteDirect("Redist", "Installed", true, iniPath);
                        break;
                    }
                    if (!Ini.ReadDirect("Redist", "Installed", iniPath).EqualsEx("True", "False"))
                    {
                        MessageBoxEx.TopMost = true;
                        var msg = $"This application requires Microsoft Visual C++ {year} Redistributable Package ({arch}).{Environment.NewLine}{Environment.NewLine}Do you want to install it permanently (yes) or temporary (no)?";
                        var info = FileVersionInfo.GetVersionInfo(PathEx.LocalPath ?? string.Empty);
                        var result = MessageBoxEx.Show(msg, info.FileDescription, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        Ini.WriteDirect("Redist", "Installed", result == DialogResult.Yes, iniPath);
                        if (result != DialogResult.Yes)
                            Elevation.RestartAsAdministrator(EnvironmentEx.CommandLine(false));
                    }
                    using (var p = ProcessEx.Start(path, "/q /norestart", Elevation.IsAdministrator, false))
                        if (p?.HasExited == false)
                            p.WaitForExit();
                    if (!EnvironmentEx.RedistPackIsInstalled(version))
                    {
                        Environment.ExitCode = 1;
                        Environment.Exit(Environment.ExitCode);
                    }
                    break;
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
                        var key = pair.Key;
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        var value = pair.Value;
                        if (string.IsNullOrWhiteSpace(value))
                            value = "-";
                        sw.WriteLine($"\"{key}\"={value}");
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
