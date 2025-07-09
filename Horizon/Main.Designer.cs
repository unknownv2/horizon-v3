namespace NoDev.Horizon
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.ribbonHorizon = new DevComponents.DotNetBar.RibbonControl();
            this.panelXboxMb = new DevComponents.DotNetBar.RibbonPanel();
            this.lblPassword = new DevComponents.DotNetBar.LabelX();
            this.cmdAutoLogin = new DevComponents.DotNetBar.ButtonX();
            this.lblUsername = new DevComponents.DotNetBar.LabelX();
            this.barProfile = new DevComponents.DotNetBar.RibbonBar();
            this.cmdProfile = new DevComponents.DotNetBar.ButtonX();
            this.labelItem2 = new DevComponents.DotNetBar.LabelItem();
            this.barRegister = new DevComponents.DotNetBar.RibbonBar();
            this.cmdRegister = new DevComponents.DotNetBar.ButtonX();
            this.labelItem1 = new DevComponents.DotNetBar.LabelItem();
            this.txtPassword = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtUsername = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.barForgotPassword = new DevComponents.DotNetBar.RibbonBar();
            this.cmdForgotPassword = new DevComponents.DotNetBar.ButtonX();
            this.pad1 = new DevComponents.DotNetBar.LabelItem();
            this.barLogin = new DevComponents.DotNetBar.RibbonBar();
            this.cmdLogin = new DevComponents.DotNetBar.ButtonX();
            this.cmdUserGroup = new DevComponents.DotNetBar.Office2007StartButton();
            this.tabXboxMb = new DevComponents.DotNetBar.RibbonTabItem();
            this.styleMain = new DevComponents.DotNetBar.StyleManager(this.components);
            this.cmdTwitterFollowUs = new DevComponents.DotNetBar.ButtonX();
            this.cmdTwitterRefresh = new DevComponents.DotNetBar.ButtonX();
            this.exTwitter = new DevComponents.DotNetBar.ExpandablePanel();
            this.webTwitter = new System.Windows.Forms.WebBrowser();
            this.statusBar = new DevComponents.DotNetBar.PanelEx();
            this.exFatx = new DevComponents.DotNetBar.ExpandablePanel();
            this.panelFatx = new DevComponents.DotNetBar.PanelEx();
            this.progressFatx = new System.Windows.Forms.ProgressBar();
            this.cmdFatxExpand = new DevComponents.DotNetBar.ButtonX();
            this.cmdFatxContract = new DevComponents.DotNetBar.ButtonX();
            this.listFatx = new DevComponents.AdvTree.AdvTree();
            this.colTreeEntry = new DevComponents.AdvTree.ColumnHeader();
            this.colTreeInfo = new DevComponents.AdvTree.ColumnHeader();
            this.nodeConnectorFatx = new DevComponents.AdvTree.NodeConnector();
            this.fatxElementStyle = new DevComponents.DotNetBar.ElementStyle();
            this.rbFatx = new DevComponents.DotNetBar.RibbonControl();
            this.rpFatxDrives = new DevComponents.DotNetBar.RibbonPanel();
            this.cmdFatxGear = new DevComponents.DotNetBar.ButtonX();
            this.cmdFatxMod = new DevComponents.DotNetBar.ButtonX();
            this.galFatx = new DevComponents.DotNetBar.GalleryContainer();
            this.cmdFatxExtract = new DevComponents.DotNetBar.ButtonX();
            this.cmdFatxInject = new DevComponents.DotNetBar.ButtonX();
            this.cmdFatxDevicesLoaded = new DevComponents.DotNetBar.Office2007StartButton();
            this.tabFatxDrive = new DevComponents.DotNetBar.RibbonTabItem();
            this.cmdSettings = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonHorizon.SuspendLayout();
            this.panelXboxMb.SuspendLayout();
            this.barProfile.SuspendLayout();
            this.barRegister.SuspendLayout();
            this.barForgotPassword.SuspendLayout();
            this.barLogin.SuspendLayout();
            this.exTwitter.SuspendLayout();
            this.exFatx.SuspendLayout();
            this.panelFatx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listFatx)).BeginInit();
            this.rbFatx.SuspendLayout();
            this.rpFatxDrives.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonHorizon
            // 
            this.ribbonHorizon.AutoExpand = false;
            this.ribbonHorizon.AutoKeyboardExpand = false;
            this.ribbonHorizon.BackColor = System.Drawing.Color.LightGray;
            // 
            // 
            // 
            this.ribbonHorizon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonHorizon.CanCustomize = false;
            this.ribbonHorizon.CaptionVisible = true;
            this.ribbonHorizon.Controls.Add(this.panelXboxMb);
            this.ribbonHorizon.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbonHorizon.EnableQatPlacement = false;
            this.ribbonHorizon.ForeColor = System.Drawing.Color.Black;
            this.ribbonHorizon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.cmdUserGroup,
            this.tabXboxMb});
            this.ribbonHorizon.KeyTipsFont = new System.Drawing.Font("Tahoma", 7F);
            this.ribbonHorizon.Location = new System.Drawing.Point(5, 1);
            this.ribbonHorizon.MouseWheelTabScrollEnabled = false;
            this.ribbonHorizon.Name = "ribbonHorizon";
            this.ribbonHorizon.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.ribbonHorizon.Size = new System.Drawing.Size(963, 128);
            this.ribbonHorizon.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonHorizon.SystemText.MaximizeRibbonText = "&Maximize the Ribbon";
            this.ribbonHorizon.SystemText.MinimizeRibbonText = "Mi&nimize the Ribbon";
            this.ribbonHorizon.SystemText.QatAddItemText = "&Add to Quick Access Toolbar";
            this.ribbonHorizon.SystemText.QatCustomizeMenuLabel = "<b>Customize Quick Access Toolbar</b>";
            this.ribbonHorizon.SystemText.QatCustomizeText = "&Customize Quick Access Toolbar...";
            this.ribbonHorizon.SystemText.QatDialogAddButton = "&Add >>";
            this.ribbonHorizon.SystemText.QatDialogCancelButton = "Cancel";
            this.ribbonHorizon.SystemText.QatDialogCaption = "Customize Quick Access Toolbar";
            this.ribbonHorizon.SystemText.QatDialogCategoriesLabel = "&Choose commands from:";
            this.ribbonHorizon.SystemText.QatDialogOkButton = "OK";
            this.ribbonHorizon.SystemText.QatDialogPlacementCheckbox = "&Place Quick Access Toolbar below the Ribbon";
            this.ribbonHorizon.SystemText.QatDialogRemoveButton = "&Remove";
            this.ribbonHorizon.SystemText.QatPlaceAboveRibbonText = "&Place Quick Access Toolbar above the Ribbon";
            this.ribbonHorizon.SystemText.QatPlaceBelowRibbonText = "&Place Quick Access Toolbar below the Ribbon";
            this.ribbonHorizon.SystemText.QatRemoveItemText = "&Remove from Quick Access Toolbar";
            this.ribbonHorizon.TabGroupHeight = 14;
            this.ribbonHorizon.TabIndex = 0;
            // 
            // panelXboxMb
            // 
            this.panelXboxMb.CanvasColor = System.Drawing.Color.Empty;
            this.panelXboxMb.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelXboxMb.Controls.Add(this.lblPassword);
            this.panelXboxMb.Controls.Add(this.cmdAutoLogin);
            this.panelXboxMb.Controls.Add(this.lblUsername);
            this.panelXboxMb.Controls.Add(this.barProfile);
            this.panelXboxMb.Controls.Add(this.barRegister);
            this.panelXboxMb.Controls.Add(this.txtPassword);
            this.panelXboxMb.Controls.Add(this.txtUsername);
            this.panelXboxMb.Controls.Add(this.barForgotPassword);
            this.panelXboxMb.Controls.Add(this.barLogin);
            this.panelXboxMb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelXboxMb.Location = new System.Drawing.Point(0, 53);
            this.panelXboxMb.Name = "panelXboxMb";
            this.panelXboxMb.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelXboxMb.Size = new System.Drawing.Size(963, 73);
            // 
            // 
            // 
            this.panelXboxMb.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelXboxMb.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelXboxMb.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.panelXboxMb.TabIndex = 1;
            // 
            // lblPassword
            // 
            this.lblPassword.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblPassword.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblPassword.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblPassword.Location = new System.Drawing.Point(6, 32);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(57, 20);
            this.lblPassword.TabIndex = 10;
            this.lblPassword.Text = "Password:";
            // 
            // cmdAutoLogin
            // 
            this.cmdAutoLogin.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdAutoLogin.AutoCheckOnClick = true;
            this.cmdAutoLogin.BackColor = System.Drawing.Color.Transparent;
            this.cmdAutoLogin.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdAutoLogin.FocusCuesEnabled = false;
            this.cmdAutoLogin.Font = new System.Drawing.Font("Microsoft Tai Le", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.cmdAutoLogin.Location = new System.Drawing.Point(227, 6);
            this.cmdAutoLogin.Name = "cmdAutoLogin";
            this.cmdAutoLogin.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdAutoLogin.Size = new System.Drawing.Size(66, 20);
            this.cmdAutoLogin.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdAutoLogin.TabIndex = 5;
            this.cmdAutoLogin.Text = "Auto Login";
            // 
            // lblUsername
            // 
            this.lblUsername.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblUsername.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblUsername.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblUsername.Location = new System.Drawing.Point(6, 6);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(57, 20);
            this.lblUsername.TabIndex = 9;
            this.lblUsername.Text = "Username:";
            // 
            // barProfile
            // 
            this.barProfile.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.barProfile.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barProfile.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.barProfile.ContainerControlProcessDialogKey = true;
            this.barProfile.Controls.Add(this.cmdProfile);
            this.barProfile.Dock = System.Windows.Forms.DockStyle.Left;
            this.barProfile.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.labelItem2});
            this.barProfile.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.barProfile.Location = new System.Drawing.Point(581, 0);
            this.barProfile.Name = "barProfile";
            this.barProfile.Size = new System.Drawing.Size(139, 70);
            this.barProfile.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.barProfile.TabIndex = 8;
            this.barProfile.Text = "XboxMB Profile";
            // 
            // 
            // 
            this.barProfile.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barProfile.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.barProfile.Visible = false;
            // 
            // cmdProfile
            // 
            this.cmdProfile.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdProfile.BackColor = System.Drawing.Color.Transparent;
            this.cmdProfile.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdProfile.FocusCuesEnabled = false;
            this.cmdProfile.Font = new System.Drawing.Font("Microsoft Tai Le", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.cmdProfile.Location = new System.Drawing.Point(6, 6);
            this.cmdProfile.Name = "cmdProfile";
            this.cmdProfile.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdProfile.Size = new System.Drawing.Size(124, 46);
            this.cmdProfile.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdProfile.TabIndex = 0;
            this.cmdProfile.Text = "My Profile";
            // 
            // labelItem2
            // 
            this.labelItem2.Name = "labelItem2";
            this.labelItem2.Width = 132;
            // 
            // barRegister
            // 
            this.barRegister.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.barRegister.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barRegister.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.barRegister.ContainerControlProcessDialogKey = true;
            this.barRegister.Controls.Add(this.cmdRegister);
            this.barRegister.Dock = System.Windows.Forms.DockStyle.Left;
            this.barRegister.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.labelItem1});
            this.barRegister.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.barRegister.Location = new System.Drawing.Point(442, 0);
            this.barRegister.Name = "barRegister";
            this.barRegister.Size = new System.Drawing.Size(139, 70);
            this.barRegister.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.barRegister.TabIndex = 7;
            this.barRegister.Text = "Register an Account";
            // 
            // 
            // 
            this.barRegister.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barRegister.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // cmdRegister
            // 
            this.cmdRegister.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdRegister.BackColor = System.Drawing.Color.Transparent;
            this.cmdRegister.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdRegister.FocusCuesEnabled = false;
            this.cmdRegister.Font = new System.Drawing.Font("Microsoft Tai Le", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.cmdRegister.Location = new System.Drawing.Point(6, 6);
            this.cmdRegister.Name = "cmdRegister";
            this.cmdRegister.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdRegister.Size = new System.Drawing.Size(124, 46);
            this.cmdRegister.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdRegister.TabIndex = 0;
            this.cmdRegister.Text = "Sign Up Now!";
            // 
            // labelItem1
            // 
            this.labelItem1.Name = "labelItem1";
            this.labelItem1.Width = 132;
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.txtPassword.Border.Class = "TextBoxBorder";
            this.txtPassword.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtPassword.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtPassword.Location = new System.Drawing.Point(69, 32);
            this.txtPassword.MaxLength = 50;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(152, 20);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            this.txtUsername.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.txtUsername.Border.Class = "TextBoxBorder";
            this.txtUsername.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtUsername.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtUsername.Location = new System.Drawing.Point(69, 6);
            this.txtUsername.MaxLength = 20;
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(152, 20);
            this.txtUsername.TabIndex = 0;
            // 
            // barForgotPassword
            // 
            this.barForgotPassword.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.barForgotPassword.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barForgotPassword.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.barForgotPassword.ContainerControlProcessDialogKey = true;
            this.barForgotPassword.Controls.Add(this.cmdForgotPassword);
            this.barForgotPassword.Dock = System.Windows.Forms.DockStyle.Left;
            this.barForgotPassword.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.pad1});
            this.barForgotPassword.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.barForgotPassword.Location = new System.Drawing.Point(303, 0);
            this.barForgotPassword.Name = "barForgotPassword";
            this.barForgotPassword.Size = new System.Drawing.Size(139, 70);
            this.barForgotPassword.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.barForgotPassword.TabIndex = 6;
            this.barForgotPassword.Text = "Lost Password";
            // 
            // 
            // 
            this.barForgotPassword.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barForgotPassword.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // cmdForgotPassword
            // 
            this.cmdForgotPassword.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdForgotPassword.BackColor = System.Drawing.Color.Transparent;
            this.cmdForgotPassword.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdForgotPassword.FocusCuesEnabled = false;
            this.cmdForgotPassword.Font = new System.Drawing.Font("Microsoft Tai Le", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.cmdForgotPassword.Location = new System.Drawing.Point(6, 6);
            this.cmdForgotPassword.Name = "cmdForgotPassword";
            this.cmdForgotPassword.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdForgotPassword.Size = new System.Drawing.Size(124, 46);
            this.cmdForgotPassword.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdForgotPassword.TabIndex = 0;
            this.cmdForgotPassword.Text = "Forgot Password";
            // 
            // pad1
            // 
            this.pad1.Name = "pad1";
            this.pad1.Width = 132;
            // 
            // barLogin
            // 
            this.barLogin.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.barLogin.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barLogin.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.barLogin.ContainerControlProcessDialogKey = true;
            this.barLogin.Controls.Add(this.cmdLogin);
            this.barLogin.Dock = System.Windows.Forms.DockStyle.Left;
            this.barLogin.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.barLogin.Location = new System.Drawing.Point(3, 0);
            this.barLogin.MinimumSize = new System.Drawing.Size(300, 30);
            this.barLogin.Name = "barLogin";
            this.barLogin.Size = new System.Drawing.Size(300, 70);
            this.barLogin.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.barLogin.TabIndex = 5;
            this.barLogin.Text = "Login";
            // 
            // 
            // 
            this.barLogin.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.barLogin.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // cmdLogin
            // 
            this.cmdLogin.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdLogin.BackColor = System.Drawing.Color.Transparent;
            this.cmdLogin.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdLogin.FocusCuesEnabled = false;
            this.cmdLogin.Font = new System.Drawing.Font("Microsoft Tai Le", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.cmdLogin.Location = new System.Drawing.Point(224, 32);
            this.cmdLogin.Name = "cmdLogin";
            this.cmdLogin.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdLogin.Size = new System.Drawing.Size(66, 20);
            this.cmdLogin.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdLogin.TabIndex = 2;
            this.cmdLogin.Text = "Login";
            this.cmdLogin.Click += new System.EventHandler(this.cmdLogin_Click);
            // 
            // cmdUserGroup
            // 
            this.cmdUserGroup.CanCustomize = false;
            this.cmdUserGroup.ColorTable = DevComponents.DotNetBar.eButtonColor.Blue;
            this.cmdUserGroup.FixedSize = new System.Drawing.Size(75, 23);
            this.cmdUserGroup.HotTrackingStyle = DevComponents.DotNetBar.eHotTrackingStyle.Image;
            this.cmdUserGroup.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.cmdUserGroup.ImagePaddingHorizontal = 0;
            this.cmdUserGroup.ImagePaddingVertical = 0;
            this.cmdUserGroup.Name = "cmdUserGroup";
            this.cmdUserGroup.ShowSubItems = false;
            this.cmdUserGroup.Text = "Register";
            // 
            // tabXboxMb
            // 
            this.tabXboxMb.Checked = true;
            this.tabXboxMb.Name = "tabXboxMb";
            this.tabXboxMb.Panel = this.panelXboxMb;
            this.tabXboxMb.Text = "XboxMB Login";
            // 
            // styleMain
            // 
            this.styleMain.ManagerColorTint = System.Drawing.Color.DimGray;
            this.styleMain.ManagerStyle = DevComponents.DotNetBar.eStyle.Office2010Silver;
            this.styleMain.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.White, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(163)))), ((int)(((byte)(26))))));
            // 
            // cmdTwitterFollowUs
            // 
            this.cmdTwitterFollowUs.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdTwitterFollowUs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdTwitterFollowUs.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdTwitterFollowUs.FocusCuesEnabled = false;
            this.cmdTwitterFollowUs.Location = new System.Drawing.Point(99, 498);
            this.cmdTwitterFollowUs.Name = "cmdTwitterFollowUs";
            this.cmdTwitterFollowUs.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdTwitterFollowUs.Size = new System.Drawing.Size(101, 26);
            this.cmdTwitterFollowUs.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdTwitterFollowUs.TabIndex = 5;
            this.cmdTwitterFollowUs.Text = "Follow Us";
            // 
            // cmdTwitterRefresh
            // 
            this.cmdTwitterRefresh.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdTwitterRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdTwitterRefresh.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdTwitterRefresh.Enabled = false;
            this.cmdTwitterRefresh.FocusCuesEnabled = false;
            this.cmdTwitterRefresh.Location = new System.Drawing.Point(-1, 498);
            this.cmdTwitterRefresh.Name = "cmdTwitterRefresh";
            this.cmdTwitterRefresh.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdTwitterRefresh.Size = new System.Drawing.Size(101, 26);
            this.cmdTwitterRefresh.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdTwitterRefresh.TabIndex = 1;
            this.cmdTwitterRefresh.Text = "Refresh";
            // 
            // exTwitter
            // 
            this.exTwitter.CanvasColor = System.Drawing.SystemColors.Control;
            this.exTwitter.CollapseDirection = DevComponents.DotNetBar.eCollapseDirection.RightToLeft;
            this.exTwitter.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.exTwitter.Controls.Add(this.webTwitter);
            this.exTwitter.Controls.Add(this.cmdTwitterFollowUs);
            this.exTwitter.Controls.Add(this.cmdTwitterRefresh);
            this.exTwitter.Dock = System.Windows.Forms.DockStyle.Left;
            this.exTwitter.Expanded = false;
            this.exTwitter.ExpandedBounds = new System.Drawing.Rectangle(5, 129, 200, 523);
            this.exTwitter.Location = new System.Drawing.Point(5, 129);
            this.exTwitter.Name = "exTwitter";
            this.exTwitter.Size = new System.Drawing.Size(30, 523);
            this.exTwitter.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.exTwitter.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.exTwitter.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.exTwitter.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.exTwitter.Style.BorderSide = DevComponents.DotNetBar.eBorderSide.Right;
            this.exTwitter.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.exTwitter.Style.GradientAngle = 90;
            this.exTwitter.TabIndex = 6;
            this.exTwitter.TitleStyle.Alignment = System.Drawing.StringAlignment.Center;
            this.exTwitter.TitleStyle.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.exTwitter.TitleStyle.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.exTwitter.TitleStyle.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.exTwitter.TitleStyle.BorderSide = DevComponents.DotNetBar.eBorderSide.Right;
            this.exTwitter.TitleStyle.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.exTwitter.TitleStyle.GradientAngle = 90;
            this.exTwitter.TitleText = "XboxMB Twitter";
            // 
            // webTwitter
            // 
            this.webTwitter.AllowWebBrowserDrop = false;
            this.webTwitter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.webTwitter.IsWebBrowserContextMenuEnabled = false;
            this.webTwitter.Location = new System.Drawing.Point(-85, 26);
            this.webTwitter.MinimumSize = new System.Drawing.Size(20, 20);
            this.webTwitter.Name = "webTwitter";
            this.webTwitter.ScriptErrorsSuppressed = true;
            this.webTwitter.Size = new System.Drawing.Size(199, 472);
            this.webTwitter.TabIndex = 6;
            this.webTwitter.Url = new System.Uri("http://app.xboxmb.com/news_new.html", System.UriKind.Absolute);
            // 
            // statusBar
            // 
            this.statusBar.CanvasColor = System.Drawing.SystemColors.Control;
            this.statusBar.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Location = new System.Drawing.Point(35, 627);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(933, 25);
            this.statusBar.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.statusBar.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.statusBar.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.statusBar.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.statusBar.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.statusBar.Style.BorderSide = DevComponents.DotNetBar.eBorderSide.Top;
            this.statusBar.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.statusBar.Style.GradientAngle = 90;
            this.statusBar.TabIndex = 7;
            this.statusBar.Text = "Horizon";
            // 
            // exFatx
            // 
            this.exFatx.CanvasColor = System.Drawing.SystemColors.Control;
            this.exFatx.CollapseDirection = DevComponents.DotNetBar.eCollapseDirection.LeftToRight;
            this.exFatx.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.exFatx.Controls.Add(this.panelFatx);
            this.exFatx.Dock = System.Windows.Forms.DockStyle.Right;
            this.exFatx.Expanded = false;
            this.exFatx.ExpandedBounds = new System.Drawing.Rectangle(593, 129, 375, 498);
            this.exFatx.Location = new System.Drawing.Point(938, 129);
            this.exFatx.Name = "exFatx";
            this.exFatx.Size = new System.Drawing.Size(30, 498);
            this.exFatx.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.exFatx.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.exFatx.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.exFatx.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.exFatx.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.exFatx.Style.BorderSide = DevComponents.DotNetBar.eBorderSide.Left;
            this.exFatx.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.exFatx.Style.GradientAngle = 90;
            this.exFatx.TabIndex = 8;
            this.exFatx.TitleStyle.Alignment = System.Drawing.StringAlignment.Center;
            this.exFatx.TitleStyle.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.exFatx.TitleStyle.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.exFatx.TitleStyle.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.exFatx.TitleStyle.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.exFatx.TitleStyle.BorderSide = DevComponents.DotNetBar.eBorderSide.Left;
            this.exFatx.TitleStyle.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.exFatx.TitleStyle.GradientAngle = 90;
            this.exFatx.TitleText = "Device Explorer";
            // 
            // panelFatx
            // 
            this.panelFatx.Controls.Add(this.progressFatx);
            this.panelFatx.Controls.Add(this.cmdFatxExpand);
            this.panelFatx.Controls.Add(this.cmdFatxContract);
            this.panelFatx.Controls.Add(this.listFatx);
            this.panelFatx.Controls.Add(this.rbFatx);
            this.panelFatx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFatx.Location = new System.Drawing.Point(0, 26);
            this.panelFatx.Name = "panelFatx";
            this.panelFatx.Size = new System.Drawing.Size(30, 472);
            this.panelFatx.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.panelFatx.Style.BorderColor.Color = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(101)))), ((int)(((byte)(114)))));
            this.panelFatx.TabIndex = 3;
            // 
            // progressFatx
            // 
            this.progressFatx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressFatx.Location = new System.Drawing.Point(1, 449);
            this.progressFatx.Name = "progressFatx";
            this.progressFatx.Size = new System.Drawing.Size(228, 23);
            this.progressFatx.TabIndex = 9;
            // 
            // cmdFatxExpand
            // 
            this.cmdFatxExpand.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdFatxExpand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFatxExpand.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdFatxExpand.FocusCuesEnabled = false;
            this.cmdFatxExpand.Location = new System.Drawing.Point(-116, 448);
            this.cmdFatxExpand.Name = "cmdFatxExpand";
            this.cmdFatxExpand.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdFatxExpand.Size = new System.Drawing.Size(74, 25);
            this.cmdFatxExpand.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdFatxExpand.TabIndex = 7;
            this.cmdFatxExpand.Text = "Expand";
            this.cmdFatxExpand.Click += new System.EventHandler(this.cmdFatxExpand_Click);
            // 
            // cmdFatxContract
            // 
            this.cmdFatxContract.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdFatxContract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFatxContract.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdFatxContract.Enabled = false;
            this.cmdFatxContract.FocusCuesEnabled = false;
            this.cmdFatxContract.Location = new System.Drawing.Point(-43, 448);
            this.cmdFatxContract.Name = "cmdFatxContract";
            this.cmdFatxContract.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdFatxContract.Size = new System.Drawing.Size(74, 25);
            this.cmdFatxContract.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdFatxContract.TabIndex = 8;
            this.cmdFatxContract.Text = "Contract";
            this.cmdFatxContract.Click += new System.EventHandler(this.cmdFatxContract_Click);
            // 
            // listFatx
            // 
            this.listFatx.AllowDrop = true;
            this.listFatx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listFatx.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.listFatx.BackgroundStyle.Class = "TreeBorderKey";
            this.listFatx.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listFatx.CellEdit = true;
            this.listFatx.Columns.Add(this.colTreeEntry);
            this.listFatx.Columns.Add(this.colTreeInfo);
            this.listFatx.DragDropEnabled = false;
            this.listFatx.DragDropNodeCopyEnabled = false;
            this.listFatx.ExpandButtonType = DevComponents.AdvTree.eExpandButtonType.Triangle;
            this.listFatx.ExpandWidth = 14;
            this.listFatx.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listFatx.GridColumnLines = false;
            this.listFatx.GridRowLines = true;
            this.listFatx.HotTracking = true;
            this.listFatx.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.listFatx.Location = new System.Drawing.Point(0, 75);
            this.listFatx.MultiNodeDragDropAllowed = false;
            this.listFatx.MultiSelect = true;
            this.listFatx.Name = "listFatx";
            this.listFatx.NodesConnector = this.nodeConnectorFatx;
            this.listFatx.NodeStyle = this.fatxElementStyle;
            this.listFatx.PathSeparator = ";";
            this.listFatx.Size = new System.Drawing.Size(31, 374);
            this.listFatx.Styles.Add(this.fatxElementStyle);
            this.listFatx.TabIndex = 4;
            // 
            // colTreeEntry
            // 
            this.colTreeEntry.MinimumWidth = 60;
            this.colTreeEntry.Name = "colTreeEntry";
            this.colTreeEntry.Text = "Entry";
            this.colTreeEntry.Width.AutoSize = true;
            this.colTreeEntry.Width.AutoSizeMinHeader = true;
            // 
            // colTreeInfo
            // 
            this.colTreeInfo.MinimumWidth = 60;
            this.colTreeInfo.Name = "colTreeInfo";
            this.colTreeInfo.Text = "Info";
            this.colTreeInfo.Width.AutoSize = true;
            this.colTreeInfo.Width.AutoSizeMinHeader = true;
            // 
            // nodeConnectorFatx
            // 
            this.nodeConnectorFatx.LineColor = System.Drawing.SystemColors.ControlDark;
            // 
            // fatxElementStyle
            // 
            this.fatxElementStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.fatxElementStyle.Name = "fatxElementStyle";
            this.fatxElementStyle.TextColor = System.Drawing.SystemColors.ControlText;
            // 
            // rbFatx
            // 
            this.rbFatx.AutoExpand = false;
            this.rbFatx.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.rbFatx.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbFatx.CanCustomize = false;
            this.rbFatx.Controls.Add(this.rpFatxDrives);
            this.rbFatx.ForeColor = System.Drawing.Color.Black;
            this.rbFatx.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.cmdFatxDevicesLoaded,
            this.tabFatxDrive});
            this.rbFatx.KeyTipsFont = new System.Drawing.Font("Tahoma", 7F);
            this.rbFatx.Location = new System.Drawing.Point(1, 0);
            this.rbFatx.Name = "rbFatx";
            this.rbFatx.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.rbFatx.Size = new System.Drawing.Size(377, 449);
            this.rbFatx.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbFatx.SystemText.MaximizeRibbonText = "&Maximize the Ribbon";
            this.rbFatx.SystemText.MinimizeRibbonText = "Mi&nimize the Ribbon";
            this.rbFatx.SystemText.QatAddItemText = "&Add to Quick Access Toolbar";
            this.rbFatx.SystemText.QatCustomizeMenuLabel = "<b>Customize Quick Access Toolbar</b>";
            this.rbFatx.SystemText.QatCustomizeText = "&Customize Quick Access Toolbar...";
            this.rbFatx.SystemText.QatDialogAddButton = "&Add >>";
            this.rbFatx.SystemText.QatDialogCancelButton = "Cancel";
            this.rbFatx.SystemText.QatDialogCaption = "Customize Quick Access Toolbar";
            this.rbFatx.SystemText.QatDialogCategoriesLabel = "&Choose commands from:";
            this.rbFatx.SystemText.QatDialogOkButton = "OK";
            this.rbFatx.SystemText.QatDialogPlacementCheckbox = "&Place Quick Access Toolbar below the Ribbon";
            this.rbFatx.SystemText.QatDialogRemoveButton = "&Remove";
            this.rbFatx.SystemText.QatPlaceAboveRibbonText = "&Place Quick Access Toolbar above the Ribbon";
            this.rbFatx.SystemText.QatPlaceBelowRibbonText = "&Place Quick Access Toolbar below the Ribbon";
            this.rbFatx.SystemText.QatRemoveItemText = "&Remove from Quick Access Toolbar";
            this.rbFatx.TabGroupHeight = 14;
            this.rbFatx.TabIndex = 2;
            // 
            // rpFatxDrives
            // 
            this.rpFatxDrives.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rpFatxDrives.Controls.Add(this.cmdFatxGear);
            this.rpFatxDrives.Controls.Add(this.cmdFatxMod);
            this.rpFatxDrives.Controls.Add(this.cmdFatxExtract);
            this.rpFatxDrives.Controls.Add(this.cmdFatxInject);
            this.rpFatxDrives.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpFatxDrives.Location = new System.Drawing.Point(0, 25);
            this.rpFatxDrives.Name = "rpFatxDrives";
            this.rpFatxDrives.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.rpFatxDrives.Size = new System.Drawing.Size(377, 423);
            // 
            // 
            // 
            this.rpFatxDrives.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rpFatxDrives.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rpFatxDrives.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rpFatxDrives.TabIndex = 1;
            // 
            // cmdFatxGear
            // 
            this.cmdFatxGear.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdFatxGear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFatxGear.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdFatxGear.Enabled = false;
            this.cmdFatxGear.FocusCuesEnabled = false;
            this.cmdFatxGear.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdFatxGear.Image = global::NoDev.Horizon.Properties.Resources.Package_16;
            this.cmdFatxGear.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.cmdFatxGear.ImageTextSpacing = 3;
            this.cmdFatxGear.Location = new System.Drawing.Point(329, 0);
            this.cmdFatxGear.Name = "cmdFatxGear";
            this.cmdFatxGear.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdFatxGear.Size = new System.Drawing.Size(45, 51);
            this.cmdFatxGear.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdFatxGear.TabIndex = 0;
            this.cmdFatxGear.Text = "Manage";
            this.cmdFatxGear.Tooltip = "Open in Package Manager";
            // 
            // cmdFatxMod
            // 
            this.cmdFatxMod.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdFatxMod.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdFatxMod.Enabled = false;
            this.cmdFatxMod.FocusCuesEnabled = false;
            this.cmdFatxMod.Image = global::NoDev.Horizon.Properties.Resources.Thumb_QuestionMark;
            this.cmdFatxMod.ImagePosition = DevComponents.DotNetBar.eImagePosition.Right;
            this.cmdFatxMod.ImageTextSpacing = 2;
            this.cmdFatxMod.Location = new System.Drawing.Point(166, 0);
            this.cmdFatxMod.Name = "cmdFatxMod";
            this.cmdFatxMod.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdFatxMod.ShowSubItems = false;
            this.cmdFatxMod.Size = new System.Drawing.Size(164, 51);
            this.cmdFatxMod.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdFatxMod.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.galFatx});
            this.cmdFatxMod.TabIndex = 1;
            this.cmdFatxMod.Text = "Mod";
            this.cmdFatxMod.TextAlignment = DevComponents.DotNetBar.eButtonTextAlignment.Right;
            this.cmdFatxMod.Tooltip = "Mod this Package";
            // 
            // galFatx
            // 
            // 
            // 
            // 
            this.galFatx.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.galFatx.DefaultSize = new System.Drawing.Size(144, 405);
            this.galFatx.GlobalItem = false;
            this.galFatx.MinimumSize = new System.Drawing.Size(58, 58);
            this.galFatx.Name = "galFatx";
            this.galFatx.StretchGallery = true;
            // 
            // 
            // 
            this.galFatx.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // cmdFatxExtract
            // 
            this.cmdFatxExtract.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdFatxExtract.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFatxExtract.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdFatxExtract.Enabled = false;
            this.cmdFatxExtract.FocusCuesEnabled = false;
            this.cmdFatxExtract.Location = new System.Drawing.Point(-1, 0);
            this.cmdFatxExtract.Name = "cmdFatxExtract";
            this.cmdFatxExtract.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdFatxExtract.Size = new System.Drawing.Size(170, 26);
            this.cmdFatxExtract.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdFatxExtract.TabIndex = 2;
            this.cmdFatxExtract.Text = "Extract Selected File";
            // 
            // cmdFatxInject
            // 
            this.cmdFatxInject.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdFatxInject.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdFatxInject.Enabled = false;
            this.cmdFatxInject.FocusCuesEnabled = false;
            this.cmdFatxInject.Location = new System.Drawing.Point(-1, 25);
            this.cmdFatxInject.Name = "cmdFatxInject";
            this.cmdFatxInject.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdFatxInject.Size = new System.Drawing.Size(168, 26);
            this.cmdFatxInject.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdFatxInject.TabIndex = 3;
            this.cmdFatxInject.Text = "Inject New File";
            // 
            // cmdFatxDevicesLoaded
            // 
            this.cmdFatxDevicesLoaded.CanCustomize = false;
            this.cmdFatxDevicesLoaded.ColorTable = DevComponents.DotNetBar.eButtonColor.Blue;
            this.cmdFatxDevicesLoaded.FixedSize = new System.Drawing.Size(105, 23);
            this.cmdFatxDevicesLoaded.HotTrackingStyle = DevComponents.DotNetBar.eHotTrackingStyle.Image;
            this.cmdFatxDevicesLoaded.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.cmdFatxDevicesLoaded.ImagePaddingHorizontal = 0;
            this.cmdFatxDevicesLoaded.ImagePaddingVertical = 0;
            this.cmdFatxDevicesLoaded.Name = "cmdFatxDevicesLoaded";
            this.cmdFatxDevicesLoaded.ShowSubItems = false;
            this.cmdFatxDevicesLoaded.Text = "0 Devices Loaded";
            // 
            // tabFatxDrive
            // 
            this.tabFatxDrive.Checked = true;
            this.tabFatxDrive.Name = "tabFatxDrive";
            this.tabFatxDrive.Panel = this.rpFatxDrives;
            this.tabFatxDrive.Text = "Connect a Device";
            // 
            // cmdSettings
            // 
            this.cmdSettings.ColorTable = DevComponents.DotNetBar.eButtonColor.Flat;
            this.cmdSettings.Image = global::NoDev.Horizon.Properties.Resources.Gear_16;
            this.cmdSettings.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
            this.cmdSettings.Name = "cmdSettings";
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.LightGray;
            this.BackgroundImage = global::NoDev.Horizon.Properties.Resources.Tile;
            this.ClientSize = new System.Drawing.Size(973, 654);
            this.Controls.Add(this.exFatx);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.exTwitter);
            this.Controls.Add(this.ribbonHorizon);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Horizon";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Main_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Main_DragEnter);
            this.ribbonHorizon.ResumeLayout(false);
            this.ribbonHorizon.PerformLayout();
            this.panelXboxMb.ResumeLayout(false);
            this.barProfile.ResumeLayout(false);
            this.barRegister.ResumeLayout(false);
            this.barForgotPassword.ResumeLayout(false);
            this.barLogin.ResumeLayout(false);
            this.exTwitter.ResumeLayout(false);
            this.exFatx.ResumeLayout(false);
            this.panelFatx.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.listFatx)).EndInit();
            this.rbFatx.ResumeLayout(false);
            this.rbFatx.PerformLayout();
            this.rpFatxDrives.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.RibbonControl ribbonHorizon;
        private DevComponents.DotNetBar.RibbonPanel panelXboxMb;
        private DevComponents.DotNetBar.Office2007StartButton cmdUserGroup;
        private DevComponents.DotNetBar.RibbonTabItem tabXboxMb;
        private DevComponents.DotNetBar.StyleManager styleMain;
        private DevComponents.DotNetBar.ButtonX cmdTwitterFollowUs;
        private DevComponents.DotNetBar.ButtonX cmdTwitterRefresh;
        private DevComponents.DotNetBar.ExpandablePanel exTwitter;
        private DevComponents.DotNetBar.PanelEx statusBar;
        private DevComponents.DotNetBar.ExpandablePanel exFatx;
        private System.Windows.Forms.WebBrowser webTwitter;
        internal DevComponents.DotNetBar.RibbonBar barProfile;
        internal DevComponents.DotNetBar.ButtonX cmdProfile;
        private DevComponents.DotNetBar.LabelItem labelItem2;
        internal DevComponents.DotNetBar.RibbonBar barRegister;
        internal DevComponents.DotNetBar.ButtonX cmdRegister;
        private DevComponents.DotNetBar.LabelItem labelItem1;
        internal DevComponents.DotNetBar.RibbonBar barForgotPassword;
        private DevComponents.DotNetBar.ButtonX cmdForgotPassword;
        private DevComponents.DotNetBar.LabelItem pad1;
        private DevComponents.DotNetBar.RibbonBar barLogin;
        internal DevComponents.DotNetBar.ButtonX cmdLogin;
        internal DevComponents.DotNetBar.Controls.TextBoxX txtPassword;
        internal DevComponents.DotNetBar.Controls.TextBoxX txtUsername;
        private DevComponents.DotNetBar.PanelEx panelFatx;
        internal System.Windows.Forms.ProgressBar progressFatx;
        internal DevComponents.AdvTree.AdvTree listFatx;
        internal DevComponents.AdvTree.ColumnHeader colTreeEntry;
        internal DevComponents.AdvTree.ColumnHeader colTreeInfo;
        private DevComponents.AdvTree.NodeConnector nodeConnectorFatx;
        internal DevComponents.DotNetBar.RibbonControl rbFatx;
        internal DevComponents.DotNetBar.RibbonPanel rpFatxDrives;
        internal DevComponents.DotNetBar.ButtonX cmdFatxGear;
        internal DevComponents.DotNetBar.ButtonX cmdFatxMod;
        internal DevComponents.DotNetBar.GalleryContainer galFatx;
        internal DevComponents.DotNetBar.ButtonX cmdFatxExtract;
        internal DevComponents.DotNetBar.ButtonX cmdFatxInject;
        internal DevComponents.DotNetBar.Office2007StartButton cmdFatxDevicesLoaded;
        internal DevComponents.DotNetBar.RibbonTabItem tabFatxDrive;
        private DevComponents.DotNetBar.ElementStyle fatxElementStyle;
        private DevComponents.DotNetBar.LabelX lblPassword;
        private DevComponents.DotNetBar.LabelX lblUsername;
        internal DevComponents.DotNetBar.ButtonX cmdAutoLogin;
        private DevComponents.DotNetBar.ButtonItem cmdSettings;
        private DevComponents.DotNetBar.ButtonX cmdFatxExpand;
        private DevComponents.DotNetBar.ButtonX cmdFatxContract;
    }
}

