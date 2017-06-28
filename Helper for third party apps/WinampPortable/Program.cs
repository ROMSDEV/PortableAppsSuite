namespace WinampPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\winamp");
                var appPath = PathEx.Combine(appDir, "winamp.exe");
                if (!File.Exists(appPath))
                    return;

                var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
                var sortArgs = Ini.ReadDirect("Settings", "SortArgs", iniPath).EqualsEx("True");

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(sortArgs));
                    return;
                }

                if (!File.Exists(iniPath))
                {
                    Ini.WriteDirect("Associations", "FileTypes", "mp3,mp2,mp1,aac,vlb,avi,cda,mkv,webm,nsv,nsa,swf,ogg,oga,m4a,mp4,mpg,mpeg,m2v,flac,flv,wma,wmv,asf,aiff,aif,au,avr,caf,htk,iff,mat,paf,pvf,raw,rf64,sd2,sds,sf,voc,w64,wav,wve,xi,mid,midi,rmi,kar,miz,mod,mdz,nst,stm,stz,s3m,s3z,it,itz,xm,xmz,mtm,ult,669,far,amf,okt,ptm,m3u,m3u8,pls,b4s,xspf,wpl,asx", iniPath);
                    Ini.WriteDirect("Settings", "SortArgs", false, iniPath);
                }

                iniPath = PathEx.Combine(PathEx.LocalDir, "Data\\winamp.ini");
                if (!File.Exists(iniPath))
                {
                    var langDir = Path.Combine(appDir, "Lang");
                    if (Directory.Exists(langDir))
                    {
                        var langs = Directory.EnumerateFiles(langDir, "*.wlz").Select(Path.GetFileNameWithoutExtension).ToArray();
                        if (langs.Length > 0)
                        {
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Form langSelection = new LangSelectionForm(langs, iniPath);
                            if (langSelection.ShowDialog() != DialogResult.OK)
                            {
                                Application.Exit();
                                return;
                            }
                        }
                    }
                }
                Ini.WriteDirect("Winamp", "no_registry", 0, iniPath);
                Ini.WriteDirect("WinampReg", "NeedReg", 0, iniPath);
                Ini.WriteDirect("Winamp", "skin", "Big Bento", iniPath, false, true);
                Ini.WriteDirect("WinampReg", "skin", "Big Bento", iniPath, false, true);
                Ini.WriteDirect("Winamp", "eq_data", "24,19,40,46,42,31,19,16,26,32", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "nbkeys", 15, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "version", 2, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "enabled", 0, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "appcommand", 0, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action0", "ghkdc play", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey0", 3629, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action1", "ghkdc pause", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey1", 3620, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action2", "ghkdc stop", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey2", 3619, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action3", "ghkdc prev", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey3", 3617, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action4", "ghkdc next", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey4", 3618, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action5", "ghkdc vup", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey5", 3622, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action6", "ghkdc vdown", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey6", 3624, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action7", "ghkdc forward", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey7", 3623, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action8", "ghkdc rewind", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey8", 3621, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action9", "ghkdc jump", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey9", 3658, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action10", "ghkdc file", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey10", 3660, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action11", "ghkdc stop", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey11", 2226, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action12", "ghkdc play/pause", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey12", 2227, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action13", "ghkdc prev", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey13", 2225, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "action14", "ghkdc next", iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "hotkey14", 2224, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "col1", 212, iniPath, false, true);
                Ini.WriteDirect("gen_hotkeys", "col2", 177, iniPath, false, true);

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\winamp\\Plugins\\ml",
                        "%CurDir%\\Data\\Plugins\\ml"
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\winamp\\gen_jumpex.m3u8",
                        "%CurDir%\\Data\\gen_jumpex.m3u8"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.bm",
                        "%CurDir%\\Data\\winamp.bm"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.bm3",
                        "%CurDir%\\Data\\winamp.bm3"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.bm8",
                        "%CurDir%\\Data\\winamp.bm8"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.ini",
                        "%CurDir%\\Data\\winamp.ini"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.m3u",
                        "%CurDir%\\Data\\winamp.m3u"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.m3u8",
                        "%CurDir%\\Data\\winamp.m3u8"
                    },
                    {
                        "%CurDir%\\App\\winamp\\winamp.q1",
                        "%CurDir%\\Data\\winamp.q1"
                    },
                    {
                        "%CurDir%\\App\\winamp\\links.xml",
                        "%CurDir%\\Data\\links.xml"
                    },
                    {
                        "%CurDir%\\App\\winamp\\studio.xnf",
                        "%CurDir%\\Data\\studio.xnf"
                    },
                    {
                        "%CurDir%\\App\\winamp\\Plugins\\feedback.ini",
                        "%CurDir%\\Data\\Plugins\\feedback.ini"
                    },
                    {
                        "%CurDir%\\App\\winamp\\Plugins\\gen_ml.ini",
                        "%CurDir%\\Data\\Plugins\\gen_ml.ini"
                    },
                    {
                        "%CurDir%\\App\\winamp\\Plugins\\Milkdrop2\\milk2.ini",
                        "%CurDir%\\Data\\Plugins\\Milkdrop2\\milk2.ini"
                    },
                    {
                        "%CurDir%\\App\\winamp\\Plugins\\Milkdrop2\\milk2_img.ini",
                        "%CurDir%\\Data\\Plugins\\Milkdrop2\\milk2_img.ini"
                    },
                    {
                        "%CurDir%\\App\\winamp\\Plugins\\Milkdrop2\\milk2_msg.ini",
                        "%CurDir%\\Data\\Plugins\\Milkdrop2\\milk2_msg.ini"
                    }
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(sortArgs));

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }
    }
}
