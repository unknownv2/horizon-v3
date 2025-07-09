using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoDev.Horizon.Controls;
using NoDev.Horizon.DeviceExplorer;
using NoDev.Horizon.Properties;
using NoDev.XProfile;
using NoDev.XProfile.Records;
using NoDev.XProfile.Trackers;
using DevComponents.AdvTree;

namespace NoDev.Horizon.Editors.Achievement_Unlocker
{
    [EditorInfo(0x05, "Achievement Unlocker", "Thumb_Generic_Trophey", Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Profile)]
    partial class AchievementUnlocker : ProfileEditor
    {
        internal AchievementUnlocker()
        {
            InitializeComponent();

            var dtFormat = CultureInfo.CurrentUICulture.DateTimeFormat;
            dateTimeTitle.CustomFormat = dtFormat.ShortDatePattern + " " + dtFormat.ShortTimePattern;
            dateTimeAch.CustomFormat = dateTimeTitle.CustomFormat;
        }

        private AchievementTracker _tracker;

        private AchievementRecord _selectedRecord;
        private Node _selectedNode;

        private bool _working;

        protected override void CloseStreams()
        {
            if (this._tracker != null)
                this._tracker.Close();

            base.CloseStreams();
        }

        protected override void Open()
        {
            int pValue = 0, pMax = 0, credEarned = 0;

            var titles = new List<ListViewItem>();

            foreach (var title in this.Profile.GetAllTitleRecords().Where(title => title.AchievementsPossible != 0))
            {
                credEarned += title.CreditEarned;

                pValue += title.AchievementsEarned;
                pMax += title.AchievementsPossible;

                titles.Add(new ListViewItem(new[] { title.TitleName, GetCreditProgessText(title) }) { Tag = title.TitleID });
            }

            if (titles.Count == 0)
                throw new Exception("There are no achievements to unlock in this profile!");

            // Update or create the achievements earned profile setting.
            bool achExists = this.Profile.Settings.Exists(XProfileID.GamercardAchievementsEarned);
            if (!achExists || this.Profile.Settings.GetSetting(XProfileID.GamercardAchievementsEarned).Value1 != pValue)
                this.Profile.Settings.UpdateOrCreateInt32Setting(XProfileID.GamercardAchievementsEarned, pValue);

            // Update or create the credit earned profile setting.
            bool credExists = this.Profile.Settings.Exists(XProfileID.GamercardCreditEarned);
            if (!credExists || this.Profile.Settings.GetSetting(XProfileID.GamercardCreditEarned).Value1 != credEarned)
                this.Profile.Settings.UpdateOrCreateInt32Setting(XProfileID.GamercardCreditEarned, credEarned);

            progressTotal.Maximum = pMax;
            progressTotal.Value = pValue;
            progressTotal.Text = GetPercentString(pValue, pMax);

            this.listTitles.BeginUpdate();
            this.listTitles.Items.AddRange(titles.ToArray());
            this.listTitles.EndUpdate();

            this.listTitles.Items[0].Selected = true;

            this.listTitles.Select();
        }

        protected override void Save()
        {
            this.UpdateAchievedTime();
            this.UpdateLastPlayed();

            this._tracker.Flush();
        }

        private void UpdateLastPlayed()
        {
            if (this._tracker == null)
                return;

            var n = dateTimeTitle.LockUpdateChecked ? dateTimeTitle.Value.ToUniversalTime() : DateTime.FromFileTimeUtc(0);

            if (n == this._tracker.TitleRecord.LastLoaded)
                return;

            this._tracker.TitleRecord.LastLoaded = n.ToUniversalTime();
            this.Profile.UpdateTitleRecord(this._tracker.TitleRecord);
        }

