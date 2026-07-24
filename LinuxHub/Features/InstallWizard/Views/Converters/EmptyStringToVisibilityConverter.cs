using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LinuxHub.Features.InstallWizard.Views.Converters
{
    /// <summary>Visível quando a string está vazia — usado para placeholders de campo.</summary>
    public sealed class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Hidden;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
