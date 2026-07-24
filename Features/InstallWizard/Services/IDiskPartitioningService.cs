namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IDiskPartitioningService
    {
        /// <summary>
        /// Encolhe o volume C: e cria uma nova partição primária com o tamanho dado,
        /// via diskpart em processo elevado. Lança se o processo não puder ser iniciado.
        /// </summary>
        void CreatePartition(int sizeInGb);
    }
}
