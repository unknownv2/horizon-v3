using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using DevComponents.DotNetBar;

namespace NoDev.Horizon
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ControlInfo : Attribute
    {
        internal Type Class;
        internal readonly uint ControlID;
        internal readonly string Title;
        internal readonly Bitmap Thumbnail;
        internal readonly Group Access;
        internal readonly ControlGroup Group;
        internal readonly bool Singleton;

        internal ControlInfo(uint controlId, string title, string thumbnailResourceName, Group access, ControlGroup controlGroup, bool singleton = false)
        {
            this.ControlID = controlId;
            this.Title = title;
            if (thumbnailResourceName != null)
                this.Thumbnail = (Bitmap)Resources.ResourceManager.GetObject(thumbnailResourceName);
            this.Access = access;
            this.Group = controlGroup;
            this.Singleton = singleton;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class EditorInfo : ControlInfo
    {
        internal readonly uint TitleID;

        internal EditorInfo(uint controlId, string title, string thumbnailResourceName, Group access, ControlGroup controlGroup, uint titleId = 0)
            : base(controlId, title, thumbnailResourceName, access, controlGroup)
        {
            this.TitleID = titleId;
        }
    }

    internal static class ControlManager
    {
        internal static readonly List<ControlInfo> Controls;
        internal static readonly List<EditorInfo> Editors;

        static ControlManager()
        {
            Controls = new List<ControlInfo>();
            Editors = new List<EditorInfo>();
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass && IsTypeOfControl(type, typeof(BaseControl)))
                {
                    var attr = type.GetCustomAttributes(typeof(ControlInfo), false);
                    if (attr.Length == 0 || !(attr[0] is ControlInfo))
                        continue;
                    var info = (ControlInfo)attr[0];
                    info.Class = type;
                    Controls.Add(info);
                    var editorInfo = info as EditorInfo;
                    if (editorInfo != null)
                        Editors.Add(editorInfo);
                }
            }
            Controls.Sort((c1, c2) => string.Compare(c1.Title, c2.Title, StringComparison.Ordinal));
        }

        internal static void ControlButtonClicked(object sender, EventArgs e)
        {
            CreateControl((ControlInfo)GetTag(sender));
        }

        internal static void TransferButtonClicked(object sender, EventArgs e)
        {
            object tag = GetTag(sender);

            if (tag == null)
                return;

            Transfer((TransferParameters)tag);
        }

        internal static async void Transfer(TransferParameters transParams)
        {
            if (transParams.Sender != null)
                transParams.Sender.CloseForTransfer();
            else if (Parent.HasChildren)
            {
                foreach (var frm in Parent.MdiChildren)
                {
                    var packageEditor = frm as PackageEditor;
                    if (packageEditor == null || packageEditor.Package != transParams.Package)
                        continue;

                    DialogBox.Show("This file is already open in a tool!", "Already Open", MessageBoxIcon.Warning);
                    packageEditor.WindowState = FormWindowState.Normal;
                    packageEditor.BringToFront();
                    packageEditor.Focus();
                    return;
                }
            }

            var newEditor = (PackageEditor)CreateControl(transParams.Editor);

            await newEditor.LoadPackageNoExceptions(transParams.Package);
        }

        private static object GetTag(object sender)
        {
            return sender is PopupItem ? ((PopupItem)sender).Tag : ((Control)sender).Tag;
        }

        private static bool IsTypeOfControl(Type controlType, Type isType)
        {
            while (controlType.BaseType != null)
            {
                if (controlType.BaseType == isType)
                    return true;
                controlType = controlType.BaseType;
            }
            return false;
        }

        internal static ControlInfo ControlInfoFromType(Type controlType)
        {
            return Controls.Find(c => c.Class == controlType);
        }

        private static Dictionary<ControlInfo, BaseControl> _singletons;

        internal static void UnregisterSingleton(ControlInfo info)
        {
            if (_singletons != null && _singletons.ContainsKey(info))
                _singletons.Remove(info);
        }

        internal static Form Parent;
        internal static BaseControl CreateControl(ControlInfo info)
        {
            if (info.Singleton && _singletons != null && _singletons.ContainsKey(info))
            {
                _singletons[info].BringToFront();
                _singletons[info].Focus();
                return _singletons[info];
            }

            var constructorInfo = info.Class.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            if (constructorInfo == null)
                return null;

            var control = (BaseControl)constructorInfo.Invoke(null);
            control.MdiParent = Parent;
            control.Info = info;
            control.Show();

            if (info.Singleton)
            {
                if (_singletons == null)
                    _singletons = new Dictionary<ControlInfo, BaseControl>();
                _singletons.Add(info, control);
            }

            return control;
        }

        internal class TransferParameters
        {
            internal readonly EditorInfo Editor;
            internal readonly XContentPackage Package;
            internal readonly PackageEditor Sender;

            internal TransferParameters(EditorInfo editor, XContentPackage package, PackageEditor sender = null)
            {
                this.Editor = editor;
                this.Package = package;
                this.Sender = sender;
            }
        }

        internal static void FillModButton(ButtonX b, XContentPackage p, PackageEditor sender = null)
        {
            var g = (GalleryContainer)b.SubItems[0];

            if (p.Header.Metadata.ContentType == XContentTypes.Profile)
            {
                bool wasOpened = p.IsOpened;

                p.Open();
                    
                if (!p.IsMounted)
                    p.Mount();

                var profileTools = Editors.FindAll(x => x.Group == ControlGroup.Profile);

                string formatPath = p.Drive.Name + "{0:X8}.gpd";

                g.SubItems.Clear();
                foreach (EditorInfo i in profileTools)
                    if (!i.Class.IsInstanceOfType(typeof(TitleSettingsEditor)) || File.Exists(string.Format(formatPath, i.TitleID)))
                        g.SubItems.Add(CreateGalleryItem(i, p, sender));

                if (!wasOpened)
                    p.Close();

                b.AutoExpandOnClick = true;
                b.Enabled = true;
                b.Image = Resources.Thumb_Generic_Dots;
            }
            else
            {
                uint titleId = TitleControl.GetProperTitleID(p);
                EditorInfo editor = Editors.FirstOrDefault(x => titleId == x.TitleID && x.Group == ControlGroup.Game);
                if (editor != null)
                {
                    b.Enabled = true;
                    b.Image = editor.Thumbnail;
                    b.Tag = new TransferParameters(editor, p, sender);
                    b.AutoExpandOnClick = false;
                }
                else
                {
                    b.Enabled = false;
                    b.Image = Resources.Thumb_QuestionMark;
                }
            }
        }

        private static ButtonItem CreateGalleryItem(EditorInfo e, XContentPackage p, PackageEditor sender)
        {
            var i = new ButtonItem();
            i.Click += TransferButtonClicked;
            i.ImagePosition = eImagePosition.Top;
            i.Text = e.Title;
            i.Image = e.Thumbnail;
            i.Tag = new TransferParameters(e, p, sender);
            return i;
        }
    }
}
