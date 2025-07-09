using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NoDev.Horizon.DeviceExplorer;
using DevComponents.AdvTree;
using DevComponents.DotNetBar;

namespace NoDev.Horizon.Forms.Misc
{
    public partial class ProfileManager : OfficeForm
    {
        private ProfileManager()
        {
            InitializeComponent();

            this.PopulateList();

            this.listProfiles.SelectionChanged += (a, b) => cmdRemoveSelected.Enabled = this.listProfiles.SelectedNodes.Count != 0;
            this.listProfiles.AfterNodeInsert += listProfiles_AfterNodeInsertOrRemove;
            this.listProfiles.AfterNodeRemove += listProfiles_AfterNodeInsertOrRemove;
        }

        internal static new void Show(IWin32Window owner)
        {
            new ProfileManager().ShowDialog(owner);
        }

        private void PopulateList()
        {
            this.listProfiles.Nodes.Clear();

            var profiles = ProfileCache.GetAll();
            foreach (var profile in profiles)
                this.listProfiles.Nodes.Add(CreateProfileNode(profile));

            if (this.listProfiles.Nodes.Count != 0)
                cmdClearHistory.Enabled = true;

            ProfileCache.CacheChange += ProfileCache_CacheChange;
        }

        private bool _ignoreChanges;

        private void ProfileCache_CacheChange(object sender, EventArgs e)
        {
            if (this._ignoreChanges)
                return;

            this.PopulateList();
        }

        private void listProfiles_AfterNodeInsertOrRemove(object sender, TreeNodeCollectionEventArgs e)
        {
            this.cmdClearHistory.Enabled = this.listProfiles.Nodes.Count != 0;
        }

        private static Node CreateProfileNode(ProfileData profile)
        {
            var node = new Node(profile.Gamertag + FatxNode.LineBreak + FatxNode.CreateGrayText(profile.ProfileID.ToString("X16")));
            node.Tag = profile.ProfileID;
            node.Image = profile.Gamerpic;
            var ckFav = new CheckBoxItem();
            ckFav.Text = "Favorite";
            ckFav.Tag = profile.ProfileID;
            ckFav.Checked = profile.Favorite;
            ckFav.CheckedChanged += ckFav_CheckedChanged;
            node.Cells.Add(new Cell() { HostedItem = ckFav });
            return node;
        }

        private static void ckFav_CheckedChanged(object sender, CheckBoxChangeEventArgs e)
        {
            var ckFav = (CheckBoxItem)sender;
            ulong profileId = (ulong)ckFav.Tag;

            if (ProfileCache.Contains(profileId))
            {
                ProfileCache.GetProfile(profileId).Favorite = ckFav.Checked;
                ProfileCache.Save();
            }
        }

        internal static IEnumerable<ButtonItem> CreateFavoriteButtons()
        {
            var profiles = ProfileCache.GetFavorites();
            var items = new List<ButtonItem>();
            foreach (var profile in profiles)
            {
                var b = new ButtonItem();
                b.Text = profile.Gamertag + FatxNode.LineBreak + FatxNode.CreateGrayText(profile.ProfileID.ToString("X16"));
                b.Image = profile.Gamerpic;
                b.ImageFixedSize = new Size(32, 32);
                b.Tag = profile.ProfileID;
                b.CanCustomize = false;
                items.Add(b);
            }
            items.Sort((b1, b2) => string.Compare(b1.Text, b2.Text, StringComparison.Ordinal));
            return items;
        }

        private void cmdRemoveSelected_Click(object sender, EventArgs e)
        {
            var nodes = new List<Node>(this.listProfiles.SelectedNodes.Count);
            nodes.AddRange(this.listProfiles.SelectedNodes.Cast<object>().Cast<Node>());

            _ignoreChanges = true;
            foreach (Node n in nodes)
            {
                ProfileCache.Remove((ulong)n.Tag);
                n.Remove();
            }
            _ignoreChanges = false;

            ProfileCache.Save();
        }

        private void cmdClearHistory_Click(object sender, EventArgs e)
        {
            if (DialogBox.Show("Are you sure you want to clear your entire profile history?", "Confirm",
                MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button3, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            ProfileCache.Clear();
            ProfileCache.Save();

            cmdClearHistory.Enabled = false;
        }
    }
}