using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class DiskPartitioningServiceTests
    {
        [Fact]
        public void BuildScript_TargetsSelectedDiskAndPartition()
        {
            string script = DiskPartitioningService.BuildScript(diskIndex: 1, partitionIndex: 4, sizeInGb: 50);

            Assert.Contains("select disk 1", script);
            Assert.Contains("select partition 4", script);
            Assert.Contains("shrink desired=51200", script);
        }

        [Fact]
        public void BuildScript_NeverCreatesPartitionOrAssignsLetter()
        {
            string script = DiskPartitioningService.BuildScript(diskIndex: 0, partitionIndex: 2, sizeInGb: 20);

            Assert.DoesNotContain("create partition", script);
            Assert.DoesNotContain("assign letter", script);
        }
    }
}
