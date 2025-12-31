using System;
using System.Collections.Generic;
using System.Text;

namespace LinuxHub.Models
{
    public class PartitionInfo
    {
        public int DiskIndex { get; set; }
        public int PartitionIndex { get; set; }
        public string DriveLetter { get; set; }
        public string FileSystem { get; set; }
        public long SizeBytes { get; set; }
        public bool IsSystem { get; set; }

        public string SizeGB =>
            $"{SizeBytes / (1024 * 1024 * 1024)} GB";

        public override string ToString()
        {
            string sys = IsSystem ? " (Sistema)" : "";
            string drive = string.IsNullOrEmpty(DriveLetter) ? "" : $" [{DriveLetter}]";

            return $"Disco {DiskIndex} - Partição {PartitionIndex}{drive} - {SizeGB} - {FileSystem}{sys}";
        }
    }
}
