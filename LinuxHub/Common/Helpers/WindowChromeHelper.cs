using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LinuxHub.Common.Helpers
{
    /// <summary>
    /// Extrai a chamada de dark mode via DWM que existia duplicada em
    /// MainWindow e DistroWindow.
    /// </summary>
    public static class WindowChromeHelper
    {
        private const int DwmwaUseImmersiveDarkMode = 20;

        public static void EnableDarkMode(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref darkMode, Marshal.SizeOf(typeof(int)));
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize
        );
    }
}
