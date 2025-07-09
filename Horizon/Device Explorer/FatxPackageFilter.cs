using System.Collections.Generic;
using NoDev.XContent;

namespace NoDev.Horizon.DeviceExplorer
{
    internal enum ProfileFilterType
    {
        PrivateOnly,
        PublicOnly,
        Custom,
        All
    }

    internal class FatxPackageFilter
    {
        internal uint TitleID;
        internal XContentTypes[] ContentTypes;

        private readonly List<ulong> _profileIds;

        internal FatxPackageFilter()
        {
            this._profileIds = new List<ulong>();
            this.ContentTypes = null;
        }

        internal void AddProfileID(ulong profileId)
        {
            this._profileIds.Add(profileId);
        }

        internal List<ulong> GetProfileIDs()
        {
            return this._profileIds;
        }

        internal void SetProfileFilterType(ProfileFilterType type, FatxDevice device)
        {
            this._profileIds.Clear();

            switch (type)
            {
                case ProfileFilterType.All:
                    this._profileIds.Add(0);
                    foreach (ProfileInfo profile in device.Profiles)
                        this._profileIds.Add(profile.ProfileID);
                    break;
                case ProfileFilterType.PrivateOnly:
                    foreach (ProfileInfo profile in device.Profiles)
                        this._profileIds.Add(profile.ProfileID);
                    break;
                case ProfileFilterType.PublicOnly:
                    this._profileIds.Add(0);
                    break;
            }
        }
    }
}
