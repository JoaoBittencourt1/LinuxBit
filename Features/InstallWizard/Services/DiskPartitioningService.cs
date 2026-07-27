using System.Diagnostics;
using System.IO;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class DiskPartitioningService : IDiskPartitioningService
    {
        public void ShrinkPartition(int diskIndex, int partitionIndex, int sizeInGb)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), "linuxhub_diskpart_script.txt");
            string logPath = Path.Combine(Path.GetTempPath(), "linuxhub_diskpart_output.log");

            File.WriteAllText(scriptPath, BuildScript(diskIndex, partitionIndex, sizeInGb));

            if (File.Exists(logPath))
                File.Delete(logPath);

            // diskpart.exe não suporta redirecionamento de saída quando lançado com
            // Verb=runas (elevação exige UseShellExecute=true, que é incompatível com
            // RedirectStandardOutput) — por isso a saída passa por cmd.exe /c, que roda
            // elevado e redireciona para um arquivo de log que lemos depois.
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c diskpart.exe /s \"{scriptPath}\" > \"{logPath}\" 2>&1",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            };

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Não foi possível iniciar o diskpart.exe.");

            process.WaitForExit();

            string output = File.Exists(logPath) ? File.ReadAllText(logPath) : string.Empty;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Falha ao encolher a partição {partitionIndex} do disco {diskIndex} " +
                    $"(diskpart retornou código {process.ExitCode}). Saída: {output}");
            }
        }

        internal static string BuildScript(int diskIndex, int partitionIndex, int sizeInGb) => $@"
select disk {diskIndex}
select partition {partitionIndex}
shrink desired={sizeInGb * 1024}
exit";
    }
}
