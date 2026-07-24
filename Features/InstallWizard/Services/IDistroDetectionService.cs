using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IDistroDetectionService
    {
        /// <summary>
        /// Identifica a distro a partir do nome do arquivo ISO. Nunca retorna null —
        /// usa uma distro "desconhecida" como fallback, para não travar o fluxo.
        /// </summary>
        DistroInfo Detect(string isoPath);
    }
}
