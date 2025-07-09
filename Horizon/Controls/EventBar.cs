using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace NoDev.Horizon.Controls
{
    [ToolboxItem(false)]
    sealed class EventBar : RibbonBar
    {
        private static int _lastX;
        internal EventBar(ControlInfo controlInfo)
            : this(controlInfo.Title, controlInfo.Thumbnail, ControlManager.ControlButtonClicked, controlInfo)
        {
            
        }

        internal EventBar(string title, Image thumbnail, Action<object, EventArgs> onButtonClick)
            : this(title, thumbnail, onButtonClick, null)
        {

        }

        internal EventBar(string title, Image thumbnail, Action<object, EventArgs> onButtonClick, object tag)
        {
            this.LicenseKey = Main.LicenseKey;
            this.AutoOverflowEnabled = false;
            this.Dock = DockStyle.Left;
            this.MinimumSize = new Size(138, 0);
            this.MaximumSize = new Size(138, 69);
            this.Location = new Point(_lastX++, 0);
            this.Text = title;

            ButtonItem controlButton = new ButtonItem();
            controlButton.Shape = new RoundRectangleShapeDescriptor();
            controlButton.ColorTable = eButtonColor.OrangeWithBackground;
            controlButton.Click += new EventHandler(onButtonClick);
            controlButton.Image = thumbnail;
            controlButton.Tag = tag;
            this.Items.Add(controlButton);
        }
    }
}
