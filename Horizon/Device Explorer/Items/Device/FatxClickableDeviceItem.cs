using System;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxClickableDeviceItem : FatxDeviceItem
    {
        private readonly Action<FatxDevice> _onClick;

        internal FatxClickableDeviceItem(FatxDevice device, Action<FatxDevice> onClick)
            : base(device)
        {
            this._onClick = onClick;
            this.Click += FatxClickableDeviceItem_Click;
        }

        private void FatxClickableDeviceItem_Click(object sender, EventArgs e)
        {
            this._onClick(Device);
        }
    }
}
