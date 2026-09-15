using Microsoft.Extensions.Logging;
using SistemPengirimanApp.Services;

#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
#endif

namespace SistemPengirimanApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // Ganti DatabaseService menjadi ApiService
        builder.Services.AddSingleton<ApiService>();

        // Semua halaman
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<Views.BerandaPage>();
        builder.Services.AddTransient<Views.InputPengirimanPage>();
        builder.Services.AddTransient<Views.RiwayatPage>();
        builder.Services.AddTransient<Views.LaporanPage>();
        builder.Services.AddTransient<Views.MasterDataPage>();
        builder.Services.AddTransient<Views.SlipGajiPage>();
        builder.Services.AddTransient<Views.DetailSlipGajiPage>();
        builder.Services.AddTransient<Views.RincianSlipGajiPage>();
        builder.Services.AddTransient<Views.ProfilPage>();
        builder.Services.AddTransient<Views.MasterDataPage>();
        builder.Services.AddTransient<Views.MasterDataPegawaiPage>();
        builder.Services.AddTransient<Views.MasterDataWilayahPage>();
        builder.Services.AddTransient<Views.MasterDataTarifTahunanPage>();

#if WINDOWS
        // Perbaikan tampilan Picker di Windows: Title dijadikan placeholder yang
        // hilang otomatis setelah user memilih. Posisi teks rata kiri, center secara vertikal.
        // "ComboBoxTopHeaderMargin" di-override ke 0 karena WinUI tetap menyisakan
        // ruang kosong untuk area header walau Header=null, yang bikin teks kedorong ke bawah.
        PickerHandler.Mapper.AppendToMapping("CustomPickerPlaceholder", (handler, view) =>
        {
            if (handler.PlatformView is ComboBox comboBox && view is Picker picker)
            {
                comboBox.Header = null;
                comboBox.HeaderTemplate = null;
                comboBox.PlaceholderText = picker.Title;
                comboBox.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                comboBox.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                comboBox.Padding = new Microsoft.UI.Xaml.Thickness(12, 0, 12, 0);
                comboBox.MinHeight = 44;
                comboBox.Height = 44;
                comboBox.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            }
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}