        private void UpdateAchievedTime()
        {
            if (this._selectedNode == null)
                return;

            var n = dateTimeAch.LockUpdateChecked ? dateTimeAch.Value.ToUniversalTime() : DateTime.FromFileTimeUtc(0);

            if (n == this._selectedRecord.DateTimeAchieved)
                return;

            var selRecord = this._selectedRecord;

            if (dateTimeAch.LockUpdateChecked)
            {
                selRecord.Achieved = false;
                selRecord.AchievedOnline = true;
                selRecord.DateTimeAchieved = n;
            }
            else
            {
                selRecord.Achieved = true;
                selRecord.AchievedOnline = false;
                selRecord.DateTimeAchieved = n;
            }
        }

        private void CloseTracker()
        {
            if (this._tracker == null)
                return;

            this.UpdateAchievedTime();
            this.UpdateLastPlayed();

            this._tracker.Close();
        }

        private void listTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listTitles.SelectedItems.Count == 0)
                return;

            this.CloseTracker();

            var selectedTitle = this.listTitles.SelectedItems[0];

            this._tracker = new AchievementTracker(this.Profile, (uint)selectedTitle.Tag);

            this._working = true;

            if (this._tracker.TitleRecord.LastLoaded.ToFileTimeUtc() == 0)
            {
                dateTimeTitle.IsEmpty = true;
                dateTimeTitle.LockUpdateChecked = false;
            }
            else
            {
                dateTimeTitle.IsEmpty = false;
                dateTimeTitle.LockUpdateChecked = true;
                dateTimeTitle.Value = this._tracker.TitleRecord.LastLoaded.ToLocalTime();
            }

            this._working = false;

            this._tracker.SortAchievements((a1, a2) => {
                if (a1.AchievedOnline)
                    return a2.AchievedOnline ? string.Compare(a1.Label, a2.Label, StringComparison.Ordinal) : 1;

                if (a1.Achieved)
                    return a2.Achieved ? string.Compare(a1.Label, a2.Label, StringComparison.Ordinal) : 1;

                return (a2.Achieved || a2.AchievedOnline) ? -1 : string.Compare(a1.Label, a2.Label, StringComparison.Ordinal);
            });

            this.FillAchievementList();

