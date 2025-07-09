namespace NoDev.Horizon.Forms.Misc
{
    partial class Tutorial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tutorial));
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.squareButton1 = new NoDev.Horizon.Controls.SquareButton();
            this.SuspendLayout();
            // 
            // labelX1
            // 
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelX1.Location = new System.Drawing.Point(12, 69);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(620, 95);
            this.labelX1.TabIndex = 0;
            this.labelX1.Text = "A tutorial will go here one day, but you\'ll have to fend for yourself until then." +
    " Sorry.";
            this.labelX1.TextAlignment = System.Drawing.StringAlignment.Center;
            // 
            // squareButton1
            // 
            this.squareButton1.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.squareButton1.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.squareButton1.FocusCuesEnabled = false;
            this.squareButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.squareButton1.Location = new System.Drawing.Point(161, 203);
            this.squareButton1.Name = "squareButton1";
            this.squareButton1.Shape = new DevComponents.DotNetBar.RoundRectangleShapeDescriptor();
            this.squareButton1.Size = new System.Drawing.Size(291, 52);
            this.squareButton1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.squareButton1.TabIndex = 1;
            this.squareButton1.Text = "Start Horizon";
            this.squareButton1.Click += new System.EventHandler(this.squareButton1_Click);
            // 
            // Tutorial
            // 
            this.ClientSize = new System.Drawing.Size(644, 437);
            this.Controls.Add(this.squareButton1);
            this.Controls.Add(this.labelX1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Tutorial";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome to Horizon";
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.LabelX labelX1;
        private Controls.SquareButton squareButton1;
        private DevComponents.DotNetBar.StyleManager styleManager;

    }
}