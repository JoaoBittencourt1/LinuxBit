namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Resolve os caminhos dos binários GRUB2 pré-compilados empacotados com o app. Esses
    /// binários não são construídos em runtime — não há toolchain GRUB (grub-mkimage/
    /// grub-bios-setup) nativo no Windows; eles precisam ser gerados uma vez, fora do app
    /// (ex.: WSL/Linux), e empacotados em <c>Assets/Grub/</c>. Ver design.md, Open
    /// Questions, e <c>Assets/Grub/README.md</c>.
    /// </summary>
    public interface IGrubAssetProvider
    {
        /// <summary>Caminho do <c>grubx64.efi</c> standalone (com módulos loopback/ntfs/
        /// iso9660 embutidos) copiado para a ESP em sistemas UEFI.</summary>
        string GetUefiBootloaderPath();

        /// <summary>Caminho do <c>boot.img</c> (440 bytes de código de boot) escrito no
        /// MBR em sistemas BIOS legado. Já vem com o campo <c>kernel_sector</c> apontando
        /// para o LBA 1 — o ponto em que <see cref="GetBiosCoreImagePath"/> é sempre
        /// embutido (ver <c>Assets/Grub/README.md</c>).</summary>
        string GetBiosBootSectorPath();

        /// <summary>Caminho do <c>core.img</c> (gerado com prefixo genérico
        /// <c>/boot/grub</c>, via módulo <c>search</c> — não fixo a nenhum disco/partição)
        /// embutido no gap pós-MBR em sistemas BIOS legado.</summary>
        string GetBiosCoreImagePath();
    }
}
