using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IDiskInventoryService
    {
        IReadOnlyList<DiskInfo> GetDisks();
    }
}
