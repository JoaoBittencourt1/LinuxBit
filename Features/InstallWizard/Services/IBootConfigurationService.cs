namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IBootConfigurationService
    {
        /// <summary>
        /// Adiciona uma entrada de boot via bcdedit em processo elevado. Lança se o
        /// processo não puder ser iniciado.
        /// </summary>
        void AddBootEntry(string description);
    }
}
