namespace NoDev.Horizon.Forms.Misc
{
    partial class ProgramSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgramSettings));
            this.ckAlwaysOnTop = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckDeviceExplorerEnabled = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckCreateBackups = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.cmdProfileManager = new NoDev.Horizon.Controls.SquareButton();
            this.SuspendLayout();
            // 
            // ckAlwaysOnTop
            // 
            // 
            // 
            // 
            this.ckAlwaysOnTop.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ckAlwaysOnTop.FocusCuesEnabled = false;
            this.ckAlwaysOnTop.Location = new System.Drawing.Point(12, 12);
            this.ckAlwaysOnTop.Name = "ckAlwaysOnTop";
            this.ckAlwaysOnTop.Size = new System.Drawing.Size(146, 27);
            this.ckAlwaysOnTop.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ckAlwaysOnTop.TabIndex = 0;
            this.ckAlwaysOnTop.Tag = "AlwaysOnTop";
            this.ckAlwaysOnTop.Text = "Always on Top";
            // 
            // ckDeviceExplorerEnabled
            // 
            // 
            // 
            // 
            this.ckDeviceExplorerEnabled.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ckDeviceExplorerEnabled.FocusCuesEnabled = false;
            this.ckDeviceExplorerEnabled.Location = new System.Drawing.Point(12, 45);
            this.ckDeviceExplorerEnabled.Name = "ckDeviceExplorerEnabled";
            this.ckDeviceExplorerEnabled.Size = new System.Drawing.Size(146, 27);
            this.ckDeviceExplorerEnabled.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ckDeviceExplorerEnabled.TabIndex = 1;
            this.ckDeviceExplorerEnabled.Tag = "DeviceExplorerEnabled";
            this.ckDeviceExplorerEnabled.Text = "Device Explorer Enabled";
            // 
            // ckCreateBackups
            // 
            // 
            // 
            // 
            this.ckCreateBackups.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ckCreateBackups.FocusCuesEnabled = false;
            this.ckCreateBackups.Location = new System.Drawing.Point(12, 78);
            this.ckCreateBackups.Name = "ckCreateBackups";
            this.ckCreateBackups.Size = new System.Drawing.Size(146, 27);
            this.ckCreateBackups.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ckCreateBackups.TabIndex = 3;
            this.ckCreateBackups.Tag = "BackupsEnabled";
            this.ckCreateBackups.Text = "Create Backups";
            // 
            // cmdProfileManager
            // 
            this.cmdProfileManager.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdProfileManager.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdProfileManager.FocusCuesEnabled = false;
            this.cmdProfileManager.Location = new System.Drawing.Point(12, 115);
            this.cmdProfileManager.Name = "cmdProfileManager";
            this.cmdProfileManager.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdProfileManager.Size = new System.Drawing.Size(146, 35);
            this.cmdProfileManager.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdProfileManager.TabIndex = 4;
            this.cmdProfileManager.Text = "Open Profile Manager";
            this.cmdProfileManager.Click += new System.EventHandler(this.cmdProfileManager_Click);
            // 
            // ProgramSettings
            // 
            this.ClientSize = new System.Drawing.Size(170, 162);
            this.Controls.Add(this.cmdProfileManager);
            this.Controls.Add(this.ckCreateBackups);
            this.Controls.Add(this.ckDeviceExplorerEnabled);
            this.Controls.Add(this.ckAlwaysOnTop);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ProgramSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Controls.CheckBoxX ckAlwaysOnTop;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckDeviceExplorerEnabled;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckCreateBackups;
        private Controls.SquareButton cmdProfileManager;
    }
}