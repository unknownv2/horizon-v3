using System;
using NoDev.Horizon.Properties;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxTitleItem : FatxItem
    {
        internal readonly uint TitleID;

        internal FatxTitleItem(FatxDevice device, uint titleId, Action<FatxTitleItem> onExpand)
            : base(device)
        {
            this.TitleID = titleId;

            this.Image = Resources.Dot_Gray;

            this.PopupShowing += delegate { onExpand(this); };

            this.AddLoadingItem();

            this.Text = TitleNameCache.GetTitleName(titleId);
        }
    }
}
