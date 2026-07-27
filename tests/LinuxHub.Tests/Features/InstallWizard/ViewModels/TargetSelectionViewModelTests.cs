using LinuxHub.Common.Models;
using LinuxHub.Features.InstallWizard.Services;
using LinuxHub.Features.InstallWizard.ViewModels;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.ViewModels
{
    public class TargetSelectionViewModelTests
    {
        private sealed class FakeDiskInventoryService : IDiskInventoryService
        {
            public IReadOnlyList<DiskInfo> Disks { get; set; } = Array.Empty<DiskInfo>();
            public IReadOnlyList<DiskInfo> GetDisks() => Disks;
        }

        private sealed class ThrowingDiskInventoryService : IDiskInventoryService
        {
            public IReadOnlyList<DiskInfo> GetDisks() => throw new InvalidOperationException("WMI indisponível");
        }

        private sealed class FakePartitionInventoryService : IPartitionInventoryService
        {
            public IReadOnlyList<PartitionInfo> GetEligiblePartitions() => Array.Empty<PartitionInfo>();
        }

        private sealed class FakeFirmwareService : IFirmwareService
        {
            public bool IsUefi() => true;
        }

        private static DiskInfo[] TwoDisks() => new[]
        {
            new DiskInfo { Index = 0, Model = "Sistema", SizeBytes = 256L * 1024 * 1024 * 1024, IsSystemDisk = true },
            new DiskInfo { Index = 1, Model = "Secundário", SizeBytes = 512L * 1024 * 1024 * 1024, IsSystemDisk = false }
        };

        [Fact]
        public void ReplaceMode_SystemDisk_IsAllowedButFlaggedForStrongerWarning()
        {
            // O wipe real só acontece depois do reboot (boot-staging + Linux live), quando o
            // Windows não está mais rodando — replace no disco de sistema é permitido, só
            // aciona um aviso mais forte na UI/confirmação (ver design.md D2, revisado).
            var diskInventory = new FakeDiskInventoryService { Disks = TwoDisks() };

            var vm = new TargetSelectionViewModel(diskInventory, new FakePartitionInventoryService(), new FakeFirmwareService())
            {
                Mode = InstallMode.Replace,
                SelectedDisk = diskInventory.Disks[0]
            };

            Assert.True(vm.IsReplacingSystemDisk);
        }

        [Fact]
        public void ReplaceMode_SecondaryDisk_IsNotFlagged()
        {
            var diskInventory = new FakeDiskInventoryService { Disks = TwoDisks() };

            var vm = new TargetSelectionViewModel(diskInventory, new FakePartitionInventoryService(), new FakeFirmwareService())
            {
                Mode = InstallMode.Replace,
                SelectedDisk = diskInventory.Disks[1]
            };

            Assert.False(vm.IsReplacingSystemDisk);
        }

        [Fact]
        public void DiskDetectionFailure_SetsErrorAndEmptiesDisks()
        {
            var vm = new TargetSelectionViewModel(
                new ThrowingDiskInventoryService(),
                new FakePartitionInventoryService(),
                new FakeFirmwareService());

            Assert.True(vm.HasDiskDetectionError);
            Assert.Empty(vm.Disks);
        }
    }
}
