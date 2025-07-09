namespace NoDev.Horizon
{
    partial class PackageEditor
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
            this.tabMain = new DevComponents.DotNetBar.RibbonTabItem();
            this.panelMain = new DevComponents.DotNetBar.RibbonPanel();
            this.cmdOpen = new DevComponents.DotNetBar.Office2007StartButton();
            this.cmdSave = new DevComponents.DotNetBar.Office2007StartButton();
            this.cmdPackageManager = new DevComponents.DotNetBar.ButtonItem();
            this.controlRibbon.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Controls.Add(this.panelMain);
            this.controlRibbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.cmdOpen,
            this.cmdSave,
            this.tabMain});
            this.controlRibbon.QuickToolbarItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.cmdPackageManager});
            this.controlRibbon.Size = new System.Drawing.Size(561, 371);
            this.controlRibbon.SystemText.MaximizeRibbonText = "&Maximize the Ribbon";
            this.controlRibbon.SystemText.MinimizeRibbonText = "Mi&nimize the Ribbon";
            this.controlRibbon.SystemText.QatAddItemText = "&Add to Quick Access Toolbar";
            this.controlRibbon.SystemText.QatCustomizeMenuLabel = "<b>Customize Quick Access Toolbar</b>";
            this.controlRibbon.SystemText.QatCustomizeText = "&Customize Quick Access Toolbar...";
            this.controlRibbon.SystemText.QatDialogAddButton = "&Add >>";
            this.controlRibbon.SystemText.QatDialogCancelButton = "Cancel";
            this.controlRibbon.SystemText.QatDialogCaption = "Customize Quick Access Toolbar";
            this.controlRibbon.SystemText.QatDialogCategoriesLabel = "&Choose commands from:";
            this.controlRibbon.SystemText.QatDialogOkButton = "OK";
            this.controlRibbon.SystemText.QatDialogPlacementCheckbox = "&Place Quick Access Toolbar below the Ribbon";
            this.controlRibbon.SystemText.QatDialogRemoveButton = "&Remove";
            this.controlRibbon.SystemText.QatPlaceAboveRibbonText = "&Place Quick Access Toolbar above the Ribbon";
            this.controlRibbon.SystemText.QatPlaceBelowRibbonText = "&Place Quick Access Toolbar below the Ribbon";
            this.controlRibbon.SystemText.QatRemoveItemText = "&Remove from Quick Access Toolbar";
            // 
            // tabMain
            // 
            this.tabMain.Checked = true;
            this.tabMain.Name = "tabMain";
            this.tabMain.Panel = this.panelMain;
            this.tabMain.Text = "Main Tab";
            // 
            // panelMain
            // 
            this.panelMain.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelMain.Size = new System.Drawing.Size(561, 368);
            // 
            // 
            // 
            this.panelMain.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelMain.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelMain.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.panelMain.TabIndex = 1;
            // 
            // cmdOpen
            // 
            this.cmdOpen.CanCustomize = false;
            this.cmdOpen.ColorTable = DevComponents.DotNetBar.eButtonColor.Blue;
            this.cmdOpen.FixedSize = new System.Drawing.Size(60, 23);
            this.cmdOpen.HotTrackingStyle = DevComponents.DotNetBar.eHotTrackingStyle.Image;
            this.cmdOpen.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.cmdOpen.ImagePaddingHorizontal = 0;
            this.cmdOpen.ImagePaddingVertical = 0;
            this.cmdOpen.Name = "cmdOpen";
            this.cmdOpen.ShowSubItems = false;
            this.cmdOpen.Text = "Open";
            this.cmdOpen.Click += new System.EventHandler(this.cmdOpen_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.CanCustomize = false;
            this.cmdSave.FixedSize = new System.Drawing.Size(60, 23);
            this.cmdSave.HotTrackingStyle = DevComponents.DotNetBar.eHotTrackingStyle.Image;
            this.cmdSave.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.cmdSave.ImagePaddingHorizontal = 0;
            this.cmdSave.ImagePaddingVertical = 0;
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.ShowSubItems = false;
            this.cmdSave.Text = "Save";
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // cmdPackageManager
            // 
            this.cmdPackageManager.Enabled = false;
            this.cmdPackageManager.Image = global::NoDev.Horizon.Properties.Resources.Package_16;
            this.cmdPackageManager.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
            this.cmdPackageManager.Name = "cmdPackageManager";
            // 
            // PackageEditor
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 374);
            this.Name = "PackageEditor";
            this.Text = "Package Editor";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.PackageEditor_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.PackageEditor_DragEnter);
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected internal DevComponents.DotNetBar.RibbonPanel panelMain;
        protected internal DevComponents.DotNetBar.RibbonTabItem tabMain;
        protected internal DevComponents.DotNetBar.Office2007StartButton cmdOpen;
        protected internal DevComponents.DotNetBar.Office2007StartButton cmdSave;
        protected DevComponents.DotNetBar.ButtonItem cmdPackageManager;


    }
}