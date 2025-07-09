using System;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace NoDev.Horizon
{
    internal static class DialogBox
    {
        internal static DialogResult Show(string message)
        {
            return Show(message, null, MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        internal static DialogResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        internal static DialogResult Show(string message, string title, MessageBoxIcon icon)
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, icon);
        }

        internal static DialogResult Show(string message, string title, MessageBoxButtons buttons)
        {
            return Show(message, title, buttons, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        internal static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            return Show(message, title, buttons, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        internal static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon)
        {
            return MessageBoxEx.Show(Main.GetInstance(), message, title, buttons, icon, defaultButton);
        }

        internal static void ShowException(Exception ex)
        {
            Show(ex.Message, "An Error Has Occured", MessageBoxIcon.Error);
        }
    }
}
