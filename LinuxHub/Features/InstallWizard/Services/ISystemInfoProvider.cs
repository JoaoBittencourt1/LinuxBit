namespace LinuxHub.Features.InstallWizard.Services
{
    public interface ISystemInfoProvider
    {
        string GetLocale();
        string GetKeymap();
        string GetTimezone();
    }
}
