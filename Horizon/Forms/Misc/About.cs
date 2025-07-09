using System.Diagnostics;
using NoDev.Horizon.Net;

namespace NoDev.Horizon.Forms.Misc
{
    [ControlInfo(0x00, "About", "Thumb_Generic_Info", Group.Guest | Group.Regular | Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Misc, true)]
    partial class About : TabbedControl
    {
        internal About()
        {
            InitializeComponent();
        }

        private void cmdXboxMB_Click(object sender, System.EventArgs e)
        {
            Process.Start(Server.HomepageURL);
        }
    }
}
