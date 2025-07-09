namespace NoDev.Horizon
{
    partial class BaseControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseControl));
            this.controlRibbon = new DevComponents.DotNetBar.RibbonControl();
            this.SuspendLayout();
            // 
            // controlRibbon
            // 
            this.controlRibbon.AutoExpand = false;
            this.controlRibbon.AutoKeyboardExpand = false;
            // 
            // 
            // 
            this.controlRibbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.controlRibbon.CanCustomize = false;
            this.controlRibbon.CaptionVisible = true;
            this.controlRibbon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlRibbon.KeyTipsFont = new System.Drawing.Font("Tahoma", 7F);
            this.controlRibbon.Location = new System.Drawing.Point(5, 1);
            this.controlRibbon.Name = "controlRibbon";
            this.controlRibbon.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.controlRibbon.Size = new System.Drawing.Size(545, 363);
            this.controlRibbon.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
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
            this.controlRibbon.TabGroupHeight = 14;
            this.controlRibbon.TabIndex = 0;
            // 
            // BaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BottomLeftCornerSize = 0;
            this.BottomRightCornerSize = 0;
            this.ClientSize = new System.Drawing.Size(555, 366);
            this.Controls.Add(this.controlRibbon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BaseControl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Base Control";
            this.ResumeLayout(false);

        }

        #endregion

        protected DevComponents.DotNetBar.RibbonControl controlRibbon;



    }
}
