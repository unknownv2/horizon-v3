using System;
using System.Linq;
using System.Threading.Tasks;
using NoDev.Horizon.DeviceExplorer;
using NoDev.XContent;
using NoDev.XProfile;

namespace NoDev.Horizon
{
    internal class ProfileEditor : PackageEditor
    {
        internal ProfileEditor()
        {
            InitializeComponent();
        }

        protected ProfileFile Profile;

        protected XProfileAccount Account
        {
            get { return this.Profile.Account; }
        }

        protected override void CloseStreams()
        {
            if (this.Profile != null)
                this.Profile.Close();
        }

        protected override void AfterSave()
        {
            this.Profile.Flush();
        }

        protected override async Task LoadPackage(XContentPackage package)
        {
            var meta = package.Header.Metadata;

            if (meta.ExecutionId.TitleID != ProfileFile.TitleID)
                throw new Exception("Invalid title ID for profile.");

            if (meta.ContentType != XContentTypes.Profile)
                throw new Exception("This editor can only open profiles.");

            if (!package.IsMounted)
                package.Mount();

            this.Profile = new ProfileFile(package.Drive, package.Header.Metadata.Creator);

            ProfileCache.SetProfile(new ProfileData(Account.Gamertag, meta.Creator, Account.XuidOnline, meta.Thumbnail.ToImage()));

            this.Package = package;

            this.BeforeOpen();

            await base.LoadPackage(package);
        }

        protected virtual void BeforeOpen()
        {

        }

        private void InitializeComponent()
        {
            this.cmdOpen.FixedSize = new System.Drawing.Size(85, 23);
            this.cmdOpen.Text = "Open Profile";
        }

        protected override void OnDeviceItemExpand(FatxDeviceItem item)
        {
            foreach (ProfileInfo profile in item.Device.Profiles.Where(p => !p.UnknownOrCorrupted))
            {
                item.SubItems.Add(new FatxClickableProfileItem(item.Device, profile, async profileItem => {
                    var p = profileItem.Profile.Package;
                    p.Open();
                    if (!p.IsMounted)
                        p.Mount();
                    await this.LoadPackageNoExceptions(p);
                }));
            }

            if (item.SubItems.Count == 0)
                item.SubItems.AddDisabledButton("No Profiles");
        }
    }
}
