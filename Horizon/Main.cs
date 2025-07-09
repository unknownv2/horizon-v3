using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NoDev.Horizon.Controls;
using NoDev.Horizon.Editors.Package_Manager;
using NoDev.Horizon.Forms.Misc;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Rendering;

namespace NoDev.Horizon
{
    internal partial class Main : Office2007RibbonForm
    {
        private static Main _instance;

        internal Main()
        {
            _instance = this;

            InitializeComponent();

            if (Settings.GetBoolean("AlwaysOnTop"))
                this.TopMost = true;

            ApplyStyleChanges();

            ControlManager.Parent = this;

            CreateRibbonTab(ControlGroup.Game, "Games");
            PopulateRibbonPanel(ControlGroup.Game);

            CreateRibbonTab(ControlGroup.Profile, "Profile Mods");
            PopulateRibbonPanel(ControlGroup.Profile);

            CreateRibbonTab(ControlGroup.Tool, "Tools");
            PopulateRibbonPanel(ControlGroup.Tool);

            CreateRibbonTab(ControlGroup.Misc, "Miscellaneous");
            PopulateRibbonPanel(ControlGroup.Misc);

            PopulateMiscEvents();

            MiscSetup();

            InitializeDeviceExplorer();
        }

        internal static Main GetInstance()
        {
            return _instance;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HZN_SHOW)
            {
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.TopMost = true;
            }
            base.WndProc(ref m);
        }

        private void PopulateMiscEvents()
        {
            Control.ControlCollection miscControls = GetRibbonTabFromControlGroup(ControlGroup.Misc).Panel.Controls;
            miscControls.Add(new EventBar("Show Tutorial", Resources.Thumb_Generic_Person_Bubble, (a, b) => new Tutorial().ShowDialog(this)));
            miscControls.Add(new EventBar("Visit XboxMB.com", Resources.Thumb_Generic_Person_Bubble, VisitXboxMB));
        }

        private void MiscSetup()
        {
            this.cmdSettings.Click += (a, b) => ProgramSettings.Show(this);
            ribbonHorizon.Items.Add(this.cmdSettings);

            int winX = Settings.GetInt32("WindowX");
            int winY = Settings.GetInt32("WindowY");
            if (winX != int.MinValue && winY != int.MinValue)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(winX, winY);
                if (Settings.GetBoolean("WindowMaximized"))
                    this.WindowState = FormWindowState.Maximized;
            }
        }

        private static void VisitXboxMB(object sender, EventArgs e)
        {
            Process.Start("http://www.xboxmb.com/");
        }

        private void ApplyStyleChanges()
        {
            Office2007ColorTable colorTable = ((Office2007Renderer)GlobalManager.Renderer).ColorTable;

            colorTable.Form.MdiClientBackgroundImage = null;

            RibbonPredefinedColorSchemes.ApplyOffice2007ColorTable(this);
        }

        internal const string LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
        private void CreateRibbonTab(ControlGroup linkedControlGroup, string tabText)
        {
            var ribbonPanel = new RibbonPanel();
            ribbonPanel.ColorSchemeStyle = eDotNetBarStyle.StyleManagerControlled;
            ribbonPanel.Dock = DockStyle.Fill;
            ribbonPanel.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            
            ribbonHorizon.Controls.Add(ribbonPanel);

            var ribbonTab = new RibbonTabItem();
            ribbonTab.Text = tabText;
            ribbonTab.Tag = linkedControlGroup;
            ribbonTab.Panel = ribbonPanel;
            ribbonHorizon.Items.Add(ribbonTab);
        }

        private void PopulateRibbonPanel(ControlGroup linkedControlGroup)
        {
            RibbonPanel ribbonPanel = GetRibbonTabFromControlGroup(linkedControlGroup).Panel;
            List<ControlInfo> controls = ControlManager.Controls.FindAll(info => info.Group == linkedControlGroup);
            foreach (ControlInfo info in controls)
                ribbonPanel.Controls.Add(new EventBar(info));
        }

        private RibbonTabItem GetRibbonTabFromControlGroup(ControlGroup controlGroup)
        {
            foreach (BaseItem item in ribbonHorizon.Items)
                if (item is RibbonTabItem && item.Tag is ControlGroup && (ControlGroup)item.Tag == controlGroup)
                    return (RibbonTabItem)item;
            throw new Exception(string.Format("No ribbon tabs linked to {0} were found!", controlGroup));
        }

        private void cmdFatxContract_Click(object sender, EventArgs e)
        {
            this.ResizeDeviceExplorer(-25);
        }

        private void cmdFatxExpand_Click(object sender, EventArgs e)
        {
            this.ResizeDeviceExplorer(25);
        }

        private void ResizeDeviceExplorer(int widthChange)
        {
            exFatx.Size = new Size(exFatx.Size.Width + widthChange, exFatx.Size.Height);
            rbFatx.Size = new Size(rbFatx.Size.Width + widthChange, rbFatx.Size.Height);
            progressFatx.Size = new Size(progressFatx.Size.Width + widthChange, progressFatx.Size.Height);
            cmdFatxGear.Size = new Size(cmdFatxGear.Size.Width + widthChange, cmdFatxGear.Size.Height);
            cmdFatxContract.Enabled = exFatx.Size.Width > 375;
            cmdFatxExpand.Enabled = exFatx.Size.Width < 575;
        }

        private void Main_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Main_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
                return;

            if (files.Length > 20)
                DialogBox.Show("Only 20 files will attempted to be opened.");

            var eInfo = (EditorInfo)ControlManager.ControlInfoFromType(typeof(PackageManager));

            for (int x = 0; x < files.Length && x <= 20; x++)
            {
                try
                {
                    ControlManager.Transfer(new ControlManager.TransferParameters(eInfo, new XContentPackage(files[x])));
                }
                catch (Exception ex)
                {
                    DialogBox.Show(string.Format("Failed to open \"{0}\"\n\n{1}", files[x], ex.Message), "Error", MessageBoxIcon.Error);
                }
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                return;

            Settings.Set("WindowMaximized", this.WindowState == FormWindowState.Maximized);
            Settings.Set("WindowX", this.Location.X);
            Settings.Set("WindowY", this.Location.Y);
            Settings.Save();
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var package = new XContentPackage(ofd.FileName);

            package.Mount();

            var io = new Common.IO.EndianIO(package.Drive.Name + "FFFE07D1.gpd", Common.IO.EndianType.Big);
            io.Write((byte)123);
            io.Flush();
            io.Close();

            package.Save();
            package.UnMount();
            package.Close();
            //DialogBox.Show("Not implemented yet.");
        }
    }
}
