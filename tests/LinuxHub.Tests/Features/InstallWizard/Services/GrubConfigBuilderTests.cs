using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class GrubConfigBuilderTests
    {
        [Fact]
        public void ToGrubPath_StripsDriveLetterAndConvertsSlashes()
        {
            string result = GrubConfigBuilder.ToGrubPath(@"C:\Users\joao\AppData\Roaming\LinuxHub\ISOs\ubuntu.iso");

            Assert.Equal("/Users/joao/AppData/Roaming/LinuxHub/ISOs/ubuntu.iso", result);
        }

        [Fact]
        public void BuildIsoBootEntry_SearchesForIsoInsteadOfAssumingDiskNumbering()
        {
            string entry = GrubConfigBuilder.BuildIsoBootEntry("Ubuntu", @"C:\ISOs\ubuntu.iso");

            Assert.Contains("search --no-floppy --file --set=root $isofile", entry);
            Assert.Contains("loopback loop $isofile", entry);
            Assert.Contains("boot=casper", entry);
            Assert.Contains("iso-scan/filename=$isofile", entry);
            Assert.DoesNotContain("(hd0", entry);
        }

        [Fact]
        public void BuildWindowsChainloadEntry_SearchesForBootmgrInsteadOfAssumingPartitionIndex()
        {
            string entry = GrubConfigBuilder.BuildWindowsChainloadEntry();

            Assert.Contains("search --no-floppy --file --set=root /bootmgr", entry);
            Assert.Contains("chainloader +1", entry);
        }

        [Fact]
        public void BuildConfig_OmitsWindowsEntryWhenNotRequested()
        {
            string config = GrubConfigBuilder.BuildConfig("Ubuntu", @"C:\ISOs\ubuntu.iso", includeWindowsChainload: false);

            Assert.DoesNotContain("chainloader +1", config);
        }

        [Fact]
        public void BuildConfig_IncludesWindowsEntryWhenRequested()
        {
            string config = GrubConfigBuilder.BuildConfig("Ubuntu", @"C:\ISOs\ubuntu.iso", includeWindowsChainload: true);

            Assert.Contains("chainloader +1", config);
        }
    }
}
