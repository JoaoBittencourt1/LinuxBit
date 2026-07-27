using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class BootConfigurationServiceTests
    {
        [Fact]
        public void BuildAddFirmwareBootEntryScript_CopiesBootmgrEntryInsteadOfCreatingOsloader()
        {
            // /application osloader falhava em teste real com 0xc000007b — GRUB não segue o
            // contrato de loader NT. Copiar {bootmgr} é a forma correta de registrar um
            // aplicativo EFI de terceiros via BCD.
            string script = BootConfigurationService.BuildAddFirmwareBootEntryScript(
                "Ubuntu (LinuxHub staging)", driveLetter: 'S', efiPathOnVolume: @"\EFI\linuxhub\grubx64.efi");

            Assert.Contains("bcdedit /copy '{bootmgr}'", script);
            Assert.DoesNotContain("/application osloader", script);
            Assert.Contains("device partition=S:", script);
            Assert.Contains(@"path '\EFI\linuxhub\grubx64.efi'", script);
            Assert.Contains("/displayorder $guid /addlast", script);
            Assert.Contains("/bootsequence $guid", script);
            Assert.DoesNotContain("/deletevalue", script);
        }
    }
}
