using System;
using System.Threading.Tasks;
using NoDev.Horizon.Properties;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxContentItem : FatxItem
    {
        internal readonly GeneralContentType GeneralContentType;

        internal FatxContentItem(FatxDevice device, GeneralContentType generalContentType, Action<FatxContentItem> onExpand)
            : base(device)
        {
            this.GeneralContentType = generalContentType;
            this.Image = Resources.Folder_Closed_16;

            this.PopupShowing += delegate { onExpand(this); };

            this.AddLoadingItem();

            this.Text = GeneralContentType.ToString().Replace("_", " ");
        }
    }
}
