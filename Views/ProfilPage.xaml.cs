using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class ProfilPage : ContentPage
{
    private readonly ApiService _api;

    public ProfilPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        EntryNama.Text = SesiPengguna.Nama;
        EntryUsername.Text = SesiPengguna.Username;

        EntryPasswordLama.Text = string.Empty;
        EntryPasswordBaru.Text = string.Empty;
        EntryKonfirmasiPassword.Text = string.Empty;
        LabelPesan.IsVisible = false;
        LabelPesanProfil.IsVisible = false;
    }

    private async void OnSimpanProfilClicked(object sender, EventArgs e)
    {
        var (ok, pesan) = await _api.UpdateProfilAsync(EntryNama.Text, EntryUsername.Text);
        LabelPesanProfil.Text = pesan;
        LabelPesanProfil.IsVisible = true;
        if (ok)
        {
            SesiPengguna.Nama = EntryNama.Text;
            SesiPengguna.Username = EntryUsername.Text;
        }
    }

    private async void OnSimpanPasswordClicked(object sender, EventArgs e)
    {
        var lama = EntryPasswordLama.Text ?? string.Empty;
        var baru = EntryPasswordBaru.Text ?? string.Empty;
        var konfirmasi = EntryKonfirmasiPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(lama) || string.IsNullOrWhiteSpace(baru) || string.IsNullOrWhiteSpace(konfirmasi))
        {
            TampilkanPesan("Semua kolom wajib diisi.", true);
            return;
        }

        if (baru.Length < 6)
        {
            TampilkanPesan("Password baru minimal 6 karakter.", true);
            return;
        }

        if (baru != konfirmasi)
        {
            TampilkanPesan("Konfirmasi password baru tidak cocok.", true);
            return;
        }

        var (berhasil, pesan) = await _api.UpdatePasswordAsync(lama, baru, konfirmasi);

        TampilkanPesan(pesan, !berhasil);

        if (berhasil)
        {
            EntryPasswordLama.Text = string.Empty;
            EntryPasswordBaru.Text = string.Empty;
            EntryKonfirmasiPassword.Text = string.Empty;
        }
    }

    private void TampilkanPesan(string pesan, bool error)
    {
        LabelPesan.Text = pesan;
        LabelPesan.TextColor = error
            ? (Color)Application.Current!.Resources["MerahHapus"]
            : (Color)Application.Current!.Resources["HijauSukses"];
        LabelPesan.IsVisible = true;
    }

    private void TampilkanPesanProfil(string pesan, bool error)
    {
        LabelPesanProfil.Text = pesan;
        LabelPesanProfil.TextColor = error
            ? (Color)Application.Current!.Resources["MerahHapus"]
            : (Color)Application.Current!.Resources["HijauSukses"];
        LabelPesanProfil.IsVisible = true;
    }
}