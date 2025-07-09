using System;
using System.Linq;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;

namespace NoDev.Horizon.Forms.Misc
{
    public partial class ProgramSettings : OfficeForm
    {
        private ProgramSettings()
        {
            InitializeComponent();

            foreach (var c in this.Controls.OfType<CheckBoxX>())
            {
                c.Checked = Settings.GetBoolean((string)c.Tag);
                c.CheckedChanged += CheckChanged;
            }
        }

        internal static new void Show(IWin32Window owner)
        {
            new ProgramSettings().ShowDialog(owner);
        }

        private void CheckChanged(object sender, EventArgs e)
        {
            var ck = (CheckBoxX)sender;

            string setting = (string)ck.Tag;
            bool value = ck.Checked;

            Settings.Set(setting, value);
            Settings.Save();

            var owner = Main.GetInstance();

            switch (setting)
            {
                case "AlwaysOnTop":
                    owner.TopMost = value;
                    break;
                case "DeviceExplorerEnabled":
                    
                    break;
            }
        }

        private void cmdProfileManager_Click(object sender, EventArgs e)
        {
            ProfileManager.Show(this);
        }
    }
}