using System.Diagnostics;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class BootConfigurationService : IBootConfigurationService
    {
        public void AddBootEntry(string description)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "bcdedit.exe",
                Verb = "runas",
                UseShellExecute = true
            };

            // ArgumentList evita montar uma string de shell (cmd.exe /c "...") com o
            // texto de description interpolado — bcdedit.exe recebe os argumentos
            // diretamente, sem passar por um interpretador de shell.
            startInfo.ArgumentList.Add("/create");
            startInfo.ArgumentList.Add("/d");
            startInfo.ArgumentList.Add(description);
            startInfo.ArgumentList.Add("/application");
            startInfo.ArgumentList.Add("bootsector");

            var process = Process.Start(startInfo);

            if (process is null)
                throw new InvalidOperationException("Não foi possível iniciar o bcdedit.exe.");
        }
    }
}
