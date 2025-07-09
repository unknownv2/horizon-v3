using System;
using System.Linq;
using DevComponents.DotNetBar;

namespace NoDev.Horizon.DeviceExplorer
{
    internal abstract class FatxItem : ButtonItem
    {
        internal FatxDevice Device { get; private set; }

        protected FatxItem(FatxDevice device)
        {
            this.Device = device;
        }

        protected const string LineBreak = "<br></br>";

        protected static string CreateGrayText(string text)
        {
            return String.Format("<font color=\"#7D7974\">{0}</font>", text);
        }

        private const string LoadingString = "Loading...";

        internal bool ContainsLoadingItem
        {
            get
            {
                return this.SubItems.Cast<BaseItem>().Any(item => item is ButtonItem && !item.Enabled && item.Text == LoadingString);
            }
        }

        internal void AddLoadingItem()
        {
            this.SubItems.AddDisabledButton(LoadingString);
        }

        internal void InsertDisabledButton(string text)
        {
            this.InsertItemAt(new ButtonItem { Text = text, Enabled = false }, this.SubItems.Count, false);
        }
    }
}
