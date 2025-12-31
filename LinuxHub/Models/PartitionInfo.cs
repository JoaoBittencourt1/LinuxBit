using System;
using System.Collections.Generic;
using System.Text;

namespace LinuxHub.Models
{
    public class PartitionInfo
    {
        public int DiskIndex { get; set; }
        public int PartitionIndex { get; set; }
        public long SizeBytes { get; set; }
        public string Type { get; set; }
        public bool IsSystem { get; set; }

        public override string ToString()
        {
            string sizeGb = (SizeBytes / 1024d / 1024d / 1024d).ToString("0");

            return $"Disco {DiskIndex} | Partição {PartitionIndex} | {sizeGb} GB";
        }
    }

}
