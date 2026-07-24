using LinuxHub.Features.InstallWizard.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IInstallerConfigWriter
    {
        /// <summary>Grava a configuração para consumo pelo instalador Linux-side.</summary>
        void Save(InstallerConfig config);
    }
}
