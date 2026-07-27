using System.Text.RegularExpressions;

namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Antes desta implementação, <c>AddBootEntry</c> criava uma entrada BCD do tipo
    /// <c>bootsector</c> sem device/path — inutilizável (ver tasks.md 3.6). Uma revisão
    /// intermediária criava a entrada via <c>/create /application osloader</c>, mas isso
    /// falhava em teste real com <c>0xc000007b</c> ("formato de imagem inválido") — o tipo
    /// <c>osloader</c> exige que o binário siga o contrato de loader NT (tipo
    /// <c>winload.efi</c>), que o GRUB não segue. A forma correta de registrar um aplicativo
    /// EFI de terceiros (GRUB, rEFInd, systemd-boot) via BCD é copiar a entrada existente do
    /// <c>{bootmgr}</c> (que já sabe encadear pra outro <c>.efi</c> arbitrário) e só trocar
    /// device/path/descrição — não criar do zero como <c>osloader</c>.
    /// </summary>
    public sealed partial class BootConfigurationService : IBootConfigurationService
    {
        public string AddFirmwareBootEntry(string description, char driveLetter, string efiPathOnVolume)
        {
            string script = $"$ErrorActionPreference = 'Stop'\n{BuildAddFirmwareBootEntryCommands(description, driveLetter, efiPathOnVolume)}";
            string output = ElevatedPowerShellRunner.Run(script, "registro da entrada de boot BCD");

            return ExtractGuidOrThrow(output);
        }

        /// <summary>
        /// Comandos bcdedit puros (sem <c>$ErrorActionPreference</c> nem elevação própria) —
        /// pensado pra ser embutido dentro de um script maior que já monta a letra de
        /// unidade temporária da ESP (ver <see cref="BootStagingService"/>). A letra
        /// referenciada em <paramref name="driveLetter"/> precisa continuar montada até
        /// esses comandos terminarem — chamar isso DEPOIS de desmontar a ESP falha com
        /// "dispositivo não é válido como especificado" (bcdedit não resolve uma letra que
        /// não existe mais). Usa <c>bcdedit /bootsequence</c> (boot único) em vez de só
        /// <c>/displayorder</c> — a próxima reinicialização entra direto na entrada do
        /// LinuxHub, sem o usuário precisar escolher nada num menu de boot; da reinicialização
        /// seguinte em diante volta sozinho a bootar o Windows por padrão (mesma proteção que
        /// o "boot único" do Windows usa pra firmware setup/recovery).
        ///
        /// IMPORTANTE: <c>/displayorder</c> e <c>/bootsequence</c> precisam mirar
        /// explicitamente <c>{fwbootmgr}</c> (a lista de boot da FIRMWARE/UEFI). Sem isso,
        /// bcdedit usa o store padrão do objeto <c>{bootmgr}</c> — aí a entrada do GRUB entra
        /// na lista de OS do próprio Windows Boot Manager, que tenta encadear pra ela como se
        /// fosse um loader NT e falha com <c>0xc000007b</c> (bug encontrado em teste real: a
        /// tela de erro mostrava "Gerenciador de Inicialização do Windows" tentando carregar o
        /// grubx64.efi).
        /// </summary>
        internal static string BuildAddFirmwareBootEntryCommands(string description, char driveLetter, string efiPathOnVolume) => $@"
$create = bcdedit /copy '{{bootmgr}}' /d ""{description}""
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /copy falhou: $create"" }}
$match = [regex]::Match($create, '\{{[0-9a-fA-F-]+\}}')
if (-not $match.Success) {{ throw ""Não foi possível extrair o GUID da entrada BCD criada: $create"" }}
$guid = $match.Value
bcdedit /set $guid device partition={driveLetter}:
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /set device falhou"" }}
bcdedit /set $guid path '{efiPathOnVolume}'
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /set path falhou"" }}
bcdedit /set '{{fwbootmgr}}' displayorder $guid /addlast
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /set {{fwbootmgr}} displayorder falhou"" }}
bcdedit /bootsequence '{{fwbootmgr}}' $guid
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /bootsequence falhou"" }}
Write-Output ""BCDGUID:$guid""";

        internal static string BuildAddFirmwareBootEntryScript(string description, char driveLetter, string efiPathOnVolume) =>
            $"$ErrorActionPreference = 'Stop'\n{BuildAddFirmwareBootEntryCommands(description, driveLetter, efiPathOnVolume)}";

        /// <summary>Extrai o GUID BCD (<c>{...}</c>) da saída de <see cref="ElevatedPowerShellRunner.Run"/>
        /// depois de rodar <see cref="BuildAddFirmwareBootEntryCommands"/> — usado tanto pela
        /// execução standalone (<see cref="AddFirmwareBootEntry"/>) quanto pela embutida em
        /// <see cref="BootStagingService"/>.</summary>
        internal static string ExtractGuidOrThrow(string scriptOutput)
        {
            var match = BcdGuidRegex().Match(scriptOutput);
            if (!match.Success)
            {
                throw new InvalidOperationException(
                    $"A entrada BCD pode não ter sido criada corretamente — GUID não encontrado na saída: {scriptOutput}");
            }

            return match.Value;
        }

        [GeneratedRegex(@"\{[0-9a-fA-F-]+\}")]
        private static partial Regex BcdGuidRegex();
    }
}
