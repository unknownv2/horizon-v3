namespace NoDev.Horizon.Forms.Tools
{
    partial class TitleIDFinder
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
            this.cmdCopy = new DevComponents.DotNetBar.ButtonX();
            this.cmdSearch = new DevComponents.DotNetBar.ButtonX();
            this.listTitles = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.columnTitleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnTitleID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtTitleName = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.comboRegion = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmdSave = new DevComponents.DotNetBar.ButtonX();
            this.pbTitleImage = new System.Windows.Forms.PictureBox();
            this.ribbonMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTitleImage)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonMain
            // 
            this.ribbonMain.Controls.Add(this.cmdSave);
            this.ribbonMain.Controls.Add(this.comboRegion);
            this.ribbonMain.Controls.Add(this.txtTitleName);
            this.ribbonMain.Controls.Add(this.pbTitleImage);
            this.ribbonMain.Controls.Add(this.listTitles);
            this.ribbonMain.Controls.Add(this.cmdCopy);
            this.ribbonMain.Controls.Add(this.cmdSearch);
            this.ribbonMain.Location = new System.Drawing.Point(0, 53);
            this.ribbonMain.Size = new System.Drawing.Size(436, 204);
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
            this.tabMain.Text = "Search Titles";
            // 
            // cmdCopy
            // 
            this.cmdCopy.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdCopy.BackColor = System.Drawing.Color.Transparent;
            this.cmdCopy.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdCopy.Enabled = false;
            this.cmdCopy.FocusCuesEnabled = false;
            this.cmdCopy.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCopy.Location = new System.Drawing.Point(344, 6);
            this.cmdCopy.Name = "cmdCopy";
            this.cmdCopy.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdCopy.Size = new System.Drawing.Size(85, 21);
            this.cmdCopy.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdCopy.TabIndex = 12;
            this.cmdCopy.Text = "Copy Title ID";
            this.cmdCopy.Click += new System.EventHandler(this.cmdCopy_Click);
            // 
            // cmdSearch
            // 
            this.cmdSearch.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdSearch.BackColor = System.Drawing.Color.Transparent;
            this.cmdSearch.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdSearch.FocusCuesEnabled = false;
            this.cmdSearch.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdSearch.Location = new System.Drawing.Point(253, 6);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdSearch.Size = new System.Drawing.Size(85, 21);
            this.cmdSearch.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdSearch.TabIndex = 11;
            this.cmdSearch.Text = "Search";
            this.cmdSearch.Click += new System.EventHandler(this.cmdSearch_Click);
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
            this.columnTitleID});
            this.listTitles.FullRowSelect = true;
            this.listTitles.GridLines = true;
            this.listTitles.Location = new System.Drawing.Point(6, 32);
            this.listTitles.MultiSelect = false;
            this.listTitles.Name = "listTitles";
            this.listTitles.Size = new System.Drawing.Size(332, 166);
            this.listTitles.TabIndex = 14;
            this.listTitles.UseCompatibleStateImageBehavior = false;
            this.listTitles.View = System.Windows.Forms.View.Details;
            this.listTitles.SelectedIndexChanged += new System.EventHandler(this.listTitles_SelectedIndexChanged);
            // 
            // columnTitleName
            // 
            this.columnTitleName.Text = "Title Name";
            this.columnTitleName.Width = 186;
            // 
            // columnTitleID
            // 
            this.columnTitleID.Text = "Title ID";
            this.columnTitleID.Width = 90;
            // 
            // txtTitleName
            // 
            // 
            // 
            // 
            this.txtTitleName.Border.Class = "TextBoxBorder";
            this.txtTitleName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtTitleName.Location = new System.Drawing.Point(6, 6);
            this.txtTitleName.MaxLength = 64;
            this.txtTitleName.Name = "txtTitleName";
            this.txtTitleName.Size = new System.Drawing.Size(175, 20);
            this.txtTitleName.TabIndex = 16;
            this.txtTitleName.WatermarkText = "Search...";
            this.txtTitleName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTitleName_KeyPress);
            // 
            // comboRegion
            // 
            this.comboRegion.DisplayMember = "Text";
            this.comboRegion.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRegion.FocusCuesEnabled = false;
            this.comboRegion.FormattingEnabled = true;
            this.comboRegion.ItemHeight = 14;
            this.comboRegion.Location = new System.Drawing.Point(187, 6);
            this.comboRegion.Name = "comboRegion";
            this.comboRegion.Size = new System.Drawing.Size(60, 20);
            this.comboRegion.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.comboRegion.TabIndex = 17;
            // 
            // cmdSave
            // 
            this.cmdSave.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdSave.BackColor = System.Drawing.Color.Transparent;
            this.cmdSave.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdSave.Enabled = false;
            this.cmdSave.FocusCuesEnabled = false;
            this.cmdSave.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdSave.Image = global::NoDev.Horizon.Properties.Resources.Disk_16;
            this.cmdSave.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.cmdSave.Location = new System.Drawing.Point(344, 159);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.cmdSave.Size = new System.Drawing.Size(85, 39);
            this.cmdSave.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdSave.TabIndex = 18;
            this.cmdSave.Text = "Save Image...";
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // pbTitleImage
            // 
            this.pbTitleImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbTitleImage.ImageLocation = "";
            this.pbTitleImage.Location = new System.Drawing.Point(344, 33);
            this.pbTitleImage.Name = "pbTitleImage";
            this.pbTitleImage.Size = new System.Drawing.Size(85, 120);
            this.pbTitleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbTitleImage.TabIndex = 15;
            this.pbTitleImage.TabStop = false;
            // 
            // TitleIDFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 263);
            this.MaximumSize = new System.Drawing.Size(446, 263);
            this.MinimumSize = new System.Drawing.Size(446, 263);
            this.Name = "TitleIDFinder";
            this.Text = "Title ID Finder";
            this.ribbonMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbTitleImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ButtonX cmdCopy;
        private DevComponents.DotNetBar.ButtonX cmdSearch;
        private DevComponents.DotNetBar.Controls.TextBoxX txtTitleName;
        private System.Windows.Forms.PictureBox pbTitleImage;
        private DevComponents.DotNetBar.Controls.ListViewEx listTitles;
        private System.Windows.Forms.ColumnHeader columnTitleName;
        private System.Windows.Forms.ColumnHeader columnTitleID;
        private DevComponents.DotNetBar.Controls.ComboBoxEx comboRegion;
        private DevComponents.DotNetBar.ButtonX cmdSave;
    }
}
