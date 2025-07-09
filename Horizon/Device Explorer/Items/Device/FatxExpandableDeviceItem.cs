using System;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxExpandableDeviceItem : FatxDeviceItem
    {
        private readonly Action<FatxDeviceItem> _onExpand;

        internal FatxExpandableDeviceItem(FatxDevice device, Action<FatxDeviceItem> onExpand)
            : base(device)
        {
            this._onExpand = onExpand;
            this.ExpandChange += FatxExpandableDeviceItem_ExpandChange;
            this.AddLoadingItem();
        }

        private void FatxExpandableDeviceItem_ExpandChange(object sender, EventArgs e)
        {
            this.SubItems.Clear();
            if (this.Expanded)
                this._onExpand(this);
            else
                this.AddLoadingItem();
        }
    }
}
