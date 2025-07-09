using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoDev.Common;
using NoDev.Horizon.DeviceExplorer;
using NoDev.Horizon.Forms.Misc;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using NoDev.XProfile;
using DevComponents.DotNetBar;

namespace NoDev.Horizon.Editors.Package_Manager
{
    [EditorInfo(0x04, "Package Manager", "Thumb_Generic_FolderGear", Group.Guest | Group.Regular | Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Tool)]
    partial class PackageManager : PackageEditor
    {
        internal PackageManager()
        {
            InitializeComponent();

            pbIcon1.ContextMenu = this.CreateThumbnailContextMenu();
            pbIcon1.ContextMenu.Tag = pbIcon1;

            pbIcon2.ContextMenu = this.CreateThumbnailContextMenu();
            pbIcon2.ContextMenu.Tag = pbIcon2;

            cmdModPackage.Click += ControlManager.TransferButtonClicked;

            ProfileCache.CacheChange += ProfileCache_CacheChange;
        }

        private void ProfileCache_CacheChange(object sender, EventArgs e)
        {
            if (this.Package != null)
                this.UpdateProfileInfo();
        }

        protected override void OnFormLoad()
        {
            base.OnFormLoad();

            cmdPackageManager.Visible = false;
        }

        private ContextMenu CreateThumbnailContextMenu()
        {
            var c = new ContextMenu();
            c.MenuItems.Add(new MenuItem("Extract...", Event_ContextMenu_Extract_Click));
            c.MenuItems.Add(new MenuItem("Replace...", Event_ContextMenu_Replace_Click));
            return c;
        }

        private void Event_ContextMenu_Extract_Click(object sender, EventArgs e)
        {
            var pb = (PictureBox)((MenuItem)sender).GetContextMenu().Tag;

            var sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image|*.png";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                pb.Image.Save(sfd.FileName);
            }
            catch (Exception ex)
            {
                DialogBox.ShowException(ex);
            }
        }

