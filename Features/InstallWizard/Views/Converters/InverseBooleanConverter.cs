using System.Globalization;
using System.Windows.Data;

namespace LinuxHub.Features.InstallWizard.Views.Converters
{
    public sealed class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b ? !b : value!;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b ? !b : value!;
    }
}
