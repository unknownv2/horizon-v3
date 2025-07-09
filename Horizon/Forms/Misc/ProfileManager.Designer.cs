namespace NoDev.Horizon.Forms.Misc
{
    partial class ProfileManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileManager));
            this.listProfiles = new DevComponents.AdvTree.AdvTree();
            this.colTreeEntry = new DevComponents.AdvTree.ColumnHeader();
            this.colTreeFavorite = new DevComponents.AdvTree.ColumnHeader();
            this.nodeConnector1 = new DevComponents.AdvTree.NodeConnector();
            this.elementStyle1 = new DevComponents.DotNetBar.ElementStyle();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.cmdRemoveSelected = new NoDev.Horizon.Controls.SquareButton();
            this.cmdClearHistory = new NoDev.Horizon.Controls.SquareButton();
            ((System.ComponentModel.ISupportInitialize)(this.listProfiles)).BeginInit();
            this.SuspendLayout();
            // 
            // listProfiles
            // 
            this.listProfiles.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.listProfiles.BackgroundStyle.Class = "TreeBorderKey";
            this.listProfiles.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.listProfiles.Columns.Add(this.colTreeEntry);
            this.listProfiles.Columns.Add(this.colTreeFavorite);
            this.listProfiles.DragDropEnabled = false;
            this.listProfiles.DragDropNodeCopyEnabled = false;
            this.listProfiles.ExpandButtonType = DevComponents.AdvTree.eExpandButtonType.Triangle;
            this.listProfiles.ExpandWidth = 14;
            this.listProfiles.GridRowLines = true;
            this.listProfiles.HotTracking = true;
            this.listProfiles.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.listProfiles.Location = new System.Drawing.Point(12, 57);
            this.listProfiles.MultiNodeDragDropAllowed = false;
            this.listProfiles.MultiSelect = true;
            this.listProfiles.Name = "listProfiles";
            this.listProfiles.NodesConnector = this.nodeConnector1;
            this.listProfiles.NodeStyle = this.elementStyle1;
            this.listProfiles.PathSeparator = ";";
            this.listProfiles.SelectionBoxStyle = DevComponents.AdvTree.eSelectionStyle.FullRowSelect;
            this.listProfiles.Size = new System.Drawing.Size(257, 199);
            this.listProfiles.Styles.Add(this.elementStyle1);
            this.listProfiles.TabIndex = 5;
            this.listProfiles.TileSize = new System.Drawing.Size(230, 64);
            this.listProfiles.View = DevComponents.AdvTree.eView.Tile;
            // 
            // colTreeEntry
            // 
            this.colTreeEntry.MinimumWidth = 80;
            this.colTreeEntry.Name = "colTreeEntry";
            this.colTreeEntry.Text = "Entry";
            this.colTreeEntry.Width.AutoSizeMinHeader = true;
            // 
            // colTreeFavorite
            // 
            this.colTreeFavorite.MinimumWidth = 10;
            this.colTreeFavorite.Name = "colTreeFavorite";
            this.colTreeFavorite.Text = "Favorite";
            this.colTreeFavorite.Width.AutoSize = true;
            this.colTreeFavorite.Width.AutoSizeMinHeader = true;
            // 
            // nodeConnector1
            // 
            this.nodeConnector1.LineColor = System.Drawing.SystemColors.ControlDark;
            // 
            // elementStyle1
            // 
            this.elementStyle1.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle1.Name = "elementStyle1";
            this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;
            // 
            // labelX1
            // 
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Location = new System.Drawing.Point(12, 9);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(257, 42);
            this.labelX1.TabIndex = 6;
            this.labelX1.Text = "This is a list of profiles that Horizon has remembered for you so you can easily " +
    "identify the owners of your content.";
            this.labelX1.WordWrap = true;
            // 
            // cmdRemoveSelected
            // 
            this.cmdRemoveSelected.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdRemoveSelected.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdRemoveSelected.Enabled = false;
            this.cmdRemoveSelected.FocusCuesEnabled = false;
            this.cmdRemoveSelected.Location = new System.Drawing.Point(12, 262);
            this.cmdRemoveSelected.Name = "cmdRemoveSelected";
            this.cmdRemoveSelected.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdRemoveSelected.Size = new System.Drawing.Size(125, 30);
            this.cmdRemoveSelected.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdRemoveSelected.Symbol = "";
            this.cmdRemoveSelected.SymbolSize = 15F;
            this.cmdRemoveSelected.TabIndex = 8;
            this.cmdRemoveSelected.Text = "Remove Selected";
            this.cmdRemoveSelected.Click += new System.EventHandler(this.cmdRemoveSelected_Click);
            // 
            // cmdClearHistory
            // 
            this.cmdClearHistory.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdClearHistory.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdClearHistory.Enabled = false;
            this.cmdClearHistory.FocusCuesEnabled = false;
            this.cmdClearHistory.Location = new System.Drawing.Point(143, 262);
            this.cmdClearHistory.Name = "cmdClearHistory";
            this.cmdClearHistory.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdClearHistory.Size = new System.Drawing.Size(126, 30);
            this.cmdClearHistory.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdClearHistory.Symbol = "";
            this.cmdClearHistory.SymbolSize = 13F;
            this.cmdClearHistory.TabIndex = 7;
            this.cmdClearHistory.Text = "Clear History";
            this.cmdClearHistory.Click += new System.EventHandler(this.cmdClearHistory_Click);
            // 
            // ProfileManager
            // 
            this.ClientSize = new System.Drawing.Size(281, 304);
            this.Controls.Add(this.cmdRemoveSelected);
            this.Controls.Add(this.cmdClearHistory);
            this.Controls.Add(this.labelX1);
            this.Controls.Add(this.listProfiles);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ProfileManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Profile Manager";
            ((System.ComponentModel.ISupportInitialize)(this.listProfiles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.AdvTree.AdvTree listProfiles;
        internal DevComponents.AdvTree.ColumnHeader colTreeEntry;
        internal DevComponents.AdvTree.ColumnHeader colTreeFavorite;
        private DevComponents.AdvTree.NodeConnector nodeConnector1;
        private DevComponents.DotNetBar.ElementStyle elementStyle1;
        private DevComponents.DotNetBar.LabelX labelX1;
        private Controls.SquareButton cmdClearHistory;
        private Controls.SquareButton cmdRemoveSelected;
    }
}