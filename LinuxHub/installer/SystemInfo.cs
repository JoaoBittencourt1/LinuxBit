using System.Globalization;

namespace LinuxHub.Installer
{
    public static class SystemInfo
    {
        public static string GetLocale()
        {
            return CultureInfo.CurrentCulture.Name.Replace("-", "_") + ".UTF-8";
        }

        public static string GetKeymap()
        {
            return "us"; 
        }

        public static string GetTimezone()
        {
            
            return "America/Sao_Paulo";
        }
    }
}
