#define VIRTUAL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using NoDev.Common;
using NoDev.Fatx.Drives;

namespace NoDev.Fatx
{
    internal class FatxDeviceService : ServiceBase
    {
        internal FatxDeviceService()
        {
            this.ServiceName = "FatxDeviceHost";
            this.CanHandlePowerEvent = false;
            this.CanHandleSessionChangeEvent = false;
            this.CanPauseAndContinue = true;
            this.CanShutdown = false;
            this.CanStop = true;
        }

        private bool _paused;
        private Thread _deviceMonitorThread;
        private volatile List<Drive> _loadedDrives;

        private readonly object _mutex = new object();

        private static readonly string MountPath = Win32.ProgramDataFolder + "\\mount.fatx";
        private static readonly string UnmountPath = Win32.ProgramDataFolder + "\\unmount.fatx";

        private static void BroadcastMount()
        {
            File.Create(MountPath).Close();
        }

        private static void BroadcastUnmount()
        {
            File.Create(UnmountPath).Close();
        }

        protected override void OnStart(string[] args)
        {
            this._loadedDrives = new List<Drive>();
            this.InitializeDeviceMonitor();
        }

#if !VIRTUAL
        [Conditional("DEBUG")]
        internal static void StartDebug()
        {
            Drive drive = new VirtualDrive(@"C:\san anadreas\", VirtualDrive.InputTypes.USBDirectory);
            
            if (drive.IsValid)
            {
                drive.MountVolumes();
                BroadcastMount();
            }

            Thread.Sleep(Timeout.Infinite);
        }
#else
        [Conditional("DEBUG")]
        internal static void StartDebug()
        {
            var deviceService = new FatxDeviceService();
            deviceService.OnStart(null);
            Thread.Sleep(Timeout.Infinite);
        }
#endif

        protected override void OnContinue()
        {
            this._paused = false;
        }

        protected override void OnPause()
        {
            this._paused = true;
        }

        protected override void OnStop()
        {
            if (this._deviceMonitorThread != null && this._deviceMonitorThread.IsAlive)
                this._deviceMonitorThread.Abort();

            int loadedDrivesCount = this._loadedDrives.Count;
            lock (this._mutex)
            {
                while (this._loadedDrives.Count != 0)
                {
                    this._loadedDrives[0].UnmountVolumes();
                    this._loadedDrives.RemoveAt(0);
                }
            }
            if (loadedDrivesCount != this._loadedDrives.Count)
                BroadcastUnmount();
        }

        private void InitializeDeviceMonitor()
        {
            this._deviceMonitorThread = new Thread(DeviceMonitor);
            this._deviceMonitorThread.IsBackground = true;
            this._deviceMonitorThread.Start();
        }

        private void DeviceMonitor()
        {
            this._loadedDrives = new List<Drive>();

            this.MountNewDrives();

            var watcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent"));
            watcher.EventArrived += DeviceChangeEvent;
            watcher.Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private long _lastEventTime;
        private void DeviceChangeEvent(object sender, EventArrivedEventArgs e)
        {
            if (this._paused)
                return;

            var triggerType = (ushort)e.NewEvent.Properties["EventType"].Value;
            if ((triggerType != 2 && triggerType != 3) || DateTime.Now.ToFileTime() < this._lastEventTime + 100000)
                return;

            if (triggerType == 2)
            {
                for (int x = 0; x < 5; x++)
                {
                    var numDrives = this._loadedDrives.Count;
                    this.MountNewDrives();
                    if (numDrives != this._loadedDrives.Count)
                        break;
                    Thread.Sleep(10);
                }
            }
            else
            {
                this.RemoveEjectedDrives();
            }

            this._lastEventTime = DateTime.Now.ToFileTime();
        }

        private void MountNewDrives()
        {
            Parallel.ForEach(GetFatxDrives(), MountDrive);
        }

        private void MountDrive(Drive drive)
        {
            drive.MountVolumes();

            lock (this._mutex)
            {
                this._loadedDrives.Add(drive);
            }

            BroadcastMount();
        }

        private void RemoveEjectedDrives()
        {
            lock (this._mutex)
            {
                int driveCount = this._loadedDrives.Count;
                for (int x = 0; x < this._loadedDrives.Count; x++)
                {
                    if (this._loadedDrives[x].IsMounted)
                        continue;

                    this._loadedDrives[x].UnmountVolumes();
                    this._loadedDrives.RemoveAt(x);

                    x--;
                }
                if (driveCount != this._loadedDrives.Count)
                    BroadcastUnmount();
            }
        }

        private bool IsDriveLoaded(string driveName)
        {
            lock (this._mutex)
            {
                return this._loadedDrives.Any(drive => driveName == drive.Name);
            }
        }

        private IEnumerable<Drive> GetFatxDrives()
        {
            var fatxDrives = new List<Drive>();
            var logicalDrives = DriveInfo.GetDrives();

            foreach (var logicDriveInfo in logicalDrives)
            {
                if (!logicDriveInfo.IsReady || this.IsDriveLoaded(logicDriveInfo.Name))
                    continue;

                try
                {
                    var logicDrive = new LogicalDrive(logicDriveInfo);
                    if (logicDrive.IsValid)
                    {
                        logicDrive.MountVolumes();
                        if (logicDrive.VolumesMounted)
                            fatxDrives.Add(logicDrive);
                    }
                }
                catch
                {

                }
            }

            ManagementObjectCollection physicalDisks = PhysicalDrive.GetPhysicalDisks();

            foreach (ManagementObject physicalDisk in physicalDisks)
            {
                if (this.IsDriveLoaded((string)physicalDisk["Name"]))
                    continue;

                try
                {
                    var physicalDrive = new PhysicalDrive((string)physicalDisk["Name"], (ulong)physicalDisk["Size"]);
                    if (physicalDrive.IsValid)
                    {
                        physicalDrive.MountVolumes();
                        if (physicalDrive.VolumesMounted)
                            fatxDrives.Add(physicalDrive);
                    }
                }
                catch
                {

                }
            }

            return fatxDrives;
        }
    }
}
