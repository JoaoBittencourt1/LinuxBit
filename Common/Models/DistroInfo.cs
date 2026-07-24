namespace LinuxHub.Common.Models
{
    public class DistroInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string DownloadLink { get; set; } = string.Empty;
        public string DirectDownloadLink { get; set; } = string.Empty;
        public string[] CarouselImages { get; set; } = Array.Empty<string>();
    }
}
