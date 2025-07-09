using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NoDev.Common;
using NoDev.Horizon.DeviceExplorer;
using NoDev.Horizon.Editors.Package_Manager;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using DevComponents.AdvTree;

namespace NoDev.Horizon
{
    using FatxFileInfo = Tuple<string, string, uint>;
    using FileList = List<Tuple<string, string, uint>>;

    partial class Main
    {
        private string _extractMessage;

        private void InitializeDeviceExplorer()
        {
            this._extractMessage = cmdFatxExtract.Text;

            listFatx.BeforeExpand += FatxNode.Event_BeforeExpand;
            listFatx.BeforeNodeInsert += FatxNode.Event_BeforeNodeInsert;
            listFatx.BeforeCellEdit += FatxNode.Event_BeforeCellEdit;
            listFatx.AfterCellEdit += FatxNode.Event_AfterCellEdit;

            listFatx.AfterNodeSelect += this.Event_AfterNodeSelect;
            listFatx.NodeDoubleClick += this.Event_NodeDoubleClick;

            this._contextMenu = CreateContextMenu();

            this.SetContextMenuEnabled(false);

            listFatx.ContextMenu = this._contextMenu;

            cmdFatxMod.Click += ControlManager.TransferButtonClicked;
            cmdFatxGear.Click += ControlManager.TransferButtonClicked;

            cmdFatxExtract.Click += Event_MenuExtract_Click;
            cmdFatxInject.Click += Event_MenuInject_Click;

            _mountWatcher = new FileSystemWatcher(Win32.ProgramDataFolder, "*.fatx");
            _mountWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _mountWatcher.Changed += _fatxWatcher_Changed;
            _mountWatcher.EnableRaisingEvents = true;

            ThreadPool.QueueUserWorkItem(threadObj => this.OnDeviceMount());
        }

