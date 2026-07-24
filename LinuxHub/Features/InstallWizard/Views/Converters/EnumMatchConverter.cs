using System.Globalization;
using System.Windows.Data;

namespace LinuxHub.Features.InstallWizard.Views.Converters
{
    /// <summary>Liga um RadioButton.IsChecked a um valor específico de um enum.</summary>
    public sealed class EnumMatchConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not null && parameter is not null && value.Equals(parameter);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is true ? parameter : Binding.DoNothing;
    }
}
