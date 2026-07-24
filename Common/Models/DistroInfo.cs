namespace LinuxHub.Common.Models
{
    public class DistroInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string CreatedYear { get; set; } = string.Empty;

        /// <summary>Quão recomendada a distro é para iniciantes, de 1 (nada) a 5 (muito).</summary>
        public int BeginnerRating { get; set; }

        // Texto de Description/Maintainer nunca é hardcoded aqui — são chaves de recurso
        // (ver constitution.md, "Nenhuma string hardcoded"), resolvidas via
        // LocalizationManager para poderem ser traduzidas e trocar de idioma em runtime.
        public string DescriptionKey => $"Distro_{Id}_Description";
        public string MaintainerKey => $"Distro_{Id}_Maintainer";
        public string ImagePath { get; set; } = string.Empty;
        public string DownloadLink { get; set; } = string.Empty;
        public string DirectDownloadLink { get; set; } = string.Empty;
        public string[] CarouselImages { get; set; } = Array.Empty<string>();
    }
}
