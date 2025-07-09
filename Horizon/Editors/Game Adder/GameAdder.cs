using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoDev.Horizon.Properties;
using NoDev.XProfile.Records;
using NoDev.XProfile.Trackers;

namespace NoDev.Horizon.Editors.Game_Adder
{
    [EditorInfo(0x45, "Game Adder", "Thumb_Generic_DownArrow", Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Profile)]
    partial class GameAdder : ProfileEditor
    {
        internal GameAdder()
        {
            InitializeComponent();
        }

        private const string
            DownloadingDataMessage = "Downloading game data. Please wait...",
            AddGamesMessage = "Game data downloaded. We're ready to add them!",
            Yes = "Yes",
            No = "No";

        protected override void Open()
        {
            DialogBox.Show("This editor isn't complete. Don't do anything in it.");
            this.PopulateTitles();
        }

        protected override void Save()
        {
            
        }

        private void cmdSelectAll_Click(object sender, EventArgs e)
        {
            bool check = listTitles.CheckedItems.Count != listTitles.Items.Count;
            for (int x = 0; x < listTitles.Items.Count; x++)
                listTitles.Items[x].Checked = check;
        }

        private void txtSearch_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmdNext_Click(null, null);
        }

        private int _searchIndexTop;
        private readonly List<int> _searchIndexes = new List<int>();
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchIndexes.Clear();

            if (txtSearch.Text.Length != 0)
            {
                string searchText = txtSearch.Text.ToLower();
                var items = listTitles.Items;
                for (int x = 0; x < items.Count; x++)
                    if (((string)items[x].SubItems[0].Tag).Contains(searchText))
                        _searchIndexes.Add(x);
            }

            if (_searchIndexes.Count != 0)
            {
                _searchIndexTop = 0;
                listTitles.TopItem = listTitles.Items[_searchIndexes[0]];
            }
        }

        private void cmdNext_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Length != 0 && _searchIndexes.Count != 0)
                listTitles.TopItem = listTitles.Items[_searchIndexes[_searchIndexTop++ % _searchIndexes.Count]];
        }

        private void cmdDetails_Click(object sender, EventArgs e)
        {
            if (listTitles.SelectedItems.Count == 0)
                return;

            listAchievements.Items.Clear();
            listAwards.Items.Clear();

            tabMain.Visible = false;

            tabDetails.Visible = true;
            tabDetails.Select();

            var title = (Title)listTitles.SelectedItems[0].Tag;

            if (!TitleCache.HasData(title.ID))
            {
                pbGameTile.Image = Resources.QuestionMark_64;
                lblTitleName.Text = DownloadingDataMessage;

                // download data
            }

            pbGameTile.ImageLocation = string.Format("http://image.xboxlive.com/global/t.{0:X8}/icon/0/8000", title.ID);
            lblTitleName.Text = title.Name;

            foreach (var ach in title.Achievements)
                listAchievements.Items.Add(new ListViewItem(new[] { ach.Label, ach.Credit.ToString(CultureInfo.InvariantCulture) }));

            foreach (var aw in title.Awards)
                listAwards.Items.Add(new ListViewItem(new[] { aw.Name, "", "N/I" }));
        }

        private void PopulateTitles()
        {
            this.listTitles.BeginUpdate();

            this.listTitles.Items.Clear();

            foreach (var title in TitleCollection.Titles)
            {
                if (this.Profile.TitleRecordExists(title.Key))
                    continue;

                this.listTitles.Items.Add(new ListViewItem(new[] { 
                    title.Value.Name, 
                    title.Key.ToString("X8"), 
                    title.Value.Credit.ToString(CultureInfo.InvariantCulture), 
                    title.Value.AwardCount.ToString(CultureInfo.InvariantCulture) 
                }) { Tag = title.Key });
            }

            this.listTitles.EndUpdate();
        }

        private void cmdContinue_Click(object sender, EventArgs e)
        {
            if (listTitles.CheckedItems.Count == 0)
            {
                DialogBox.Show("You have to select games to add first!");
                return;
            }

            this.listQueue.BeginUpdate();

            this.listQueue.Items.Clear();

            foreach (ListViewItem li in this.listTitles.CheckedItems)
            {
                this.listQueue.Items.Add(new ListViewItem(new[] { li.Text, No, No }) { Tag = li.Tag });
            }

            this.listQueue.EndUpdate();

            tabMain.Visible = false;

            cmdAddGames.Enabled = false;
            cmdCancel.Enabled = true;

            tabAddGames.Visible = true;
            tabAddGames.Select();

            panelStatus.Text = DownloadingDataMessage;

            progressAdder.Value = 0;
            progressAdder.Maximum = this.listQueue.Items.Count;

            foreach (ListViewItem li in this.listQueue.Items)
            {
                // download data

                li.SubItems[1].Text = Yes;

                progressAdder.Value++;

                if (this._cancel)
                {
                    this._cancel = false;
                    break;
                }
            }

            panelStatus.Text = AddGamesMessage;

            cmdAddGames.Enabled = true;
            cmdCancel.Enabled = true;
        }

        private bool _cancel = false;
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            tabAddGames.Visible = false;

            tabMain.Visible = true;
            tabMain.Select();

            this._cancel = true;
        }

        private void cmdGoBack_Click(object sender, EventArgs e)
        {
            tabDetails.Visible = false;

            tabMain.Visible = true;
            tabMain.Select();
        }

        private async void cmdAddGames_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AddGames());
        }

        private void AddGames()
        {
            progressAdder.Value = 0;
            progressAdder.Maximum = this.listQueue.Items.Count;

            foreach (ListViewItem li in this.listQueue.Items)
            {
                this.listQueue.TopItem = li;

                var titleId = (uint)li.Tag;

                var x = TitleCollection.Titles[titleId];

                var titleRecord = new TitleRecord(titleId, x.Name, x.AchievementCount, x.Credit, x.AwardCount, x.MaleAwardCount, x.FemaleAwardCount);

                this.Profile.AddTitleRecord(titleRecord);

                if (x.AchievementCount != 0)
                {
                    var achTracker = new AchievementTracker(this.Profile, titleId);
                    foreach (var ach in x.Achievements)
                    {
                        var a = new AchievementRecord(ach.ID);
                        a.ImageID = ach.ImageID;
                        a.Flags = ach.Flags;
                        a.Label = ach.Label;
                        a.Credit = ach.Credit;
                        a.Description = ach.AchievedDescription;
                        a.UnachievedDescription = ach.UnachievedDescription;

                        achTracker.AddAchievement(a);
                    }
                    achTracker.Close();
                }

                if (x.AwardCount != 0)
                {
                    var awTracker = new AvatarAwardTracker(this.Profile, titleId);
                    foreach (var aw in x.Awards)
                    {
                        var a = new AvatarAwardRecord();
                        a.ID = aw.ID;
                        a.ImageID = aw.ImageID;
                        a.Reserved = aw.Reserved;
                        a.Name = aw.Name;
                        a.Description = aw.Description;
                        a.UnawardedDescription = aw.UnawardedDescription;

                        awTracker.AddAward(a);
                    }
                    awTracker.Close();
                }

                li.SubItems[2].Text = Yes;

                progressAdder.Value++;
            }
        }

        private void listTitles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Unchecked)
                cmdSelectAll.Text = "Select All";
            else if (listTitles.CheckedItems.Count == listTitles.Items.Count)
                cmdSelectAll.Text = "Deselect All";
        }
    }
}
