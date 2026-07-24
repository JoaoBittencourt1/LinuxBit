using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LinuxHub.Features.Catalog.Views.Converters
{
    /// <summary>
    /// Por padrão, visível quando o valor é null. Com ConverterParameter="Invert",
    /// visível quando o valor NÃO é null — usado pra alternar grade/detalhe/overlay
    /// sem precisar de Window/Frame separados.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool visibleWhenNull = !string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
            bool isNull = value is null;
            bool visible = isNull == visibleWhenNull;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
