using System;
using System.ComponentModel;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace NoDev.Horizon
{
    internal enum ControlGroup
    {
        Hidden,
        Game,
        Profile,
        Tool,
        Misc
    }

    [Flags]
    internal enum Group
    {
        Guest = 0x01,
        Regular = 0x02,
        Diamond = 0x04,
        Tester = 0x08,
        Developer = 0x10,

        All = -1
    }

    [ToolboxItem(false)]
    internal partial class BaseControl : Office2007RibbonForm
    {
        internal BaseControl()
        {
            InitializeComponent();
            this.Load += BaseControl_Load;
            this.FormClosing += BaseControl_FormClosing;
        }

        internal ControlInfo Info;

        private bool _panelsEnabled;
        internal virtual bool PanelsEnabled
        {
            get
            {
                return this._panelsEnabled;
            }
            set
            {
                this._panelsEnabled = value;
                foreach (Control control in this.controlRibbon.Controls)
                    if (control is RibbonPanel)
                        control.Enabled = value;
            }
        }

        private void BaseControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
                return;

            try
            {
                this.OnFormClose(e);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                DialogBox.ShowException(ex);
                return;
            }

            if (!e.Cancel)
                ControlManager.UnregisterSingleton(Info);
        }

        protected virtual void OnFormClose(FormClosingEventArgs e)
        {

        }

        private void BaseControl_Load(object sender, EventArgs e)
        {
            this.OnFormLoad();
        }

        protected virtual void OnFormLoad()
        {

        }
    }
}
