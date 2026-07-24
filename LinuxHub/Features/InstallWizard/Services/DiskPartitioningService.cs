using System.Diagnostics;
using System.IO;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class DiskPartitioningService : IDiskPartitioningService
    {
        public void CreatePartition(int sizeInGb)
        {
            string script = $@"
select volume C
shrink desired={sizeInGb * 1024}
create partition primary size={sizeInGb * 1024}
assign letter=Z
exit";

            string scriptPath = Path.Combine(Path.GetTempPath(), "linuxhub_diskpart_script.txt");
            File.WriteAllText(scriptPath, script);

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "diskpart.exe",
                Arguments = $"/s \"{scriptPath}\"",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            });

            if (process is null)
                throw new InvalidOperationException("Não foi possível iniciar o diskpart.exe.");
        }
    }
}
