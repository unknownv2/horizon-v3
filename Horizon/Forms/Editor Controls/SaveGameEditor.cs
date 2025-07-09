using System;
using System.Threading.Tasks;
using NoDev.Horizon.DeviceExplorer;
using NoDev.XContent;

namespace NoDev.Horizon
{
    class SaveGameEditor : PackageEditor
    {
        protected override async Task LoadPackage(XContentPackage package)
        {
            uint realTitleId = TitleControl.GetProperTitleID(package);
            if (realTitleId != this.Info.TitleID)
                throw new Exception();

            if (package.Header.Metadata.ContentType != XContentTypes.SavedGame)
                throw new Exception();

            if (!package.IsMounted)
                package.Mount();

            await base.LoadPackage(package);
        }

        protected override void OnDeviceItemExpand(FatxDeviceItem item)
        {
            if (item.Device.Profiles.Count == 0)
                item.SubItems.AddDisabledButton("No Profiles");
            else
            {
                foreach (ProfileInfo profile in item.Device.Profiles)
                    item.SubItems.Add(new FatxExpandableProfileItem(item.Device, profile, FatxProfileItemExpanded));
            }
        }

        private async void FatxProfileItemExpanded(FatxProfileItem profileItem)
        {
            var filter = new FatxPackageFilter();
            filter.AddProfileID(profileItem.Profile.ProfileID);
            filter.TitleID = Info.TitleID;
            filter.ContentTypes = new[] { XContentTypes.SavedGame };

            await profileItem.Device.GetPackagesAsync(filter, package =>
                this.Invoke(() => {
                    profileItem.InsertItemAt(new FatxPackageItem(profileItem.Device, package, this.LoadPackageNoExceptions), profileItem.SubItems.Count, false);
                    profileItem.SubItems.Sort();
                })
            );

            profileItem.SubItems.RemoveDisabledItems();
            if (profileItem.SubItems.Count == 0)
                profileItem.InsertDisabledButton("No Content");
        }
    }
}