        private void Event_ContextMenu_Replace_Click(object sender, EventArgs e)
        {
            var pb = (PictureBox)((MenuItem)sender).GetContextMenu().Tag;

            var ofd = new OpenFileDialog();
            ofd.Filter = "PNG Image|*.png";
            ofd.FileName = "Thumbnail";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                byte[] newThumb = File.ReadAllBytes(ofd.FileName);

                Image newImage = newThumb.ToImage();

                if (newImage == null)
                    throw new Exception("Invalid image file.");

                if (pb == this.pbIcon1)
                    this.Package.Header.Metadata.Thumbnail = newThumb;
                else
                    this.Package.Header.Metadata.TitleThumbnail = newThumb;

                pb.Image = newImage;
            }
            catch (Exception ex)
            {
                DialogBox.ShowException(ex);
            }
        }

        protected override void Open()
        {
            var meta = this.Package.Header.Metadata;

            this.pbIcon1.Image = this.Package.Header.Metadata.Thumbnail.ToImage() ?? Resources.Console_64;
            this.pbIcon2.Image = this.Package.Header.Metadata.TitleThumbnail.ToImage() ?? Resources.Console_64;

            if (meta.ContentType == XContentTypes.Profile)
            {
                txtCreator.ReadOnly = true;
                this.SetProfileEnabled(false);

                if (!ProfileCache.Contains(meta.Creator))
                {
                    if (!this.Package.IsMounted)
                        this.Package.Mount();

                    string accPath = this.Package.Drive.CreatePath("Account");

                    if (!File.Exists(accPath))
                        throw new Exception("Account file not found in profile.");

                    var acc = new XProfileAccount(File.ReadAllBytes(this.Package.Drive.CreatePath("Account")));

                    string gamertag = acc.Gamertag;
                    ulong xuid = acc.XuidOnline;

                    ProfileCache.SetProfile(new ProfileData(gamertag, meta.Creator, xuid, (Image)pbIcon1.Image.Clone()));

                    ProfileCache.Save();
                }
            }
            else
            {
                txtCreator.ReadOnly = false;
                this.SetProfileEnabled(true);
            }

            this.SetReadOnly(this.Package.IsReadOnly);

            txtDisplayName.Text = meta.DisplayName;
            txtTitleName.Text = meta.TitleName;

            txtCreator.Text = meta.Creator.ToString("X16");
            txtDeviceID.Text = Formatting.ByteArrayToHexString(meta.DeviceID, true);
            txtConsoleID.Text = Formatting.ByteArrayToHexString(meta.ConsoleID, true);
            txtTitleID.Text = meta.ExecutionId.TitleID.ToString("X8");

            this.UpdateProfileInfo();

            //ControlManager.FillModButton(this.cmdModPackage, this.Package, this);
        }

        private void SetProfileEnabled(bool enabled)
        {
            cmdSwitchProfile.Enabled = enabled;
        }

        private void UpdateProfileInfo()
        {
            if (txtCreator.Text.Length == 0)
            {
                this.SetProfilePublic();
                return;
            }

            ulong profileId;
            if (!Numbers.TryParseUInt64Hex(txtCreator.Text, out profileId))
            {
                this.SetProfileUnknown();
                return;
            }

            if (profileId == 0)
            {
                this.SetProfilePublic();
                return;
            }

            if (!ProfileCache.Contains(profileId))
            {
                this.SetProfileUnknown();
                return;
            }

            var profile = ProfileCache.GetProfile(profileId);

            this.pbGamerpic.Image = profile.Gamerpic;
            this.lblGamertag.Text = profile.Gamertag;
        }

        private void cmdManageProfiles_Click(object sender, EventArgs e)
        {
            ProfileManager.Show(this);
        }

        private void txtCreator_TextChanged(object sender, EventArgs e)
        {
            this.UpdateProfileInfo();
        }

        private void SetProfilePublic()
        {
            this.pbGamerpic.Image = Resources.QuestionMark_64;
            this.lblGamertag.Text = "Public Profile";
        }

        private void SetProfileUnknown()
        {
            this.pbGamerpic.Image = Resources.QuestionMark_64;
            this.lblGamertag.Text = "Unknown Profile";
        }

        private void SetReadOnly(bool readOnly)
        {
            cmdSave.Visible = !readOnly;
            gpProfile.Enabled = !readOnly;

            txtDisplayName.ReadOnly = readOnly;
            txtTitleName.ReadOnly = readOnly;

            txtCreator.ReadOnly = readOnly;
            txtDeviceID.ReadOnly = readOnly;
            txtConsoleID.ReadOnly = readOnly;

            pbIcon1.ContextMenu.MenuItems[1].Enabled = !readOnly;
            pbIcon2.ContextMenu.MenuItems[1].Enabled = !readOnly;

            controlRibbon.Refresh();
        }

        protected override async void Save()
        {
            ulong creator = 0;

            if (txtCreator.Text.Length != 0 && !Numbers.TryParseUInt64Hex(txtCreator.Text, out creator))
                throw new Exception("Invalid Profile ID entered.");

            byte[] deviceId;
            if (txtDeviceID.Text.Length == 0)
                deviceId = new byte[20];
            else if (txtDeviceID.Text.Length != 40)
                throw new Exception("Device ID must be 40 characters in length or null.");
            else
                deviceId = Formatting.HexStringToByteArray(txtDeviceID.Text);

            byte[] consoleId;
            if (txtConsoleID.Text.Length == 0)
                consoleId = new byte[5];
            else if (txtConsoleID.Text.Length != 10)
                throw new Exception("Console ID must be 10 characters in length or null.");
            else
                consoleId = Formatting.HexStringToByteArray(txtConsoleID.Text);

            XContentMetadata meta = this.Package.Header.Metadata;

            if (meta.ContentType != XContentTypes.Profile && meta.Creator != creator)
            {
                var driveInfo = new DriveInfo(Path.GetPathRoot(this.Package.Filename));

                if (driveInfo.DriveFormat == "FATX")
                {
                    ulong originalCreator = meta.Creator;
                    meta.Creator = creator;
                    string newFilePath = this.Package.FormatFATXDevicePath();
                    if (!File.Exists(newFilePath) ||
                        DialogBox.Show("This file already exists for the target profile. Do you want to overwrite it?",
                            "Overwrite?", MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button3,
                            MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        try
                        {
                            await this.Package.Move(newFilePath);
                        }
                        catch (Exception ex)
                        {
                            meta.Creator = originalCreator;
                            DialogBox.ShowException(ex);
                            return;
                        }
                    }
                    else
                    {
                        meta.Creator = originalCreator;
                        return;
                    }
                }
            }

            meta.Creator = creator;
            meta.DeviceID = deviceId;
            meta.ConsoleID = consoleId;
            meta.DisplayName = txtDisplayName.Text;
            meta.TitleName = txtTitleName.Text;

            if (meta.ContentType == XContentTypes.Profile && ProfileCache.Contains(meta.Creator))
            {
                var profileData = ProfileCache.GetProfile(meta.Creator);
                profileData.Gamerpic = (Image)pbIcon1.Image.Clone();
                ProfileCache.SetProfile(profileData);
            }
        }

        private void cmdViewContents_Click(object sender, EventArgs e)
        {
            if (!this.Package.IsMounted)
                this.Package.Mount();
            Process.Start(this.Package.Drive.Name);
        }

        protected override void OnDeviceItemExpand(FatxDeviceItem item)
        {
            foreach (GeneralContentType contentType in Enum.GetValues(typeof(GeneralContentType)))
                item.SubItems.Add(new FatxContentItem(item.Device, contentType, this.OnFatxContentItemExpand));
        }

        private async void OnFatxContentItemExpand(FatxContentItem contentItem)
        {
            if (!contentItem.ContainsLoadingItem)
                return;

            GeneralContentType type = contentItem.GeneralContentType;
            FatxDevice device = contentItem.Device;

            if (type == GeneralContentType.Games)
            {
                contentItem.AddLoadingItem();

                var titleIds = await device.GetTitlesAsync(ContentTypeHelper.GetContentTypes(GeneralContentType.Games));

                if (titleIds.Count == 0)
                {

                    contentItem.SubItems.RemoveAt(0);
                    contentItem.SubItems[0].Text = "No Content";
                    return;
                }

                contentItem.SubItems.RemoveAt(0);

                foreach (uint titleId in titleIds)
                    contentItem.InsertItemAt(new FatxTitleItem(device, titleId, this.OnFatxTitleItemExpand), 0, false);

                contentItem.SubItems.RemoveAt(0);
                
                contentItem.SubItems.Sort();
            }
            else if (type == GeneralContentType.Gamer_Profiles)
            {
                foreach (var profileInfo in device.Profiles)
                    if (!profileInfo.Unknown && !profileInfo.Corrupted)
                        contentItem.SubItems.Add(new FatxClickableProfileItem(device, profileInfo,
                            async profileItem => await this.LoadFileNoExceptions(profileItem.Device.FormatProfileFilePath(profileItem.Profile.ProfileID))));
                contentItem.SubItems.RemoveDisabledItems();
                if (contentItem.SubItems.Count == 0)
                    contentItem.InsertDisabledButton("No Profiles");
                else
                    contentItem.SubItems.Sort();
            }
            else
            {
                contentItem.AddLoadingItem();

                var filter = new FatxPackageFilter();
                filter.ContentTypes = ContentTypeHelper.GetContentTypes(type);
                filter.SetProfileFilterType(ContentTypeHelper.IsPublicGeneralContent(type) ? ProfileFilterType.PublicOnly : ProfileFilterType.All, device);

                await device.GetPackagesAsync(filter, package => // package added
                    this.Invoke(() => {
                        contentItem.SubItems.RemoveDisabledItems();
                        contentItem.InsertItemAt(new FatxPackageItem(device, package, this.LoadPackageNoExceptions), 0, false);
                        contentItem.SubItems.Sort();
                    })
                );

                if (contentItem.SubItems.Count == 2)
                {
                    contentItem.SubItems.RemoveAt(0);
                    contentItem.SubItems[0].Text = "No Content";
                }
            }
        }

        private async void OnFatxTitleItemExpand(FatxTitleItem titleItem)
        {
            if (!titleItem.ContainsLoadingItem)
                return;

            titleItem.AddLoadingItem();

            var filter = new FatxPackageFilter();
            filter.ContentTypes = ContentTypeHelper.GetContentTypes(GeneralContentType.Games);
            filter.SetProfileFilterType(ProfileFilterType.All, titleItem.Device);
            filter.TitleID = titleItem.TitleID;

            await titleItem.Device.GetPackagesAsync(filter, package =>  // package added
                this.Invoke(() => {
                    titleItem.SubItems.RemoveDisabledItems();
                    titleItem.InsertItemAt(new FatxPackageItem(titleItem.Device, package, this.LoadPackageNoExceptions), 0, false);
                    titleItem.SubItems.Sort();
                })
            );

            if (titleItem.SubItems.Count == 2)
            {
                titleItem.SubItems.RemoveAt(0);
                titleItem.SubItems[0].Text = "No Content";
            }

            titleItem.SubItems.Sort();
        }

        private void cmdSwitchProfile_PopupOpen(object sender, EventArgs e)
        {
            while (cmdSwitchProfile.SubItems.Count > 1)
                cmdSwitchProfile.SubItems.RemoveAt(1);

            ulong profileId;
            Numbers.TryParseUInt64Hex(txtCreator.Text, out profileId);

            IEnumerable<ButtonItem> profileButtons = ProfileManager.CreateFavoriteButtons();

            foreach (var profileButton in profileButtons)
            {
                if ((ulong)profileButton.Tag == profileId)
                    continue;

                profileButton.Click += (b, args) => txtCreator.Text = ((ulong) ((ButtonItem) b).Tag).ToString("X16");

                cmdSwitchProfile.SubItems.Add(profileButton);
            }

            cmdNoProfiles.Visible = cmdSwitchProfile.SubItems.Count == 1;
        }
    }
}
