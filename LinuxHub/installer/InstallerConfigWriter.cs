using System.IO;
using System.Text;
using LinuxHub.Installer;

namespace LinuxHub.Installer
{
    public static class InstallerConfigWriter
    {
        public static void Save(InstallerConfig cfg)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# === Distro ===");
            sb.AppendLine($"DISTRO_ID=\"{cfg.DistroId}\"");
            sb.AppendLine($"DISTRO_NAME=\"{cfg.DistroName}\"");
            sb.AppendLine($"DISTRO_FAMILY=\"{cfg.DistroFamily}\"");
            sb.AppendLine($"DISTRO_VERSION=\"{cfg.DistroVersion}\"");
            sb.AppendLine($"ISO_PATH=\"{cfg.IsoPath}\"");

            sb.AppendLine();
            sb.AppendLine("# === Install ===");
            sb.AppendLine($"BOOT_MODE=\"{cfg.BootMode}\"");
            sb.AppendLine($"INSTALL_MODE=\"{cfg.InstallMode}\"");
            sb.AppendLine($"TARGET_DISK_INDEX={cfg.TargetDiskIndex}");

            if (cfg.TargetPartitionIndex.HasValue)
                sb.AppendLine($"TARGET_PARTITION_INDEX={cfg.TargetPartitionIndex}");

            if (cfg.EfiPartitionIndex.HasValue)
                sb.AppendLine($"EFI_PARTITION_INDEX={cfg.EfiPartitionIndex}");

            sb.AppendLine();
            sb.AppendLine("# === User ===");
            sb.AppendLine($"USERNAME=\"{cfg.Username}\"");
            sb.AppendLine($"PASSWORD_HASH=\"{cfg.PasswordHash}\"");
            sb.AppendLine($"HOSTNAME=\"{cfg.Hostname}\"");

            sb.AppendLine();
            sb.AppendLine("# === System ===");
            sb.AppendLine($"LOCALE=\"{cfg.Locale}\"");
            sb.AppendLine($"KEYMAP=\"{cfg.Keymap}\"");
            sb.AppendLine($"TIMEZONE=\"{cfg.Timezone}\"");

            Directory.CreateDirectory(@"C:\LinuxHub");
            File.WriteAllText(@"C:\LinuxHub\install.conf", sb.ToString());
        }
    }
}
