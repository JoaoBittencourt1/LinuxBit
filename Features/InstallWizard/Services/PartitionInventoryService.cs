using System.Management;
using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class PartitionInventoryService : IPartitionInventoryService
    {
        private const long MinimumEligibleSizeBytes = 20L * 1024 * 1024 * 1024;

        public IReadOnlyList<PartitionInfo> GetEligiblePartitions()
        {
            var partitions = new List<PartitionInfo>();

            using var searcher = new ManagementObjectSearcher(
                "SELECT DeviceID, DiskIndex, Index, Size, Type, BootPartition FROM Win32_DiskPartition");

            foreach (ManagementBaseObject partition in searcher.Get())
            {
                long size = Convert.ToInt64(partition["Size"] ?? 0);

                // Ignora partições muito pequenas (EFI, MSR, Recovery)
                if (size < MinimumEligibleSizeBytes)
                    continue;

                bool isBoot = partition["BootPartition"] != null && (bool)partition["BootPartition"];
                string type = partition["Type"]?.ToString() ?? "";

                partitions.Add(new PartitionInfo
                {
                    DiskIndex = Convert.ToInt32(partition["DiskIndex"]),
                    // Win32_DiskPartition.Index é 0-based (documentado assim pela
                    // Microsoft) — mas diskpart ("select partition N") e parted (número
                    // de partição no disco.sh Linux-side) numeram 1-based. Sem o +1, o
                    // shrink (DiskPartitioningService) e a criação da partição Linux
                    // (disk.sh) mirariam na partição errada em qualquer disco com mais
                    // de uma partição.
                    PartitionIndex = Convert.ToInt32(partition["Index"]) + 1,
                    SizeBytes = size,
                    Type = type,
                    IsSystem = isBoot
                });
            }

            return partitions;
        }
    }
}
