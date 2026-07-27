using System.IO;
using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class BootStagingServiceTests
    {
        private sealed class FakeEspLocator : IEspLocatorService
        {
            public int? FindEfiSystemPartitionIndex(int diskIndex) => null;
        }

        private sealed class FakeGrubAssets : IGrubAssetProvider
        {
            public string GetUefiBootloaderPath() => "uefi.efi";
            public string GetBiosBootSectorPath() => "boot.img";
            public string GetBiosCoreImagePath() => "core.img";
        }

        private sealed class FakeMbrBackup : IMbrBackupService
        {
            public byte[] MbrToReturn { get; set; } = new byte[512];
            public string BackupMbr(int diskIndex, string backupPath) => backupPath;
            public void WriteBootCode(int diskIndex, string bootCodeFilePath) { }
            public void RestoreMbr(int diskIndex, string backupPath) { }
            public byte[] ReadMbr(int diskIndex) => MbrToReturn;
            public void WriteCoreImageToGap(int diskIndex, string coreImageFilePath) { }
        }

        private sealed class FakeBootConfiguration : IBootConfigurationService
        {
            public string AddFirmwareBootEntry(string description, char driveLetter, string efiPathOnVolume) => "{guid}";
        }

        private static byte[] BuildMbrWithFirstPartitionAt(uint startLba)
        {
            var mbr = new byte[512];
            mbr[446 + 4] = 0x07;
            BitConverter.GetBytes(startLba).CopyTo(mbr, 446 + 8);
            return mbr;
        }

        [Fact]
        public void EnsurePostMbrGapFitsCoreImage_ModernAlignment_DoesNotThrow()
        {
            string coreImage = Path.GetTempFileName();
            File.WriteAllBytes(coreImage, new byte[150_000]); // ~293 sectors, well under 2047

            var mbrBackup = new FakeMbrBackup { MbrToReturn = BuildMbrWithFirstPartitionAt(2048) };
            var service = new BootStagingService(new FakeEspLocator(), new FakeGrubAssets(), mbrBackup, new FakeBootConfiguration());

            service.EnsurePostMbrGapFitsCoreImage(diskIndex: 0, coreImage);

            File.Delete(coreImage);
        }

        [Fact]
        public void EnsurePostMbrGapFitsCoreImage_TinyLegacyGap_ThrowsBeforeAnyWrite()
        {
            string coreImage = Path.GetTempFileName();
            File.WriteAllBytes(coreImage, new byte[150_000]);

            // Alinhamento pré-Vista: partição em LBA 63 (~31KB de gap, insuficiente).
            var mbrBackup = new FakeMbrBackup { MbrToReturn = BuildMbrWithFirstPartitionAt(63) };
            var service = new BootStagingService(new FakeEspLocator(), new FakeGrubAssets(), mbrBackup, new FakeBootConfiguration());

            Assert.Throws<InvalidOperationException>(() => service.EnsurePostMbrGapFitsCoreImage(diskIndex: 0, coreImage));

            File.Delete(coreImage);
        }

        [Fact]
        public void BuildEspStagingScript_MountsAndUnmountsTheSameAccessPath()
        {
            string script = BootStagingService.BuildEspStagingScript(
                diskIndex: 0, partitionIndex: 1, driveLetter: 'S',
                grubEfiSourcePath: @"C:\App\Assets\Grub\uefi\grubx64.efi",
                grubCfgContent: "menuentry \"x\" {}\n");

            Assert.Contains("Add-PartitionAccessPath -DiskNumber 0 -PartitionNumber 1 -AccessPath 'S:\\'", script);
            Assert.Contains("Remove-PartitionAccessPath -DiskNumber 0 -PartitionNumber 1 -AccessPath 'S:\\'", script);
            Assert.Contains(@"S:\EFI\linuxhub\grubx64.efi", script);
            Assert.Contains(@"S:\EFI\linuxhub\grub.cfg", script);
        }

        [Fact]
        public void BuildEspStagingAndBcdScript_RegistersBcdEntryBeforeUnmountingEsp()
        {
            // Bug real encontrado em teste: bcdedit falhava com "dispositivo não é válido
            // como especificado" porque a ESP já tinha sido desmontada antes do bcdedit
            // rodar (eram duas execuções elevadas separadas). Precisa ser uma só, com o
            // bcdedit dentro do mesmo try, antes do Remove-PartitionAccessPath.
            string script = BootStagingService.BuildEspStagingAndBcdScript(
                diskIndex: 0, partitionIndex: 1, driveLetter: 'S',
                grubEfiSourcePath: @"C:\App\Assets\Grub\uefi\grubx64.efi",
                grubCfgContent: "menuentry \"x\" {}\n",
                description: "Ubuntu (LinuxHub staging)",
                efiPathOnVolume: @"\EFI\linuxhub\grubx64.efi");

            int bcdCreateIndex = script.IndexOf("bcdedit /create", StringComparison.Ordinal);
            int removeAccessPathIndex = script.IndexOf("Remove-PartitionAccessPath", StringComparison.Ordinal);

            Assert.True(bcdCreateIndex >= 0, "bcdedit /create deveria estar no script.");
            Assert.True(removeAccessPathIndex >= 0, "Remove-PartitionAccessPath deveria estar no script.");
            Assert.True(bcdCreateIndex < removeAccessPathIndex,
                "bcdedit precisa rodar ANTES de desmontar a ESP, senão 'device partition=S:' falha.");

            Assert.Contains("device partition=S:", script);
            Assert.Contains(@"path '\EFI\linuxhub\grubx64.efi'", script);
        }

        [Fact]
        public void BuildGrubCfgWriteScript_WritesUnderSystemDriveBootGrub()
        {
            string script = BootStagingService.BuildGrubCfgWriteScript(@"C:\", "set timeout=10\n");

            Assert.Contains(@"C:\boot\grub", script);
            Assert.Contains("set timeout=10", script);
        }

        [Fact]
        public void PickFreeDriveLetter_NeverReturnsALetterAlreadyInUse()
        {
            char letter = BootStagingService.PickFreeDriveLetter();
            var usedLetters = System.IO.DriveInfo.GetDrives()
                .Select(d => char.ToUpperInvariant(d.Name[0]))
                .ToHashSet();

            Assert.DoesNotContain(letter, usedLetters);
        }
    }
}
