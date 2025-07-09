using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace NoDev.Horizon
{
    static class Extensions
    {
        internal static Bitmap ToBitmap(this Control control)
        {
            IntPtr hwnd = control.Handle;
            var bitmap = new Bitmap(control.Size.Width, control.Size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                IntPtr bitmapHandle = g.GetHdc();
                NativeMethods.PrintWindow(hwnd, bitmapHandle, 0);
                g.ReleaseHdc(bitmapHandle);
            }
            return bitmap;
        }

        internal static void Invoke(this Control control, Action function)
        {
            if (control.InvokeRequired)
                control.Invoke(function);
            else
                function();
        }

        internal static Image ToImage(this byte[] buffer)
        {
            try
            {
                var ms = new MemoryStream(buffer);
                var img = Image.FromStream(ms);
                ms.Close();
                return img;
            }
            catch
            {
                return null;
            }
        }

        internal static string CreatePath(this DriveInfo driveInfo, string relativePath)
        {
            return driveInfo.Name + relativePath;
        }

        internal static void AddDisabledButton(this SubItemsCollection collection, string buttonText)
        {
            collection.Add(new ButtonItem { Text = buttonText, Enabled = false });
        }

        internal static void RemoveDisabledItems(this SubItemsCollection collection)
        {
            for (int x = 0; x < collection.Count; x++)
            {
                if (collection[x].Enabled)
                    continue;

                collection.RemoveAt(x);
                x--;
            }
        }
    }
}
