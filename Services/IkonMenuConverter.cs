using System.Globalization;
using Microsoft.Maui.Controls;

namespace SistemPengirimanApp.Services;

public class IkonMenuConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var judul = value as string ?? "";

        return judul switch
        {
            "Login" => "user_icon.png",
            "Beranda" => "home_icon.png",
            "Profil" => "profile_icon.png",
            "Input Pengiriman" => "truck_icon.png",
            "Riwayat Pengiriman" => "time_delivery_icon.png",
            "Slip Gaji" => "payslip_icon.png",
            "Master Data" => "folder_icon.png",
            "Laporan" => "laporan_icon.png",
            "Summary" => "file_icon.png",
            _ => "folder_icon.png"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}