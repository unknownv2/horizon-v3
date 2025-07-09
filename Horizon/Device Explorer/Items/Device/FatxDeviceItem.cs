using NoDev.Horizon.Properties;

namespace NoDev.Horizon.DeviceExplorer
{
    internal abstract class FatxDeviceItem : FatxItem
    {
        internal FatxDeviceItem(FatxDevice device)
            : base(device)
        {
            this.SetData();
        }

        private void SetData()
        {
            this.Text = Device.Name + LineBreak + "Storage Device";
            this.Image = Resources.FatxHDD_24;
        }
    }
}
