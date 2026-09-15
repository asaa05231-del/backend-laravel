using System.Globalization;

namespace SistemPengirimanApp.Services;

public class NullableDateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? dt : DateTime.Today;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? (DateTime?)dt : null;
}