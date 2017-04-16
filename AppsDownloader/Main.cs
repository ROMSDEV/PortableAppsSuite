﻿namespace AppsDownloader
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;

    internal static class Main
    {
        internal static string Text;
        internal static readonly bool UpdateSearch = Environment.CommandLine.ContainsEx("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");
        internal static readonly string HomeDir = PathEx.Combine(PathEx.LocalDir, "..");
        internal static readonly string TmpDir = PathEx.Combine(HomeDir, "Documents\\.cache");
        internal static readonly string AppsDbPath = PathEx.Combine(TmpDir, $"AppInfo{Convert.ToByte(UpdateSearch)}.ini");

        [Flags]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal enum InternetProtocols
        {
            None = -0x10,
            Version4 = 0x20,
            Version6 = 0x40
        }

        private static InternetProtocols _availableProtocols = InternetProtocols.None;

        internal static InternetProtocols AvailableProtocols
        {
            get
            {
                if (!_availableProtocols.HasFlag(InternetProtocols.None))
                    return _availableProtocols;

                if (NetEx.InternetIsAvailable())
                    _availableProtocols = InternetProtocols.Version4;
                if (NetEx.InternetIsAvailable(true))
                    if (!_availableProtocols.HasFlag(InternetProtocols.None))
                        _availableProtocols |= InternetProtocols.Version6;
                    else
                        _availableProtocols = InternetProtocols.Version6;

                if (!_availableProtocols.HasFlag(InternetProtocols.Version4) && _availableProtocols.HasFlag(InternetProtocols.Version6))
                    MessageBoxEx.Show(Lang.GetText("InternetProtocolWarningMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return _availableProtocols;
            }
        }

        internal static List<string> AppsDbSections = new List<string>();
        internal static string TmpAppsDbDir = PathEx.Combine(TmpDir, PathEx.GetTempDirName());
        internal static string TmpAppsDbPath = PathEx.Combine(TmpAppsDbDir, "update.ini");

        internal static void ResetAppDb(bool force = false)
        {
            var appsDbLastWriteTime = DateTime.Now.AddHours(1d);
            long appsDbLength = 0;
            if (!File.Exists(TmpAppsDbPath) && File.Exists(AppsDbPath))
                try
                {
                    var fi = new FileInfo(AppsDbPath);
                    appsDbLastWriteTime = fi.LastWriteTime;
                    appsDbLength = fi.Length;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            if (!force && !UpdateSearch && !File.Exists(TmpAppsDbPath) && !((DateTime.Now - appsDbLastWriteTime).TotalHours >= 1d) && appsDbLength >= 0x23000 && (AppsDbSections = Ini.GetSections(AppsDbPath)).Count >= 400)
                return;
            try
            {
                if (File.Exists(AppsDbPath))
                    File.Delete(AppsDbPath);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        internal static void UpdateAppDb()
        {
            if (File.Exists(AppsDbPath))
                return;

            if (!Directory.Exists(TmpAppsDbDir))
            {
                Directory.CreateDirectory(TmpAppsDbDir);
                AppDomain.CurrentDomain.ProcessExit += (s, args) =>
                {
                    try
                    {
                        Directory.Delete(TmpAppsDbDir, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                };
            }

            for (var i = 0; i < 3; i++)
            {
                NetEx.Transfer.DownloadFile(AvailableProtocols.HasFlag(InternetProtocols.Version4) ? "https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/AppInfo.ini" : $"{InternalMirrors[0]}/Downloads/Portable%20Apps%20Suite/.free/AppInfo.ini", AppsDbPath);
                if (!File.Exists(AppsDbPath))
                {
                    if (i > 1)
                        throw new InvalidOperationException("Server connection failed.");
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }

            var externDbPath = Path.Combine(TmpAppsDbDir, "AppInfo.7z");
            string[] externDbSrvs =
            {
                "Downloads/Portable%20Apps%20Suite/.free/PortableAppsInfo.7z",
                "portableapps.com/updater/update.7z"
            };
            var internCheck = false;
            foreach (var srv in externDbSrvs)
            {
                long length = 0;
                if (!internCheck)
                {
                    internCheck = true;
                    foreach (var mirror in InternalMirrors)
                    {
                        string tmpSrv = $"{mirror}/{srv}";
                        if (!NetEx.FileIsAvailable(tmpSrv, 60000))
                            continue;
                        NetEx.Transfer.DownloadFile(tmpSrv, externDbPath);
                        if (!File.Exists(externDbPath))
                            continue;
                        length = new FileInfo(externDbPath).Length;
                        if (File.Exists(externDbPath) && length > 0x6000)
                            break;
                    }
                }
                else
                {
                    NetEx.Transfer.DownloadFile(srv, externDbPath);
                    if (File.Exists(externDbPath))
                        length = new FileInfo(externDbPath).Length;
                }
                if (File.Exists(externDbPath) && length > 0x6000)
                    break;
            }

            AppsDbSections = Ini.GetSections(AppsDbPath);
            if (!File.Exists(externDbPath))
                return;
            using (var p = Compaction.Zip7Helper.Unzip(externDbPath, TmpAppsDbDir))
                if (!p?.HasExited == true)
                    p?.WaitForExit();
            File.Delete(externDbPath);
            externDbPath = TmpAppsDbPath;
            if (File.Exists(externDbPath))
                foreach (var section in Ini.GetSections(externDbPath))
                {
                    if (AppsDbSections.ContainsEx(section) || section.EqualsEx("sPortable") || section.ContainsEx("PortableApps.com", "ByPortableApps"))
                        continue;
                    var nam = Ini.Read(section, "Name", externDbPath);
                    if (string.IsNullOrWhiteSpace(nam) || nam.ContainsEx("jPortable Launcher"))
                        continue;
                    if (!nam.StartsWithEx("jPortable", "PortableApps.com"))
                    {
                        var tmp = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(nam, string.Empty);
                        tmp = Regex.Replace(tmp, @"\s+", " ");
                        if (!string.IsNullOrWhiteSpace(tmp) && tmp != nam)
                            nam = tmp.Trim().TrimEnd(',');
                    }
                    var des = Ini.Read(section, "Description", externDbPath);
                    if (string.IsNullOrWhiteSpace(des))
                        continue;
                    var cat = Ini.Read(section, "Category", externDbPath);
                    if (string.IsNullOrWhiteSpace(cat))
                        continue;
                    var ver = Ini.Read(section, "DisplayVersion", externDbPath);
                    if (string.IsNullOrWhiteSpace(ver))
                        continue;
                    var pat = Ini.Read(section, "DownloadPath", externDbPath);
                    pat = $"{(string.IsNullOrWhiteSpace(pat) ? "http://downloads.sourceforge.net/portableapps" : pat)}/{Ini.Read(section, "DownloadFile", externDbPath)}";
                    if (!pat.EndsWithEx(".paf.exe"))
                        continue;
                    var has = Ini.Read(section, "Hash", externDbPath);
                    if (string.IsNullOrWhiteSpace(has))
                        continue;
                    var phs = new Dictionary<string, List<string>>();
                    foreach (var lang in Lang.GetText("availableLangs").Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(lang))
                            continue;
                        var tmpFile = Ini.Read(section, $"DownloadFile_{lang}", externDbPath);
                        if (string.IsNullOrWhiteSpace(tmpFile))
                            continue;
                        var tmphash = Ini.Read(section, $"Hash_{lang}", externDbPath);
                        if (string.IsNullOrWhiteSpace(tmphash) || !string.IsNullOrWhiteSpace(tmphash) && tmphash == has)
                            continue;
                        var tmpPath = Ini.Read(section, "DownloadPath", externDbPath);
                        tmpFile = $"{(string.IsNullOrWhiteSpace(tmpPath) ? "http://downloads.sourceforge.net/portableapps" : tmpPath)}/{tmpFile}";
                        phs.Add(lang, new List<string> { tmpFile, tmphash });
                    }
                    var dis = Ini.ReadLong(section, "DownloadSize", 1, externDbPath);
                    var siz = Ini.ReadLong(section, "InstallSizeTo", externDbPath);
                    if (siz == 0)
                        siz = Ini.ReadLong(section, "InstallSize", 1, externDbPath);
                    var adv = Ini.Read(section, "Advanced", externDbPath);
                    File.AppendAllText(AppsDbPath, Environment.NewLine);
                    Ini.Write(section, "Name", nam, AppsDbPath);
                    Ini.Write(section, "Description", des, AppsDbPath);
                    Ini.Write(section, "Category", cat, AppsDbPath);
                    Ini.Write(section, "Version", ver, AppsDbPath);
                    Ini.Write(section, "ArchivePath", pat, AppsDbPath);
                    Ini.Write(section, "ArchiveHash", has, AppsDbPath);
                    if (phs.Count > 0)
                    {
                        Ini.Write(section, "AvailableArchiveLangs", phs.Keys.Join(","), AppsDbPath);
                        foreach (var item in phs)
                        {
                            Ini.Write(section, $"ArchivePath_{item.Key}", item.Value[0], AppsDbPath);
                            Ini.Write(section, $"ArchiveHash_{item.Key}", item.Value[1], AppsDbPath);
                        }
                    }
                    Ini.Write(section, "DownloadSize", dis, AppsDbPath);
                    Ini.Write(section, "InstallSize", siz, AppsDbPath);
                    if (adv.EqualsEx("true"))
                        Ini.Write(section, "Advanced", true, AppsDbPath);
                }

            if (SwData.IsEnabled)
                try
                {
                    var externDb = NetEx.Transfer.DownloadString($"{SwData.ServerAddress}/AppInfo.ini", SwData.Username, SwData.Password);
                    if (!string.IsNullOrWhiteSpace(externDb))
                        File.AppendAllText(AppsDbPath, $@"{Environment.NewLine}{externDb}");
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            File.WriteAllText(AppsDbPath, File.ReadAllText(AppsDbPath).FormatNewLine());

            AppsDbSections = Ini.GetSections(AppsDbPath);
            if (AppsDbSections.Count == 0)
                throw new InvalidOperationException("No available apps found.");
        }

        internal static void SearchAppUpdates()
        {
            var outdatedApps = new List<string>();
            var appFlags = AppFlags.Core | AppFlags.Free;
            if (SwData.IsEnabled)
                appFlags |= AppFlags.Share;
            foreach (var dir in GetInstalledApps(appFlags))
            {
                var section = Path.GetFileName(dir);
                if (Ini.ReadBoolean(section, "NoUpdates"))
                {
                    if ((DateTime.Now - Ini.ReadDateTime(section, "NoUpdatesTime")).TotalDays <= 7d)
                        continue;
                    Ini.RemoveKey(section, "NoUpdates");
                    Ini.RemoveKey(section, "NoUpdatesTime");
                }
                if (dir.ContainsEx("\\.share\\"))
                    section = $"{section}###";
                if (!AppsDbSections.ContainsEx(section))
                    continue;
                var fileData = new Dictionary<string, string>();
                var verData = Ini.Read(section, "VersionData", AppsDbPath);
                var verHash = Ini.Read(section, "VersionHash", AppsDbPath);
                if (!string.IsNullOrWhiteSpace(verData) && !string.IsNullOrWhiteSpace(verHash))
                {
                    if (!verData.Contains(","))
                        verData = $"{verData},";
                    var verDataSplit = verData.Split(',');
                    if (!verHash.Contains(","))
                        verHash = $"{verHash},";
                    var verHashSplit = verHash.Split(',');
                    if (verDataSplit.Length != verHashSplit.Length)
                        continue;
                    for (var i = 0; i < verDataSplit.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(verDataSplit[i]) || string.IsNullOrWhiteSpace(verHashSplit[i]))
                            continue;
                        fileData.Add(verDataSplit[i], verHashSplit[i]);
                    }
                }
                if (fileData.Count > 0)
                {
                    if (fileData.Select(data => new { data, filePath = Path.Combine(dir, data.Key) })
                                .Where(x => File.Exists(x.filePath))
                                .Where(x => Crypto.EncryptFileToSha256(x.filePath) != x.data.Value)
                                .Select(x => x.data).Any())
                        if (!outdatedApps.ContainsEx(section))
                            outdatedApps.Add(section);
                    continue;
                }
                if (dir.ContainsEx("\\.share\\"))
                    continue;
                var appIniPath = Path.Combine(dir, "App\\AppInfo\\appinfo.ini");
                if (!File.Exists(appIniPath))
                    continue;
                var localVer = Ini.ReadVersion("Version", "DisplayVersion", appIniPath);
                var serverVer = Ini.ReadVersion(section, "Version", AppsDbPath);
                if (localVer >= serverVer)
                    continue;
                Log.Write($"Update for '{section}' found (Local: '{localVer}'; Server: '{serverVer}').");
                if (!outdatedApps.ContainsEx(section))
                    outdatedApps.Add(section);
            }
            AppsDbSections = outdatedApps;
        }

        [Flags]
        internal enum AppFlags
        {
            Core = 0x10,
            Free = 0x20,
            Repack = 0x40,
            Share = 0x80
        }

        internal static List<string> GetInstalledApps(AppFlags flags = AppFlags.Core | AppFlags.Free | AppFlags.Repack, bool sections = false)
        {
            var list = new List<string>();
            try
            {
                var dirs = new Dictionary<AppFlags, string>
                {
                    { AppFlags.Core, Path.Combine(HomeDir, "Apps") },
                    { AppFlags.Free, Path.Combine(HomeDir, "Apps\\.free") },
                    { AppFlags.Repack, Path.Combine(HomeDir, "Apps\\.repack") },
                    { AppFlags.Share, Path.Combine(HomeDir, "Apps\\.share") }
                };

                if (flags.HasFlag(AppFlags.Core))
                    list.AddRange(Directory.GetDirectories(dirs[AppFlags.Core], "*", SearchOption.TopDirectoryOnly).Where(s => !s.StartsWith(".")).ToList());
                if (flags.HasFlag(AppFlags.Free))
                    list.AddRange(Directory.GetDirectories(dirs[AppFlags.Free], "*", SearchOption.TopDirectoryOnly));
                if (flags.HasFlag(AppFlags.Repack))
                    list.AddRange(Directory.GetDirectories(dirs[AppFlags.Repack], "*", SearchOption.TopDirectoryOnly));
                if (flags.HasFlag(AppFlags.Share))
                    list.AddRange(Directory.GetDirectories(dirs[AppFlags.Share], "*", SearchOption.TopDirectoryOnly));

                try
                {
                    list = list.Where(x => Directory.GetFiles(x, "*.exe", SearchOption.TopDirectoryOnly).Length > 0 ||
                                           Directory.GetFiles(x, Path.GetFileNameWithoutExtension(x) + ".ini", SearchOption.TopDirectoryOnly).Length > 0 &&
                                           Directory.GetFiles(x, "*.exe", SearchOption.AllDirectories).Length > 0).ToList();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                if (sections)
                {
                    list = list.Select(x => x.StartsWithEx(dirs[AppFlags.Share]) ? $"{Path.GetFileName(x)}###" : Path.GetFileName(x)).ToList();
                    foreach (var s in new[] { "Java", "Java64" })
                    {
                        var jPath = Path.Combine(HomeDir, $"Apps\\CommonFiles\\{s}\\bin\\java.exe");
                        if (!list.ContainsEx(s) && File.Exists(jPath))
                            list.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return list;
        }

        internal static List<string> GetAllAppInstaller()
        {
            var list = new List<string>();
            try
            {
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps"), "*.paf.exe", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.repack"), "*.7z", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.free"), "*.7z", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.share"), "*.7z", SearchOption.TopDirectoryOnly));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return list;
        }

        internal static int StartInstall()
        {
            var appInstaller = GetAllAppInstaller();
            foreach (var filePath in appInstaller)
            {
                if (!File.Exists(filePath))
                    continue;

                var appDir = string.Empty;
                if (!filePath.EndsWithEx(".paf.exe"))
                {
                    var fDir = Path.GetDirectoryName(filePath);
                    var fName = Path.GetFileNameWithoutExtension(filePath);
                    if (!fName.StartsWith("_") && fName.Contains("_"))
                        fName = fName.Split('_')[0];
                    appDir = Path.Combine(fDir, fName);
                }
                else
                    foreach (var dir in GetInstalledApps())
                        if (Path.GetFileName(filePath).StartsWithEx(Path.GetFileName(dir)))
                        {
                            appDir = dir;
                            break;
                        }

                if (Directory.Exists(appDir))
                    if (!BreakFileLocks(appDir, false))
                    {
                        var fName = Path.GetFileNameWithoutExtension(filePath);
                        if (fName.EndsWithEx(".paf"))
                            fName = fName.RemoveText(".paf");
                        MessageBoxEx.Show(string.Format(Lang.GetText("InstallSkippedMsg"), fName), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }

                foreach (var transfer in TransferManager)
                {
                    Process p = null;
                    try
                    {
                        if (!transfer.Value.FilePath.EqualsEx(filePath))
                            continue;
                        var appName = string.Empty;
                        var fileName = Path.GetFileName(filePath);
                        foreach (var section in Ini.GetSections(AppsDbPath))
                        {
                            if (filePath.ContainsEx("\\.share\\") && !section.EndsWith("###"))
                                continue;
                            if (Ini.Read(section, "ArchivePath", AppsDbPath).EndsWith(fileName))
                                appName = section;
                            else
                            {
                                var found = false;
                                var archiveLangs = Ini.Read(section, "AvailableArchiveLangs", AppsDbPath);
                                if (archiveLangs.Contains(","))
                                    foreach (var lang in archiveLangs.Split(','))
                                    {
                                        found = Ini.Read(section, $"ArchivePath_{lang}", AppsDbPath).EndsWith(fileName);
                                        if (!found)
                                            continue;
                                        appName = section;
                                        break;
                                    }
                                if (!found)
                                    continue;
                            }
                            var archiveLang = Ini.Read(section, "ArchiveLang");
                            if (archiveLang.StartsWithEx("Default"))
                                archiveLang = string.Empty;
                            var archiveHash = !string.IsNullOrEmpty(archiveLang) ? Ini.Read(section, $"ArchiveHash_{archiveLang}", AppsDbPath) : Ini.Read(section, "ArchiveHash", AppsDbPath);
                            var localHash = Crypto.EncryptFileToMd5(filePath);
                            if (localHash == archiveHash)
                                break;
                            throw new InvalidOperationException($"Checksum is invalid. - Key: '{transfer.Key}'; Section: '{section}'; File: '{filePath}'; Current: '{archiveHash}'; Requires: '{localHash}';");
                        }
                        if (filePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                            (p = Compaction.Zip7Helper.Unzip(filePath, appDir, ProcessWindowStyle.Minimized))?.WaitForExit();
                        else
                        {
                            var appsDir = Path.Combine(HomeDir, "Apps");
                            (p = ProcessEx.Start(filePath, appsDir, $"/DESTINATION=\"{appsDir}\\\"", false, false))?.WaitForExit();

                            // Fix for messy app installer
                            var retries = 0;
                            retry:
                            try
                            {
                                var appDirs = new[]
                                {
                                    Path.Combine(appsDir, "App"),
                                    Path.Combine(appsDir, "Data"),
                                    Path.Combine(appsDir, "Other")
                                };
                                if (appDirs.Any(Directory.Exists))
                                {
                                    if (string.IsNullOrWhiteSpace(appDir) || appDir.EqualsEx(appsDir))
                                        appDir = Path.Combine(appsDir, appName);
                                    if (!Directory.Exists(appDir))
                                        Directory.CreateDirectory(appDir);
                                    else
                                    {
                                        BreakFileLocks(appDir);
                                        foreach (var d in new[] { "App", "Other" })
                                        {
                                            var dir = Path.Combine(appDir, d);
                                            if (!Directory.Exists(dir))
                                                continue;
                                            Directory.Delete(dir, true);
                                        }
                                        foreach (var f in Directory.EnumerateFiles(appDir, "*.*", SearchOption.TopDirectoryOnly))
                                            File.Delete(f);
                                    }
                                    foreach (var d in appDirs)
                                    {
                                        if (!Directory.Exists(d))
                                            continue;
                                        BreakFileLocks(d);
                                        if (Path.GetFileName(d).EqualsEx("Data"))
                                        {
                                            Directory.Delete(d, true);
                                            continue;
                                        }
                                        var dir = Path.Combine(appDir, Path.GetFileName(d));
                                        Directory.Move(d, dir);
                                    }
                                    foreach (var f in Directory.EnumerateFiles(appsDir, "*.*", SearchOption.TopDirectoryOnly))
                                    {
                                        if (Data.MatchAttributes(f, FileAttributes.Hidden) || f.EndsWithEx(".7z", ".paf.exe"))
                                            continue;
                                        BreakFileLocks(f);
                                        var file = Path.Combine(appDir, Path.GetFileName(f));
                                        File.Move(f, file);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                                if (retries >= 15)
                                    throw new UnauthorizedAccessException();
                                Thread.Sleep(1000);
                                retries++;
                                goto retry;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        transfer.Value.HasCanceled = true;
                    }
                    finally
                    {
                        p?.Dispose();
                    }
                    break;
                }
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
            return appInstaller.Count;
        }

        internal static volatile Dictionary<string, NetEx.AsyncTransfer> TransferManager = new Dictionary<string, NetEx.AsyncTransfer>();
        internal static string LastTransferItem = string.Empty;

        internal struct DownloadInfo
        {
            internal static int Amount;
            internal static int Count;
            internal static volatile int Retries;
            internal static int IsFinishedTick;
        }

        private static List<string> _internalMirrors;

        internal static List<string> InternalMirrors
        {
            get
            {
                if (_internalMirrors == null)
                    _internalMirrors = new List<string>();
                if (_internalMirrors.Count > 0)
                    return _internalMirrors;
                var dnsInfo = new Dictionary<string, Dictionary<string, string>>();
                for (var i = 0; i < 3; i++)
                {
                    if (!AvailableProtocols.HasFlag(InternetProtocols.Version4) && AvailableProtocols.HasFlag(InternetProtocols.Version6))
                    {
                        dnsInfo = Ini.ReadAll(Resources.IPv6DNS, false);
                        break;
                    }
                    dnsInfo = Ini.ReadAll(NetEx.Transfer.DownloadString("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini"), false);
                    if (dnsInfo.Count == 0 && i < 2)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    break;
                }
                if (dnsInfo.Count == 0)
                    return _internalMirrors;
                foreach (var section in dnsInfo.Keys)
                    try
                    {
                        var addr = dnsInfo[section][AvailableProtocols.HasFlag(InternetProtocols.Version4) ? "addr" : "ipv6"];
                        if (string.IsNullOrEmpty(addr))
                            continue;
                        var domain = dnsInfo[section]["domain"];
                        if (string.IsNullOrEmpty(domain))
                            continue;
                        bool ssl;
                        bool.TryParse(dnsInfo[section]["ssl"], out ssl);
                        domain = ssl ? $"https://{domain}" : $"http://{domain}";
                        if (!_internalMirrors.ContainsEx(domain))
                            _internalMirrors.Add(domain);
                    }
                    catch (KeyNotFoundException) { }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                return _internalMirrors;
            }
        }

        internal static readonly string[] ExternalMirrors =
        {
            "downloads.sourceforge.net",
            "netcologne.dl.sourceforge.net",
            "freefr.dl.sourceforge.net",
            "heanet.dl.sourceforge.net",
            "kent.dl.sourceforge.net",
            "vorboss.dl.sourceforge.net",
            "netix.dl.sourceforge.net"
        };

        internal static readonly Dictionary<string, List<string>> LastExternalMirrors = new Dictionary<string, List<string>>();
        internal static volatile List<string> ExternalMirrorsSorted = new List<string>();

        internal static class SwData
        {
            internal static string ServerAddress = Ini.ReadString("Host", "Srv");
            internal static string Username = Ini.ReadString("Host", "Usr");
            internal static string Password = Ini.ReadString("Host", "Pwd");
            private static bool? _isEnabled;

            internal static bool IsEnabled
            {
                get
                {
                    if (_isEnabled != null)
                        return _isEnabled == true;

                    _isEnabled = !string.IsNullOrEmpty(ServerAddress) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
                    if (_isEnabled == false)
                        return false;

                    _isEnabled = false;
                    try
                    {
                        var winId = Win32_OperatingSystem.SerialNumber;
                        if (string.IsNullOrWhiteSpace(winId))
                            throw new PlatformNotSupportedException();
                        var aesPw = winId.EncryptToSha256();
                        if (ServerAddress.StartsWithEx("http://", "https://"))
                        {
                            Ini.Write("Host", "Srv", ServerAddress.EncryptToAes256(aesPw).EncodeToBase85());
                            Ini.Write("Host", "Usr", Username.EncryptToAes256(aesPw).EncodeToBase85());
                            Ini.Write("Host", "Pwd", Password.EncryptToAes256(aesPw).EncodeToBase85());
                        }
                        else
                        {
                            ServerAddress = ServerAddress.DecodeByteArrayFromBase85().DecryptFromAes256(aesPw).FromByteArrayToString();
                            Username = Username.DecodeByteArrayFromBase85().DecryptFromAes256(aesPw).FromByteArrayToString();
                            Password = Password.DecodeByteArrayFromBase85().DecryptFromAes256(aesPw).FromByteArrayToString();
                        }
                        _isEnabled = NetEx.FileIsAvailable($"{ServerAddress}/AppInfo.ini", Username, Password);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        _isEnabled = false;
                    }

                    return _isEnabled == true;
                }
            }
        }

        internal static Thread DownloadStarter { get; set; }

        internal static void StartDownload(string section, string archivePath, string localArchivePath, string group)
        {
            if (!archivePath.StartsWithEx("http"))
            {
                if (group.EqualsEx("*Shareware"))
                    TransferManager[LastTransferItem].DownloadFile($"{(SwData.ServerAddress.EndsWith("/") ? SwData.ServerAddress.Substring(0, SwData.ServerAddress.Length - 1) : SwData.ServerAddress)}/{archivePath}", localArchivePath, SwData.Username, SwData.Password);
                else
                    foreach (var mirror in InternalMirrors)
                    {
                        string newArchivePath = $"{mirror}/Downloads/Portable%20Apps%20Suite/{archivePath}";
                        if (!NetEx.FileIsAvailable(newArchivePath, 60000))
                            continue;
                        Log.Write($"{Path.GetFileName(newArchivePath)} has been found on '{mirror}'.");
                        TransferManager[LastTransferItem].DownloadFile(newArchivePath, localArchivePath);
                        break;
                    }
            }
            else
            {
                if (archivePath.ContainsEx("sourceforge"))
                {
                    var newArchivePath = archivePath;
                    if (ExternalMirrorsSorted.Count == 0)
                    {
                        var sortHelper = new Dictionary<string, long>();
                        foreach (var mirror in ExternalMirrors)
                        {
                            if (sortHelper.Keys.ContainsEx(mirror))
                                continue;
                            var path = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                            sortHelper.Add(mirror, NetEx.Ping(path));
                        }
                        ExternalMirrorsSorted = sortHelper.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
                    }
                    if (ExternalMirrorsSorted.Count > 0)
                        foreach (var mirror in ExternalMirrorsSorted)
                        {
                            if (DownloadInfo.Retries < ExternalMirrorsSorted.Count - 1 && LastExternalMirrors.ContainsKey(section) && LastExternalMirrors[section].ContainsEx(mirror))
                                continue;
                            newArchivePath = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                            if (!NetEx.FileIsAvailable(newArchivePath, 60000))
                                continue;
                            if (!LastExternalMirrors.ContainsKey(section))
                                LastExternalMirrors.Add(section, new List<string> { mirror });
                            else
                                LastExternalMirrors[section].Add(mirror);
                            Log.Write($"'{Path.GetFileName(newArchivePath)}' has been found at '{mirror}'.");
                            break;
                        }
                    TransferManager[LastTransferItem].DownloadFile(newArchivePath, localArchivePath);
                }
                else
                    TransferManager[LastTransferItem].DownloadFile(archivePath, localArchivePath, 60000, "Mozilla/5.0");
            }
        }

        internal static bool BreakFileLocks(string path, bool force = true)
        {
            if (!PathEx.DirOrFileExists(path))
                return true;
            var locks = Data.GetLocks(path);
            if (locks.Count == 0)
                return true;
            if (!force)
            {
                var result = MessageBoxEx.Show(string.Format(Lang.GetText("FileLocksMsg"), locks.Count == 1 ? Lang.GetText("FileLocksMsg1") : Lang.GetText("FileLocksMsg2"), $"{locks.Select(p => p.ProcessName).Join($".exe; {Environment.NewLine}")}.exe"), Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result != DialogResult.OK)
                    return false;
            }
            if (ProcessEx.Terminate(locks))
                ProcessEx.Start("%CurDir%\\Helper\\rvta\\RefreshVisibleTrayArea.exe");
            return true;
        }

        internal static void ApplicationExit(int exitCode = 0)
        {
            Environment.ExitCode = exitCode;
            Application.Exit();
        }
    }
}