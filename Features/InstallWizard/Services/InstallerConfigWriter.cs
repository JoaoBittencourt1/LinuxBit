using System.IO;
using System.Text;
using LinuxHub.Features.InstallWizard.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class InstallerConfigWriter : IInstallerConfigWriter
    {
        private const string OutputDirectory = @"C:\LinuxHub";
        private const string OutputFileName = "install.conf";

        public void Save(InstallerConfig cfg)
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

            if (cfg.LinuxPartitionSizeGb > 0)
                sb.AppendLine($"LINUX_PARTITION_SIZE_GB={cfg.LinuxPartitionSizeGb}");

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

            Directory.CreateDirectory(OutputDirectory);
            File.WriteAllText(Path.Combine(OutputDirectory, OutputFileName), sb.ToString());
        }
    }
}
