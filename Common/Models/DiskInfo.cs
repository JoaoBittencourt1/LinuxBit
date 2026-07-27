namespace LinuxHub.Common.Models
{
    public class DiskInfo
    {
        public int Index { get; set; }
        public string Model { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public bool IsSystemDisk { get; set; }

        public string SizeFormatted =>
            $"{SizeBytes / 1024 / 1024 / 1024} GB";

        public override string ToString() =>
            $"Disco {Index} - {SizeFormatted} ({Model})";
    }
}
