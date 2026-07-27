using System.IO;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class GrubAssetProvider : IGrubAssetProvider
    {
        private readonly string _assetsRoot;

        public GrubAssetProvider() : this(Path.Combine(AppContext.BaseDirectory, "Assets", "Grub"))
        {
        }

        internal GrubAssetProvider(string assetsRoot) => _assetsRoot = assetsRoot;

        public string GetUefiBootloaderPath() => ResolveAsset(Path.Combine("uefi", "grubx64.efi"));

        public string GetBiosBootSectorPath() => ResolveAsset(Path.Combine("bios", "boot.img"));

        public string GetBiosCoreImagePath() => ResolveAsset(Path.Combine("bios", "core.img"));

        private string ResolveAsset(string relativePath)
        {
            string fullPath = Path.Combine(_assetsRoot, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"Binário GRUB2 não encontrado em '{fullPath}'. Esses binários são " +
                    "pré-compilados fora do app (grub-mkstandalone/grub-bios-setup num " +
                    "ambiente Linux/WSL) e precisam ser empacotados em Assets/Grub/ antes " +
                    "de instalar o staging de boot — ver Assets/Grub/README.md.",
                    fullPath);
            }

            return fullPath;
        }
    }
}
