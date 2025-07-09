namespace NoDev.Horizon
{
    partial class TabbedControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabMain = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonMain = new DevComponents.DotNetBar.RibbonPanel();
            this.controlRibbon.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Controls.Add(this.ribbonMain);
            this.controlRibbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.tabMain});
            this.controlRibbon.Size = new System.Drawing.Size(553, 359);
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
            this.tabMain.Panel = this.ribbonMain;
            this.tabMain.Text = "Main Tab";
            // 
            // ribbonMain
            // 
            this.ribbonMain.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonMain.Location = new System.Drawing.Point(0, 0);
            this.ribbonMain.Name = "ribbonMain";
            this.ribbonMain.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonMain.Size = new System.Drawing.Size(553, 356);
            // 
            // 
            // 
            this.ribbonMain.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonMain.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonMain.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonMain.TabIndex = 1;
            // 
            // TabbedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 362);
            this.Name = "TabbedControl";
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected DevComponents.DotNetBar.RibbonPanel ribbonMain;
        protected DevComponents.DotNetBar.RibbonTabItem tabMain;

    }
}
