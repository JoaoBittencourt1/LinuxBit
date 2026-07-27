namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IDiskPartitioningService
    {
        /// <summary>
        /// Encolhe a partição indicada (disco/partição reais, selecionados no wizard) em
        /// <paramref name="sizeInGb"/>, via diskpart em processo elevado. Não cria
        /// partição nem sistema de arquivos — isso é responsabilidade exclusiva do
        /// lib/disk.sh do lado Linux, executado depois do reboot (ver design.md D1).
        /// Lança se o diskpart não puder ser iniciado ou retornar código de erro
        /// (ex.: espaço insuficiente para encolher).
        /// </summary>
        void ShrinkPartition(int diskIndex, int partitionIndex, int sizeInGb);
    }
}
