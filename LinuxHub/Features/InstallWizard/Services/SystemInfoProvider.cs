using System.Globalization;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class SystemInfoProvider : ISystemInfoProvider
    {
        public string GetLocale() =>
            CultureInfo.CurrentCulture.Name.Replace("-", "_") + ".UTF-8";

        public string GetKeymap() => "us";

        public string GetTimezone() => "America/Sao_Paulo";
    }
}
