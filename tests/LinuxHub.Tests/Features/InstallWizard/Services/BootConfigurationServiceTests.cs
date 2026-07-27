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
            Assert.DoesNotContain("/deletevalue", script);
        }

        [Fact]
        public void BuildAddFirmwareBootEntryScript_TargetsFwbootmgrViaSetNotBareSubcommands()
        {
            // Bug real, encontrado DUAS vezes: `bcdedit /displayorder` e `bcdedit /bootsequence`
            // não aceitam objeto-alvo — agem sempre sobre {bootmgr}. Passar {fwbootmgr} como
            // primeiro argumento não redireciona nada, só põe {fwbootmgr} DENTRO da lista do
            // Windows Boot Manager, que então tenta carregar o GRUB como loader NT => 0xc000007b.
            // Só `bcdedit /set {fwbootmgr} <datatype>` mexe de fato na lista da firmware.
            string script = BootConfigurationService.BuildAddFirmwareBootEntryScript(
                "Ubuntu (LinuxHub staging)", driveLetter: 'S', efiPathOnVolume: @"\EFI\linuxhub\grubx64.efi");

            Assert.Contains("bcdedit /set '{fwbootmgr}' displayorder $guid /addlast", script);
            Assert.Contains("bcdedit /set '{fwbootmgr}' bootsequence $guid", script);
            Assert.DoesNotContain("bcdedit /displayorder $guid", script);
            Assert.DoesNotContain("bcdedit /bootsequence '{fwbootmgr}'", script);
        }

        [Fact]
        public void BuildAddFirmwareBootEntryScript_RemovesStaleStagingEntriesBeforeCopying()
        {
            // Sem isso cada tentativa deixa um `bcdedit /copy` órfão no menu de boot.
            string script = BootConfigurationService.BuildAddFirmwareBootEntryScript(
                "Ubuntu (LinuxHub staging)", driveLetter: 'S', efiPathOnVolume: @"\EFI\linuxhub\grubx64.efi");

            Assert.Contains("bcdedit /delete $stale /f", script);
            Assert.Contains(BootConfigurationService.StagingEntryMarker, script);
            Assert.True(
                script.IndexOf("bcdedit /delete $stale /f", StringComparison.Ordinal)
                    < script.IndexOf("bcdedit /copy", StringComparison.Ordinal),
                "a limpeza precisa rodar ANTES do /copy, senão apaga a entrada recém-criada.");
        }

        [Fact]
        public void ExtractGuidOrThrow_IgnoresGuidsFromCleanupOutputAndUsesTheBcdguidMarker()
        {
            // A limpeza roda antes e imprime saída do bcdedit com GUIDs de entradas ANTIGAS;
            // pegar o primeiro {...} da saída devolveria o GUID errado.
            string output = string.Join("\n",
                "A operação foi concluída com êxito.",
                "{11111111-1111-1111-1111-111111111111}",
                "BCDGUID:{22222222-2222-2222-2222-222222222222}");

            Assert.Equal("{22222222-2222-2222-2222-222222222222}", BootConfigurationService.ExtractGuidOrThrow(output));
        }

        [Fact]
        public void ExtractGuidOrThrow_WithoutMarker_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => BootConfigurationService.ExtractGuidOrThrow("{11111111-1111-1111-1111-111111111111}"));
        }
    }
}