            this.UpdateAchievementsChecked();
        }

        private static string GetPercentString(int value, int max)
        {
            if (max == 0)
                max = 1;

            return Math.Round((decimal)value * 100 / max).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static string GetCreditProgessText(TitleRecord title)
        {
            return string.Format("{0} / {1}", title.CreditEarned, title.CreditPossible);
        }

        private static string GetAchievementStateText(AchievementRecord ach)
        {
            if (ach.AchievedOnline)
                return "Unlocked Online";

            if (ach.Achieved)
                return "Unlocked Offline";

            return "Locked";
        }

        private static string GetAchievementDescription(AchievementRecord ach)
        {
            if (ach.Achieved | ach.AchievedOnline)
                return ach.Description.Replace(Environment.NewLine, " "); 

            if (ach.ShowUnachieved)
                return ach.UnachievedDescription.Replace(Environment.NewLine, " ");

            return "Secret Achievement";
        }

        private static Image GetAchievementTile(AchievementTracker tracker, AchievementRecord ach)
        {
            if (ach.Achieved | ach.AchievedOnline)
            {
                byte[] tile = tracker.GetAchievementTile(ach);
                if (tile != null)
                    return ResizeImage(tile.ToImage());
            }
            return ResizeImage(Resources.QuestionMark_64);
        }

        private void FillAchievementList()
        {
            listAch.BeginUpdate();

            listAch.Nodes.Clear();

            this._visibleCheckBoxes = 0;

            int pValue = 0, pMax = 0, credEarned = 0;

            foreach (var ach in this._tracker.Achievements)
            {
                pMax++;

                if (ach.Achieved || ach.AchievedOnline)
                {
                    pValue++;
                    credEarned += ach.Credit;
                }

                listAch.Nodes.Add(CreateAchievementNode(ach));
            }

            listAch.AutoScrollPosition = new Point(0, 0);

            listAch.EndUpdate();

            var rec = this._tracker.TitleRecord;

            if (rec.AchievementsEarned != pValue || rec.CreditEarned != credEarned)
            {
                rec.AchievementsEarned = pValue;
                rec.CreditEarned = credEarned;
                this.Profile.UpdateTitleRecord(rec);
            }

            progressTitle.Maximum = pMax;
            progressTitle.Value = pValue;
            progressTitle.Text = GetPercentString(pValue, pMax);

            this.listAch.SelectedNode = this.listAch.Nodes[0];
        }

        private Node CreateAchievementNode(AchievementRecord ach)
        {
            var x = new Node(GetCell0Text(ach));
            x.Image = GetAchievementTile(this._tracker, ach);
            x.Tag = ach;

            bool pendingSync = this._tracker.IsPendingSync(ach);

            if (pendingSync || (!ach.Achieved && !ach.AchievedOnline))
            {
                x.CheckBoxVisible = true;
                this._visibleCheckBoxes++;
            }

            x.Cells.Add(new Cell(ach.Credit.ToString(CultureInfo.InvariantCulture) + " G"));
            x.Cells.Add(new Cell(GetAchievementDescription(ach)));

            return x;
        }

        private static string GetCell0Text(AchievementRecord ach)
        {
            return ach.Label + FatxNode.LineBreak + FatxNode.CreateGrayText(GetAchievementStateText(ach));
        }

        private static Image ResizeImage(Image image)
        {
            var b = new Bitmap(32, 41);
            var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 5, 32, 32);
            g.Dispose();
            return b;
        }

        private void cmdRules_Click(object sender, EventArgs e)
        {
            new RuleManager().ShowDialog(this);
        }

        private void listAch_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            if (this._working || listAch.SelectedNode == null)
                return;

            this.UpdateAchievedTime();

            this.FillAchievementStatus(listAch.SelectedNode);
        }

        private void FillAchievementStatus(Node node)
        {
            this._working = true;

            this._selectedNode = node;
            this._selectedRecord = (AchievementRecord)node.Tag;

            var ach = this._selectedRecord;

            cmdUnlock.Checked = ach.Achieved || ach.AchievedOnline;

            bool notSynced = this._tracker.IsPendingSync(ach) || !cmdUnlock.Checked;

            cmdUnlock.Enabled = notSynced;

            this.dateTimeAch.MaxDate = DateTime.Now;

            if (ach.AchievedOnline)
            {
                dateTimeAch.LockUpdateChecked = true;
                dateTimeAch.Enabled = notSynced;
                dateTimeAch.Value = ach.DateTimeAchieved;
            }
            else if (ach.Achieved)
            {
                dateTimeAch.LockUpdateChecked = false;
                dateTimeAch.Enabled = notSynced;
                dateTimeAch.IsEmpty = true;
            }
            else
            {
                dateTimeAch.LockUpdateChecked = false;
                dateTimeAch.Enabled = false;
                dateTimeAch.IsEmpty = true;
            }

            this._working = false;
        }

        private void cmdUnlock_CheckedChanged(object sender, EventArgs e)
        {
            if (this._working)
                return;

            if (!cmdUnlock.Checked)
                this._tracker.LockAchievement(this._selectedRecord);
            else if (dateTimeAch.LockUpdateChecked)
                this._tracker.UnlockAchievementOnline(this._selectedRecord, dateTimeAch.Value);
            else
                this._tracker.UnlockAchievement(this._selectedRecord);

            this.FillAchievementStatus(this._selectedNode);

            this._selectedNode.Text = GetCell0Text(this._selectedRecord);

            if (cmdUnlock.Checked)
            {
                progressTitle.Value++;
                progressTotal.Value++;
            }
            else
            {
                progressTitle.Value--;
                progressTotal.Value--;
            }

            progressTitle.Text = GetPercentString(progressTitle.Value, progressTitle.Maximum);
            progressTotal.Text = GetPercentString(progressTotal.Value, progressTotal.Maximum);

            this.listAch.Focus();
        }

        private void dateTimeAch_LockUpdateChanged(object sender, EventArgs e)
        {
            if (this._working)
                return;

            if (dateTimeAch.LockUpdateChecked)
            {
                var dtNow = DateTime.Now;
                this.dateTimeAch.MaxDate = dtNow;
                dateTimeAch.Value = dtNow;
            }
            else
                dateTimeAch.IsEmpty = true;
        }

        private void dateTimeTitle_LockUpdateChanged(object sender, EventArgs e)
        {
            if (this._working)
                return;

            if (dateTimeTitle.LockUpdateChecked)
            {
                var dtNow = DateTime.Now;
                this.dateTimeTitle.MaxDate = dtNow;
                dateTimeTitle.Value = dtNow;
            }
            else
                dateTimeTitle.IsEmpty = true;
        }

        private int _visibleCheckBoxes;
        private void UpdateAchievementsChecked()
        {
            cmdUnlockTitle.Enabled = this.listAch.CheckedNodes.Count != 0;

            cmdSelectAll.Enabled = _visibleCheckBoxes != 0;
            cmdSelectAll.Text = _visibleCheckBoxes != 0 && _visibleCheckBoxes == this.listAch.CheckedNodes.Count ? "Deselect All" : "Select All";
        }

        private void listAch_AfterCheck(object sender, AdvTreeCellEventArgs e)
        {
            this.UpdateAchievementsChecked();
        }

        private void cmdSelectAll_Click(object sender, EventArgs e)
        {
            bool se = _visibleCheckBoxes != this.listAch.CheckedNodes.Count;
            foreach (Node node in this.listAch.Nodes)
                if (node.CheckBoxVisible)
                    node.Checked = se;
        }

        private void listTitles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
                this.listAch.Focus();
        }

        private void listAch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                this.listTitles.Focus();
            else if (e.KeyCode == Keys.Enter && this.listAch.SelectedNode != null && this.listAch.SelectedNode.CheckBoxVisible)
                this.listAch.SelectedNode.Checked = !this.listAch.SelectedNode.Checked;
        }

        private void listAch_NodeDoubleClick(object sender, TreeNodeMouseEventArgs e)
        {
            if (this.listAch.SelectedNode.CheckBoxVisible)
                this.listAch.SelectedNode.Checked = !this.listAch.SelectedNode.Checked;
        }

        private void listAch_MouseEnter(object sender, EventArgs e)
        {
            this.listAch.Focus();
        }

        private void listTitles_MouseEnter(object sender, EventArgs e)
        {
            this.listTitles.Focus();
        }

        private void cmdUnlockTitle_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < this.listAch.CheckedNodes.Count; x++)
            {
                this.listAch.SelectedIndex = x;
                cmdUnlock.Checked = true;
            }
        }

        private async void cmdUnlockAll_Click(object sender, EventArgs e)
        {
            if (DialogBox.Show("Are you sure you want to unlock ALL of your achievements?\n\nThis will unlock everything offline.", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button3, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            this.CloseTracker();

            await ControlLoader.RunAsync(this, UnlockAll);

            listTitles_SelectedIndexChanged(null, null);
        }

        private dynamic UnlockAll(ControlLoader cl)
        {
            var count = listTitles.Items.Count;

            cl.Maximum = count;
            cl.Value = 0;

            foreach (var title in this.Profile.GetAllTitleRecords().Where(title => title.AchievementsPossible != 0))
            {
                var tracker = new AchievementTracker(this.Profile, title.TitleID);

                foreach (var ach in tracker.Achievements.Where(a => !a.Achieved && !a.AchievedOnline))
                    tracker.UnlockAchievement(ach);

                tracker.Close();

                cl.Value++;
            }

            return null;
        }
    }
}
