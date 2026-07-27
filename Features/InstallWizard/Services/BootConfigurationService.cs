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
            string script = BuildAddFirmwareBootEntryScript(description, driveLetter, efiPathOnVolume);
            string output = ElevatedPowerShellRunner.Run(script, "registro da entrada de boot BCD");

            var match = BcdGuidRegex().Match(output);
            if (!match.Success)
            {
                throw new InvalidOperationException(
                    $"A entrada BCD pode não ter sido criada corretamente — GUID não encontrado na saída: {output}");
            }

            return match.Value;
        }

        internal static string BuildAddFirmwareBootEntryScript(string description, char driveLetter, string efiPathOnVolume) => $@"
$ErrorActionPreference = 'Stop'
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

        [GeneratedRegex(@"\{[0-9a-fA-F-]+\}")]
        private static partial Regex BcdGuidRegex();
    }
}
