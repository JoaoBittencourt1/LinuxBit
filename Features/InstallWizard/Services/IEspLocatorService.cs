namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IEspLocatorService
    {
        /// <summary>
        /// Localiza o número da EFI System Partition no disco indicado, pelo GUID de
        /// tipo GPT (c12a7328-f81f-11d2-ba4b-00a0c93ec93b), nunca por índice fixo.
        /// Retorna null se o disco não tiver ESP (BIOS legado, ou disco sem GPT).
        /// </summary>
        int? FindEfiSystemPartitionIndex(int diskIndex);
    }
}
