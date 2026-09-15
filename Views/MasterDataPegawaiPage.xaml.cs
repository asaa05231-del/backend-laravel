using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class MasterDataPegawaiPage : ContentPage
{
    private readonly ApiService _api;

    public MasterDataPegawaiPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MuatPegawai();
    }

    private async Task MuatPegawai()
    {
        ListPegawai.ItemsSource = await _api.GetPegawaiAsync();
    }

    private async void OnPilihJabatanBaruTapped(object sender, EventArgs e)
    {
        string pilihan = await DisplayActionSheet("Pilih Jabatan", "Batal", null, "sopir", "kernet");
        if (string.IsNullOrEmpty(pilihan) || pilihan == "Batal") return;

        LabelJabatanBaru.Text = pilihan;
        LabelJabatanBaru.TextColor = Colors.Black;
    }

    private async void OnPilihJabatanPegawaiTapped(object sender, EventArgs e)
    {
        if (sender is not Label label || label.BindingContext is not Pegawai pegawai) return;

        string pilihan = await DisplayActionSheet("Pilih Jabatan", "Batal", null, "sopir", "kernet");
        if (string.IsNullOrEmpty(pilihan) || pilihan == "Batal") return;

        pegawai.Jabatan = pilihan;
        label.Text = pilihan;
    }

    private async void OnTambahPegawaiClicked(object sender, EventArgs e)
    {
        var nama = EntryNamaPegawaiBaru.Text?.Trim();
        var jabatan = LabelJabatanBaru.Text == "Pilih jabatan" ? null : LabelJabatanBaru.Text;
        var tanggalMasuk = PickerTanggalMasukBaru.Date;

        if (string.IsNullOrWhiteSpace(nama))
        {
            await DisplayAlert("Perhatian", "Nama pegawai harus diisi.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(jabatan))
        {
            await DisplayAlert("Perhatian", "Jabatan harus dipilih.", "OK");
            return;
        }
        decimal.TryParse(EntryUpahHarianBaru.Text, out decimal upahHarian);
        decimal.TryParse(EntryUpahMingguanBaru.Text, out decimal upahMingguan);
        bool berhasil = await _api.TambahPegawaiAsync(
            nama,
            jabatan,
            tanggalMasuk,
            upahHarian,
            upahMingguan,
            0,
            0, // persen jamsostek tidak lagi dipakai
            SwitchIkutJamsostekBaru.IsToggled);

        if (!berhasil)
        {
            await DisplayAlert("Gagal", "Pegawai gagal ditambahkan.", "OK");
            return;
        }
        EntryNamaPegawaiBaru.Text = "";
        LabelJabatanBaru.Text = "Pilih jabatan";
        LabelJabatanBaru.TextColor = Colors.Gray;
        EntryUpahHarianBaru.Text = "";
        EntryUpahMingguanBaru.Text = "";
        SwitchIkutJamsostekBaru.IsToggled = true;

        await MuatPegawai();

        await DisplayAlert("Berhasil", "Pegawai berhasil ditambahkan.", "OK");
    }

    private async void OnSimpanPegawaiClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Pegawai pegawai)
        {
            var berhasil = await _api.SimpanTarifPegawaiAsync(pegawai);

            if (berhasil)
                await DisplayAlert("Berhasil", $"Tarif {pegawai.Nama} berhasil diperbarui.", "OK");
            else
                await DisplayAlert("Gagal", "Tarif gagal disimpan.", "OK");
        }
    }
}