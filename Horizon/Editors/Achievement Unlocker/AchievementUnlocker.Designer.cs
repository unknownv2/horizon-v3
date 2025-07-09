namespace NoDev.Horizon.Editors.Achievement_Unlocker
{
    partial class AchievementUnlocker
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
            this.listAch = new DevComponents.AdvTree.AdvTree();
            this.colAchievement = new DevComponents.AdvTree.ColumnHeader();
            this.colCredit = new DevComponents.AdvTree.ColumnHeader();
            this.colDescription = new DevComponents.AdvTree.ColumnHeader();
            this.nodeConnector2 = new DevComponents.AdvTree.NodeConnector();
            this.elementStyle3 = new DevComponents.DotNetBar.ElementStyle();
            this.cmdUnlockTitle = new NoDev.Horizon.Controls.SquareButton();
            this.cmdUnlock = new NoDev.Horizon.Controls.SquareButton();
            this.dateTimeTitle = new DevComponents.Editors.DateTimeAdv.DateTimeInput();
            this.lblLastPlayed = new DevComponents.DotNetBar.LabelX();
            this.progressTitle = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.progressTotal = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.node1 = new DevComponents.AdvTree.Node();
            this.cmdRules = new NoDev.Horizon.Controls.SquareButton();
            this.cmdSelectAll = new NoDev.Horizon.Controls.SquareButton();
            this.dateTimeAch = new DevComponents.Editors.DateTimeAdv.DateTimeInput();
            this.listTitles = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnTitleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnGs = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdUnlockAll = new DevComponents.DotNetBar.ButtonItem();
            this.panelMain.SuspendLayout();
            this.controlRibbon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listAch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimeTitle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimeAch)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.listTitles);
            this.panelMain.Controls.Add(this.dateTimeAch);
            this.panelMain.Controls.Add(this.cmdSelectAll);
            this.panelMain.Controls.Add(this.cmdRules);
            this.panelMain.Controls.Add(this.progressTitle);
            this.panelMain.Controls.Add(this.lblLastPlayed);
            this.panelMain.Controls.Add(this.dateTimeTitle);
            this.panelMain.Controls.Add(this.cmdUnlock);
            this.panelMain.Controls.Add(this.cmdUnlockTitle);
            this.panelMain.Controls.Add(this.listAch);
            this.panelMain.Location = new System.Drawing.Point(0, 53);
            this.panelMain.Size = new System.Drawing.Size(726, 406);
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
            // 
            // tabMain
            // 
            this.tabMain.Text = "Titles and Achievements";
            // 
            // controlRibbon
            // 
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.cmdUnlockAll});
            this.controlRibbon.Size = new System.Drawing.Size(726, 462);
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
            // listAch
            // 
            this.listAch.AllowDrop = true;
            this.listAch.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.listAch.BackgroundStyle.Class = "TreeBorderKey";
            this.listAch.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listAch.Columns.Add(this.colAchievement);
            this.listAch.Columns.Add(this.colCredit);
            this.listAch.Columns.Add(this.colDescription);
            this.listAch.ColumnsVisible = false;
            this.listAch.DragDropNodeCopyEnabled = false;
            this.listAch.ExpandButtonType = DevComponents.AdvTree.eExpandButtonType.Triangle;
            this.listAch.ExpandWidth = 3;
            this.listAch.GridRowLines = true;
            this.listAch.HotTracking = true;
            this.listAch.Indent = 3;
            this.listAch.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.listAch.Location = new System.Drawing.Point(229, 12);
            this.listAch.MultiNodeDragDropAllowed = false;
            this.listAch.Name = "listAch";
            this.listAch.NodesConnector = this.nodeConnector2;
            this.listAch.NodeStyle = this.elementStyle3;
            this.listAch.PathSeparator = ";";
            this.listAch.SelectionBoxStyle = DevComponents.AdvTree.eSelectionStyle.FullRowSelect;
            this.listAch.Size = new System.Drawing.Size(485, 314);
            this.listAch.Styles.Add(this.elementStyle3);
            this.listAch.TabIndex = 19;
            this.listAch.AfterCheck += new DevComponents.AdvTree.AdvTreeCellEventHandler(this.listAch_AfterCheck);
            this.listAch.AfterNodeSelect += new DevComponents.AdvTree.AdvTreeNodeEventHandler(this.listAch_AfterNodeSelect);
            this.listAch.NodeDoubleClick += new DevComponents.AdvTree.TreeNodeMouseEventHandler(this.listAch_NodeDoubleClick);
            this.listAch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listAch_KeyUp);
            this.listAch.MouseEnter += new System.EventHandler(this.listAch_MouseEnter);
            // 
            // colAchievement
            // 
            this.colAchievement.Editable = false;
            this.colAchievement.Name = "colAchievement";
            this.colAchievement.Text = "Achievement";
            this.colAchievement.Width.Absolute = 190;
            // 
            // colCredit
            // 
            this.colCredit.Editable = false;
            this.colCredit.Name = "colCredit";
            this.colCredit.Text = "G";
            this.colCredit.Width.Absolute = 35;
            // 
            // colDescription
            // 
            this.colDescription.Editable = false;
            this.colDescription.Name = "colDescription";
            this.colDescription.StretchToFill = true;
            this.colDescription.Text = "Description";
            this.colDescription.Width.Absolute = 150;
            // 
            // nodeConnector2
            // 
            this.nodeConnector2.LineColor = System.Drawing.SystemColors.ControlDark;
            // 
            // elementStyle3
            // 
            this.elementStyle3.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle3.Name = "elementStyle3";
            this.elementStyle3.TextColor = System.Drawing.SystemColors.ControlText;
            this.elementStyle3.WordWrap = true;
            // 
            // cmdUnlockTitle
            // 
            this.cmdUnlockTitle.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdUnlockTitle.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdUnlockTitle.FocusCuesEnabled = false;
            this.cmdUnlockTitle.Location = new System.Drawing.Point(346, 332);
            this.cmdUnlockTitle.Name = "cmdUnlockTitle";
            this.cmdUnlockTitle.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdUnlockTitle.Size = new System.Drawing.Size(191, 39);
            this.cmdUnlockTitle.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdUnlockTitle.Symbol = "";
            this.cmdUnlockTitle.SymbolSize = 15F;
            this.cmdUnlockTitle.TabIndex = 21;
            this.cmdUnlockTitle.Text = "<span align=\"center\">Unlock Selected Achievements<br></br>\r\n<font color=\"#A5A5A5\"" +
    ">Using unlock rule 1</font></span>";
            this.cmdUnlockTitle.Click += new System.EventHandler(this.cmdUnlockTitle_Click);
            // 
            // cmdUnlock
            // 
            this.cmdUnlock.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdUnlock.AutoCheckOnClick = true;
            this.cmdUnlock.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdUnlock.FocusCuesEnabled = false;
            this.cmdUnlock.Location = new System.Drawing.Point(543, 332);
            this.cmdUnlock.Name = "cmdUnlock";
            this.cmdUnlock.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdUnlock.Size = new System.Drawing.Size(171, 39);
            this.cmdUnlock.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdUnlock.Symbol = "";
            this.cmdUnlock.SymbolSize = 15F;
            this.cmdUnlock.TabIndex = 23;
            this.cmdUnlock.Text = "Unlocked";
            this.cmdUnlock.CheckedChanged += new System.EventHandler(this.cmdUnlock_CheckedChanged);
            // 
            // dateTimeTitle
            // 
            this.dateTimeTitle.AntiAlias = true;
            this.dateTimeTitle.AutoAdvance = true;
            // 
            // 
            // 
            this.dateTimeTitle.BackgroundStyle.Class = "DateTimeInputBackground";
            this.dateTimeTitle.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeTitle.ButtonDropDown.Shortcut = DevComponents.DotNetBar.eShortcut.AltDown;
            this.dateTimeTitle.ButtonDropDown.Visible = true;
            this.dateTimeTitle.DateTimeSelectorVisibility = DevComponents.Editors.DateTimeAdv.eDateTimeSelectorVisibility.Both;
            this.dateTimeTitle.Format = DevComponents.Editors.eDateTimePickerFormat.Custom;
            this.dateTimeTitle.IsPopupCalendarOpen = false;
            this.dateTimeTitle.Location = new System.Drawing.Point(12, 377);
            this.dateTimeTitle.MinDate = new System.DateTime(2005, 11, 22, 0, 0, 0, 0);
            // 
            // 
            // 
            this.dateTimeTitle.MonthCalendar.AnnuallyMarkedDates = new System.DateTime[0];
            // 
            // 
            // 
            this.dateTimeTitle.MonthCalendar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeTitle.MonthCalendar.CalendarDimensions = new System.Drawing.Size(1, 1);
            this.dateTimeTitle.MonthCalendar.ClearButtonVisible = true;
            // 
            // 
            // 
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground2;
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.BackColorGradientAngle = 90;
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.BorderTopColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.BorderTopWidth = 1;
            this.dateTimeTitle.MonthCalendar.CommandsBackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeTitle.MonthCalendar.DayClickAutoClosePopup = false;
            this.dateTimeTitle.MonthCalendar.DisplayMonth = new System.DateTime(2012, 11, 1, 0, 0, 0, 0);
            this.dateTimeTitle.MonthCalendar.MarkedDates = new System.DateTime[0];
            this.dateTimeTitle.MonthCalendar.MonthlyMarkedDates = new System.DateTime[0];
            // 
            // 
            // 
            this.dateTimeTitle.MonthCalendar.NavigationBackgroundStyle.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.dateTimeTitle.MonthCalendar.NavigationBackgroundStyle.BackColorGradientAngle = 90;
            this.dateTimeTitle.MonthCalendar.NavigationBackgroundStyle.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.dateTimeTitle.MonthCalendar.NavigationBackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeTitle.MonthCalendar.TodayButtonVisible = true;
            this.dateTimeTitle.MonthCalendar.WeeklyMarkedDays = new System.DayOfWeek[0];
            this.dateTimeTitle.Name = "dateTimeTitle";
            this.dateTimeTitle.ShowCheckBox = true;
            this.dateTimeTitle.ShowUpDown = true;
            this.dateTimeTitle.Size = new System.Drawing.Size(211, 20);
            this.dateTimeTitle.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.dateTimeTitle.TabIndex = 24;
            this.dateTimeTitle.Value = new System.DateTime(2005, 11, 22, 0, 0, 0, 0);
            this.dateTimeTitle.LockUpdateChanged += new System.EventHandler(this.dateTimeTitle_LockUpdateChanged);
            // 
            // lblLastPlayed
            // 
            this.lblLastPlayed.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblLastPlayed.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblLastPlayed.Location = new System.Drawing.Point(12, 352);
            this.lblLastPlayed.Name = "lblLastPlayed";
            this.lblLastPlayed.Size = new System.Drawing.Size(211, 19);
            this.lblLastPlayed.TabIndex = 25;
            this.lblLastPlayed.Text = "Last played (check for online):";
            // 
            // progressTitle
            // 
            // 
            // 
            // 
            this.progressTitle.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.progressTitle.FocusCuesEnabled = false;
            this.progressTitle.Location = new System.Drawing.Point(346, 377);
            this.progressTitle.Name = "progressTitle";
            this.progressTitle.Size = new System.Drawing.Size(191, 20);
            this.progressTitle.TabIndex = 29;
            this.progressTitle.TextVisible = true;
            // 
            // progressTotal
            // 
            // 
            // 
            // 
            this.progressTotal.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.progressTotal.FocusCuesEnabled = false;
            this.progressTotal.Location = new System.Drawing.Point(509, 30);
            this.progressTotal.Name = "progressTotal";
            this.progressTotal.Size = new System.Drawing.Size(219, 20);
            this.progressTotal.TabIndex = 30;
            this.progressTotal.TextVisible = true;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(179, 162);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(210, 184);
            this.checkedListBox1.TabIndex = 30;
            // 
            // node1
            // 
            this.node1.Expanded = true;
            this.node1.HostedControl = this.checkedListBox1;
            this.node1.Name = "node1";
            this.node1.Text = "node1";
            // 
            // cmdRules
            // 
            this.cmdRules.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdRules.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdRules.FocusCuesEnabled = false;
            this.cmdRules.ImageTextSpacing = 5;
            this.cmdRules.Location = new System.Drawing.Point(229, 332);
            this.cmdRules.Name = "cmdRules";
            this.cmdRules.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdRules.Size = new System.Drawing.Size(111, 39);
            this.cmdRules.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdRules.Symbol = "";
            this.cmdRules.SymbolSize = 15F;
            this.cmdRules.TabIndex = 30;
            this.cmdRules.Text = "Unlock Rules";
            this.cmdRules.TextAlignment = DevComponents.DotNetBar.eButtonTextAlignment.Left;
            this.cmdRules.Click += new System.EventHandler(this.cmdRules_Click);
            // 
            // cmdSelectAll
            // 
            this.cmdSelectAll.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdSelectAll.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdSelectAll.FocusCuesEnabled = false;
            this.cmdSelectAll.Location = new System.Drawing.Point(229, 377);
            this.cmdSelectAll.Name = "cmdSelectAll";
            this.cmdSelectAll.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdSelectAll.Size = new System.Drawing.Size(111, 20);
            this.cmdSelectAll.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdSelectAll.Symbol = "";
            this.cmdSelectAll.SymbolSize = 10F;
            this.cmdSelectAll.TabIndex = 31;
            this.cmdSelectAll.Text = "Select All";
            this.cmdSelectAll.Click += new System.EventHandler(this.cmdSelectAll_Click);
            // 
            // dateTimeAch
            // 
            this.dateTimeAch.AntiAlias = true;
            this.dateTimeAch.AutoAdvance = true;
            // 
            // 
            // 
            this.dateTimeAch.BackgroundStyle.Class = "DateTimeInputBackground";
            this.dateTimeAch.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeAch.ButtonDropDown.Shortcut = DevComponents.DotNetBar.eShortcut.AltDown;
            this.dateTimeAch.ButtonDropDown.Visible = true;
            this.dateTimeAch.DateTimeSelectorVisibility = DevComponents.Editors.DateTimeAdv.eDateTimeSelectorVisibility.Both;
            this.dateTimeAch.Format = DevComponents.Editors.eDateTimePickerFormat.Custom;
            this.dateTimeAch.IsPopupCalendarOpen = false;
            this.dateTimeAch.Location = new System.Drawing.Point(543, 377);
            this.dateTimeAch.MinDate = new System.DateTime(2005, 11, 22, 0, 0, 0, 0);
            // 
            // 
            // 
            this.dateTimeAch.MonthCalendar.AnnuallyMarkedDates = new System.DateTime[0];
            // 
            // 
            // 
            this.dateTimeAch.MonthCalendar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeAch.MonthCalendar.CalendarDimensions = new System.Drawing.Size(1, 1);
            this.dateTimeAch.MonthCalendar.ClearButtonVisible = true;
            // 
            // 
            // 
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground2;
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.BackColorGradientAngle = 90;
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.BorderTopColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.BorderTopWidth = 1;
            this.dateTimeAch.MonthCalendar.CommandsBackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeAch.MonthCalendar.DayClickAutoClosePopup = false;
            this.dateTimeAch.MonthCalendar.DisplayMonth = new System.DateTime(2012, 11, 1, 0, 0, 0, 0);
            this.dateTimeAch.MonthCalendar.MarkedDates = new System.DateTime[0];
            this.dateTimeAch.MonthCalendar.MonthlyMarkedDates = new System.DateTime[0];
            // 
            // 
            // 
            this.dateTimeAch.MonthCalendar.NavigationBackgroundStyle.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.dateTimeAch.MonthCalendar.NavigationBackgroundStyle.BackColorGradientAngle = 90;
            this.dateTimeAch.MonthCalendar.NavigationBackgroundStyle.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.dateTimeAch.MonthCalendar.NavigationBackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.dateTimeAch.MonthCalendar.TodayButtonVisible = true;
            this.dateTimeAch.MonthCalendar.WeeklyMarkedDays = new System.DayOfWeek[0];
            this.dateTimeAch.Name = "dateTimeAch";
            this.dateTimeAch.ShowCheckBox = true;
            this.dateTimeAch.ShowUpDown = true;
            this.dateTimeAch.Size = new System.Drawing.Size(171, 20);
            this.dateTimeAch.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.dateTimeAch.TabIndex = 33;
            this.dateTimeAch.Value = new System.DateTime(2005, 11, 22, 0, 0, 0, 0);
            this.dateTimeAch.LockUpdateChanged += new System.EventHandler(this.dateTimeAch_LockUpdateChanged);
            // 
            // listTitles
            // 
            // 
            // 
            // 
            this.listTitles.Border.Class = "ListViewBorder";
            this.listTitles.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listTitles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTitleName,
            this.columnGs});
            this.listTitles.FocusCuesEnabled = false;
            this.listTitles.FullRowSelect = true;
            this.listTitles.Location = new System.Drawing.Point(12, 12);
            this.listTitles.MultiSelect = false;
            this.listTitles.Name = "listTitles";
            this.listTitles.Size = new System.Drawing.Size(211, 314);
            this.listTitles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listTitles.TabIndex = 34;
            this.listTitles.UseCompatibleStateImageBehavior = false;
            this.listTitles.View = System.Windows.Forms.View.Details;
            this.listTitles.SelectedIndexChanged += new System.EventHandler(this.listTitles_SelectedIndexChanged);
            this.listTitles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listTitles_KeyDown);
            this.listTitles.MouseEnter += new System.EventHandler(this.listTitles_MouseEnter);
            // 
            // columnTitleName
            // 
            this.columnTitleName.Text = "Title Name";
            this.columnTitleName.Width = 131;
            // 
            // columnGs
            // 
            this.columnGs.Text = "GS";
            // 
            // cmdUnlockAll
            // 
            this.cmdUnlockAll.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.cmdUnlockAll.Name = "cmdUnlockAll";
            this.cmdUnlockAll.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdUnlockAll.Symbol = "";
            this.cmdUnlockAll.SymbolSize = 15F;
            this.cmdUnlockAll.Text = "Unlock All";
            this.cmdUnlockAll.Click += new System.EventHandler(this.cmdUnlockAll_Click);
            // 
            // AchievementUnlocker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 465);
            this.Controls.Add(this.progressTotal);
            this.MaximumSize = new System.Drawing.Size(736, 465);
            this.MinimumSize = new System.Drawing.Size(736, 465);
            this.Name = "AchievementUnlocker";
            this.Text = "Achievement Unlocker";
            this.Controls.SetChildIndex(this.controlRibbon, 0);
            this.Controls.SetChildIndex(this.progressTotal, 0);
            this.panelMain.ResumeLayout(false);
            this.controlRibbon.ResumeLayout(false);
            this.controlRibbon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listAch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimeTitle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimeAch)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.AdvTree.AdvTree listAch;
        private DevComponents.AdvTree.NodeConnector nodeConnector2;
        private DevComponents.DotNetBar.ElementStyle elementStyle3;
        private DevComponents.AdvTree.ColumnHeader colAchievement;
        private DevComponents.AdvTree.ColumnHeader colCredit;
        private DevComponents.AdvTree.ColumnHeader colDescription;
        private Controls.SquareButton cmdUnlockTitle;
        private Controls.SquareButton cmdUnlock;
        private DevComponents.DotNetBar.LabelX lblLastPlayed;
        private DevComponents.Editors.DateTimeAdv.DateTimeInput dateTimeTitle;
        private DevComponents.DotNetBar.Controls.ProgressBarX progressTitle;
        private DevComponents.DotNetBar.Controls.ProgressBarX progressTotal;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private DevComponents.AdvTree.Node node1;
        private Controls.SquareButton cmdRules;
        private Controls.SquareButton cmdSelectAll;
        private DevComponents.Editors.DateTimeAdv.DateTimeInput dateTimeAch;
        private DevComponents.DotNetBar.Controls.ListViewEx listTitles;
        private System.Windows.Forms.ColumnHeader columnTitleName;
        private System.Windows.Forms.ColumnHeader columnGs;
        private DevComponents.DotNetBar.ButtonItem cmdUnlockAll;

    }
}
