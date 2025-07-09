using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NoDev.Common;
using NoDev.Odd;

namespace NoDev.Horizon.Forms.Tools
{
    [ControlInfo(0x0F, "Game Disc Explorer", "Thumb_Generic_Controller", Group.Guest | Group.Regular | Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Tool, true)]
    internal partial class GameDiscExplorer : TabbedControl
    {
        private const string
            MountText = "Mount",
            UnmountText = "Unmount",
            NoMountingPoint = "N/A",
            NoDrives = "--",
            DriveAppend = ":\\";

        internal GameDiscExplorer()
        {
            InitializeComponent();

            IEnumerable<char> driveLetters = Win32.GetFreeDriveLetters();

            foreach (char driveLetter in driveLetters)
                comboDriveLetters.Items.Add(driveLetter + DriveAppend);

            if (comboDriveLetters.Items.Count == 0)
                comboDriveLetters.Items.Add(NoDrives);

            comboDriveLetters.SelectedItem = comboDriveLetters.Items[0];
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Game Disc Image|*.iso|All Files|*";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            this.AddImage(ofd.FileName);
        }

        private void AddImage(string fileName)
        {
            foreach (ListViewItem row in listDiscs.Items)
            {
                if (row.Text != fileName)
                    continue;

                DialogBox.Show("This image is already on the list.", "Already Added", MessageBoxIcon.Error);
                row.Selected = true;
                return;
            }

            listDiscs.Items.Add(new ListViewItem(new[] { fileName, NoMountingPoint }));

            listDiscs.Items[listDiscs.Items.Count - 1].Selected = true;
        }

        private void listDiscs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listDiscs.SelectedItems.Count == 0)
            {
                cmdMount.Enabled = false;
                cmdExplore.Enabled = false;
                comboDriveLetters.Enabled = false;
                return;
            }

            this.UpdateButtons(listDiscs.SelectedItems[0]);
        }

        private void cmdExplore_Click(object sender, EventArgs e)
        {
            Process.Start(((OddDevice)listDiscs.SelectedItems[0].Tag).MountPoint);
        }

        private void UpdateButtons(ListViewItem li)
        {
            cmdMount.Enabled = true;

            if (li.Tag == null)
            {
                cmdMount.Text = MountText;
                cmdExplore.Enabled = false;
                comboDriveLetters.Enabled = true;

                this.RepopulateDriveLetters();
            }
            else
            {
                cmdMount.Text = UnmountText;
                cmdExplore.Enabled = true;
                comboDriveLetters.Enabled = false;

                comboDriveLetters.SelectedItem = li.SubItems[1].Text;
            }
        }

        private void cmdMount_Click(object sender, EventArgs e)
        {
            var li = listDiscs.SelectedItems[0];

            if (cmdMount.Text == MountText)
            {
                string mountingPoint = (string)comboDriveLetters.SelectedItem;

                if (mountingPoint == NoDrives)
                {
                    DialogBox.Show("There are no free drive letters available on your computer!", "No Free Drives", MessageBoxIcon.Error);
                    return;
                }

                IEnumerable<char> driveLetters = Win32.GetFreeDriveLetters();

                if (!driveLetters.Contains(mountingPoint[0]))
                {
                    DialogBox.Show("The selected drive letter is unavailable.", "Drive Unavailable", MessageBoxIcon.Error);
                    return;
                }

                li.SubItems[1].Text = mountingPoint;

                mountingPoint = mountingPoint[0] + ":";

                li.Tag = new OddDevice(mountingPoint, li.Text);

                Process.Start(mountingPoint);
            }
            else
            {
                li.SubItems[1].Text = NoMountingPoint;
                ((OddDevice)li.Tag).Unmount();
                li.Tag = null;
            }

            this.UpdateButtons(li);
        }

        private void RepopulateDriveLetters()
        {
            char selectedDrive = ((string)comboDriveLetters.SelectedItem)[0];

            comboDriveLetters.Items.Clear();

            IEnumerable<char> driveLetters = Win32.GetFreeDriveLetters();

            foreach (char driveLetter in driveLetters)
            {
                string item = driveLetter + DriveAppend;
                comboDriveLetters.Items.Add(item);
                if (driveLetter == selectedDrive)
                    comboDriveLetters.SelectedItem = item;
            }

            if (comboDriveLetters.Items.Count == 0)
            {
                comboDriveLetters.Items.Add(NoDrives);
                comboDriveLetters.SelectedItem = comboDriveLetters.Items[0];
            }
        }

        private void comboDriveLetters_DropDown(object sender, EventArgs e)
        {
            this.RepopulateDriveLetters();
        }

        private void GameDiscExplorer_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void GameDiscExplorer_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
                return;

            foreach (string file in files)
                this.AddImage(file);
        }

        protected override void OnFormClose(FormClosingEventArgs e)
        {
            bool mounted = listDiscs.Items.Cast<ListViewItem>().Any(li => li.Tag != null);

            if (!mounted)
                return;

            if (DialogBox.Show("You still have images mounted. Closing this form will unmount them.\n\nContinue?", "Unmount Images",
                MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button3, MessageBoxIcon.Exclamation) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            foreach (ListViewItem li in listDiscs.Items)
                if (li.Tag != null)
                    ((OddDevice)li.Tag).Unmount();
        }
    }
}