        private void _fatxWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "mount.fatx")
                this.OnDeviceMount();
            else if (e.Name == "unmount.fatx")
                this.rbFatx.Invoke(this.OnDeviceUnmount);
        }

        private FileSystemWatcher _mountWatcher;
        private ContextMenu _contextMenu;

        internal static volatile List<FatxDevice> Devices = new List<FatxDevice>();

        internal static bool HasDevices
        {
            get { return Devices.Count != 0; }
        }

        private static bool DeviceLoaded(DriveInfo driveInfo)
        {
            return Devices.Any(t => driveInfo.Name == t.Drive.Name);
        }

        private void SelectAndExpandFirstNode()
        {
            this.listFatx.SelectedNode = this.listFatx.Nodes[0];
            this.listFatx.Nodes[0].Expand();
        }

        private void UpdateDevicesLoadedButton()
        {
            this.cmdFatxDevicesLoaded.Text = string.Format("{0} Device{1} Loaded", Devices.Count, Devices.Count == 1 ? "" : "s");
        }

        private void OnDeviceMount()
        {
            this.progressFatx.Invoke(() => {
                this.progressFatx.Style = ProgressBarStyle.Marquee;
            });
            var logicalDrives = DriveInfo.GetDrives();
            foreach (var driveInfo in logicalDrives)
            {
                if (!driveInfo.IsReady || driveInfo.DriveFormat != "FATX" || DeviceLoaded(driveInfo))
                    continue;
                var fatxDevice = new FatxDevice(driveInfo);
                Devices.Add(fatxDevice);
                this.listFatx.Invoke(() => {
                    this.listFatx.Nodes.Add(new FatxDeviceNode(fatxDevice));
                    this.UpdateDevicesLoadedButton();
                    if (this.listFatx.Nodes.Count == 1)
                        this.SelectAndExpandFirstNode();
                    this.SetContextMenuEnabled(true);
                    this.rbFatx.Refresh();
                    exFatx.Expanded = true;
                });
            }
            this.progressFatx.Invoke(() => {
                this.progressFatx.Style = ProgressBarStyle.Blocks;
            });
        }

        private void OnDeviceUnmount()
        {
            int startCount = Devices.Count;

            for (int x = 0; x < Devices.Count; x++)
            {
                var devNode = (FatxDeviceNode)this.listFatx.Nodes[x];
                if (!devNode.Device.Drive.RootDirectory.Exists)
                {
                    Devices.Remove(devNode.Device);
                    this.listFatx.Nodes.RemoveAt(x);
                    x--;
                }
            }

            if (startCount == Devices.Count)
                return;

            this.UpdateDevicesLoadedButton();

            if (Devices.Count != 0)
                this.SelectAndExpandFirstNode();
            else
            {
                this.SetContextMenuEnabled(false);
                this.tabFatxDrive.Text = "Connect a Device";
            }

            this.rbFatx.Refresh();
        }

        private void SetContextMenuEnabled(bool enabled)
        {
            foreach (MenuItem x in this._contextMenu.MenuItems)
                x.Enabled = enabled;
        }

        private void Event_NodeDoubleClick(object sender, TreeNodeMouseEventArgs e)
        {
            if (e.Node == null || !(e.Node is FatxNode))
                return;

            cmdFatxGear.PerformClick();
        }

        private static readonly bool[,] UIMap =
        {
            { true, true, false, false, false, true, true, true, false }, // FatxDeviceNode
            { true, true, false, true, false, true, false, true, true },  // FatxContentNode
            { true, true, true, true, false, true, false, true, true },   // FatxTitleNode
            { true, true, true, true, true, false, false, false, true },  // FatxPackageNode
            { true, true, true, true, false, false, false, false, true },  // FatxPackageNode (Readonly)
            { true, true, true, true, false, false, false, false, true }  // FatxProfileNode
        };

        private void Event_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            var node = listFatx.SelectedNode as FatxNode;

            if (node is FatxPackageNode && !File.Exists(((FatxPackageNode)node).Package.Filename))
            {
                DialogBox.Show("This package no longer exists on your device.");
                var fatxProfileNode = node as FatxProfileNode;
                if (fatxProfileNode != null)
                    node.Device.Profiles.Remove(fatxProfileNode.ProfileInfo);
                if (node.Parent.Nodes.Count == 1)
                    node.Parent.Nodes.Add(FatxNode.NoContentNode);
                node.Remove();
                node = null;
            }

            if (node == null)
            {
                cmdFatxExtract.Text = this._extractMessage;
                cmdFatxExtract.Enabled = false;
                cmdFatxInject.Enabled = false;
                cmdFatxGear.Enabled = false;

                cmdFatxMod.Enabled = false;
                cmdFatxMod.Image = Resources.Thumb_QuestionMark;

                foreach (MenuItem i in this._contextMenu.MenuItems)
                    i.Enabled = false;

                listFatx.ContextMenu = null;
                
                return;
            }

            cmdFatxExtract.Text = listFatx.SelectedNodes.Count == 1 ? node.ExtractText : "Extract Selected Content";

            int contextIndex;

            if (node is FatxPackageNode && listFatx.SelectedNodes.Count == 1)
            {
                var packageNode = node as FatxPackageNode;

                contextIndex = node is FatxProfileNode ? 5 : (packageNode.Package.IsReadOnly ? 4 : 3);

                this._menuExtract.Text = "Extract File...";

                cmdFatxGear.Enabled = true;
                cmdFatxMod.Enabled = true;

                ControlManager.FillModButton(this.cmdFatxMod, packageNode.Package);
                cmdFatxGear.Tag = new ControlManager.TransferParameters((EditorInfo)ControlManager.ControlInfoFromType(typeof(PackageManager)), packageNode.Package);
            }
            else
            {
                cmdFatxGear.Enabled = false;
                cmdFatxMod.Enabled = false;
                cmdFatxMod.Image = Resources.Thumb_QuestionMark;
                if (node is FatxPackageNode)
                {
                    contextIndex = node is FatxProfileNode ? 5 : (((FatxPackageNode)node).Package.IsReadOnly ? 4 : 3);
                    this._menuExtract.Text = "Extract Files...";
                }
                else
                {
                    this._menuExtract.Text = "Extract Content...";
                    if (node is FatxDeviceNode)
                        contextIndex = 0;
                    else if (node is FatxContentNode)
                        contextIndex = 1;
                    else if (node is FatxTitleNode)
                        contextIndex = 2;
                    else
                        throw new Exception(string.Format("Unknown FatxNode type ({0}).", node.GetType()));
                }
            }

            this.tabFatxDrive.Text = node.Device.Name;

            this._menuExtract.Visible = UIMap[contextIndex, 0];
            this.cmdFatxExtract.Enabled = UIMap[contextIndex, 0];

            this._menuInject.Visible =  UIMap[contextIndex, 1];
            this.cmdFatxInject.Enabled = UIMap[contextIndex, 1];

            this._menuDelete.Visible = UIMap[contextIndex, 2];
            this._menuCopy.Visible = UIMap[contextIndex, 3];
            this._menuChangeOwner.Visible = UIMap[contextIndex, 4];
            this._menuDivider.Visible = UIMap[contextIndex, 5];
            this._menuRenameDevice.Visible = UIMap[contextIndex, 6];
            this._menuCollapseAll.Visible = UIMap[contextIndex, 7];

            listFatx.MultiSelect = UIMap[contextIndex, 8];

            listFatx.ContextMenu = this._contextMenu;
        }

        private MenuItem
            _menuExtract,
            _menuInject,
            _menuDelete,
            _menuCopy,
            _menuChangeOwner,
            _menuDivider,
            _menuRenameDevice,
            _menuCollapseAll;

        private ContextMenu CreateContextMenu()
        {
            this._menuExtract = new MenuItem("Extract File...", Event_MenuExtract_Click, Shortcut.CtrlE);
            this._menuInject = new MenuItem("Inject File...", Event_MenuInject_Click, Shortcut.CtrlI);
            this._menuDelete = new MenuItem("Delete", Event_MenuDelete_Click, Shortcut.Del);
            this._menuCopy = new MenuItem("Copy To Device");
            this._menuChangeOwner = new MenuItem("Change Owner");
            this._menuDivider = new MenuItem("-");
            this._menuRenameDevice = new MenuItem("Rename Device...", Event_MenuRenameDevice_Click, Shortcut.CtrlR);
            this._menuCollapseAll = new MenuItem("Collapse All", Event_MenuCollapseAll_Click);

            this._menuCopy.MenuItems.Add(new MenuItem("No Devices Found"));
            this._menuCopy.Popup += Event_MenuCopy_Popup;

            this._menuChangeOwner.MenuItems.Add(new MenuItem("No Profiles Found"));
            this._menuChangeOwner.Popup += Event_MenuChangeOwner_Popup;

            var menu = new ContextMenu();
            menu.MenuItems.Add(this._menuExtract);
            menu.MenuItems.Add(this._menuInject);
            menu.MenuItems.Add(this._menuDelete);
            menu.MenuItems.Add(this._menuCopy);
            menu.MenuItems.Add(this._menuChangeOwner);
            menu.MenuItems.Add(this._menuDivider);
            menu.MenuItems.Add(this._menuRenameDevice);
            menu.MenuItems.Add(this._menuCollapseAll);
            return menu;
        }

        private static void ClearMenuItems(Menu.MenuItemCollection c)
        {
            c[0].Visible = true;
            while (c.Count != 1)
                c.RemoveAt(1);
        }

        private void Event_MenuCopy_Popup(object sender, EventArgs e)
        {
            ClearMenuItems(this._menuCopy.MenuItems);

            var node = listFatx.SelectedNode as FatxPackageNode;

            if (node == null || Devices.Count == 1)
                return;

            this._menuCopy.MenuItems[0].Visible = false;

            foreach (FatxDeviceNode n in listFatx.Nodes)
            {
                if (node.Device == n.Device)
                    continue;

                var m = new MenuItem(n.Device.Name);
                m.Tag = n.Device;
                m.Click += Event_MenuCopyItem_Click;
                this._menuCopy.MenuItems.Add(m);
            }
        }

        private void Event_MenuChangeOwner_Popup(object sender, EventArgs e)
        {
            ClearMenuItems(this._menuChangeOwner.MenuItems);

            var node = listFatx.SelectedNode as FatxPackageNode;

            var favs = ProfileCache.GetFavorites();

            if (node == null || favs.Count == 0)
                return;

            this._menuChangeOwner.MenuItems[0].Visible = false;

            foreach (var profile in favs)
            {
                var m = new MenuItem(string.Format("{0} - {1:X16}", profile.Gamertag, profile.ProfileID));
                m.Tag = profile;
                m.Click += Event_MenuChangeOwnerItem_Click;
                this._menuChangeOwner.MenuItems.Add(m);
            }
        }

        private async void Event_MenuChangeOwnerItem_Click(object sender, EventArgs e)
        {
            var node = listFatx.SelectedNode as FatxPackageNode;

            if (node == null)
                return;

            progressFatx.Maximum = listFatx.SelectedNodes.Count;

            progressFatx.Value = 0;

            progressFatx.Style = listFatx.SelectedNodes.Count == 1 ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;

            var profile = (ProfileData)((MenuItem)sender).Tag;

            foreach (FatxPackageNode packageNode in listFatx.SelectedNodes)
            {
                progressFatx.Value++;

                var meta = packageNode.Package.Header.Metadata;

                ulong originalCreator = meta.Creator;

                meta.Creator = profile.ProfileID;

                string newFilePath = packageNode.Package.FormatFATXDevicePath();

                if (!File.Exists(newFilePath) ||
                    DialogBox.Show(packageNode.Package.Header.Metadata.DisplayName + 
                        " already exists for the target profile. Do you want to overwrite it?",
                        "Overwrite?", MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button3,
                        MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    try
                    {
                        await packageNode.Package.Move(newFilePath);
                    }
                    catch (Exception ex)
                    {
                        meta.Creator = originalCreator;
                        DialogBox.ShowException(ex);
                        continue;
                    }
                }

                packageNode.Package.Save();
                packageNode.Package.Close();

                packageNode.UpdateCells();
            }

            progressFatx.Value = 0;

            progressFatx.Style = ProgressBarStyle.Blocks;

            if (listFatx.SelectedNodes.Count == 1)
                DialogBox.Show("The owner of this package has been changed to " + profile.Gamertag + "!");
            else
                DialogBox.Show("The owners of these packages have been changed to " + profile.Gamertag + "!");
        }

        private void Event_MenuCollapseAll_Click(object sender, EventArgs e)
        {
            var node = listFatx.SelectedNode;

            if (node == null)
                return;

            node.CollapseAll();
        }

        private void Event_MenuRenameDevice_Click(object sender, EventArgs e)
        {
            var node = listFatx.SelectedNode;

            if (node == null)
                return;

            ToParent(ref node);

            if (!(node is FatxDeviceNode))
                return;

            node.BeginEdit();
        }

        private void Event_MenuDelete_Click(object sender, EventArgs e)
        {
            var node = listFatx.SelectedNode as FatxPackageNode;

            if (node == null)
                return;

            if (DialogBox.Show("Are you sure you want to delete this content?", "Confirm", MessageBoxIcon.Question) != DialogResult.OK)
                return;

            var files = new List<string>(listFatx.SelectedNodes.Count);

            foreach (FatxPackageNode packageNode in listFatx.SelectedNodes)
            {
                string srcFile = packageNode.Package.Filename;

                if (File.Exists(srcFile))
                    files.Add(srcFile);

                string dataDir = srcFile + ".data";

                if (Directory.Exists(dataDir))
                {
                    files.AddRange(Directory.GetFiles(dataDir));
                    files.Add(dataDir);
                }
            }

            if (files.Count == 0)
                throw new Exception("The content could not be found on your device!");

            var opHelper = new FileOperationHelper(this.Handle, NativeMethods.FOF_NOCONFIRMATION 
                | NativeMethods.FOF_NOERRORUI | NativeMethods.FOF_SIMPLEPROGRESS, "Deleting content...");

            opHelper.Delete(files.ToArray());
        }

        private void Event_MenuInject_Click(object sender, EventArgs e)
        {
            var node = listFatx.SelectedNode;

            if (node == null)
                return;

            ToParent(ref node);

            var devNode = node as FatxDeviceNode;

            if (devNode == null)
                return;

            var res = DialogBox.Show("Do you want to inject an entire folder?\n\nPress \"No\" if you only have one file.", 
                "Inject folder?", MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button2, MessageBoxIcon.Question);

            string[] filePaths;
            switch (res)
            {
                case DialogResult.Yes:
                    var fbd = new FolderBrowserDialog();
                    fbd.ShowNewFolderButton = true;

                    if (fbd.ShowDialog() != DialogResult.OK)
                        return;

                    filePaths = Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories);
                    break;
                case DialogResult.No:
                    var ofd = new OpenFileDialog();
                    ofd.Multiselect = true;

                    ofd.Filter = "XContent Packages|*";

                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;

                    filePaths = ofd.FileNames;
                    break;
                default:
                    return;
            }

            var files = new FileList(filePaths.Length);

            foreach (string fileName in filePaths)
            {
                if (!IsValidDriveLetter(fileName[0]))
                    throw new Exception("Only local paths are supported by this operation.");

                XContentPackage package;
                try
                {
                    package = new XContentPackage(fileName);
                }
                catch
                {
                    continue;
                }

                files.Add(new FatxFileInfo(
                    package.Filename, 
                    devNode.Device.Drive.Name + package.FormatFATXDevicePath(true), 
                    package.Header.Metadata.DataFiles
                ));

                package.Close();

                if (filePaths.Length == 1)
                {
                    this.CopyFiles("Injecting " + package.Header.Metadata.DisplayName + "...", files);
                    return;
                }
            }

            this.CopyFiles("Injecting content...", files);
        }

        private void Event_MenuCopyItem_Click(object sender, EventArgs e)
        {
            var to = (FatxDevice)((MenuItem)sender).Tag;

            if (listFatx.SelectedNodes.Count == 0)
                return;

            var files = new FileList(listFatx.SelectedNodes.Count);

            foreach (FatxPackageNode node in this.listFatx.SelectedNodes)
            {
                string fileName = node.Package.Filename;
                string newPath = to.Drive.Name + fileName.Substring(fileName.IndexOf('\\') + 1);
                files.Add(new FatxFileInfo(fileName, newPath, node.Package.Header.Metadata.DataFiles));
            }

            string title;
            if (listFatx.SelectedNodes.Count == 1)
                title = "Moving " + ((FatxPackageNode)listFatx.SelectedNodes[0]).Package.Header.Metadata.DisplayName + "...";
            else
                title = "Moving content...";

            this.CopyFiles(title, files);
        }

        private async void Event_MenuExtract_Click(object sender, EventArgs e)
        {
            if (listFatx.SelectedNode == null)
                return;

            var selectedNode = (FatxNode)listFatx.SelectedNode;

            if (listFatx.SelectedNodes.Count == 1 && selectedNode is FatxPackageNode)
            {
                ExtractPackage((FatxPackageNode)selectedNode);
                return;
            }

            var fbd = new FolderBrowserDialog();
            fbd.Description = "Select a destination folder.";

            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            if (!IsValidDriveLetter(fbd.SelectedPath[0]))
                throw new Exception("Only local paths are supported by this operation.");

            var selectedNodes = new List<FatxNode>(listFatx.SelectedNodes.Cast<FatxNode>());

            this.progressFatx.Style = ProgressBarStyle.Marquee;

            var packages = new List<XContentPackage>();

            if (selectedNode is FatxPackageNode)
                packages.AddRange(selectedNodes.Where(node => node != null).Select(node => ((FatxPackageNode) selectedNode).Package));
            else
                foreach (var node in selectedNodes)
                    await node.GetPackages(packages);

            if (packages.Count == 0)
            {
                this.progressFatx.Style = ProgressBarStyle.Blocks;
                DialogBox.Show("There's no content to extract.", "No Content");
                return;
            }

            string basePath = fbd.SelectedPath;

            var files = new FileList(packages.Count);
                
            foreach (var p in packages)
            {
                if (!File.Exists(p.Filename))
                    continue;

                var meta = p.Header.Metadata;

                uint titleId = meta.ExecutionId.TitleID;

                string destinationPath = string.Format(
                    @"{0}\{1} ({2:X16})\{3} ({4:X8})\{5}\{6}",
                    basePath,
                    meta.Creator == 0 ? "Public Content" : ProfileCache.Contains(meta.Creator) ? ProfileCache.GetProfile(meta.Creator).Gamertag : "Unknown",
                    meta.Creator,
                    TitleNameCache.Contains(titleId) ? TitleNameCache.GetTitleName(titleId) : "Game",
                    titleId,
                    ContentTypeHelper.ContentTypeToPluralString(meta.ContentType),
                    Path.GetFileName(p.Filename)
                );

                files.Add(new FatxFileInfo(p.Filename, destinationPath, p.Header.Metadata.DataFiles));
            }

            this.progressFatx.Style = ProgressBarStyle.Blocks;
            this.CopyFiles("Extracting content...", files);
        }

        private void ExtractPackage(FatxPackageNode node)
        {
            var sfd = new SaveFileDialog();

            sfd.FileName = Path.GetFileName(node.Package.Filename);

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            if (!IsValidDriveLetter(sfd.FileName[0]))
                throw new Exception("Only local paths are supported by this operation.");

            var package = node.Package;

            var files = new FileList(listFatx.SelectedNodes.Count);

            files.Add(new FatxFileInfo(package.Filename, sfd.FileName, package.Header.Metadata.DataFiles));

            this.CopyFiles("Extracting " + package.Header.Metadata.DisplayName + "...", files);
        }

        private void CopyFiles(string titleText, FileList files)
        {
            var srcFiles = new List<string>(files.Count);
            var destFiles = new List<string>(files.Count);

            foreach (var file in files)
            {
                if (file.Item1[0] == file.Item2[0])
                    throw new Exception("The destination device cannot be the same as the source device.");

                srcFiles.Add(file.Item1);
                destFiles.Add(file.Item2);

                if (file.Item3 == 0)
                    continue;

                string srcDataDir = file.Item1 + ".data";
                string destDataDir = file.Item2 + ".data";

                for (int x = 0; x < file.Item3; x++)
                {
                    string append = string.Format(@"\Data{0:D4}", x);
                    string srcDataFile = srcDataDir + append;
                    if (!File.Exists(srcDataFile))
                        throw new FileNotFoundException(string.Format("Missing data file {0}.", x));
                    srcFiles.Add(srcDataFile);
                    destFiles.Add(destDataDir + append);
                }
            }

            var opHelper = new FileOperationHelper(this.Handle, NativeMethods.FOF_MULTIDESTFILES 
                | NativeMethods.FOF_NOCONFIRMMKDIR | NativeMethods.FOF_RENAMEONCOLLISION 
                | NativeMethods.FOF_SIMPLEPROGRESS, titleText);

            opHelper.Copy(srcFiles.ToArray(), destFiles.ToArray());
        }

        private static bool IsValidDriveLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private static void ToParent(ref Node node)
        {
            while (true)
            {
                if (node.Parent == null)
                    break;
                node = node.Parent;
            }
        }
    }
}
