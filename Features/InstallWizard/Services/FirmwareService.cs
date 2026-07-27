using System.Runtime.InteropServices;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class FirmwareService : IFirmwareService
    {
        private enum FirmwareType
        {
            Unknown = 0,
            Bios = 1,
            Uefi = 2,
            Max = 3
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFirmwareType(out FirmwareType firmwareType);

        public bool IsUefi() =>
            GetFirmwareType(out var firmwareType) && firmwareType == FirmwareType.Uefi;
    }
}
