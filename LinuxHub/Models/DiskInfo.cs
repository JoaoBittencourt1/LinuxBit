using System;
using System.Collections.Generic;
using System.Text;

namespace LinuxHub.Models
{
    public class DiskInfo
    {
        public int Index { get; set; }
        public string Model { get; set; }
        public long SizeBytes { get; set; }

        public string SizeFormatted =>
            $"{SizeBytes / 1024 / 1024 / 1024} GB";

        public override string ToString()
        {
            return $"Disco {Index} - {SizeFormatted} ({Model})";
        }
    }

}
