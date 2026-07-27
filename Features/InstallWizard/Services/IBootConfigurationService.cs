namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IBootConfigurationService
    {
        /// <summary>
        /// Registra uma entrada de boot de firmware (BCD) apontando para um aplicativo EFI
        /// já existente em <paramref name="efiPathOnVolume"/>, na partição identificada por
        /// <paramref name="driveLetter"/> — adiciona ao final da ordem de boot, sem remover
        /// a entrada existente do Windows Boot Manager. Retorna o GUID BCD criado. Ver spec
        /// boot-staging — "Instalar bootloader de chainload em sistemas UEFI".
        /// </summary>
        string AddFirmwareBootEntry(string description, char driveLetter, string efiPathOnVolume);
    }
}
