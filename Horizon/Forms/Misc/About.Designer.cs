namespace NoDev.Horizon.Forms.Misc
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.lblNoDevelopment = new DevComponents.DotNetBar.LabelX();
            this.cmdXboxMB = new DevComponents.DotNetBar.ButtonX();
            this.gpNoDevelopment = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.tabSpecialThanks = new DevComponents.DotNetBar.RibbonTabItem();
            this.panelSpecialThanks = new DevComponents.DotNetBar.RibbonPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtSpecialThanks = new System.Windows.Forms.TextBox();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.ribbonMain.SuspendLayout();
            this.controlRibbon.SuspendLayout();
            this.gpNoDevelopment.SuspendLayout();
            this.panelSpecialThanks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonMain
            // 
            this.ribbonMain.Controls.Add(this.pbLogo);
            this.ribbonMain.Controls.Add(this.gpNoDevelopment);
            this.ribbonMain.Controls.Add(this.cmdXboxMB);
            this.ribbonMain.Location = new System.Drawing.Point(0, 53);
            this.ribbonMain.Size = new System.Drawing.Size(358, 115);
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
            this.ribbonMain.Visible = true;
            // 
            // tabMain
            // 
            this.tabMain.Text = "About";
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Controls.Add(this.panelSpecialThanks);
            this.controlRibbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.tabSpecialThanks});
            this.controlRibbon.Size = new System.Drawing.Size(358, 171);
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
            this.controlRibbon.Controls.SetChildIndex(this.panelSpecialThanks, 0);
            this.controlRibbon.Controls.SetChildIndex(this.ribbonMain, 0);
            // 
            // lblNoDevelopment
            // 
            this.lblNoDevelopment.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblNoDevelopment.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNoDevelopment.Font = new System.Drawing.Font("Arial Rounded MT Bold", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoDevelopment.Location = new System.Drawing.Point(3, 3);
            this.lblNoDevelopment.Name = "lblNoDevelopment";
            this.lblNoDevelopment.Size = new System.Drawing.Size(232, 32);
            this.lblNoDevelopment.TabIndex = 1;
            this.lblNoDevelopment.Text = "No Development Inc.";
            this.lblNoDevelopment.TextAlignment = System.Drawing.StringAlignment.Center;
            // 
            // cmdXboxMB
            // 
            this.cmdXboxMB.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdXboxMB.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdXboxMB.FocusCuesEnabled = false;
            this.cmdXboxMB.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdXboxMB.Location = new System.Drawing.Point(6, 69);
            this.cmdXboxMB.Name = "cmdXboxMB";
            this.cmdXboxMB.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdXboxMB.Size = new System.Drawing.Size(240, 40);
            this.cmdXboxMB.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdXboxMB.TabIndex = 3;
            this.cmdXboxMB.Text = "XboxMB.com";
            this.cmdXboxMB.Click += new System.EventHandler(this.cmdXboxMB_Click);
            // 
            // gpNoDevelopment
            // 
            this.gpNoDevelopment.BackColor = System.Drawing.Color.Transparent;
            this.gpNoDevelopment.CanvasColor = System.Drawing.SystemColors.Control;
            this.gpNoDevelopment.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.gpNoDevelopment.Controls.Add(this.lblNoDevelopment);
            this.gpNoDevelopment.Location = new System.Drawing.Point(6, 3);
            this.gpNoDevelopment.Name = "gpNoDevelopment";
            this.gpNoDevelopment.Size = new System.Drawing.Size(240, 60);
            // 
            // 
            // 
            this.gpNoDevelopment.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.gpNoDevelopment.Style.BackColorGradientAngle = 90;
            this.gpNoDevelopment.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.gpNoDevelopment.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpNoDevelopment.Style.BorderBottomWidth = 1;
            this.gpNoDevelopment.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.gpNoDevelopment.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpNoDevelopment.Style.BorderLeftWidth = 1;
            this.gpNoDevelopment.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpNoDevelopment.Style.BorderRightWidth = 1;
            this.gpNoDevelopment.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpNoDevelopment.Style.BorderTopWidth = 1;
            this.gpNoDevelopment.Style.CornerDiameter = 4;
            this.gpNoDevelopment.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.gpNoDevelopment.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.gpNoDevelopment.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.gpNoDevelopment.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.gpNoDevelopment.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.gpNoDevelopment.TabIndex = 4;
            this.gpNoDevelopment.Text = "Developed by";
            // 
            // tabSpecialThanks
            // 
            this.tabSpecialThanks.Name = "tabSpecialThanks";
            this.tabSpecialThanks.Panel = this.panelSpecialThanks;
            this.tabSpecialThanks.Text = "Special Thanks";
            // 
            // panelSpecialThanks
            // 
            this.panelSpecialThanks.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelSpecialThanks.Controls.Add(this.pictureBox1);
            this.panelSpecialThanks.Controls.Add(this.txtSpecialThanks);
            this.panelSpecialThanks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSpecialThanks.Location = new System.Drawing.Point(0, 53);
            this.panelSpecialThanks.Name = "panelSpecialThanks";
            this.panelSpecialThanks.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelSpecialThanks.Size = new System.Drawing.Size(358, 115);
            // 
            // 
            // 
            this.panelSpecialThanks.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelSpecialThanks.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelSpecialThanks.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.panelSpecialThanks.TabIndex = 2;
            this.panelSpecialThanks.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::NoDev.Horizon.Properties.Resources.Logo_Horizon_100;
            this.pictureBox1.Location = new System.Drawing.Point(252, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // txtSpecialThanks
            // 
            this.txtSpecialThanks.Location = new System.Drawing.Point(6, 4);
            this.txtSpecialThanks.Multiline = true;
            this.txtSpecialThanks.Name = "txtSpecialThanks";
            this.txtSpecialThanks.ReadOnly = true;
            this.txtSpecialThanks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSpecialThanks.Size = new System.Drawing.Size(240, 105);
            this.txtSpecialThanks.TabIndex = 0;
            this.txtSpecialThanks.Text = resources.GetString("txtSpecialThanks.Text");
            this.txtSpecialThanks.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pbLogo
            // 
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.Image = global::NoDev.Horizon.Properties.Resources.Logo_Horizon_100;
            this.pbLogo.Location = new System.Drawing.Point(252, 9);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(100, 100);
            this.pbLogo.TabIndex = 5;
            this.pbLogo.TabStop = false;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 174);
            this.MaximumSize = new System.Drawing.Size(368, 174);
            this.MinimumSize = new System.Drawing.Size(368, 174);
            this.Name = "About";
            this.Text = "About Horizon";
            this.ribbonMain.ResumeLayout(false);
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            this.gpNoDevelopment.ResumeLayout(false);
            this.panelSpecialThanks.ResumeLayout(false);
            this.panelSpecialThanks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Controls.GroupPanel gpNoDevelopment;
        private DevComponents.DotNetBar.ButtonX cmdXboxMB;
        private DevComponents.DotNetBar.LabelX lblNoDevelopment;
        private System.Windows.Forms.PictureBox pbLogo;
        private DevComponents.DotNetBar.RibbonPanel panelSpecialThanks;
        private DevComponents.DotNetBar.RibbonTabItem tabSpecialThanks;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtSpecialThanks;


    }
}
