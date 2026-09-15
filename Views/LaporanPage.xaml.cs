using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class LaporanPage : ContentPage
{
    private readonly ApiService _api;

    public LaporanPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MuatLaporan();
    }

    private async Task MuatLaporan()
    {
        var riwayat = await _api.GetRiwayatLengkapAsync();

        var perHari = riwayat
            .GroupBy(t => t.Tanggal.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new { TanggalAsli = g.Key, Tanggal = g.Key.ToString("dd MMM yyyy"), Jumlah = g.Count() })
            .ToList();
        ListPerHari.ItemsSource = perHari;

        var perWilayah = riwayat
            .Where(t => t.Wilayah != null)
            .GroupBy(t => t.Wilayah!.NamaWilayah)
            .OrderByDescending(g => g.Sum(x => x.TotalBiaya))
            .Select(g => new { NamaWilayah = g.Key, TotalBiaya = g.Sum(x => x.TotalBiaya) })
            .ToList();
        ListPerWilayah.ItemsSource = perWilayah;

        var pendapatansopir = riwayat
            .Where(t => t.Sopir != null)
            .GroupBy(t => t.Sopir!.Nama)
            .Select(g => new { Nama = g.Key, Total = g.Sum(x => x.Wilayah?.Tarifsopir ?? 0) });

        var pendapatanKernet = riwayat
            .Where(t => t.Kernet != null)
            .GroupBy(t => t.Kernet!.Nama)
            .Select(g => new { Nama = g.Key, Total = g.Sum(x => x.Wilayah?.TarifKernet ?? 0) });

        var gabungan = pendapatansopir.Concat(pendapatanKernet)
            .GroupBy(x => x.Nama)
            .OrderByDescending(g => g.Sum(x => x.Total))
            .Select(g => new { Nama = g.Key, TotalPendapatan = g.Sum(x => x.Total) })
            .ToList();

        ListPendapatan.ItemsSource = gabungan;
    }

    private async void OnHariDipilih(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not { } item)
            return;

        ListPerHari.SelectedItem = null;

        var tanggalAsli = (DateTime)item.GetType().GetProperty("TanggalAsli")!.GetValue(item)!;
        var nilaiEscaped = Uri.EscapeDataString(tanggalAsli.ToString("O"));

        await Shell.Current.GoToAsync($"DetailPengirimanPage?mode=hari&nilai={nilaiEscaped}");
    }

    private async void OnWilayahDipilih(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not { } item)
            return;

        ListPerWilayah.SelectedItem = null;

        var namaWilayah = (string)item.GetType().GetProperty("NamaWilayah")!.GetValue(item)!;
        var nilaiEscaped = Uri.EscapeDataString(namaWilayah);

        await Shell.Current.GoToAsync($"DetailPengirimanPage?mode=wilayah&nilai={nilaiEscaped}");
    }
}