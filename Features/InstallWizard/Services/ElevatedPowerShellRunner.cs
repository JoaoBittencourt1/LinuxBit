using System.Diagnostics;
using System.IO;

namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Executa um script PowerShell em processo elevado. bcdedit/diskpart/PowerShell
    /// lançados com <c>Verb=runas</c> exigem <c>UseShellExecute=true</c>, que não suporta
    /// redirecionamento direto de stdout/stderr — por isso o script roda via
    /// <c>cmd.exe</c>, que redireciona a saída para um arquivo de log lido depois.
    /// Compartilhado entre <see cref="MbrBackupService"/>, <see cref="BootStagingService"/>
    /// e <see cref="BootConfigurationService"/> para não duplicar esse boilerplate de
    /// elevação (mesmo padrão usado em <see cref="DiskPartitioningService"/>, que não usa
    /// PowerShell e por isso não compartilha este helper).
    /// </summary>
    internal static class ElevatedPowerShellRunner
    {
        public static string Run(string script, string operationDescription)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), $"linuxhub_{Guid.NewGuid():N}.ps1");
            string logPath = Path.Combine(Path.GetTempPath(), $"linuxhub_{Guid.NewGuid():N}.log");

            File.WriteAllText(scriptPath, script);

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" > \"{logPath}\" 2>&1",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            };

            try
            {
                using var process = Process.Start(startInfo)
                    ?? throw new InvalidOperationException(
                        $"Não foi possível iniciar o processo elevado para {operationDescription}.");

                process.WaitForExit();

                string output = File.Exists(logPath) ? File.ReadAllText(logPath) : string.Empty;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Falha na {operationDescription} (código {process.ExitCode}). Saída: {output}");
                }

                return output;
            }
            finally
            {
                if (File.Exists(scriptPath))
                    File.Delete(scriptPath);
                if (File.Exists(logPath))
                    File.Delete(logPath);
            }
        }
    }
}
