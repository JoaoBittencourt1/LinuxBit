using LinuxHub.Common.Helpers;
using LinuxHub.Common.Models;
using LinuxHub.Features.InstallWizard.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed record BuildInstallerConfigRequest(
        DistroInfo Distro,
        string IsoPath,
        bool IsUefi,
        InstallMode Mode,
        int? TargetDiskIndex,
        int? TargetPartitionIndex,
        int LinuxPartitionSizeGb,
        string Username,
        string Password,
        string Hostname);

    /// <summary>
    /// Monta um <see cref="InstallerConfig"/> a partir do estado do wizard. Não depende
    /// de System.Windows.* — testável isoladamente, ao contrário do BuildInstallerConfig
    /// original que lia direto de controles do MainWindow.
    /// </summary>
    public sealed class InstallerConfigBuilder
    {
        private readonly ISystemInfoProvider _systemInfo;

        public InstallerConfigBuilder(ISystemInfoProvider systemInfo)
        {
            _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
        }

        public InstallerConfig Build(BuildInstallerConfigRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var cfg = new InstallerConfig
            {
                DistroId = request.Distro.Id,
                DistroName = request.Distro.Name,
                DistroFamily = request.Distro.Family,
                DistroVersion = request.Distro.Version,
                IsoPath = request.IsoPath,

                BootMode = request.IsUefi ? "uefi" : "bios",
                InstallMode = request.Mode == InstallMode.Replace ? "replace" : "dualboot",
                EfiPartitionIndex = request.IsUefi ? 1 : null,
                TargetDiskIndex = request.TargetDiskIndex ?? 0,

                Username = request.Username.Trim(),
                PasswordHash = CryptoHelper.GenerateSha512Hash(request.Password),
                Hostname = request.Hostname.Trim(),

                Locale = _systemInfo.GetLocale(),
                Timezone = _systemInfo.GetTimezone(),
                Keymap = _systemInfo.GetKeymap(),

                SwapEnabled = true,
                SwapSizeGb = 8
            };

            if (request.Mode == InstallMode.DualBoot && request.TargetPartitionIndex.HasValue)
            {
                cfg.TargetPartitionIndex = request.TargetPartitionIndex;
                cfg.LinuxPartitionSizeGb = request.LinuxPartitionSizeGb;
            }
            else
            {
                cfg.TargetPartitionIndex = null;
                cfg.LinuxPartitionSizeGb = 0;
            }

            return cfg;
        }
    }
}
