namespace LinuxHub.Features.InstallWizard.Services
{
    public interface IFirmwareService
    {
        /// <summary>
        /// Consulta o tipo de firmware via GetFirmwareType (kernel32), não uma
        /// heurística de caminho — detecta UEFI mesmo quando pastas do Windows Boot
        /// Manager não estão presentes, e nunca reporta UEFI por engano.
        /// </summary>
        bool IsUefi();
    }
}
