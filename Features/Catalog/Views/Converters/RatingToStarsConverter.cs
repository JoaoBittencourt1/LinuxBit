using System.Globalization;
using System.Windows.Data;

namespace LinuxHub.Features.Catalog.Views.Converters
{
    /// <summary>
    /// Converte um rating de 1 a 5 (int) numa string de estrelas cheias/vazias,
    /// ex.: BeginnerRating = 3 vira "★★★☆☆".
    /// </summary>
    public sealed class RatingToStarsConverter : IValueConverter
    {
        private const int MaxStars = 5;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var rating = value is int i ? Math.Clamp(i, 0, MaxStars) : 0;
            return new string('★', rating) + new string('☆', MaxStars - rating);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
