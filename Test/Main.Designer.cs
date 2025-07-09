namespace Test
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.cmdProfileExtractAll = new System.Windows.Forms.Button();
            this.cmdAddTitleRecords = new System.Windows.Forms.Button();
            this.txtTitleIds = new System.Windows.Forms.TextBox();
            this.panelMountStfs = new System.Windows.Forms.Panel();
            this.cmdCloseStfs = new System.Windows.Forms.Button();
            this.lblMountStfs = new System.Windows.Forms.Label();
            this.cmdCreateGameAdderData = new System.Windows.Forms.Button();
            this.cmdInstallDriver = new System.Windows.Forms.Button();
            this.cmdRegisterPublicKey = new System.Windows.Forms.Button();
            this.panelMountStfs.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdProfileExtractAll
            // 
            this.cmdProfileExtractAll.Location = new System.Drawing.Point(12, 12);
            this.cmdProfileExtractAll.Name = "cmdProfileExtractAll";
            this.cmdProfileExtractAll.Size = new System.Drawing.Size(145, 48);
            this.cmdProfileExtractAll.TabIndex = 0;
            this.cmdProfileExtractAll.Text = "Select Profile Folder\r\nand Extract All Records";
            this.cmdProfileExtractAll.UseVisualStyleBackColor = true;
            this.cmdProfileExtractAll.Click += new System.EventHandler(this.cmdProfileExtractAll_Click);
            // 
            // cmdAddTitleRecords
            // 
            this.cmdAddTitleRecords.Location = new System.Drawing.Point(498, 155);
            this.cmdAddTitleRecords.Name = "cmdAddTitleRecords";
            this.cmdAddTitleRecords.Size = new System.Drawing.Size(145, 24);
            this.cmdAddTitleRecords.TabIndex = 1;
            this.cmdAddTitleRecords.Text = "Add Title Records";
            this.cmdAddTitleRecords.UseVisualStyleBackColor = true;
            this.cmdAddTitleRecords.Click += new System.EventHandler(this.cmdAddTitleRecords_Click);
            // 
            // txtTitleIds
            // 
            this.txtTitleIds.Location = new System.Drawing.Point(314, 12);
            this.txtTitleIds.MaxLength = 999999999;
            this.txtTitleIds.Multiline = true;
            this.txtTitleIds.Name = "txtTitleIds";
            this.txtTitleIds.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTitleIds.Size = new System.Drawing.Size(329, 137);
            this.txtTitleIds.TabIndex = 2;
            this.txtTitleIds.Text = resources.GetString("txtTitleIds.Text");
            // 
            // panelMountStfs
            // 
            this.panelMountStfs.AllowDrop = true;
            this.panelMountStfs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMountStfs.Controls.Add(this.cmdCloseStfs);
            this.panelMountStfs.Location = new System.Drawing.Point(12, 337);
            this.panelMountStfs.Name = "panelMountStfs";
            this.panelMountStfs.Size = new System.Drawing.Size(186, 74);
            this.panelMountStfs.TabIndex = 3;
            this.panelMountStfs.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelMountStfs_DragDrop);
            this.panelMountStfs.DragEnter += new System.Windows.Forms.DragEventHandler(this.panelMountStfs_DragEnter);
            // 
            // cmdCloseStfs
            // 
            this.cmdCloseStfs.Location = new System.Drawing.Point(125, 51);
            this.cmdCloseStfs.Name = "cmdCloseStfs";
            this.cmdCloseStfs.Size = new System.Drawing.Size(60, 22);
            this.cmdCloseStfs.TabIndex = 0;
            this.cmdCloseStfs.Text = "Close";
            this.cmdCloseStfs.UseVisualStyleBackColor = true;
            this.cmdCloseStfs.Click += new System.EventHandler(this.cmdCloseStfs_Click);
            // 
            // lblMountStfs
            // 
            this.lblMountStfs.AutoSize = true;
            this.lblMountStfs.Location = new System.Drawing.Point(12, 321);
            this.lblMountStfs.Name = "lblMountStfs";
            this.lblMountStfs.Size = new System.Drawing.Size(163, 13);
            this.lblMountStfs.TabIndex = 4;
            this.lblMountStfs.Text = "Drop a package here to mount it.";
            // 
            // cmdCreateGameAdderData
            // 
            this.cmdCreateGameAdderData.Location = new System.Drawing.Point(163, 12);
            this.cmdCreateGameAdderData.Name = "cmdCreateGameAdderData";
            this.cmdCreateGameAdderData.Size = new System.Drawing.Size(145, 48);
            this.cmdCreateGameAdderData.TabIndex = 5;
            this.cmdCreateGameAdderData.Text = "Profile to\r\nGame Adder Data";
            this.cmdCreateGameAdderData.UseVisualStyleBackColor = true;
            this.cmdCreateGameAdderData.Click += new System.EventHandler(this.cmdCreateGameAdderData_Click);
            // 
            // cmdInstallDriver
            // 
            this.cmdInstallDriver.Location = new System.Drawing.Point(498, 375);
            this.cmdInstallDriver.Name = "cmdInstallDriver";
            this.cmdInstallDriver.Size = new System.Drawing.Size(145, 36);
            this.cmdInstallDriver.TabIndex = 6;
            this.cmdInstallDriver.Text = "Install Driver";
            this.cmdInstallDriver.UseVisualStyleBackColor = true;
            this.cmdInstallDriver.Click += new System.EventHandler(this.cmdInstallDriver_Click);
            // 
            // cmdRegisterPublicKey
            // 
            this.cmdRegisterPublicKey.Location = new System.Drawing.Point(347, 375);
            this.cmdRegisterPublicKey.Name = "cmdRegisterPublicKey";
            this.cmdRegisterPublicKey.Size = new System.Drawing.Size(145, 36);
            this.cmdRegisterPublicKey.TabIndex = 7;
            this.cmdRegisterPublicKey.Text = "Register Public Key";
            this.cmdRegisterPublicKey.UseVisualStyleBackColor = true;
            this.cmdRegisterPublicKey.Click += new System.EventHandler(this.cmdRegisterPublicKey_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 423);
            this.Controls.Add(this.cmdRegisterPublicKey);
            this.Controls.Add(this.cmdInstallDriver);
            this.Controls.Add(this.cmdCreateGameAdderData);
            this.Controls.Add(this.lblMountStfs);
            this.Controls.Add(this.panelMountStfs);
            this.Controls.Add(this.txtTitleIds);
            this.Controls.Add(this.cmdProfileExtractAll);
            this.Controls.Add(this.cmdAddTitleRecords);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Horizon Test";
            this.panelMountStfs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdProfileExtractAll;
        private System.Windows.Forms.Button cmdAddTitleRecords;
        private System.Windows.Forms.TextBox txtTitleIds;
        private System.Windows.Forms.Panel panelMountStfs;
        private System.Windows.Forms.Label lblMountStfs;
        private System.Windows.Forms.Button cmdCloseStfs;
        private System.Windows.Forms.Button cmdCreateGameAdderData;
        private System.Windows.Forms.Button cmdInstallDriver;
        private System.Windows.Forms.Button cmdRegisterPublicKey;
    }
}

