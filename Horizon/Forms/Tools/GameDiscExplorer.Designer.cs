namespace NoDev.Horizon.Forms.Tools
{
    partial class GameDiscExplorer
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
            this.cmdOpen = new DevComponents.DotNetBar.ButtonX();
            this.listDiscs = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnDiscPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnMountPoint = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboDriveLetters = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmdMount = new DevComponents.DotNetBar.ButtonX();
            this.cmdExplore = new DevComponents.DotNetBar.ButtonX();
            this.ribbonMain.SuspendLayout();
            this.controlRibbon.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonMain
            // 
            this.ribbonMain.Controls.Add(this.cmdExplore);
            this.ribbonMain.Controls.Add(this.cmdMount);
            this.ribbonMain.Controls.Add(this.comboDriveLetters);
            this.ribbonMain.Controls.Add(this.listDiscs);
            this.ribbonMain.Controls.Add(this.cmdOpen);
            this.ribbonMain.Location = new System.Drawing.Point(0, 53);
            this.ribbonMain.Size = new System.Drawing.Size(342, 204);
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
            // 
            // tabMain
            // 
            this.tabMain.Text = "Game Discs";
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Size = new System.Drawing.Size(342, 260);
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
            this.controlRibbon.Controls.SetChildIndex(this.ribbonMain, 0);
            // 
            // cmdOpen
            // 
            this.cmdOpen.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdOpen.BackColor = System.Drawing.Color.Transparent;
            this.cmdOpen.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdOpen.FocusCuesEnabled = false;
            this.cmdOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmdOpen.Location = new System.Drawing.Point(6, 173);
            this.cmdOpen.Name = "cmdOpen";
            this.cmdOpen.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdOpen.Size = new System.Drawing.Size(85, 25);
            this.cmdOpen.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdOpen.TabIndex = 11;
            this.cmdOpen.Text = "Add Image...";
            this.cmdOpen.Click += new System.EventHandler(this.cmdOpen_Click);
            // 
            // listDiscs
            // 
            // 
            // 
            // 
            this.listDiscs.Border.Class = "ListViewBorder";
            this.listDiscs.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listDiscs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnDiscPath,
            this.columnMountPoint});
            this.listDiscs.FullRowSelect = true;
            this.listDiscs.GridLines = true;
            this.listDiscs.Location = new System.Drawing.Point(6, 5);
            this.listDiscs.MultiSelect = false;
            this.listDiscs.Name = "listDiscs";
            this.listDiscs.Size = new System.Drawing.Size(330, 162);
            this.listDiscs.TabIndex = 14;
            this.listDiscs.UseCompatibleStateImageBehavior = false;
            this.listDiscs.View = System.Windows.Forms.View.Details;
            this.listDiscs.SelectedIndexChanged += new System.EventHandler(this.listDiscs_SelectedIndexChanged);
            // 
            // columnDiscPath
            // 
            this.columnDiscPath.Text = "Disc Path";
            this.columnDiscPath.Width = 229;
            // 
            // columnMountPoint
            // 
            this.columnMountPoint.Text = "Mount Point";
            this.columnMountPoint.Width = 78;
            // 
            // comboDriveLetters
            // 
            this.comboDriveLetters.DisplayMember = "Text";
            this.comboDriveLetters.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboDriveLetters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDriveLetters.Enabled = false;
            this.comboDriveLetters.FocusCuesEnabled = false;
            this.comboDriveLetters.FormattingEnabled = true;
            this.comboDriveLetters.ItemHeight = 14;
            this.comboDriveLetters.Location = new System.Drawing.Point(137, 175);
            this.comboDriveLetters.Name = "comboDriveLetters";
            this.comboDriveLetters.Size = new System.Drawing.Size(42, 20);
            this.comboDriveLetters.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.comboDriveLetters.TabIndex = 17;
            this.comboDriveLetters.DropDown += new System.EventHandler(this.comboDriveLetters_DropDown);
            // 
            // cmdMount
            // 
            this.cmdMount.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdMount.BackColor = System.Drawing.Color.Transparent;
            this.cmdMount.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdMount.Enabled = false;
            this.cmdMount.FocusCuesEnabled = false;
            this.cmdMount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmdMount.Location = new System.Drawing.Point(185, 173);
            this.cmdMount.Name = "cmdMount";
            this.cmdMount.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdMount.Size = new System.Drawing.Size(83, 25);
            this.cmdMount.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdMount.TabIndex = 18;
            this.cmdMount.Text = "Mount";
            this.cmdMount.Click += new System.EventHandler(this.cmdMount_Click);
            // 
            // cmdExplore
            // 
            this.cmdExplore.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdExplore.BackColor = System.Drawing.Color.Transparent;
            this.cmdExplore.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdExplore.Enabled = false;
            this.cmdExplore.FocusCuesEnabled = false;
            this.cmdExplore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmdExplore.Location = new System.Drawing.Point(274, 173);
            this.cmdExplore.Name = "cmdExplore";
            this.cmdExplore.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdExplore.Size = new System.Drawing.Size(62, 25);
            this.cmdExplore.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdExplore.TabIndex = 19;
            this.cmdExplore.Text = "Explore";
            this.cmdExplore.Click += new System.EventHandler(this.cmdExplore_Click);
            // 
            // GameDiscExplorer
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 263);
            this.MaximumSize = new System.Drawing.Size(352, 263);
            this.MinimumSize = new System.Drawing.Size(352, 263);
            this.Name = "GameDiscExplorer";
            this.Text = "Game Disc Explorer";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.GameDiscExplorer_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.GameDiscExplorer_DragEnter);
            this.ribbonMain.ResumeLayout(false);
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ButtonX cmdOpen;
        private DevComponents.DotNetBar.Controls.ListViewEx listDiscs;
        private System.Windows.Forms.ColumnHeader columnDiscPath;
        private System.Windows.Forms.ColumnHeader columnMountPoint;
        private DevComponents.DotNetBar.Controls.ComboBoxEx comboDriveLetters;
        private DevComponents.DotNetBar.ButtonX cmdMount;
        private DevComponents.DotNetBar.ButtonX cmdExplore;
    }
}
