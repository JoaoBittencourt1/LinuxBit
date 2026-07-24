namespace LinuxHub.Features.InstallWizard.Models
{
    public class InstallerConfig
    {
        // === Distro ===
        public string DistroId { get; set; } = string.Empty;
        public string DistroName { get; set; } = string.Empty;
        public string DistroFamily { get; set; } = string.Empty;
        public string DistroVersion { get; set; } = string.Empty;
        public string IsoPath { get; set; } = string.Empty;

        // === Install ===
        public string BootMode { get; set; } = string.Empty;      // uefi | bios
        public string InstallMode { get; set; } = string.Empty;   // replace | dualboot
        public int TargetDiskIndex { get; set; }
        public int? TargetPartitionIndex { get; set; }
        public int? EfiPartitionIndex { get; set; }

        public int LinuxPartitionSizeGb { get; set; }

        // === User ===
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;

        // === System ===
        public string Locale { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Keymap { get; set; } = string.Empty;

        // === Swap ===
        public bool SwapEnabled { get; set; }
        public int SwapSizeGb { get; set; }
    }
}
