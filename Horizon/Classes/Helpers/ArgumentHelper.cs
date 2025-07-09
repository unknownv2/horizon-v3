using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NoDev.Odd;
using NoDev.XContent;

namespace NoDev.Horizon
{
    internal static class ArgumentHelper
    {
        internal static bool ProcessArguments(string[] args)
        {

#if DEBUG
            //Stfs.StfsDevice.InstallDriver();
            // THIS NEXT LINE IS FOR TESTING ONLY.
            // PLEASE DO NOT COMMIT IT UNCOMMENTED.
            // args = new string[] { "mount", "xcontent", @"C:\Users\Thierry\Desktop\derp.bin" };
#endif

            if (args == null)
                return false;

            if (args.Length == 0)
                return false;

            for (int x = 0; x < args.Length; x++)
            {
                // You never know...
                if (string.IsNullOrWhiteSpace(args[x]))
                    return false;
                args[x] = args[x].ToLower();
            }

            try
            {
                switch (args[0])
                {
                    case "mount":
                        ProcessMount(args);
                        break;
                    default:
                        throw new Exception("Invalid arguments passed.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(null, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        private static void ProcessMount(string[] args)
        {
            if (args.Length != 3)
                throw new Exception("Invalid number of arguments.");

            if (!File.Exists(args[2]))
                throw new Exception("File not found.");

            switch (args[1])
            {
                case "xcontent":
                    var package = new XContentPackage(args[2]);
                    package.Mount();
                    Process.Start(package.Drive.Name);
                    Thread.Sleep(Timeout.Infinite);
                    break;
                case "gdf":
                    var odd = new OddDevice(null, args[2]);
                    Process.Start(odd.MountPoint);
                    Thread.Sleep(Timeout.Infinite);
                    break;
                default:
                    throw new Exception("Invalid mountable device specified.");
            }
        }
    }
}
