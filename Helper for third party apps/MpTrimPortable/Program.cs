namespace mpTrimPortable
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var iniPath = PathEx.Combine("%CurDir%\\mpTrim\\mpTrim.ini");

                    int left;
                    if (!int.TryParse(Ini.Read("Settings", "Left", iniPath), out left))
                        left = (int)Math.Round(Screen.PrimaryScreen.WorkingArea.Width / 2f - 335f / 2f);
                    int top;
                    if (!int.TryParse(Ini.Read("Settings", "Top", iniPath), out top))
                        top = (int)Math.Round(Screen.PrimaryScreen.WorkingArea.Height / 2f - 410f / 2f);
                    Reg.CreateNewSubKey("HKCU\\Software\\mpTrim");
                    Reg.WriteValue("HKCU", "Software\\mpTrim", "MainFormLeft", left, Reg.RegValueKind.DWord);
                    Reg.WriteValue("HKCU", "Software\\mpTrim", "MainFormTop", top, Reg.RegValueKind.DWord);

                    using (var p = ProcessEx.Start("%CurDir%\\mpTrim\\mpTrim.exe", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    var fLeft = Reg.ReadStringValue("HKCU\\Software\\mpTrim", "MainFormLeft");
                    var fTop = Reg.ReadStringValue("HKCU\\Software\\mpTrim", "MainFormTop");
                    if (!string.IsNullOrWhiteSpace(fLeft) && fLeft != left.ToString() &&
                        !string.IsNullOrWhiteSpace(fTop) && fTop != top.ToString())
                    {
                        Ini.File(iniPath);
                        Ini.Write("Settings", "Left", fLeft);
                        Ini.Write("Settings", "Top", fTop);
                    }
                    Reg.RemoveExistSubKey("HKCU\\Software\\mpTrim");
                }
                else
                    ProcessEx.Start("%CurDir%\\mpTrim\\mpTrim.exe");
        }
    }
}
