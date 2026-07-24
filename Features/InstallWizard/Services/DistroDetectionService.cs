using System.IO;
using LinuxHub.Common.Data;
using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class DistroDetectionService : IDistroDetectionService
    {
        public DistroInfo Detect(string isoPath)
        {
            if (string.IsNullOrWhiteSpace(isoPath) || !File.Exists(isoPath))
                return Unknown();

            var fileName = Path.GetFileName(isoPath);
            return DistroCatalog.FindByIsoFileName(fileName) ?? Unknown();
        }

        private static DistroInfo Unknown() => new()
        {
            Name = "Distribuição desconhecida",
            Description = "Não foi possível identificar a distro",
            // Nota: este asset não existe no projeto hoje (pré-existente ao refactor) —
            // escolher um placeholder real é uma decisão de design fora deste change.
            ImagePath = "pack://application:,,,/Assets/Images/unknown.png"
        };
    }
}
