namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed record BootStagingRequest(string DistroName, string IsoPath, bool IsUefi, int TargetDiskIndex);

    /// <summary>
    /// Instala o bootloader de staging (GRUB2 chainloaded) que permite bootar a ISO já
    /// baixada via loopback, sem USB — cobre UEFI (ESP + BCD) e BIOS legado (MBR, com
    /// backup do MBR original antes de qualquer escrita). Ver design.md D4 e specs
    /// boot-staging. Feature própria (não estende BootConfigurationService/
    /// DiskPartitioningService) por SRP — cada service concreto que ele orquestra continua
    /// com uma única responsabilidade.
    /// </summary>
    public interface IBootStagingService
    {
        void InstallStagingBootloader(BootStagingRequest request);
    }
}
