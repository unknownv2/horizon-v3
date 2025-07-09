using System;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxClickableProfileItem : FatxProfileItem
    {
        private readonly Action<FatxProfileItem> _onClick;

        internal FatxClickableProfileItem(FatxDevice device, ProfileInfo profile, Action<FatxProfileItem> onClick)
            : base(device, profile)
        {
            this._onClick = onClick;
            this.Click += delegate { this._onClick(this); };
        }
    }
}
