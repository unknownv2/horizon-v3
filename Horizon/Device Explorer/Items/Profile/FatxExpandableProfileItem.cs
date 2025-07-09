using System;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxExpandableProfileItem : FatxProfileItem
    {
        internal FatxExpandableProfileItem(FatxDevice device, ProfileInfo profile, Action<FatxProfileItem> onExpand)
            : base(device, profile)
        {
            this.PopupOpen += delegate { onExpand(this); };
            this.AddLoadingItem();
        }
    }
}
