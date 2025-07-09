namespace NoDev.Horizon.Editors.Game_Adder
{
    partial class GameAdder
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
            this.listTitles = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnTitleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnTitleID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnGamerscore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAwards = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblNotPlayed = new DevComponents.DotNetBar.LabelX();
            this.cmdNext = new NoDev.Horizon.Controls.SquareButton();
            this.txtSearch = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.cmdSelectAll = new NoDev.Horizon.Controls.SquareButton();
            this.cmdContinue = new NoDev.Horizon.Controls.SquareButton();
            this.tabAddGames = new DevComponents.DotNetBar.RibbonTabItem();
            this.panelAddGames = new DevComponents.DotNetBar.RibbonPanel();
            this.panelStatus = new DevComponents.DotNetBar.PanelEx();
            this.listQueue = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnQueueTitleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnQueueDownloaded = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnQueueAdded = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdAddGames = new NoDev.Horizon.Controls.SquareButton();
            this.progressAdder = new System.Windows.Forms.ProgressBar();
            this.cmdCancel = new NoDev.Horizon.Controls.SquareButton();
            this.cmdDetails = new NoDev.Horizon.Controls.SquareButton();
            this.tabDetails = new DevComponents.DotNetBar.RibbonTabItem();
            this.panelDetails = new DevComponents.DotNetBar.RibbonPanel();
            this.lblTitleName = new DevComponents.DotNetBar.LabelX();
            this.pbGameTile = new System.Windows.Forms.PictureBox();
            this.cmdGoBack = new NoDev.Horizon.Controls.SquareButton();
            this.listAwards = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnAwardName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAwardType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAwardGender = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listAchievements = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnAchievementName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnCredit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelMain.SuspendLayout();
            this.controlRibbon.SuspendLayout();
            this.panelAddGames.SuspendLayout();
            this.panelDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGameTile)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.cmdDetails);
            this.panelMain.Controls.Add(this.cmdContinue);
            this.panelMain.Controls.Add(this.cmdSelectAll);
            this.panelMain.Controls.Add(this.txtSearch);
            this.panelMain.Controls.Add(this.cmdNext);
            this.panelMain.Controls.Add(this.lblNotPlayed);
            this.panelMain.Controls.Add(this.listTitles);
            this.panelMain.Location = new System.Drawing.Point(0, 53);
            this.panelMain.Size = new System.Drawing.Size(485, 378);
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
            this.tabMain.Text = "Games";
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Controls.Add(this.panelAddGames);
            this.controlRibbon.Controls.Add(this.panelDetails);
            this.controlRibbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.tabDetails,
            this.tabAddGames});
            this.controlRibbon.Size = new System.Drawing.Size(485, 434);
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
            this.controlRibbon.Controls.SetChildIndex(this.panelDetails, 0);
            this.controlRibbon.Controls.SetChildIndex(this.panelAddGames, 0);
            this.controlRibbon.Controls.SetChildIndex(this.panelMain, 0);
            // 
            // listTitles
            // 
            this.listTitles.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            // 
            // 
            // 
            this.listTitles.Border.Class = "ListViewBorder";
            this.listTitles.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listTitles.CheckBoxes = true;
            this.listTitles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTitleName,
            this.columnTitleID,
            this.columnGamerscore,
            this.columnAwards});
            this.listTitles.FullRowSelect = true;
            this.listTitles.GridLines = true;
            this.listTitles.Location = new System.Drawing.Point(12, 43);
            this.listTitles.MultiSelect = false;
            this.listTitles.Name = "listTitles";
            this.listTitles.Size = new System.Drawing.Size(461, 300);
            this.listTitles.TabIndex = 15;
            this.listTitles.UseCompatibleStateImageBehavior = false;
            this.listTitles.View = System.Windows.Forms.View.Details;
            this.listTitles.ItemActivate += new System.EventHandler(this.cmdDetails_Click);
            this.listTitles.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listTitles_ItemCheck);
            // 
            // columnTitleName
            // 
            this.columnTitleName.Text = "Title Name";
            this.columnTitleName.Width = 188;
            // 
            // columnTitleID
            // 
            this.columnTitleID.Text = "Title ID";
            this.columnTitleID.Width = 90;
            // 
            // columnGamerscore
            // 
            this.columnGamerscore.Text = "Gamerscore";
            this.columnGamerscore.Width = 80;
            // 
            // columnAwards
            // 
            this.columnAwards.Text = "Awards";
            this.columnAwards.Width = 80;
            // 
            // lblNotPlayed
            // 
            this.lblNotPlayed.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblNotPlayed.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNotPlayed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblNotPlayed.Location = new System.Drawing.Point(12, 3);
            this.lblNotPlayed.Name = "lblNotPlayed";
            this.lblNotPlayed.Size = new System.Drawing.Size(265, 34);
            this.lblNotPlayed.TabIndex = 16;
            this.lblNotPlayed.Text = "Games that you have not played.";
            // 
            // cmdNext
            // 
            this.cmdNext.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdNext.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdNext.FocusCuesEnabled = false;
            this.cmdNext.Location = new System.Drawing.Point(261, 349);
            this.cmdNext.Name = "cmdNext";
            this.cmdNext.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdNext.Size = new System.Drawing.Size(95, 20);
            this.cmdNext.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdNext.Symbol = "";
            this.cmdNext.SymbolSize = 12F;
            this.cmdNext.TabIndex = 17;
            this.cmdNext.Text = "Next";
            this.cmdNext.Click += new System.EventHandler(this.cmdNext_Click);
            // 
            // txtSearch
            // 
            // 
            // 
            // 
            this.txtSearch.Border.Class = "TextBoxBorder";
            this.txtSearch.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtSearch.Location = new System.Drawing.Point(12, 349);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(243, 20);
            this.txtSearch.TabIndex = 19;
            this.txtSearch.WatermarkText = "Quick search...";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // cmdSelectAll
            // 
            this.cmdSelectAll.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdSelectAll.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdSelectAll.FocusCuesEnabled = false;
            this.cmdSelectAll.Location = new System.Drawing.Point(283, 14);
            this.cmdSelectAll.Name = "cmdSelectAll";
            this.cmdSelectAll.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdSelectAll.Size = new System.Drawing.Size(92, 23);
            this.cmdSelectAll.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdSelectAll.Symbol = "";
            this.cmdSelectAll.SymbolSize = 10F;
            this.cmdSelectAll.TabIndex = 20;
            this.cmdSelectAll.Text = "Select All";
            this.cmdSelectAll.Click += new System.EventHandler(this.cmdSelectAll_Click);
            // 
            // cmdContinue
            // 
            this.cmdContinue.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdContinue.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdContinue.FocusCuesEnabled = false;
            this.cmdContinue.Location = new System.Drawing.Point(381, 14);
            this.cmdContinue.Name = "cmdContinue";
            this.cmdContinue.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdContinue.Size = new System.Drawing.Size(92, 23);
            this.cmdContinue.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdContinue.Symbol = "";
            this.cmdContinue.SymbolSize = 10F;
            this.cmdContinue.TabIndex = 21;
            this.cmdContinue.Text = "Continue";
            this.cmdContinue.Click += new System.EventHandler(this.cmdContinue_Click);
            // 
            // tabAddGames
            // 
            this.tabAddGames.Name = "tabAddGames";
            this.tabAddGames.Panel = this.panelAddGames;
            this.tabAddGames.Text = "Add Games";
            this.tabAddGames.Visible = false;
            // 
            // panelAddGames
            // 
            this.panelAddGames.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelAddGames.Controls.Add(this.panelStatus);
            this.panelAddGames.Controls.Add(this.listQueue);
            this.panelAddGames.Controls.Add(this.cmdAddGames);
            this.panelAddGames.Controls.Add(this.progressAdder);
            this.panelAddGames.Controls.Add(this.cmdCancel);
            this.panelAddGames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAddGames.Location = new System.Drawing.Point(0, 53);
            this.panelAddGames.Name = "panelAddGames";
            this.panelAddGames.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelAddGames.Size = new System.Drawing.Size(485, 378);
            // 
            // 
            // 
            this.panelAddGames.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelAddGames.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelAddGames.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.panelAddGames.TabIndex = 2;
            this.panelAddGames.Visible = false;
            // 
            // panelStatus
            // 
            this.panelStatus.CanvasColor = System.Drawing.SystemColors.Control;
            this.panelStatus.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelStatus.Location = new System.Drawing.Point(0, 349);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(485, 29);
            this.panelStatus.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelStatus.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.panelStatus.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.panelStatus.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.panelStatus.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.panelStatus.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.panelStatus.Style.GradientAngle = 90;
            this.panelStatus.TabIndex = 18;
            this.panelStatus.Text = "Downloading game data. Please wait...";
            // 
            // listQueue
            // 
            // 
            // 
            // 
            this.listQueue.Border.Class = "ListViewBorder";
            this.listQueue.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listQueue.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnQueueTitleName,
            this.columnQueueDownloaded,
            this.columnQueueAdded});
            this.listQueue.FullRowSelect = true;
            this.listQueue.GridLines = true;
            this.listQueue.Location = new System.Drawing.Point(12, 10);
            this.listQueue.MultiSelect = false;
            this.listQueue.Name = "listQueue";
            this.listQueue.Size = new System.Drawing.Size(461, 257);
            this.listQueue.TabIndex = 16;
            this.listQueue.UseCompatibleStateImageBehavior = false;
            this.listQueue.View = System.Windows.Forms.View.Details;
            // 
            // columnQueueTitleName
            // 
            this.columnQueueTitleName.Text = "Title Name";
            this.columnQueueTitleName.Width = 247;
            // 
            // columnQueueDownloaded
            // 
            this.columnQueueDownloaded.Text = "Downloaded";
            this.columnQueueDownloaded.Width = 90;
            // 
            // columnQueueAdded
            // 
            this.columnQueueAdded.Text = "Added";
            this.columnQueueAdded.Width = 90;
            // 
            // cmdAddGames
            // 
            this.cmdAddGames.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdAddGames.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdAddGames.Enabled = false;
            this.cmdAddGames.FocusCuesEnabled = false;
            this.cmdAddGames.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.cmdAddGames.Location = new System.Drawing.Point(12, 273);
            this.cmdAddGames.Name = "cmdAddGames";
            this.cmdAddGames.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdAddGames.Size = new System.Drawing.Size(182, 70);
            this.cmdAddGames.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdAddGames.Symbol = "";
            this.cmdAddGames.TabIndex = 5;
            this.cmdAddGames.Text = "Add Games";
            this.cmdAddGames.Click += new System.EventHandler(this.cmdAddGames_Click);
            // 
            // progressAdder
            // 
            this.progressAdder.Location = new System.Drawing.Point(200, 273);
            this.progressAdder.Name = "progressAdder";
            this.progressAdder.Size = new System.Drawing.Size(273, 38);
            this.progressAdder.TabIndex = 4;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdCancel.FocusCuesEnabled = false;
            this.cmdCancel.Location = new System.Drawing.Point(200, 317);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdCancel.Size = new System.Drawing.Size(273, 26);
            this.cmdCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdCancel.TabIndex = 2;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdDetails
            // 
            this.cmdDetails.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdDetails.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdDetails.FocusCuesEnabled = false;
            this.cmdDetails.Location = new System.Drawing.Point(362, 349);
            this.cmdDetails.Name = "cmdDetails";
            this.cmdDetails.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdDetails.Size = new System.Drawing.Size(111, 20);
            this.cmdDetails.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdDetails.Symbol = "";
            this.cmdDetails.SymbolSize = 12F;
            this.cmdDetails.TabIndex = 22;
            this.cmdDetails.Text = "Details";
            this.cmdDetails.Click += new System.EventHandler(this.cmdDetails_Click);
            // 
            // tabDetails
            // 
            this.tabDetails.Name = "tabDetails";
            this.tabDetails.Panel = this.panelDetails;
            this.tabDetails.Text = "Game Details";
            this.tabDetails.Visible = false;
            // 
            // panelDetails
            // 
            this.panelDetails.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelDetails.Controls.Add(this.lblTitleName);
            this.panelDetails.Controls.Add(this.pbGameTile);
            this.panelDetails.Controls.Add(this.cmdGoBack);
            this.panelDetails.Controls.Add(this.listAwards);
            this.panelDetails.Controls.Add(this.listAchievements);
            this.panelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetails.Location = new System.Drawing.Point(0, 53);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelDetails.Size = new System.Drawing.Size(485, 378);
            // 
            // 
            // 
            this.panelDetails.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelDetails.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.panelDetails.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.panelDetails.TabIndex = 3;
            this.panelDetails.Visible = false;
            // 
            // lblTitleName
            // 
            this.lblTitleName.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblTitleName.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblTitleName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblTitleName.Location = new System.Drawing.Point(50, 5);
            this.lblTitleName.Name = "lblTitleName";
            this.lblTitleName.Size = new System.Drawing.Size(322, 32);
            this.lblTitleName.TabIndex = 25;
            this.lblTitleName.Text = "Downloading game data. Please wait...";
            this.lblTitleName.WordWrap = true;
            // 
            // pbGameTile
            // 
            this.pbGameTile.Image = global::NoDev.Horizon.Properties.Resources.QuestionMark_64;
            this.pbGameTile.Location = new System.Drawing.Point(12, 5);
            this.pbGameTile.Name = "pbGameTile";
            this.pbGameTile.Size = new System.Drawing.Size(32, 32);
            this.pbGameTile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbGameTile.TabIndex = 24;
            this.pbGameTile.TabStop = false;
            // 
            // cmdGoBack
            // 
            this.cmdGoBack.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdGoBack.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdGoBack.FocusCuesEnabled = false;
            this.cmdGoBack.Location = new System.Drawing.Point(381, 14);
            this.cmdGoBack.Name = "cmdGoBack";
            this.cmdGoBack.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdGoBack.Size = new System.Drawing.Size(92, 23);
            this.cmdGoBack.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdGoBack.Symbol = "";
            this.cmdGoBack.SymbolSize = 10F;
            this.cmdGoBack.TabIndex = 23;
            this.cmdGoBack.Text = "Go Back";
            this.cmdGoBack.Click += new System.EventHandler(this.cmdGoBack_Click);
            // 
            // listAwards
            // 
            // 
            // 
            // 
            this.listAwards.Border.Class = "ListViewBorder";
            this.listAwards.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listAwards.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnAwardName,
            this.columnAwardType,
            this.columnAwardGender});
            this.listAwards.FullRowSelect = true;
            this.listAwards.GridLines = true;
            this.listAwards.Location = new System.Drawing.Point(12, 207);
            this.listAwards.MultiSelect = false;
            this.listAwards.Name = "listAwards";
            this.listAwards.Size = new System.Drawing.Size(461, 162);
            this.listAwards.TabIndex = 17;
            this.listAwards.UseCompatibleStateImageBehavior = false;
            this.listAwards.View = System.Windows.Forms.View.Details;
            // 
            // columnAwardName
            // 
            this.columnAwardName.Text = "Award Name";
            this.columnAwardName.Width = 239;
            // 
            // columnAwardType
            // 
            this.columnAwardType.Text = "Type";
            this.columnAwardType.Width = 126;
            // 
            // columnAwardGender
            // 
            this.columnAwardGender.Text = "Gender";
            // 
            // listAchievements
            // 
            // 
            // 
            // 
            this.listAchievements.Border.Class = "ListViewBorder";
            this.listAchievements.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listAchievements.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnAchievementName,
            this.columnCredit});
            this.listAchievements.FullRowSelect = true;
            this.listAchievements.GridLines = true;
            this.listAchievements.Location = new System.Drawing.Point(12, 43);
            this.listAchievements.MultiSelect = false;
            this.listAchievements.Name = "listAchievements";
            this.listAchievements.Size = new System.Drawing.Size(461, 158);
            this.listAchievements.TabIndex = 16;
            this.listAchievements.UseCompatibleStateImageBehavior = false;
            this.listAchievements.View = System.Windows.Forms.View.Details;
            // 
            // columnAchievementName
            // 
            this.columnAchievementName.Text = "Achievement Name";
            this.columnAchievementName.Width = 333;
            // 
            // columnCredit
            // 
            this.columnCredit.Text = "Gamerscore";
            this.columnCredit.Width = 90;
            // 
            // GameAdder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 437);
            this.MaximumSize = new System.Drawing.Size(495, 437);
            this.MinimumSize = new System.Drawing.Size(495, 437);
            this.Name = "GameAdder";
            this.Text = "Game Adder";
            this.panelMain.ResumeLayout(false);
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            this.panelAddGames.ResumeLayout(false);
            this.panelDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbGameTile)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.LabelX lblNotPlayed;
        private DevComponents.DotNetBar.Controls.ListViewEx listTitles;
        private System.Windows.Forms.ColumnHeader columnTitleName;
        private System.Windows.Forms.ColumnHeader columnTitleID;
        private System.Windows.Forms.ColumnHeader columnGamerscore;
        private System.Windows.Forms.ColumnHeader columnAwards;
        private DevComponents.DotNetBar.Controls.TextBoxX txtSearch;
        private Controls.SquareButton cmdNext;
        private Controls.SquareButton cmdSelectAll;
        private Controls.SquareButton cmdContinue;
        private DevComponents.DotNetBar.RibbonPanel panelAddGames;
        private DevComponents.DotNetBar.RibbonTabItem tabAddGames;
        private Controls.SquareButton cmdCancel;
        private Controls.SquareButton cmdDetails;
        private DevComponents.DotNetBar.RibbonPanel panelDetails;
        private DevComponents.DotNetBar.Controls.ListViewEx listAwards;
        private System.Windows.Forms.ColumnHeader columnAwardName;
        private System.Windows.Forms.ColumnHeader columnAwardType;
        private System.Windows.Forms.ColumnHeader columnAwardGender;
        private DevComponents.DotNetBar.Controls.ListViewEx listAchievements;
        private System.Windows.Forms.ColumnHeader columnAchievementName;
        private System.Windows.Forms.ColumnHeader columnCredit;
        private DevComponents.DotNetBar.RibbonTabItem tabDetails;
        private System.Windows.Forms.ProgressBar progressAdder;
        private Controls.SquareButton cmdAddGames;
        private DevComponents.DotNetBar.Controls.ListViewEx listQueue;
        private System.Windows.Forms.ColumnHeader columnQueueTitleName;
        private System.Windows.Forms.ColumnHeader columnQueueDownloaded;
        private System.Windows.Forms.ColumnHeader columnQueueAdded;
        private DevComponents.DotNetBar.PanelEx panelStatus;
        private DevComponents.DotNetBar.LabelX lblTitleName;
        private System.Windows.Forms.PictureBox pbGameTile;
        private Controls.SquareButton cmdGoBack;

    }
}
