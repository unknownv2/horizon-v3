using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoDev.Common.IO;
using NoDev.Horizon.Controls;
using NoDev.Horizon.DeviceExplorer;
using NoDev.Horizon.Editors.Package_Manager;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using DevComponents.DotNetBar;

namespace NoDev.Horizon
{
    internal partial class PackageEditor : BaseControl
    {
        internal PackageEditor()
        {
            InitializeComponent();
            this.cmdPackageManager.Click += cmdPackageManager_Click;

            var mi = ((Func<Task>)this.OpenAsync).GetMethodInfo();

            this._isAsync = mi != mi.GetBaseDefinition();

            if (this._isAsync)
            {
                mi = ((Action)this.Open).GetMethodInfo();

                if (mi != mi.GetBaseDefinition())
                    throw new Exception("You cannot override both Open and OpenAsync.");
            }
        }

        protected EndianIO IO;
        internal XContentPackage Package;

        private bool _keepAlive;
        private readonly bool _isAsync;

        private void cmdPackageManager_Click(object sender, EventArgs e)
        {
            ControlManager.Transfer(new ControlManager.TransferParameters(
                (EditorInfo)ControlManager.ControlInfoFromType(typeof(PackageManager)),
                this.Package, this));
        }

        protected override void OnFormLoad()
        {
            this.ChangeEnabledState(false);
        }

        protected virtual void ChangeEnabledState(bool enabled)
        {
            foreach (Control c in this.controlRibbon.Controls)
                if (c is RibbonPanel)
                    c.Enabled = enabled;

            foreach (BaseItem i in this.controlRibbon.Items)
                if (!(i is RibbonTabItem) && !(i is Office2007StartButton))
                    i.Enabled = enabled;

            this.cmdPackageManager.Enabled = enabled;
        }

        internal new EditorInfo Info
        {
            get { return (EditorInfo)base.Info; }
        }

        protected virtual void OnDeviceItemExpand(FatxDeviceItem item)
        {
            throw new NotImplementedException();
        }

        protected virtual void Open()
        {
            throw new NotImplementedException();
        }

        protected virtual Task OpenAsync()
        {
            throw new NotImplementedException();
        }

        protected virtual void Save()
        {
            throw new NotImplementedException();
        }

        private async void OpenFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "STFS Packages|*";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            await this.LoadFileNoExceptions(ofd.FileName);
        }

        private void cmdOpen_Click(object sender, EventArgs args)
        {
            if (!Main.HasDevices)
            {
                this.OpenFile();
                return;
            }

            this.cmdOpen.SubItems.Clear();
            foreach (FatxDevice dev in Main.Devices)
                this.cmdOpen.SubItems.Add(new FatxExpandableDeviceItem(dev, OnDeviceItemExpand));
            this.cmdOpen.SubItems.Add(new ActionButtonItem("Open Package...", Resources.Folder_Opened_16, OpenFile));
            this.cmdOpen.Expanded = true;
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (this.Package == null)
            {
                DialogBox.Show("You must load a package first!");
                return;
            }

            try
            {
                this.Save();

                if (this.IO != null)
                    this.IO.Flush();

                this.AfterSave();

                this.Package.Save();

                DialogBox.Show("Saved, rehashed, and resigned!", "Done");
            }
            catch(Exception ex)
            {
                DialogBox.ShowException(ex);
            }
        }

        protected virtual void AfterSave()
        {
            
        }

        protected virtual void CloseStreams()
        {
            
        }

        private void CloseStreamsPrivate()
        {
            if (this.Package != null)
                this.CloseStreams();

            if (this.IO != null)
            {
                this.IO.Flush();
                this.IO.Close();
                this.IO = null;
            }

            if (this.Package != null)
            {
                this.Package.SaveIfModified();
                this.Package.Close();
                this.Package = null;
            }
        }

        internal void CloseForTransfer()
        {
            this._keepAlive = true;
            this.Close();
        }

        protected override void OnFormClose(FormClosingEventArgs e)
        {
            if (this.Package == null)
                return;

            this.CloseStreams();

            this.Package.SaveIfModified();

            if (!this._keepAlive)
                this.Package.Close();
        }

        internal async Task<bool> LoadPackageNoExceptions(XContentPackage package)
        {
            this.CloseStreamsPrivate();

            try
            {
                package.Open();
                await this.LoadPackage(package);
                return true;
            }
            catch (Exception e)
            {
                this.CloseStreamsPrivate();
                this.ChangeEnabledState(false);
                DialogBox.ShowException(e);
                return false;
            }
        }

        protected virtual async Task LoadPackage(XContentPackage package)
        {
            this.Package = package;

            if (this._isAsync)
                await this.OpenAsync();
            else
                this.Open();

            this.ChangeEnabledState(true);
        }

        internal async Task<bool> LoadFileNoExceptions(string filename)
        {
            XContentPackage package;
            try
            {
                package = new XContentPackage(filename);
            }
            catch (Exception e)
            {
                DialogBox.ShowException(e);
                return false;
            }

            return await this.LoadPackageNoExceptions(package);
        }

        private void PackageEditor_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move;
        }

        private async void PackageEditor_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
                return;

            if (files.Length > 1)
                DialogBox.Show("You can only drop one file into this tool!", "Too many files", MessageBoxIcon.Error);
            else
                await this.LoadFileNoExceptions(files[0]);
        }
    }
}
