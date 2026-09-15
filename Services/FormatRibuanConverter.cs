using System.Globalization;

namespace SistemPengirimanApp.Services;

// Dipakai di XAML untuk menampilkan angka decimal dalam format ribuan ala Indonesia,
// contoh: 50000 tampil sebagai "50.000", bukan "50000.00"
public class FormatRibuanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d.ToString("N0", new CultureInfo("id-ID")); // "50.000"

        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            // Hilangkan titik pemisah ribuan sebelum di-parse balik jadi angka
            var bersih = s.Replace(".", "").Replace(",", "").Trim();

            if (decimal.TryParse(bersih, NumberStyles.Number, CultureInfo.InvariantCulture, out var hasil))
                return hasil;
        }

        return 0m;
    }
}