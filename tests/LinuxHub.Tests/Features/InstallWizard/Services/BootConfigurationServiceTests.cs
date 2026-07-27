using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class BootConfigurationServiceTests
    {
        [Fact]
        public void BuildAddFirmwareBootEntryScript_CreatesOsloaderEntryAndPreservesExistingDisplayOrder()
        {
            string script = BootConfigurationService.BuildAddFirmwareBootEntryScript(
                "Ubuntu (LinuxHub staging)", driveLetter: 'S', efiPathOnVolume: @"\EFI\linuxhub\grubx64.efi");

            Assert.Contains("/application osloader", script);
            Assert.Contains("device partition=S:", script);
            Assert.Contains(@"path '\EFI\linuxhub\grubx64.efi'", script);
            Assert.Contains("/displayorder $guid /addlast", script);
            Assert.DoesNotContain("/deletevalue", script);
        }
    }
}
