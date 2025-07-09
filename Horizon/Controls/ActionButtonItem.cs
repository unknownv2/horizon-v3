using System;
using System.Drawing;
using DevComponents.DotNetBar;

namespace NoDev.Horizon.Controls
{
    internal class ActionButtonItem : ButtonItem
    {
        private readonly Action _onClick;

        internal ActionButtonItem(string buttonText, Image buttonImage, Action onClick)
        {
            this.Text = buttonText;
            this.Image = buttonImage;
            this._onClick = onClick;
            this.Click += OpenFileButtonItem_Click;
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void OpenFileButtonItem_Click(object sender, EventArgs e)
        {
            this._onClick();
        }
    }
}
