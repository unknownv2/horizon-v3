using System.Collections.Generic;
using System.IO;
using NoDev.Common.IO;

namespace NoDev.Fatx.Drives
{
    internal class LogicalDrive : Drive
    {
        private static string FormatLogicalPath(string rootDirectory, int dataFile)
        {
            return string.Format(@"{0}Xbox360\Data{1:D4}", rootDirectory, dataFile);
        }

        internal override void Close()
        {
            this.IO.Close();
            this.IO = null;
        }

        internal override bool IsValid
        {
            get { return this.IsMounted && this.IO != null; }
        }

        internal override bool IsMounted
        {
            get { return File.Exists(FormatLogicalPath(this.Name, 0)); }
        }

        internal LogicalDrive(DriveInfo driveInfo)
        {
            this.Name = driveInfo.Name;

            var filePaths = new List<string>();

            for (int x = 0; x > -1; x++)
            {
                var currentPath = FormatLogicalPath(this.Name, x);
                if (!File.Exists(currentPath))
                    break;
                filePaths.Add(currentPath);
            }

            if (filePaths.Count > 0)
            {
                this.IO = new MultiFileIO(filePaths, EndianType.Big);
                this.Length = IO.Length;
            }
        }
    }
}
