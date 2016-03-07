using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinSCP_PuTTY_Launcher
{
    public partial class MainForm : Form
    {
        bool CancelExit = true;
        bool IsResizing = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SilDev.Initialization.File(Application.StartupPath, "WinSCP+PuTTYPortable.ini");
            if (File.Exists(SilDev.Initialization.File()))
            {
                int left = short.MinValue, top = short.MinValue, width = short.MinValue, height = short.MinValue;
                string value = SilDev.Initialization.ReadValue("Settings", "Left");
                if (!string.IsNullOrWhiteSpace(value))
                    int.TryParse(value, out left);
                value = SilDev.Initialization.ReadValue("Settings", "Top");
                if (!string.IsNullOrWhiteSpace(value))
                    int.TryParse(value, out top);
                value = SilDev.Initialization.ReadValue("Settings", "Width");
                if (!string.IsNullOrWhiteSpace(value))
                    int.TryParse(value, out width);
                value = SilDev.Initialization.ReadValue("Settings", "Height");
                if (!string.IsNullOrWhiteSpace(value)) 
                    int.TryParse(value, out height);
                bool maximized = false;
                value = SilDev.Initialization.ReadValue("Settings", "Maximized");
                if (!string.IsNullOrWhiteSpace(value))
                    bool.TryParse(value, out maximized);
                if (width >= 640 && height >= 480)
                    Size = new Size(width, height);
                if (left > short.MinValue && top > short.MinValue)
                {
                    Point location = new Point(left, top);
                    Rectangle workingArea = new Rectangle(short.MaxValue, short.MaxValue, short.MinValue, short.MinValue);
                    foreach (Screen screen in Screen.AllScreens)
                        workingArea = Rectangle.Union(workingArea, screen.Bounds);
                    if (location.X < (workingArea.Width - Size.Width) && location.X > 0 && location.Y < (workingArea.Height - Height) && location.Y > 0)
                    {
                        StartPosition = FormStartPosition.Manual;
                        Location = location;
                    }
                }
                if (maximized)
                    WindowState = FormWindowState.Maximized;
            }
            bwWorker.RunWorkerAsync();
            PuttyTabber.Enabled = true;
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e) =>
            IsResizing = true;

        private void MainForm_Resize(object sender, EventArgs e)
        {
            foreach (IntPtr hwnd in Main.PuTTYTabs.Values)
                SilDev.WinAPI.SafeNativeMethods.MoveWindow(hwnd, 0, 0, puttyTabCtrl.SelectedTab.Width, puttyTabCtrl.SelectedTab.Height, true);
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e) =>
            IsResizing = false;

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (var hwnd in Main.PuTTYTabs)
                {
                    Process p = Process.GetProcessById(hwnd.Key);
                    p.CloseMainWindow();
                    p.WaitForExit(10);
                    if (!p.HasExited)
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            Main.PuTTYTabs.Clear();
            puttyTabCtrl.TabPages.Clear();
            if (CancelExit)
            {
                if (ShowInTaskbar)
                    ShowInTaskbar = false;
                if (Opacity == 1f)
                    Opacity = 0;
                e.Cancel = CancelExit;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    SilDev.Initialization.WriteValue("Settings", "Left", Left);
                    SilDev.Initialization.WriteValue("Settings", "Top", Top);
                    SilDev.Initialization.WriteValue("Settings", "Width", Width);
                    SilDev.Initialization.WriteValue("Settings", "Height", Height);
                }
                SilDev.Initialization.WriteValue("Settings", "Maximized", WindowState == FormWindowState.Maximized);
                foreach (Process p in Process.GetProcessesByName("PUTTY"))
                {
                    p.CloseMainWindow();
                    p.WaitForExit(10);
                    if (!p.HasExited)
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            SilDev.Reg.RemoveExistSubKey(@"HKEY_CURRENT_USER\Software\SimonTatham");
        }

        private void bwWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string WinSCPPortable_ini = Path.Combine(Main.WinSCPPath, "WinSCPPortable.ini");
            if (!File.Exists(WinSCPPortable_ini))
                File.Create(WinSCPPortable_ini).Close();
            if (File.Exists(WinSCPPortable_ini))
                SilDev.Initialization.WriteValue("AppInfo", "Name", "Portable", WinSCPPortable_ini);
            string WinSCPPortableSettings_ini = Path.Combine(Main.WinSCPPath, @"Data\settings\WinSCPPortableSettings.ini");
            if (File.Exists(WinSCPPortableSettings_ini))
            {
                string oldPath = SilDev.Initialization.ReadValue("WinSCPPortableSettings", "LastDrive", WinSCPPortableSettings_ini);
                if (string.IsNullOrWhiteSpace(oldPath))
                {
                    oldPath = Main.WinSCPPath.Replace(" ", "%20").Replace(@"\", "%5C");
                    SilDev.Initialization.WriteValue("WinSCPPortableSettings", "LastDrive", oldPath, WinSCPPortableSettings_ini);
                }
                string newPath = Main.WinSCPPath.Replace(" ", "%20").Replace(@"\", "%5C");
                SilDev.Initialization.WriteValue("WinSCPPortableSettings", "LastDrive", newPath, WinSCPPortableSettings_ini);
                if (oldPath != newPath)
                {
                    string settingsPath = Path.Combine(Main.WinSCPPath, @"Data\settings\winscp.ini");
                    if (File.Exists(settingsPath))
                    {
                        bool overwriteSettings = false;
                        string settingsContent = string.Empty;
                        using (StreamReader sr = new StreamReader(settingsPath))
                            settingsContent = sr.ReadToEnd();
                        if (settingsContent.Contains(oldPath))
                        {
                            overwriteSettings = true;
                            settingsContent = settingsContent.Replace(oldPath, newPath);
                        }
                        if (overwriteSettings)
                        {
                            File.Delete(settingsPath);
                            File.WriteAllText(settingsPath, settingsContent);
                        }
                    }
                }
            }
            if (File.Exists(Main.PuTTYExePath))
            {
                SilDev.Run.App(new ProcessStartInfo() { FileName = Main.PuTTYExePath });
                for (int i = 0; i < 1496; i++)
                {
                    Thread.Sleep(10);
                    if (Process.GetProcessesByName("PUTTY").Length > 0)
                    {
                        foreach (Process p in Process.GetProcessesByName("PUTTY"))
                        {
                            if (p.MainWindowTitle.ToLower().Contains("putty configuration"))
                            {
                                Main.PuTTYWnd = p.MainWindowHandle;
                                SilDev.WinAPI.HideWindow(Main.PuTTYWnd);
                                break;
                            }
                        }
                        if (Main.PuTTYWnd != IntPtr.Zero)
                            break;
                    }
                }
            }
            SilDev.Run.App(new ProcessStartInfo() { FileName = Main.WinSCPExePath });
            for (int i = 0; i < 224; i++)
            {
                Thread.Sleep(10);
                bool winscpIsIdle = false;
                foreach (Process p in Process.GetProcessesByName("WinSCP"))
                {
                    if (p.MainWindowTitle.ToLower().Contains("winscp "))
                    {
                        winscpIsIdle = true;
                        break;
                    }
                }
                if (winscpIsIdle)
                    break;
            }
            bool isRunning = true;
            if (Main.PuTTYWnd != IntPtr.Zero)
            {
                while (isRunning)
                {
                    isRunning = Process.GetProcessesByName("WinSCP").Length > 0;
                    foreach (Process p in Process.GetProcessesByName("WinSCP"))
                        p.WaitForExit();
                }
                foreach (KeyValuePair<int, IntPtr> p in Main.PuTTYTabs)
                    Process.GetProcessById(p.Key).WaitForExit();
                SilDev.WinAPI.ShowWindow(Main.PuTTYWnd);
                foreach (Process p in Process.GetProcessesByName("PUTTY"))
                {
                    if (p.MainWindowTitle.ToLower().Contains("putty configuration"))
                    {
                        p.CloseMainWindow();
                        break;
                    }
                }
            }
        }

        private void bwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CancelExit = false;
            Close();
        }

        private void PuttyTabber_Tick(object sender, EventArgs e)
        {
            foreach (Process p in Process.GetProcessesByName("PUTTY"))
            {
                if (p.MainWindowHandle == IntPtr.Zero || Main.PuTTYTabs.Keys.Contains(p.Id) || Main.PuTTYTabs.Values.Contains(p.MainWindowHandle) || p.MainWindowHandle == Main.PuTTYWnd || p.MainWindowTitle.ToLower().Contains("putty configuration"))
                    continue;
                if (!ShowInTaskbar)
                    ShowInTaskbar = true;
                if (Opacity != 1f)
                    Opacity = 1f;
                puttyTabCtrl.TabPages.Add(p.MainWindowHandle.ToString());
                puttyTabCtrl.SelectedIndex = puttyTabCtrl.TabPages.Count - 1;
                if (!Main.PuTTYTabs.ContainsKey(p.Id))
                    Main.PuTTYTabs.Add(p.Id, p.MainWindowHandle);
                SilDev.WinAPI.SafeNativeMethods.SetWindowLong(p.MainWindowHandle, -16, (int)(SilDev.WinAPI.SafeNativeMethods.GetWindowLong(p.MainWindowHandle, -16) & ~0x00020000L & ~0x00010000L));
                SilDev.WinAPI.SafeNativeMethods.MoveWindow(p.MainWindowHandle, 0, 0, puttyTabCtrl.SelectedTab.Width, puttyTabCtrl.SelectedTab.Height, true);
                SilDev.WinAPI.SafeNativeMethods.SetParent(p.MainWindowHandle, puttyTabCtrl.SelectedTab.Handle);
                if (!TopMost)
                    TopMost = true;
                Thread.Sleep(5);
                if (TopMost) 
                    TopMost = false;
            }
            int ToRemove = -1;
            foreach (var hwnd in Main.PuTTYTabs)
            {
                try
                {
                    Process.GetProcessById(hwnd.Key);
                }
                catch
                {
                    foreach (TabPage tab in puttyTabCtrl.TabPages)
                    {
                        if (tab.Text == hwnd.Value.ToString())
                        {
                            puttyTabCtrl.TabPages.Remove(tab);
                            break;
                        }
                    }
                    puttyTabCtrl.SelectedIndex = puttyTabCtrl.TabPages.Count - 1;
                    ToRemove = hwnd.Key;
                }
            }
            if (Main.PuTTYTabs.Keys.Contains(ToRemove) && ToRemove != -1)
                Main.PuTTYTabs.Remove(ToRemove);
            if (!IsResizing && puttyTabCtrl.TabCount > 0)
            {
                if (SilDev.WinAPI.SafeNativeMethods.GetForegroundWindow() == Handle)
                {
                    int tabtext = 0;
                    int.TryParse(puttyTabCtrl.SelectedTab.Text, out tabtext);
                    IntPtr hwnd = (IntPtr)tabtext;
                    if (hwnd != IntPtr.Zero)
                        SilDev.WinAPI.SafeNativeMethods.SetForegroundWindow(hwnd);
                }
            }
            if (puttyTabCtrl.TabCount <= 0)
                Close();
        }
    }
}

