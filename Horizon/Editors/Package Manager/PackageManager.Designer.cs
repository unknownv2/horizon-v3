namespace NoDev.Horizon.Editors.Package_Manager
{
    partial class PackageManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageManager));
            this.cmdViewContents = new DevComponents.DotNetBar.ButtonX();
            this.gpProfile = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.pbGamerpic = new System.Windows.Forms.PictureBox();
            this.cmdSwitchProfile = new DevComponents.DotNetBar.ButtonX();
            this.cmdNoProfiles = new DevComponents.DotNetBar.ButtonItem();
            this.lblGamertag = new DevComponents.DotNetBar.LabelX();
            this.cmdManageProfiles = new DevComponents.DotNetBar.ButtonX();
            this.gpIcons = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.pbIcon1 = new System.Windows.Forms.PictureBox();
            this.pbIcon2 = new System.Windows.Forms.PictureBox();
            this.txtTitleID = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblTitleID = new DevComponents.DotNetBar.LabelX();
            this.txtConsoleID = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblConsoleID = new DevComponents.DotNetBar.LabelX();
            this.txtDeviceID = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblDeviceID = new DevComponents.DotNetBar.LabelX();
            this.txtCreator = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblProfileID = new DevComponents.DotNetBar.LabelX();
            this.txtTitleName = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtDisplayName = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblTitleName = new DevComponents.DotNetBar.LabelX();
            this.lblDisplayName = new DevComponents.DotNetBar.LabelX();
            this.cmdModPackage = new DevComponents.DotNetBar.ButtonX();
            this.galEditors = new DevComponents.DotNetBar.GalleryContainer();
            this.panelMain.SuspendLayout();
            this.controlRibbon.SuspendLayout();
            this.gpProfile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGamerpic)).BeginInit();
            this.gpIcons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon2)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.cmdViewContents);
            this.panelMain.Controls.Add(this.gpProfile);
            this.panelMain.Controls.Add(this.gpIcons);
            this.panelMain.Controls.Add(this.txtTitleID);
            this.panelMain.Controls.Add(this.lblTitleID);
            this.panelMain.Controls.Add(this.txtConsoleID);
            this.panelMain.Controls.Add(this.lblConsoleID);
            this.panelMain.Controls.Add(this.txtDeviceID);
            this.panelMain.Controls.Add(this.lblDeviceID);
            this.panelMain.Controls.Add(this.txtCreator);
            this.panelMain.Controls.Add(this.lblProfileID);
            this.panelMain.Controls.Add(this.txtTitleName);
            this.panelMain.Controls.Add(this.txtDisplayName);
            this.panelMain.Controls.Add(this.lblTitleName);
            this.panelMain.Controls.Add(this.lblDisplayName);
            this.panelMain.Controls.Add(this.cmdModPackage);
            this.panelMain.Location = new System.Drawing.Point(0, 53);
            this.panelMain.Size = new System.Drawing.Size(436, 198);
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
            this.panelMain.Visible = true;
            // 
            // tabMain
            // 
            this.tabMain.Text = "Package Information";
            // 
            // cmdSave
            // 
            this.cmdSave.FixedSize = new System.Drawing.Size(150, 23);
            this.cmdSave.Text = "Save, Rehash, and Resign";
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Size = new System.Drawing.Size(436, 254);
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
            this.controlRibbon.Controls.SetChildIndex(this.panelMain, 0);
            // 
            // cmdViewContents
            // 
            this.cmdViewContents.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdViewContents.BackColor = System.Drawing.Color.Transparent;
            this.cmdViewContents.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdViewContents.FocusCuesEnabled = false;
            this.cmdViewContents.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdViewContents.Image = global::NoDev.Horizon.Properties.Resources.Folder_Opened_16;
            this.cmdViewContents.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.cmdViewContents.ImageTextSpacing = 3;
            this.cmdViewContents.Location = new System.Drawing.Point(367, 109);
            this.cmdViewContents.Name = "cmdViewContents";
            this.cmdViewContents.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdViewContents.Size = new System.Drawing.Size(63, 83);
            this.cmdViewContents.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdViewContents.TabIndex = 57;
            this.cmdViewContents.Text = "View Contents";
            this.cmdViewContents.Click += new System.EventHandler(this.cmdViewContents_Click);
            // 
            // gpProfile
            // 
            this.gpProfile.CanvasColor = System.Drawing.SystemColors.Control;
            this.gpProfile.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.gpProfile.Controls.Add(this.pbGamerpic);
            this.gpProfile.Controls.Add(this.cmdSwitchProfile);
            this.gpProfile.Controls.Add(this.lblGamertag);
            this.gpProfile.Controls.Add(this.cmdManageProfiles);
            this.gpProfile.Location = new System.Drawing.Point(6, 135);
            this.gpProfile.Name = "gpProfile";
            this.gpProfile.Size = new System.Drawing.Size(222, 57);
            // 
            // 
            // 
            this.gpProfile.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.gpProfile.Style.BackColorGradientAngle = 90;
            this.gpProfile.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.gpProfile.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpProfile.Style.BorderBottomWidth = 1;
            this.gpProfile.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.gpProfile.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpProfile.Style.BorderLeftWidth = 1;
            this.gpProfile.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpProfile.Style.BorderRightWidth = 1;
            this.gpProfile.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpProfile.Style.BorderTopWidth = 1;
            this.gpProfile.Style.CornerDiameter = 4;
            this.gpProfile.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.gpProfile.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.gpProfile.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.gpProfile.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.gpProfile.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.gpProfile.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.gpProfile.TabIndex = 56;
            // 
            // pbGamerpic
            // 
            this.pbGamerpic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbGamerpic.Image = global::NoDev.Horizon.Properties.Resources.QuestionMark_64;
            this.pbGamerpic.Location = new System.Drawing.Point(-2, -1);
            this.pbGamerpic.Name = "pbGamerpic";
            this.pbGamerpic.Size = new System.Drawing.Size(58, 58);
            this.pbGamerpic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbGamerpic.TabIndex = 44;
            this.pbGamerpic.TabStop = false;
            // 
            // cmdSwitchProfile
            // 
            this.cmdSwitchProfile.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdSwitchProfile.AutoExpandOnClick = true;
            this.cmdSwitchProfile.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdSwitchProfile.FocusCuesEnabled = false;
            this.cmdSwitchProfile.Image = global::NoDev.Horizon.Properties.Resources.Pencil_16;
            this.cmdSwitchProfile.Location = new System.Drawing.Point(55, 27);
            this.cmdSwitchProfile.Name = "cmdSwitchProfile";
            this.cmdSwitchProfile.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdSwitchProfile.Size = new System.Drawing.Size(80, 29);
            this.cmdSwitchProfile.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdSwitchProfile.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.cmdNoProfiles});
            this.cmdSwitchProfile.TabIndex = 42;
            this.cmdSwitchProfile.Text = "Change";
            this.cmdSwitchProfile.PopupOpen += new System.EventHandler(this.cmdSwitchProfile_PopupOpen);
            // 
            // cmdNoProfiles
            // 
            this.cmdNoProfiles.Enabled = false;
            this.cmdNoProfiles.GlobalItem = false;
            this.cmdNoProfiles.Image = global::NoDev.Horizon.Properties.Resources.Info_16;
            this.cmdNoProfiles.Name = "cmdNoProfiles";
            this.cmdNoProfiles.Text = "No Profiles Found";
            // 
            // lblGamertag
            // 
            this.lblGamertag.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblGamertag.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblGamertag.Location = new System.Drawing.Point(55, 5);
            this.lblGamertag.Name = "lblGamertag";
            this.lblGamertag.Size = new System.Drawing.Size(166, 17);
            this.lblGamertag.TabIndex = 45;
            this.lblGamertag.Text = "Unknown Profile";
            this.lblGamertag.TextAlignment = System.Drawing.StringAlignment.Center;
            // 
            // cmdManageProfiles
            // 
            this.cmdManageProfiles.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdManageProfiles.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdManageProfiles.FocusCuesEnabled = false;
            this.cmdManageProfiles.Location = new System.Drawing.Point(134, 27);
            this.cmdManageProfiles.Name = "cmdManageProfiles";
            this.cmdManageProfiles.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdManageProfiles.Size = new System.Drawing.Size(87, 29);
            this.cmdManageProfiles.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdManageProfiles.TabIndex = 43;
            this.cmdManageProfiles.Text = "Manage";
            this.cmdManageProfiles.Click += new System.EventHandler(this.cmdManageProfiles_Click);
            // 
            // gpIcons
            // 
            this.gpIcons.BackColor = System.Drawing.Color.Transparent;
            this.gpIcons.CanvasColor = System.Drawing.SystemColors.Control;
            this.gpIcons.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.gpIcons.Controls.Add(this.pbIcon1);
            this.gpIcons.Controls.Add(this.pbIcon2);
            this.gpIcons.Location = new System.Drawing.Point(285, 4);
            this.gpIcons.Name = "gpIcons";
            this.gpIcons.Size = new System.Drawing.Size(146, 73);
            // 
            // 
            // 
            this.gpIcons.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.gpIcons.Style.BackColorGradientAngle = 90;
            this.gpIcons.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.gpIcons.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpIcons.Style.BorderBottomWidth = 1;
            this.gpIcons.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.gpIcons.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpIcons.Style.BorderLeftWidth = 1;
            this.gpIcons.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpIcons.Style.BorderRightWidth = 1;
            this.gpIcons.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.gpIcons.Style.BorderTopWidth = 1;
            this.gpIcons.Style.CornerDiameter = 4;
            this.gpIcons.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.gpIcons.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.gpIcons.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.gpIcons.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.gpIcons.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.gpIcons.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.gpIcons.TabIndex = 55;
            // 
            // pbIcon1
            // 
            this.pbIcon1.BackColor = System.Drawing.Color.Transparent;
            this.pbIcon1.Image = global::NoDev.Horizon.Properties.Resources.Console_64;
            this.pbIcon1.Location = new System.Drawing.Point(4, 3);
            this.pbIcon1.Name = "pbIcon1";
            this.pbIcon1.Size = new System.Drawing.Size(64, 64);
            this.pbIcon1.TabIndex = 14;
            this.pbIcon1.TabStop = false;
            // 
            // pbIcon2
            // 
            this.pbIcon2.Image = global::NoDev.Horizon.Properties.Resources.Console_64;
            this.pbIcon2.Location = new System.Drawing.Point(76, 3);
            this.pbIcon2.Name = "pbIcon2";
            this.pbIcon2.Size = new System.Drawing.Size(64, 64);
            this.pbIcon2.TabIndex = 15;
            this.pbIcon2.TabStop = false;
            // 
            // txtTitleID
            // 
            // 
            // 
            // 
            this.txtTitleID.Border.Class = "TextBoxBorder";
            this.txtTitleID.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtTitleID.Location = new System.Drawing.Point(331, 83);
            this.txtTitleID.MaxLength = 8;
            this.txtTitleID.Name = "txtTitleID";
            this.txtTitleID.ReadOnly = true;
            this.txtTitleID.Size = new System.Drawing.Size(99, 20);
            this.txtTitleID.TabIndex = 54;
            // 
            // lblTitleID
            // 
            this.lblTitleID.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblTitleID.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblTitleID.Location = new System.Drawing.Point(285, 83);
            this.lblTitleID.Name = "lblTitleID";
            this.lblTitleID.Size = new System.Drawing.Size(40, 20);
            this.lblTitleID.TabIndex = 53;
            this.lblTitleID.Text = "Title ID:";
            // 
            // txtConsoleID
            // 
            // 
            // 
            // 
            this.txtConsoleID.Border.Class = "TextBoxBorder";
            this.txtConsoleID.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtConsoleID.Location = new System.Drawing.Point(86, 109);
            this.txtConsoleID.MaxLength = 10;
            this.txtConsoleID.Name = "txtConsoleID";
            this.txtConsoleID.Size = new System.Drawing.Size(141, 20);
            this.txtConsoleID.TabIndex = 52;
            this.txtConsoleID.WatermarkText = "Public";
            // 
            // lblConsoleID
            // 
            this.lblConsoleID.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblConsoleID.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblConsoleID.Location = new System.Drawing.Point(5, 109);
            this.lblConsoleID.Name = "lblConsoleID";
            this.lblConsoleID.Size = new System.Drawing.Size(75, 20);
            this.lblConsoleID.TabIndex = 51;
            this.lblConsoleID.Text = "Console ID:";
            this.lblConsoleID.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // txtDeviceID
            // 
            // 
            // 
            // 
            this.txtDeviceID.Border.Class = "TextBoxBorder";
            this.txtDeviceID.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtDeviceID.Location = new System.Drawing.Point(86, 83);
            this.txtDeviceID.MaxLength = 40;
            this.txtDeviceID.Name = "txtDeviceID";
            this.txtDeviceID.Size = new System.Drawing.Size(192, 20);
            this.txtDeviceID.TabIndex = 50;
            this.txtDeviceID.WatermarkText = "Public";
            // 
            // lblDeviceID
            // 
            this.lblDeviceID.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblDeviceID.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblDeviceID.Location = new System.Drawing.Point(5, 83);
            this.lblDeviceID.Name = "lblDeviceID";
            this.lblDeviceID.Size = new System.Drawing.Size(75, 20);
            this.lblDeviceID.TabIndex = 49;
            this.lblDeviceID.Text = "Device ID:";
            this.lblDeviceID.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // txtCreator
            // 
            // 
            // 
            // 
            this.txtCreator.Border.Class = "TextBoxBorder";
            this.txtCreator.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtCreator.Location = new System.Drawing.Point(86, 57);
            this.txtCreator.MaxLength = 16;
            this.txtCreator.Name = "txtCreator";
            this.txtCreator.Size = new System.Drawing.Size(192, 20);
            this.txtCreator.TabIndex = 48;
            this.txtCreator.WatermarkText = "Public";
            this.txtCreator.TextChanged += new System.EventHandler(this.txtCreator_TextChanged);
            // 
            // lblProfileID
            // 
            this.lblProfileID.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblProfileID.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblProfileID.Location = new System.Drawing.Point(5, 57);
            this.lblProfileID.Name = "lblProfileID";
            this.lblProfileID.Size = new System.Drawing.Size(75, 20);
            this.lblProfileID.TabIndex = 47;
            this.lblProfileID.Text = "Profile ID:";
            this.lblProfileID.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // txtTitleName
            // 
            // 
            // 
            // 
            this.txtTitleName.Border.Class = "TextBoxBorder";
            this.txtTitleName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtTitleName.Location = new System.Drawing.Point(86, 30);
            this.txtTitleName.MaxLength = 64;
            this.txtTitleName.Name = "txtTitleName";
            this.txtTitleName.Size = new System.Drawing.Size(192, 20);
            this.txtTitleName.TabIndex = 46;
            this.txtTitleName.WatermarkText = "None";
            // 
            // txtDisplayName
            // 
            // 
            // 
            // 
            this.txtDisplayName.Border.Class = "TextBoxBorder";
            this.txtDisplayName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtDisplayName.Location = new System.Drawing.Point(86, 4);
            this.txtDisplayName.MaxLength = 128;
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.Size = new System.Drawing.Size(192, 20);
            this.txtDisplayName.TabIndex = 45;
            this.txtDisplayName.WatermarkText = "None";
            // 
            // lblTitleName
            // 
            this.lblTitleName.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblTitleName.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblTitleName.Location = new System.Drawing.Point(5, 30);
            this.lblTitleName.Name = "lblTitleName";
            this.lblTitleName.Size = new System.Drawing.Size(75, 20);
            this.lblTitleName.TabIndex = 44;
            this.lblTitleName.Text = "Title Name:";
            this.lblTitleName.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblDisplayName.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblDisplayName.Location = new System.Drawing.Point(5, 4);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Size = new System.Drawing.Size(75, 20);
            this.lblDisplayName.TabIndex = 43;
            this.lblDisplayName.Text = "Display Name:";
            this.lblDisplayName.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // cmdModPackage
            // 
            this.cmdModPackage.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdModPackage.BackColor = System.Drawing.Color.Transparent;
            this.cmdModPackage.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdModPackage.Enabled = false;
            this.cmdModPackage.FocusCuesEnabled = false;
            this.cmdModPackage.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdModPackage.Image = ((System.Drawing.Image)(resources.GetObject("cmdModPackage.Image")));
            this.cmdModPackage.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.cmdModPackage.ImageTextSpacing = 3;
            this.cmdModPackage.Location = new System.Drawing.Point(234, 109);
            this.cmdModPackage.Name = "cmdModPackage";
            this.cmdModPackage.PopupSide = DevComponents.DotNetBar.ePopupSide.Left;
            this.cmdModPackage.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdModPackage.ShowSubItems = false;
            this.cmdModPackage.Size = new System.Drawing.Size(135, 83);
            this.cmdModPackage.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdModPackage.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.galEditors});
            this.cmdModPackage.TabIndex = 58;
            this.cmdModPackage.Text = "Mod Package";
            // 
            // galEditors
            // 
            // 
            // 
            // 
            this.galEditors.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.galEditors.DefaultSize = new System.Drawing.Size(144, 405);
            this.galEditors.GlobalItem = false;
            this.galEditors.MinimumSize = new System.Drawing.Size(58, 58);
            this.galEditors.Name = "galEditors";
            this.galEditors.StretchGallery = true;
            // 
            // 
            // 
            this.galEditors.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // PackageManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 257);
            this.MaximumSize = new System.Drawing.Size(446, 257);
            this.MinimumSize = new System.Drawing.Size(446, 257);
            this.Name = "PackageManager";
            this.Text = "Package Manager";
            this.panelMain.ResumeLayout(false);
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            this.gpProfile.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbGamerpic)).EndInit();
            this.gpIcons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ButtonX cmdViewContents;
        private DevComponents.DotNetBar.Controls.GroupPanel gpProfile;
        private System.Windows.Forms.PictureBox pbGamerpic;
        private DevComponents.DotNetBar.ButtonX cmdSwitchProfile;
        private DevComponents.DotNetBar.ButtonItem cmdNoProfiles;
        private DevComponents.DotNetBar.LabelX lblGamertag;
        private DevComponents.DotNetBar.ButtonX cmdManageProfiles;
        private DevComponents.DotNetBar.Controls.GroupPanel gpIcons;
        private System.Windows.Forms.PictureBox pbIcon1;
        private System.Windows.Forms.PictureBox pbIcon2;
        private DevComponents.DotNetBar.Controls.TextBoxX txtTitleID;
        private DevComponents.DotNetBar.LabelX lblTitleID;
        private DevComponents.DotNetBar.Controls.TextBoxX txtConsoleID;
        private DevComponents.DotNetBar.LabelX lblConsoleID;
        private DevComponents.DotNetBar.Controls.TextBoxX txtDeviceID;
        private DevComponents.DotNetBar.LabelX lblDeviceID;
        private DevComponents.DotNetBar.Controls.TextBoxX txtCreator;
        private DevComponents.DotNetBar.LabelX lblProfileID;
        private DevComponents.DotNetBar.Controls.TextBoxX txtTitleName;
        private DevComponents.DotNetBar.Controls.TextBoxX txtDisplayName;
        private DevComponents.DotNetBar.LabelX lblTitleName;
        private DevComponents.DotNetBar.LabelX lblDisplayName;
        private DevComponents.DotNetBar.ButtonX cmdModPackage;
        private DevComponents.DotNetBar.GalleryContainer galEditors;

    }
}
