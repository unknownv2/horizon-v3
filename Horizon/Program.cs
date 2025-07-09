using System;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NoDev.Horizon.Classes;
using NoDev.XContent;

namespace NoDev.Horizon
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            RegisterXContentDevices();

            if (!IsValid || ArgumentHelper.ProcessArguments(args))
                return;

#if !DEBUG
            if (!Mutex.WaitOne(TimeSpan.Zero, true))
            {
                Common.NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_HZN_SHOW, IntPtr.Zero, IntPtr.Zero);
                return;
            }
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DevComponents.DotNetBar.MessageBoxEx.EnableGlass = false;

            if (Settings.GetBoolean("ShowTutorial"))
            {
                new Forms.Misc.Tutorial().ShowDialog();
                Settings.Set("ShowTutorial", false);
                Settings.Save();
            }

            Application.Run(new Main());

            Mutex.ReleaseMutex();
        }

        private static readonly Mutex Mutex = new Mutex(true, "49FFF65F-463F-41A3-BAF7-5541875B62F9");

        private static void RegisterXContentDevices()
        {
            XContentPackage.RegisterDevice(XContentVolumeType.STFS, typeof(Stfs.StfsDevice));
            XContentPackage.RegisterDevice(XContentVolumeType.SVOD, typeof(Svod.SvodDevice));
        }

        internal static Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        internal static bool IsValid
        {
            get
            {
#if !DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                    return false;
#endif

                var exAsm = Assembly.GetExecutingAssembly();

                if (!IsAssemblyValid(exAsm))
                    return false;

                var refAsm = exAsm.GetReferencedAssemblies();

                return refAsm.All(asmName => IsAssemblyValid(Assembly.Load(asmName.FullName)));
            }
        }

        private static readonly ulong[] AllowedTokens = new ulong[]
        {
            0x1079dc643ca4e1c6, // No Development Inc.
            0x89e03419565c7ab7, // Microsoft CLR
            0x3a0ad5117f5f3fb0, // Microsoft FX
            0x04de915ba3c3b77e, // DotNetBar
            0xedaea6b2e64fad30, // Newtonsoft
            0xec9b108c344b190f  // CBFS API
        };

        private static bool IsAssemblyValid(Assembly asm)
        {
            ulong token = StrongNameHelper.GetPublicKeyToken(asm.Location);

#if DEBUG
            if (token == AllowedTokens[0])
                return true;
#endif

            if (!AllowedTokens.Any(t => t == token))
                return false;

            if (!StrongNameHelper.ValidateSignature(asm.Location))
                return false;

            return true;
        }

        private static byte[] _firstMachineID;
        internal static byte[] MachineID
        {
            get
            {
                var tables = new[] {
                    new[] { "SerialNumber", "Win32_BIOS" },
                    new[] { "Product, SerialNumber", "Win32_BaseBoard" },
                    new[] { "DeviceID, UniqueId", "Win32_Processor" },
                    new[] { "DeviceID", "Win32_MotherboardDevice" }
                };
                var sha = new SHA1Managed();
                foreach (byte[] strHash in tables.Cast<object[]>().Select(table => new ManagementObjectSearcher(string.Format("SELECT {0} FROM {1}", table)).Get()).SelectMany(objs => objs.Cast<ManagementObject>().SelectMany(obj => obj.Properties.Cast<PropertyData>().Where(data => data.Value != null))).Select(data => Encoding.ASCII.GetBytes(data.Value.ToString()).ComputeSHA1()))
                    sha.TransformBlock(strHash, 0, strHash.Length, null, 0);
                byte[] procCount = BitConverter.GetBytes(Environment.ProcessorCount);
                sha.TransformFinalBlock(procCount, 0, procCount.Length);
                byte[] machineId = sha.Hash;
                sha.Clear();
                if (_firstMachineID == null)
                    _firstMachineID = machineId;
                else if (!_firstMachineID.SequenceEqual(machineId))
                    throw new Exception();
                return machineId;
            }
        }
    }

    public sealed class SecureCallbackMethodAttribute : Attribute { }
    public sealed class SecureMethodAttribute : Attribute { }
}
