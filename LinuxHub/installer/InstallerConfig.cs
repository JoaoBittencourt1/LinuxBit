using LinuxHub.Models;
using LinuxHub.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace LinuxHub.Installer
{
    public class InstallerConfig
    {
        // === Distro ===
        public string DistroId { get; set; }
        public string DistroName { get; set; }
        public string DistroFamily { get; set; }
        public string DistroVersion { get; set; }
        public string IsoPath { get; set; }

        // === Install ===
        public string BootMode { get; set; }      // uefi | bios
        public string InstallMode { get; set; }   // replace | dualboot
        public int TargetDiskIndex { get; set; }
        public int? TargetPartitionIndex { get; set; }
        public int? EfiPartitionIndex { get; set; }

        public int LinuxPartitionSizeGb { get; set; }

        // === User ===
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Hostname { get; set; }

        // === System ===
        public string Locale { get; set; }
        public string Timezone { get; set; }
        public string Keymap { get; set; }

        // === Swap ===
        public bool SwapEnabled { get; set; }
        public int SwapSizeGb { get; set; }
    }
}

