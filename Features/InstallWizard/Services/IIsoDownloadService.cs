using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public readonly record struct IsoDownloadProgress(double PercentComplete, double RemainingSeconds);

    public interface IIsoDownloadService
    {
        /// <summary>
        /// Baixa a ISO da distro para a pasta padrão de ISOs do LinuxHub e retorna o
        /// caminho do arquivo baixado. Em caso de cancelamento, o arquivo parcial é
        /// removido e a exceção de cancelamento propaga para quem chamou.
        /// </summary>
        Task<string> DownloadAsync(DistroInfo distro, IProgress<IsoDownloadProgress> progress, CancellationToken cancellationToken);
    }
}
