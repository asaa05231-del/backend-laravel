using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(Nilai), "nilai")]
public partial class DetailPengirimanPage : ContentPage
{
    private readonly ApiService _api;

    public string Mode { get; set; } = "semua";
    public string Nilai { get; set; } = "";

    public DetailPengirimanPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MuatData();
    }

    private async Task MuatData()
    {
        var semuaData = await _api.GetRiwayatLengkapAsync();

        List<Pengiriman> hasil;
        string judul;
        string subJudul;

        switch (Mode)
        {
            case "hari":
                var tanggalTerpilih = DateTime.Parse(Uri.UnescapeDataString(Nilai));
                hasil = semuaData.Where(p => p.Tanggal.Date == tanggalTerpilih.Date).ToList();
                judul = tanggalTerpilih.ToString("dd MMMM yyyy");
                subJudul = $"{hasil.Count} pengiriman";
                break;

            case "wilayah":
                var namaWilayah = Uri.UnescapeDataString(Nilai);
                hasil = semuaData.Where(p => p.Wilayah?.NamaWilayah == namaWilayah).ToList();
                judul = namaWilayah;
                subJudul = $"{hasil.Count} pengiriman • Total Rp {hasil.Sum(p => p.TotalBiaya):N0}";
                break;

            case "hariini":
                hasil = semuaData.Where(p => p.Tanggal.Date == DateTime.Today).ToList();
                judul = "Pengiriman Hari Ini";
                subJudul = $"{hasil.Count} pengiriman";
                break;

            default: // "semua"
                hasil = semuaData;
                judul = "Semua Transaksi";
                subJudul = $"{hasil.Count} pengiriman • Total Rp {hasil.Sum(p => p.TotalBiaya):N0}";
                break;
        }

        LabelJudul.Text = judul;
        LabelSubJudul.Text = subJudul;

        ListDetail.ItemsSource = hasil.OrderByDescending(p => p.Tanggal).ToList();
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await MuatData();
        RefreshViewDetail.IsRefreshing = false;
    }

    private async void OnHapusSwiped(object sender, EventArgs e)
    {
        if (!SesiPengguna.IsAdmin) return;
        if (sender is not SwipeItem item) return;
        if (item.BindingContext is not Pengiriman data) return;

        string namaData = data.Kir ? "KIR" : data.Bengkel ? "Bengkel" : (data.Wilayah?.NamaWilayah ?? "data ini");
        bool konfirmasi = await DisplayAlert("Hapus Data", $"Hapus \"{namaData}\"? Tindakan ini tidak bisa dibatalkan.", "Hapus", "Batal");
        if (!konfirmasi) return;

        bool berhasil = await _api.HapusPengirimanAsync(data.Id);

        if (berhasil)
        {
            await MuatData();
        }
        else
        {
            await DisplayAlert("Gagal", "Gagal menghapus data. Coba lagi.", "OK");
        }
    }
}