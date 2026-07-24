using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IPartitionInventoryService
    {
        /// <summary>Partições elegíveis para dual-boot (maiores que 20GB).</summary>
        IReadOnlyList<PartitionInfo> GetEligiblePartitions();
    }
}
