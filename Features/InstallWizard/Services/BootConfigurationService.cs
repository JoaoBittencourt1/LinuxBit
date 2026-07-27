using System.Text.RegularExpressions;

namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Antes desta implementação, <c>AddBootEntry</c> criava uma entrada BCD do tipo
    /// <c>bootsector</c> sem device/path — inutilizável (ver tasks.md 3.6). Esta versão
    /// cria uma entrada <c>osloader</c> de verdade, aponta para o EFI já copiado na ESP, e
    /// confirma o GUID criado antes de considerar a operação bem-sucedida.
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
        /// não existe mais).
        /// </summary>
        internal static string BuildAddFirmwareBootEntryCommands(string description, char driveLetter, string efiPathOnVolume) => $@"
$create = bcdedit /create /d ""{description}"" /application osloader
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /create falhou: $create"" }}
$match = [regex]::Match($create, '\{{[0-9a-fA-F-]+\}}')
if (-not $match.Success) {{ throw ""Não foi possível extrair o GUID da entrada BCD criada: $create"" }}
$guid = $match.Value
bcdedit /set $guid device partition={driveLetter}:
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /set device falhou"" }}
bcdedit /set $guid path '{efiPathOnVolume}'
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /set path falhou"" }}
bcdedit /displayorder $guid /addlast
if ($LASTEXITCODE -ne 0) {{ throw ""bcdedit /displayorder falhou"" }}
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
