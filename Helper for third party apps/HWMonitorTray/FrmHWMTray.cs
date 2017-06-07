namespace HwMonTray
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.Win32;
    using Properties;

    public class FrmHwmTray : Form
    {
        private IContainer components;
        private NotifyIcon _niTray;
        private ContextMenuStrip _cmsTray;
        private ToolStripMenuItem _exitToolStripMenuItem;
        private ToolStripSeparator _toolStripMenuItem1;
        private ToolStripMenuItem _showToolStripMenuItem;
        private Timer _timReg;
        private ToolStripMenuItem _infoToolStripMenuItem;
        private ToolStripSeparator _toolStripMenuItem2;
        private ToolStripMenuItem _enableDataSharingToolStripMenuItem;
        private readonly Process _process = new Process();
        private bool _visible;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        [SuppressMessage("ReSharper", "UnusedVariable")]
        private void InitializeComponent()
        {
            components = new Container();
            var componentResourceManager = new ComponentResourceManager(typeof(FrmHwmTray));
            _niTray = new NotifyIcon(components);
            _cmsTray = new ContextMenuStrip(components);
            _infoToolStripMenuItem = new ToolStripMenuItem();
            _exitToolStripMenuItem = new ToolStripMenuItem();
            _toolStripMenuItem1 = new ToolStripSeparator();
            _showToolStripMenuItem = new ToolStripMenuItem();
            _timReg = new Timer(components);
            _toolStripMenuItem2 = new ToolStripSeparator();
            _enableDataSharingToolStripMenuItem = new ToolStripMenuItem();
            _cmsTray.SuspendLayout();
            SuspendLayout();
            _niTray.ContextMenuStrip = _cmsTray;
            _niTray.Icon = Resources._niTray;
            _niTray.Text = @"CPUID Hardware Monitor (GadgetHost)";
            _niTray.MouseDoubleClick += NiTray_MouseDoubleClick;
            _cmsTray.Items.AddRange(new ToolStripItem[]
                   {
                       _infoToolStripMenuItem,
                       _exitToolStripMenuItem,
                       _toolStripMenuItem2,
                       _enableDataSharingToolStripMenuItem,
                       _toolStripMenuItem1,
                       _showToolStripMenuItem
                   });
            _cmsTray.Name = "_cmsTray";
            _cmsTray.Size = new Size(180, 126);
            _infoToolStripMenuItem.Name = "_infoToolStripMenuItem";
            _infoToolStripMenuItem.Size = new Size(179, 22);
            _infoToolStripMenuItem.Text = @"Info";
            _infoToolStripMenuItem.Click += InfoToolStripMenuItem_Click;
            _exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            _exitToolStripMenuItem.Size = new Size(179, 22);
            _exitToolStripMenuItem.Text = @"Exit";
            _exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            _toolStripMenuItem1.Name = "_toolStripMenuItem1";
            _toolStripMenuItem1.Size = new Size(176, 6);
            _showToolStripMenuItem.Name = "_showToolStripMenuItem";
            _showToolStripMenuItem.Size = new Size(179, 22);
            _showToolStripMenuItem.Text = @"Show / Hide";
            _showToolStripMenuItem.Click += ShowToolStripMenuItem_Click;
            _timReg.Interval = 1000;
            _timReg.Tick += TimReg_Tick;
            _toolStripMenuItem2.Name = "_toolStripMenuItem2";
            _toolStripMenuItem2.Size = new Size(176, 6);
            _enableDataSharingToolStripMenuItem.CheckOnClick = true;
            _enableDataSharingToolStripMenuItem.Name = "_enableDataSharingToolStripMenuItem";
            _enableDataSharingToolStripMenuItem.Size = new Size(179, 22);
            _enableDataSharingToolStripMenuItem.Text = @"Enable Data Sharing";
            _enableDataSharingToolStripMenuItem.Click += EnableDataSharingToolStripMenuItem_Click;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(269, 177);
            Name = "FrmHwmTray";
            Text = @"HWMon GadgetHost";
            Load += FrmHWMTray_Load;
            FormClosing += FrmHWMTray_FormClosing;
            _cmsTray.ResumeLayout(false);
            ResumeLayout(false);
        }

        public FrmHwmTray()
        {
            InitializeComponent();
        }

        private static bool IsMixedBuildTarget()
        {
            var flag = IntPtr.Size == 8;
            if (!flag)
                return false;
            var flag2 = HwMonAccess.Is64Bit();
            return !flag2;
        }

        private void FrmHWMTray_Load(object sender, EventArgs e)
        {
            var arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            var text = Path.Combine(Application.StartupPath, arch?.Equals("x86") == true ? "HWMonitor_x32.exe" : "HWMonitor_x64.exe");
            if (!File.Exists(text))
            {
                MessageBox.Show(@"HWMonitor not found! Put this application into the folder of your HWMonitor.exe.", @"File Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Close();
                return;
            }
            _process.StartInfo.FileName = text;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            _process.Start();
            _process.WaitForInputIdle();
            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;
            Interop.SetWindowText(_process.MainWindowHandle, "CPUID Hardware Monitor (GadgetHost)");
            Interop.ShowWindow(_process.MainWindowHandle, 0);
            Interop.SetWindowLong(_process.MainWindowHandle, -16, Interop.GetWindowLong(_process.MainWindowHandle, -16) & -131073);
            if (IsMixedBuildTarget())
            {
                MessageBox.Show(@"The 32Bit Version of HWMonitor is not supported on a 64Bit OS, use the 64Bit Version of HWMonitor instead!", @"32/64Bit Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                _process.Kill();
                return;
            }
            _niTray.Visible = false;
            _niTray.Visible = true;
            var registryKey = Registry.CurrentUser.CreateSubKey("Software\\CPUID\\HWMonitor\\");
            if (registryKey != null && Convert.ToBoolean(registryKey.GetValue("EnableDataSharing", true)))
            {
                _timReg.Enabled = true;
                _enableDataSharingToolStripMenuItem.Checked = true;
            }
            Hide();
            ShowInTaskbar = false;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            try
            {
                EventHandler method = SafeClose;
                if (InvokeRequired)
                    Invoke(method);
                else
                    Close();
            }
            catch
            {
                Close();
            }
        }

        private void SafeClose(object sender, EventArgs e) =>
            Close();

        private static IntPtr GetHwMonHandle() =>
            Interop.FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "CPUID Hardware Monitor (GadgetHost)");

        private static void ShowWindow(bool show)
        {
            var hwMonHandle = GetHwMonHandle();
            if (hwMonHandle == IntPtr.Zero)
                return;
            var nCmdShow = 0;
            if (show)
                nCmdShow = 9;
            Interop.ShowWindow(hwMonHandle, nCmdShow);
            if (show)
                Interop.SetForegroundWindow(hwMonHandle);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _process.Kill();
            }
            catch
            {
                // ignored
            }
            Close();
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow(!_visible);
            _visible = !_visible;
        }

        private void NiTray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow(!_visible);
            _visible = !_visible;
        }

        private static void TimReg_Tick(object sender, EventArgs e)
        {
            WriteRegistry();
        }

        private static void WriteRegistry()
        {
            try
            {
                var registryKey = Registry.CurrentUser.CreateSubKey("Software\\CPUID\\HWMonitor\\VistaSidebar\\");
                foreach (var current in HwMonAccess.Read())
                    registryKey?.SetValue(string.Concat(current.Type.Substring(0, 1), "[", current.ID, "-", current.Count.ToString(), "]"), current.Value);
            }
            catch
            {
                // ignored
            }
        }

        private static void ClearRegistry()
        {
            try
            {
                var registryKey = Registry.CurrentUser.CreateSubKey("Software\\CPUID\\HWMonitor\\");
                registryKey?.DeleteSubKey("VistaSidebar");
            }
            catch
            {
                // ignored
            }
        }

        private static void FrmHWMTray_FormClosing(object sender, FormClosingEventArgs e) =>
            ClearRegistry();

        private static void InfoToolStripMenuItem_Click(object sender, EventArgs e) =>
            MessageBox.Show(string.Concat(@"HWMonitor - GadgetHost", Environment.NewLine, Environment.NewLine, @"This little helper application extends the fantastic and FREE 'CPUID -Hardware-Monitor' by the following features:", Environment.NewLine, Environment.NewLine, @"-Starting/Running in Background", @"-Fast Access by Trayicon", @"-Share Sensor Data (for Sidebar Gadgets)", Environment.NewLine, Environment.NewLine, @"Version 2.0 - Copyright © Orbmu2k 2011; decompiled and optimized by Si13n7 Dev. ® 2017"), @"HWMonitor - GadgetHost", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        private void EnableDataSharingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var registryKey = Registry.CurrentUser.CreateSubKey("Software\\CPUID\\HWMonitor\\");
            if (_enableDataSharingToolStripMenuItem.Checked)
            {
                _timReg.Enabled = true;
                registryKey?.SetValue("EnableDataSharing", true);
                return;
            }
            _timReg.Enabled = false;
            registryKey?.SetValue("EnableDataSharing", false);
            ClearRegistry();
        }
    }
}
