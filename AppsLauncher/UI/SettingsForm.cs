namespace AppsLauncher.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using LangResources;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;

    public partial class SettingsForm : Form
    {
        private readonly string _selectedItem;
        private int[] _customColors;
        private DialogResult _result;

        public SettingsForm(string selectedItem)
        {
            InitializeComponent();
            _selectedItem = selectedItem;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            if (Main.ScreenDpi > 96)
                Font = SystemFonts.CaptionFont;

            Icon = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.SystemControl, Main.SystemResourcePath);

            foreach (TabPage tab in tabCtrl.TabPages)
                tab.BackColor = Main.Colors.BaseDark;

            locationBtn.BackgroundImage = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            fileTypesMenu.EnableAnimation();
            fileTypesMenu.SetFixedSingle();
            associateBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            try
            {
                restoreFileTypesBtn.Image = new Bitmap(28, 16);
                using (var g = Graphics.FromImage(restoreFileTypesBtn.Image))
                {
                    g.DrawImage(ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath).ToBitmap(), 0, 0);
                    g.DrawImage(ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Undo, Main.SystemResourcePath).ToBitmap(), 12, 0);
                }
            }
            catch
            {
                restoreFileTypesBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
                restoreFileTypesBtn.ImageAlign = ContentAlignment.MiddleLeft;
                restoreFileTypesBtn.Text = @"<=";
                if (restoreFileTypesBtn.Image != null)
                    restoreFileTypesBtn.TextAlign = ContentAlignment.MiddleRight;
            }

            previewBg.BackgroundImage = Main.BackgroundImage.Redraw((int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1);
            previewBg.BackgroundImageLayout = Main.BackgroundImageLayout;
            previewLogoBox.BackgroundImage = Resources.PortableApps_Logo_gray.Redraw(previewLogoBox.Height, previewLogoBox.Height);
            var exeIco = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.ExeFile, Main.SystemResourcePath);
            if (exeIco != null)
            {
                previewImgList.Images.Add(exeIco.ToBitmap());
                previewImgList.Images.Add(exeIco.ToBitmap());
            }

            foreach (var btn in new[] { saveBtn, exitBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }
            var strAppNames = Main.AppsInfo.Select(x => x.LongName).ToArray();
            var objAppNames = new object[strAppNames.Length];
            Array.Copy(strAppNames, objAppNames, objAppNames.Length);
            appsBox.Items.AddRange(objAppNames);

            appsBox.SelectedItem = _selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            fileTypes.MaxLength = short.MaxValue;
            addToShellBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            rmFromShellBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();

            LoadSettings();
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            var timer = new Timer(components)
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < 1d)
                {
                    Opacity += .1d;
                    return;
                }
                timer.Dispose();
                if (TopMost)
                    TopMost = false;
                _result = DialogResult.No;
            };
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) =>
            DialogResult = _result;

        private void LoadSettings()
        {
            var lang = Ini.Read<string>("Settings", "Lang", Lang.SystemUi);
            if (!setLang.Items.Contains(lang))
                lang = "en-US";
            setLang.SelectedItem = lang;
            Lang.ConfigLang = lang;

            Lang.SetControlLang(this);

            var title = Lang.GetText(nameof(en_US.settingsBtn));
            if (!string.IsNullOrWhiteSpace(title))
                Text = title;

            for (var i = 0; i < fileTypesMenu.Items.Count; i++)
                fileTypesMenu.Items[i].Text = Lang.GetText(fileTypesMenu.Items[i].Name);

            Main.SetFont(this, false);
            Main.SetFont(tabPage1);
            foreach (Control c in tabPage2.Controls)
                if (c is CheckBox || c is ComboBox || c is Label)
                    Main.SetFont(c, false);
                else if (c is Panel)
                    Main.SetFont(c);
            Main.SetFont(tabPage3);

            var value = Ini.Read("Settings", "Window.Opacity", 0);
            opacityNum.Value = value >= opacityNum.Minimum && value <= opacityNum.Maximum ? value : 95;

            value = Ini.Read("Settings", "Window.FadeInEffect", 0);
            fadeInCombo.SelectedIndex = value < fadeInCombo.Items.Count ? value : 0;

            value = Ini.Read("Settings", "Window.FadeInDuration", 0);
            fadeInNum.Value = value >= fadeInNum.Minimum && value <= fadeInNum.Maximum ? value : 100;

            defBgCheck.Checked = !File.Exists(PathEx.Combine(Main.TmpDir, "ImageBg.dat"));
            if (bgLayout.Items.Count > 0)
                bgLayout.Items.Clear();
            for (var i = 0; i < 5; i++)
                bgLayout.Items.Add(Lang.GetText($"{bgLayout.Name}Option{i}"));

            value = Ini.Read("Settings", "Window.BackgroundImageLayout", 1);
            bgLayout.SelectedIndex = value > 0 && value < bgLayout.Items.Count ? value : 1;

            _customColors = Ini.Read("Settings", "Window.CustomColors", default(int[]));
            mainColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.Base").FromHtmlToColor(Main.Colors.System);
            controlColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.Control").FromHtmlToColor(SystemColors.Window);
            controlTextColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.ControlText").FromHtmlToColor(SystemColors.WindowText);
            btnColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.Button").FromHtmlToColor(SystemColors.ButtonFace);
            btnHoverColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.ButtonHover").FromHtmlToColor(ProfessionalColors.ButtonSelectedHighlight);
            btnTextColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.ButtonText").FromHtmlToColor(SystemColors.ControlText);

            hScrollBarCheck.Checked = Ini.Read("Settings", "Window.HideHScrollBar", false);

            StylePreviewUpdate();

            appDirs.Text = Ini.Read("Settings", "AppDirs").DecodeStringFromBase64();

            if (startMenuIntegration.Items.Count > 0)
                startMenuIntegration.Items.Clear();
            for (var i = 0; i < 2; i++)
                startMenuIntegration.Items.Add(Lang.GetText($"{startMenuIntegration.Name}Option{i}"));
            startMenuIntegration.SelectedIndex = Ini.Read("Settings", "StartMenuIntegration", 0) == 1 ? 1 : 0;

            if (defaultPos.Items.Count > 0)
                defaultPos.Items.Clear();
            for (var i = 0; i < 2; i++)
                defaultPos.Items.Add(Lang.GetText($"{defaultPos.Name}Option{i}"));

            value = Ini.Read("Settings", "Window.DefaultPosition", 0);
            defaultPos.SelectedIndex = value > 0 && value < defaultPos.Items.Count ? value : 0;
            if (updateCheck.Items.Count > 0)
                updateCheck.Items.Clear();
            for (var i = 0; i < 10; i++)
                updateCheck.Items.Add(Lang.GetText($"{updateCheck.Name}Option{i}"));

            value = Ini.Read("Settings", "UpdateCheck", 4);
            if (value < 0)
            {
                Ini.Write("Settings", "UpdateCheck", 4);
                Ini.WriteAll();
            }
            updateCheck.SelectedIndex = value > 0 && value < updateCheck.Items.Count ? value : 0;
            if (updateChannel.Items.Count > 0)
                updateChannel.Items.Clear();
            for (var i = 0; i < 2; i++)
                updateChannel.Items.Add(Lang.GetText($"{updateChannel.Name}Option{i}"));

            value = Ini.Read("Settings", "UpdateChannel", 0);
            updateChannel.SelectedIndex = value > 0 ? 1 : 0;

            if (!saveBtn.Focused)
                saveBtn.Select();
        }

        private void ToolTipAtMouseEnter(object sender, EventArgs e)
        {
            if (sender is Control owner)
                toolTip.SetToolTip(owner, Lang.GetText($"{owner.Name}Tip"));
        }

        private void ExitBtn_Click(object sender, EventArgs e) =>
            Close();

        private void AppsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedApp = (sender as ComboBox)?.SelectedItem?.ToString();
            var appInfo = Main.GetAppInfo(selectedApp);
            if (!appInfo.LongName.EqualsEx(selectedApp))
                return;
            fileTypes.Text = Ini.Read(appInfo.ShortName, "FileTypes");
            var restPointDir = PathEx.Combine("%CurDir%\\Restoration", Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appInfo.ShortName, "FileAssociation");
            restoreFileTypesBtn.Enabled = Directory.Exists(restPointDir) && Directory.GetFiles(restPointDir, "*.ini", SearchOption.AllDirectories).Length > 0;
            restoreFileTypesBtn.Visible = restoreFileTypesBtn.Enabled;
            startArgsFirst.Text = Ini.Read(appInfo.ShortName, "StartArgs.First");
            var argsDecode = startArgsFirst.Text.DecodeStringFromBase64();
            if (!string.IsNullOrEmpty(argsDecode))
                startArgsFirst.Text = argsDecode;
            startArgsLast.Text = Ini.Read(appInfo.ShortName, "StartArgs.Last");
            argsDecode = startArgsLast.Text.DecodeStringFromBase64();
            if (!string.IsNullOrEmpty(argsDecode))
                startArgsLast.Text = argsDecode;
            noConfirmCheck.Checked = Ini.Read(appInfo.ShortName, "NoConfirm", false);
            runAsAdminCheck.Checked = Ini.Read(appInfo.ShortName, "RunAsAdmin", false);
            noUpdatesCheck.Checked = Ini.Read(appInfo.ShortName, "NoUpdates", false);
        }

        private void LocationBtn_Click(object sender, EventArgs e) =>
            Main.OpenAppLocation(appsBox.SelectedItem.ToString());

        private void FileTypesMenu_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripMenuItem)?.Name)
            {
                case "fileTypesMenuItem1":
                    if (!string.IsNullOrEmpty(fileTypes.SelectedText))
                        Clipboard.SetText(fileTypes.SelectedText);
                    break;
                case "fileTypesMenuItem2":
                    if (Clipboard.ContainsText())
                        if (string.IsNullOrEmpty(fileTypes.SelectedText))
                        {
                            var start = fileTypes.SelectionStart;
                            fileTypes.Text = fileTypes.Text.Insert(start, Clipboard.GetText());
                            fileTypes.SelectionStart = start + Clipboard.GetText().Length;
                        }
                        else
                            fileTypes.SelectedText = Clipboard.GetText();
                    break;
                case "fileTypesMenuItem3":
                    var appPath = Main.GetAppPath(appsBox.SelectedItem.ToString());
                    if (File.Exists(appPath))
                    {
                        var appDir = Path.GetDirectoryName(appPath);
                        if (!string.IsNullOrEmpty(appDir))
                        {
                            var iniPath = Path.Combine(appDir, "App\\AppInfo\\appinfo.ini");
                            if (!File.Exists(iniPath))
                                iniPath = Path.ChangeExtension(appPath, ".ini");
                            if (File.Exists(iniPath))
                            {
                                var types = Ini.Read("Associations", "FileTypes", iniPath);
                                if (!string.IsNullOrWhiteSpace(types))
                                {
                                    fileTypes.Text = types.RemoveChar(' ');
                                    return;
                                }
                            }
                        }
                    }
                    MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.NoDefaultTypesFoundMsg)), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    break;
            }
        }

        private bool FileTypesConflict()
        {
            var appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
            if (!appInfo.LongName.EqualsEx(appsBox.SelectedItem.ToString()))
                return false;
            var alreadyDefined = new Dictionary<string, List<string>>();
            Main.AppConfigs = new List<string>();
            foreach (var section in Main.AppConfigs)
            {
                if (section.EqualsEx(appInfo.ShortName))
                    continue;
                var types = Ini.Read(section, "FileTypes");
                if (string.IsNullOrWhiteSpace(types))
                    continue;
                var textBoxTypes = fileTypes.Text.RemoveChar('*', '.').Split(',').ToList();
                var configTypes = types.RemoveChar('*', '.').Split(',').ToList();
                foreach (var type in textBoxTypes)
                    if (configTypes.ContainsEx(type))
                        if (!alreadyDefined.ContainsKey(section))
                            alreadyDefined.Add(section, new List<string> { type });
                        else
                        {
                            if (!alreadyDefined[section].ContainsEx(type))
                                alreadyDefined[section].Add(type);
                        }
            }
            if (alreadyDefined.Count <= 0)
                return false;
            var msg = string.Empty;
            var sep = new string('-', 75);
            foreach (var entry in alreadyDefined)
            {
                string appName;
                try
                {
                    appName = Main.AppsInfo.First(x => x.ShortName.EqualsEx(entry.Key)).LongName;
                }
                catch
                {
                    Ini.RemoveSection(entry.Key);
                    Ini.WriteAll();
                    continue;
                }
                var types = entry.Value.ToArray().Sort().Join("; ");
                msg = $"{msg}{sep}{Environment.NewLine}{appName}: {types}{Environment.NewLine}";
            }
            if (string.IsNullOrEmpty(msg))
                return false;
            msg += sep;
            return MessageBoxEx.Show(this, string.Format(Lang.GetText(nameof(en_US.associateConflictMsg)), msg), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes;
        }

        private void AssociateBtn_Click(object sender, EventArgs e)
        {
            var owner = sender as Control;
            if (owner == null)
                return;
            var isNull = string.IsNullOrWhiteSpace(fileTypes.Text);
            if (!isNull)
                if (fileTypes.Text.Contains(","))
                    isNull = fileTypes.Text.Split(',').Where(s => !s.StartsWith(".")).ToArray().Length == 0;
                else
                    isNull = fileTypes.Text.StartsWith(".");
            if (isNull)
            {
                MessageBoxEx.Show(this, Lang.GetText($"{owner.Name}Msg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var appName = Main.GetAppInfo(appsBox.SelectedItem.ToString()).ShortName;
            if (string.IsNullOrWhiteSpace(appName) || FileTypesConflict())
            {
                MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!fileTypes.Text.EqualsEx(Ini.Read(appName, "FileTypes")))
                SaveBtn_Click(saveBtn, EventArgs.Empty);
            Main.AssociateFileTypesHandler(appName, this);
        }

        private void RestoreFileTypesBtn_Click(object sender, EventArgs e)
        {
            var appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
            if (string.IsNullOrWhiteSpace(appInfo.ShortName))
            {
                MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Main.RestoreFileTypesHandler(appInfo.ShortName);
        }

        private void SetBgBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { CheckFileExists = true, CheckPathExists = true, Multiselect = false })
            {
                var path = PathEx.Combine(PathEx.LocalDir, "Assets\\bg");
                if (Directory.Exists(path))
                    dialog.InitialDirectory = path;
                var imgCodecs = ImageCodecInfo.GetImageEncoders();
                var codecExts = new List<string>();
                for (var i = 0; i < imgCodecs.Length; i++)
                {
                    codecExts.Add(imgCodecs[i].FilenameExtension.ToLower());
                    dialog.Filter = string.Format("{0}{1}{2} ({3})|{3}", dialog.Filter, i > 0 ? "|" : string.Empty, imgCodecs[i].CodecName.Substring(8).Replace("Codec", "Files").Trim(), codecExts[codecExts.Count - 1]);
                }
                dialog.Filter = string.Format("{0}|Image Files ({1})|{1}", dialog.Filter, codecExts.Join(";"));
                dialog.FilterIndex = imgCodecs.Length + 1;
                dialog.ShowDialog();
                if (!File.Exists(dialog.FileName))
                    return;
                try
                {
                    var img = Image.FromFile(dialog.FileName).Redraw(SmoothingMode.HighQuality, 2048);
                    if (!Directory.Exists(Main.TmpDir))
                        Directory.CreateDirectory(Main.TmpDir);
                    var bgPath = PathEx.Combine(Main.TmpDir, "ImageBg.dat");
                    File.WriteAllBytes(bgPath, img.SerializeObject());
                    previewBg.BackgroundImage = img.Redraw((int)Math.Round(img.Width * .65f) + 1, (int)Math.Round(img.Height * .65f) + 1);
                    defBgCheck.Checked = false;
                    _result = DialogResult.Yes;
                    MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationCompletedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationFailedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void DefBgCheck_CheckedChanged(object sender, EventArgs e)
        {
            var owner = sender as CheckBox;
            if (owner == null)
                return;
            try
            {
                if (!owner.Checked)
                {
                    var bgPath = PathEx.Combine(Main.TmpDir, "ImageBg.dat");
                    var bgImg = File.ReadAllBytes(bgPath).DeserializeObject<Image>();
                    previewBg.BackgroundImage = bgImg.Redraw((int)Math.Round(bgImg.Width * .65f) + 1, (int)Math.Round(bgImg.Height * .65f) + 1);
                }
                else
                    previewBg.BackgroundImage = Depiction.DimEmpty;
            }
            catch
            {
                if (!owner.Checked)
                    owner.Checked = true;
            }
        }

        private void BgLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_result == DialogResult.No)
                _result = DialogResult.Yes;
            StylePreviewUpdate();
        }

        private void ColorPanel_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Panel owner)
                owner.BackColor = Color.FromArgb(128, owner.BackColor.R, owner.BackColor.G, owner.BackColor.B);
        }

        private void ColorPanel_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Panel owner)
                owner.BackColor = Color.FromArgb(owner.BackColor.R, owner.BackColor.G, owner.BackColor.B);
        }

        private void ColorPanel_Click(object sender, EventArgs e)
        {
            var owner = sender as Panel;
            if (owner == null)
                return;
            string title = null;
            try
            {
                title = Controls.Find(owner.Name + "Label", true).First().Text;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            using (var dialog = new ColorDialogEx(this, title)
            {
                AllowFullOpen = true,
                AnyColor = true,
                SolidColorOnly = true,
                Color = owner.BackColor,
                FullOpen = true
            })
            {
                if (_customColors?.Length > 0)
                    dialog.CustomColors = _customColors;
                if (dialog.ShowDialog() != DialogResult.Cancel)
                {
                    if (dialog.Color != owner.BackColor)
                        owner.BackColor = Color.FromArgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
                    if (dialog.CustomColors != _customColors)
                        _customColors = dialog.CustomColors;
                }
            }
            if (_result == DialogResult.No)
                _result = DialogResult.Yes;
            StylePreviewUpdate();
        }

        private void ResetColorsBtn_Click(object sender, EventArgs e)
        {
            mainColorPanel.BackColor = Main.Colors.System;
            controlColorPanel.BackColor = SystemColors.Window;
            controlTextColorPanel.BackColor = SystemColors.WindowText;
            btnColorPanel.BackColor = SystemColors.ButtonFace;
            btnHoverColorPanel.BackColor = ProfessionalColors.ButtonSelectedHighlight;
            btnTextColorPanel.BackColor = SystemColors.ControlText;
            if (_result == DialogResult.No)
                _result = DialogResult.Yes;
            StylePreviewUpdate();
        }

        private void PreviewAppList_Paint(object sender, PaintEventArgs e)
        {
            var owner = sender as Panel;
            if (owner == null)
                return;
            using (var gr = e.Graphics)
            {
                gr.TranslateTransform((int)(owner.Width / (Math.PI * 2)), owner.Width + 40);
                gr.RotateTransform(-70);
                gr.TextRenderingHint = TextRenderingHint.AntiAlias;
                using (Brush b = new SolidBrush(Color.FromArgb(50, (byte)~owner.BackColor.R, (byte)~owner.BackColor.G, (byte)~owner.BackColor.B)))
                    gr.DrawString("Preview", new Font("Comic Sans MS", 24f), b, 0f, 0f);
            }
        }

        private void ScrollBarCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (_result == DialogResult.No)
                _result = DialogResult.Yes;
            StylePreviewUpdate();
        }

        private void StylePreviewUpdate()
        {
            previewBg.BackgroundImageLayout = (ImageLayout)bgLayout.SelectedIndex;
            previewMainColor.BackColor = mainColorPanel.BackColor;
            previewAppList.ForeColor = controlTextColorPanel.BackColor;
            previewAppList.BackColor = controlColorPanel.BackColor;
            previewAppListPanel.BackColor = controlColorPanel.BackColor;
            foreach (var b in new[] { previewBtn1, previewBtn2 })
            {
                b.ForeColor = btnTextColorPanel.BackColor;
                b.BackColor = btnColorPanel.BackColor;
                b.FlatAppearance.MouseOverBackColor = btnHoverColorPanel.BackColor;
            }
            previewHScrollBar.Visible = !hScrollBarCheck.Checked;
        }

        private void ShellBtns_Click(object sender, EventArgs e) =>
            Main.SystemIntegrationHandler(sender as Button == addToShellBtn);

        private void ShellBtns_TextChanged(object sender, EventArgs e)
        {
            if (sender is Button owner)
                owner.TextAlign = owner.Text.Length < 22 ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleRight;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            var section = Main.GetAppInfo(appsBox.SelectedItem.ToString()).ShortName;
            if (!string.IsNullOrWhiteSpace(section))
            {
                var types = string.Empty;
                if (!string.IsNullOrWhiteSpace(fileTypes.Text))
                    if (e == EventArgs.Empty || !FileTypesConflict())
                    {
                        var typesList = new List<string>();
                        foreach (var item in $"{fileTypes.Text},".Split(','))
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            var type = new string(item.ToCharArray().Where(c => !Path.GetInvalidFileNameChars().Contains(c) && !char.IsWhiteSpace(c)).ToArray());
                            if (string.IsNullOrWhiteSpace(type) || type.Length < 1)
                                continue;
                            if (type.StartsWith("."))
                            {
                                while (type.Contains(".."))
                                    type = type.Replace("..", ".");
                                if (typesList.ContainsEx(type) || typesList.ContainsEx(type.Substring(1)))
                                    continue;
                            }
                            else
                            {
                                if (typesList.ContainsEx(type) || typesList.ContainsEx($".{type}"))
                                    continue;
                            }
                            if (type.Length == 1 && type.StartsWith("."))
                                continue;
                            typesList.Add(type);
                        }
                        if (typesList.Count > 0)
                        {
                            typesList.Sort();
                            types = typesList.Join(",");
                            fileTypes.Text = types;
                        }
                    }
                    else
                        fileTypes.Text = Ini.Read(section, "FileTypes");

                Ini.Write(section, "FileTypes", !string.IsNullOrWhiteSpace(types) ? types : null);

                Ini.Write(section, "StartArgs.First", !string.IsNullOrWhiteSpace(startArgsFirst.Text) ? startArgsFirst.Text.EncodeToBase64() : null);
                Ini.Write(section, "StartArgs.Last", !string.IsNullOrWhiteSpace(startArgsLast.Text) ? startArgsLast.Text.EncodeToBase64() : null);

                Ini.Write(section, "NoConfirm", noConfirmCheck.Checked ? (bool?)true : null);
                Ini.Write(section, "RunAsAdmin", runAsAdminCheck.Checked ? (bool?)true : null);
                Ini.Write(section, "NoUpdates", noUpdatesCheck.Checked ? (bool?)true : null);
            }

            if (defBgCheck.Checked)
                try
                {
                    var bgPath = PathEx.Combine(Main.TmpDir, "ImageBg.dat");
                    if (File.Exists(bgPath))
                    {
                        File.Delete(bgPath);
                        _result = DialogResult.Yes;
                    }
                    Main.BackgroundImage = Depiction.DimEmpty;
                    bgLayout.SelectedIndex = 1;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            Ini.Write("Settings", "Window.Opacity", opacityNum.Value != 95 ? (int?)opacityNum.Value : null);
            Ini.Write("Settings", "Window.FadeInEffect", fadeInCombo.SelectedIndex != 0 ? (int?)fadeInCombo.SelectedIndex : null);
            Ini.Write("Settings", "Window.FadeInDuration", fadeInNum.Value != 100 ? (int?)fadeInNum.Value : null);
            Ini.Write("Settings", "Window.BackgroundImageLayout", bgLayout.SelectedIndex != 1 ? (int?)bgLayout.SelectedIndex : null);

            Ini.Write("Settings", "Window.CustomColors", _customColors?.Length > 0 ? _customColors : null);
            var color = mainColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.Base", color != Main.Colors.System ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = controlColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.Control", color != SystemColors.Window ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = controlTextColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.ControlText", color != SystemColors.WindowText ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = btnColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.Button", color != SystemColors.ButtonFace ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = btnHoverColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.ButtonHover", color != ProfessionalColors.ButtonSelectedHighlight ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = btnTextColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.ButtonText", color != SystemColors.ControlText ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);

            Ini.Write("Settings", "Window.HideHScrollBar", hScrollBarCheck.Checked ? (bool?)hScrollBarCheck.Checked : null);

            string dirs = null;
            if (!string.IsNullOrWhiteSpace(appDirs.Text))
            {
                var dirList = new List<string>();
                var tmpDir = appDirs.Text + Environment.NewLine;
                foreach (var item in tmpDir.SplitNewLine())
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;
                    var dir = PathEx.Combine(item);
                    try
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        dir = EnvironmentEx.GetVariablePathFull(dir);
                        if (!dirList.ContainsEx(dir))
                            dirList.Add(dir);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
                if (dirList.Count > 0)
                {
                    dirList.Sort();
                    dirs = dirList.Join(Environment.NewLine);
                    appDirs.Text = dirs;
                }
            }

            Ini.Write("Settings", "AppDirs", !string.IsNullOrWhiteSpace(dirs) ? dirs.EncodeToBase64() : null);

            Ini.Write("Settings", "StartMenuIntegration", startMenuIntegration.SelectedIndex != 0 ? (bool?)true : null);
            if (startMenuIntegration.SelectedIndex == 0)
                try
                {
                    var startMenuFolderPath = PathEx.Combine("%StartMenu%\\Programs");
#if x86
                    var launcherShortcutPath = Path.Combine(startMenuFolderPath, "Apps Launcher.lnk");
#else
                    var launcherShortcutPath = Path.Combine(startMenuFolderPath, "Apps Launcher (64-bit).lnk");
#endif
                    if (File.Exists(launcherShortcutPath))
                        File.Delete(launcherShortcutPath);
                    startMenuFolderPath = Path.Combine(startMenuFolderPath, "Portable Apps");
                    if (Directory.Exists(startMenuFolderPath))
                        Directory.Delete(startMenuFolderPath, true);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            Ini.Write("Settings", "Window.DefaultPosition", defaultPos.SelectedIndex != 0 ? (int?)defaultPos.SelectedIndex : null);

            Ini.Write("Settings", "UpdateCheck", updateCheck.SelectedIndex != 4 ? (int?)updateCheck.SelectedIndex : null);
            Ini.Write("Settings", "UpdateChannel", updateChannel.SelectedIndex != 0 ? (int?)updateChannel.SelectedIndex : null);

            var lang = Ini.Read<string>("Settings", "Lang", Lang.SystemUi);
            if (!lang.EqualsEx(setLang.SelectedItem.ToString()))
            {
                Ini.Write("Settings", "Lang", !Lang.SystemUi.EqualsEx(setLang.SelectedItem.ToString()) ? setLang.SelectedItem : null);
                _result = DialogResult.Yes;
                LoadSettings();
            }

            Ini.WriteAll();
            MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.SavedSettings)), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}
