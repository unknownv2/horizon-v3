using System;
using System.Net;
using System.Windows.Forms;
using NoDev.Common;

namespace NoDev.Horizon.Forms.Tools
{
    [ControlInfo(0x0A, "Title ID Finder", "Thumb_Generic_Controller", Group.Guest | Group.Regular | Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Tool)]
    internal partial class TitleIDFinder : TabbedControl
    {
        internal TitleIDFinder()
        {
            InitializeComponent();

            pbTitleImage.ImageLocation = DefaultTitleImage;

            comboRegion.Items.AddRange(Regions);
            comboRegion.SelectedIndex = 4;
        }

        private const string DefaultTitleImage = "http://mktplassets.xbox.com/NR/rdonlyres/A2590DD9-26E3-4FD1-B784-11343803A304/0/boxxboxlivedash.jpg";

        private static readonly object[] Regions = {
            "de-DE",
            "en-AU",
            "en-CA",
            "en-GB",                   
            "en-US",
            "es-CO",
            "es-ES",
            "es-MX",
            "fr-CA",
            "fr-FR",
            "it-IT",
            "pt-BR",
            "ru-RU",
            "ja-JP"
        };

        private void txtTitleName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Keys)e.KeyChar == Keys.Enter)
                cmdSearch.PerformClick();
        }

        private async void cmdSearch_Click(object sender, EventArgs e)
        {
            string searchString = txtTitleName.Text.Trim();

            if (searchString.Length == 0)
            {
                DialogBox.Show("You must enter a title name!");
                return;
            }

            cmdSearch.Enabled = false;

            listTitles.Items.Clear();

            dynamic searchObj = await TitleControl.MarketplaceQuery(comboRegion.SelectedText, searchString, 20);

            foreach (dynamic entry in searchObj["entries"])
            {
                if ((string)entry["downloadTypeClass"] != "Game")
                    continue;

                string titleName = (string)entry["title"];
                string detailsUrl = (string)entry["detailsUrl"];
                string titleId = detailsUrl.Substring(detailsUrl.Length - 8).ToUpper();

                uint titleId32;
                if (Numbers.TryParseUInt32Hex(titleId, out titleId32) && !TitleNameCache.Contains(titleId32))
                    TitleNameCache.AddTitle(titleId32, titleName);

                ListViewItem i = new ListViewItem((string)entry["title"]);
                i.SubItems.Add(titleId);
                i.Tag = entry;
                this.listTitles.Items.Add(i);
            }

            if (this.listTitles.Items.Count == 0)
                listTitles_SelectedIndexChanged(null, null);
            else
                this.listTitles.Items[0].Selected = true;

            cmdSearch.Enabled = true;
        }

        private void listTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listTitles.SelectedItems.Count == 0)
            {
                cmdCopy.Enabled = false;
                cmdSave.Enabled = false;

                pbTitleImage.ImageLocation = DefaultTitleImage;
                return;
            }

            cmdCopy.Enabled = true;
            cmdSave.Enabled = true;

            pbTitleImage.ImageLocation = (string)((dynamic) listTitles.SelectedItems[0].Tag)["image"];
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            if (listTitles.SelectedItems.Count == 0)
                return;

            Clipboard.SetText(listTitles.SelectedItems[0].SubItems[1].Text, TextDataFormat.Text);
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (listTitles.SelectedItems.Count == 0)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = listTitles.SelectedItems[0].Text;
            sfd.Filter = "PNG Images|*.png";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            new WebClient().DownloadFileAsync(new Uri(pbTitleImage.ImageLocation), sfd.FileName);
        }
    }
}
