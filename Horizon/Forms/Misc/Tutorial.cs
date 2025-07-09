using System;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro.ColorTables;

namespace NoDev.Horizon.Forms.Misc
{
    public partial class Tutorial : OfficeForm
    {
        public Tutorial()
        {
            InitializeComponent();

            if (!Application.MessageLoop)
            {
                this.styleManager = new StyleManager(this.components);
                this.styleManager.ManagerColorTint = Color.DimGray;
                this.styleManager.ManagerStyle = eStyle.Office2010Silver;
                this.styleManager.MetroColorParameters = new MetroColorGeneratorParameters(Color.White, Color.FromArgb(255, 163, 26));
            }
        }

        private void squareButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}