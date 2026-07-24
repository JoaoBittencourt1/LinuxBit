using System.Management;
using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class DiskInventoryService : IDiskInventoryService
    {
        public IReadOnlyList<DiskInfo> GetDisks()
        {
            var disks = new List<DiskInfo>();

            using var searcher = new ManagementObjectSearcher(
                "SELECT Index, Model, Size FROM Win32_DiskDrive");

            foreach (ManagementBaseObject disk in searcher.Get())
            {
                if (disk["Size"] == null)
                    continue;

                disks.Add(new DiskInfo
                {
                    Index = Convert.ToInt32(disk["Index"]),
                    Model = disk["Model"]?.ToString() ?? "Desconhecido",
                    SizeBytes = Convert.ToInt64(disk["Size"])
                });
            }

            return disks;
        }
    }
}
