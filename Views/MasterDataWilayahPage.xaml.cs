using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class MasterDataWilayahPage : ContentPage
{
    private readonly ApiService _api;

    public MasterDataWilayahPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MuatWilayah();
    }

    private async Task MuatWilayah()
    {
        ListWilayah.ItemsSource = await _api.GetWilayahAsync();
    }
    
    private async void OnTambahWilayahClicked(object sender, EventArgs e)
    {
        var nama = EntryNamaWilayahBaru.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nama))
        {
            await DisplayAlert("Perhatian", "Nama wilayah harus diisi.", "OK");
            return;
        }
        TryParseRupiah(EntryTarifsopirBaru.Text, out decimal tarifsopir);
        TryParseRupiah(EntryTarifKernetBaru.Text, out decimal tarifKernet);

        bool berhasil = await _api.TambahWilayahAsync(nama, tarifsopir, tarifKernet);

        if (!berhasil)
        {
            await DisplayAlert("Gagal", "Wilayah gagal ditambahkan.", "OK");
            return;
        }

        EntryNamaWilayahBaru.Text = "";
        EntryTarifsopirBaru.Text = "";
        EntryTarifKernetBaru.Text = "";

        await MuatWilayah();

        await DisplayAlert("Berhasil", "Wilayah berhasil ditambahkan.", "OK");
    }

    private static bool TryParseRupiah(string? teks, out decimal nilai)
    {
        nilai = 0;
        if (string.IsNullOrWhiteSpace(teks)) return false;
        string bersih = teks.Replace(".", "").Replace(",", "");
        return decimal.TryParse(bersih, out nilai);
    }

    private async void OnSimpanWilayahClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Wilayah wilayah)
        {
            await _api.SimpanWilayahAsync(wilayah);
            await DisplayAlert("Berhasil", $"Tarif {wilayah.NamaWilayah} berhasil diperbarui.", "OK");
        }
    }

    private async void OnHapusWilayahClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Wilayah wilayah) return;

        bool konfirmasi = await DisplayAlert(
            "Hapus Wilayah",
            $"Hapus \"{wilayah.NamaWilayah}\"? Tindakan ini tidak bisa dibatalkan.",
            "Hapus", "Batal");

        if (!konfirmasi) return;

        bool berhasil = await _api.HapusWilayahAsync(wilayah.Id);

        if (berhasil)
            await MuatWilayah();
        else
            await DisplayAlert("Gagal", "Wilayah gagal dihapus. Kemungkinan wilayah ini masih dipakai di data pengiriman.", "OK");
    }
}