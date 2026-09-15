using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class MasterDataTarifTahunanPage : ContentPage
{
    private readonly ApiService _api;

    public MasterDataTarifTahunanPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await MuatData();
            await MuatPengaturanGaji();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async Task MuatData()
    {
        try
        {
            var data = await _api.GetTarifTahunanAsync();
            BindableLayout.SetItemsSource(ListTarifTahunan, data);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Gagal Memuat", $"Tidak bisa memuat tarif tahunan: {ex.Message}", "OK");
        }
    }

    private async Task MuatPengaturanGaji()
    {
        try
        {
            var pengaturan = await _api.GetPengaturanGajiAsync();
            if (pengaturan != null)
            {
                EntryJamsostekSupir.Text = pengaturan.JamsostekSupir.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                EntryJamsostekKernet.Text = pengaturan.JamsostekKernet.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                EntryKesehatanSupir.Text = pengaturan.PotonganKesehatanSupir.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                EntryKesehatanKernet.Text = pengaturan.PotonganKesehatanKernet.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Gagal Memuat", $"Tidak bisa memuat pengaturan gaji: {ex.Message}", "OK");
        }
    }

    private async void OnSimpanTarifTahunanClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is TarifUpahTahunan tarif)
        {
            var berhasil = await _api.SimpanTarifTahunanAsync(tarif);

            if (berhasil)
                await DisplayAlert("Berhasil", $"Tarif tahun {tarif.Tahun} berhasil diperbarui.", "OK");
            else
                await DisplayAlert("Gagal", "Tarif gagal disimpan.", "OK");
        }
    }

    private async void OnSimpanPengaturanJamsostekClicked(object sender, EventArgs e)
    {
        var idID = new System.Globalization.CultureInfo("id-ID");
        var gaya = System.Globalization.NumberStyles.Number;

        decimal.TryParse(EntryJamsostekSupir.Text, gaya, idID, out var jamsostekSupir);
        decimal.TryParse(EntryJamsostekKernet.Text, gaya, idID, out var jamsostekKernet);
        decimal.TryParse(EntryKesehatanSupir.Text, gaya, idID, out var kesehatanSupir);
        decimal.TryParse(EntryKesehatanKernet.Text, gaya, idID, out var kesehatanKernet);

        var berhasil = await _api.UpdatePengaturanGajiAsync(jamsostekSupir, jamsostekKernet, kesehatanSupir, kesehatanKernet);

        if (berhasil)
            await DisplayAlert("Berhasil", "Pengaturan Jamsostek & Kesehatan berhasil disimpan.", "OK");
        else
            await DisplayAlert("Gagal", "Pengaturan gagal disimpan.", "OK");
    }

    private async void OnTambahTarifTahunanClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(EntryTahunBaru.Text, out var tahun))
        {
            await DisplayAlert("Perhatian", "Tahun harus diisi dengan angka.", "OK");
            return;
        }

        decimal.TryParse(EntryUpahHariansupirBaru.Text, out var hariansopir);
        decimal.TryParse(EntryUpahHarianKernetBaru.Text, out var harianKernet);
        decimal.TryParse(EntryUpahMingguansupirBaru.Text, out var mingguansopir);
        decimal.TryParse(EntryUpahMingguanKernetBaru.Text, out var mingguanKernet);
        decimal.TryParse(EntryTunjanganMakanBaru.Text, out var tunjanganMakan);
        decimal.TryParse(EntrySubsidiTransportBaru.Text, out var subsidiTransport);
        decimal.TryParse(EntryJamsostekBaru.Text, out var jamsostek);

        var berhasil = await _api.TambahTarifTahunanAsync(tahun, hariansopir, harianKernet, mingguansopir, mingguanKernet, tunjanganMakan, subsidiTransport, jamsostek);

        if (!berhasil)
        {
            await DisplayAlert("Gagal", "Tarif gagal disimpan.", "OK");
            return;
        }

        EntryTahunBaru.Text = string.Empty;
        EntryUpahHariansupirBaru.Text = string.Empty;
        EntryUpahHarianKernetBaru.Text = string.Empty;
        EntryUpahMingguansupirBaru.Text = string.Empty;
        EntryUpahMingguanKernetBaru.Text = string.Empty;
        EntryTunjanganMakanBaru.Text = string.Empty;
        EntrySubsidiTransportBaru.Text = string.Empty;
        EntryJamsostekBaru.Text = string.Empty;

        await MuatData();
    }
}