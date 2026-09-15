using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _api;

    public LoginPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        LabelError.IsVisible = false;

        var username = EntryUsername.Text?.Trim() ?? string.Empty;
        var password = EntryPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            TampilkanError("Username dan password wajib diisi.");
            return;
        }

        try
        {
            var hasil = await _api.LoginAsync(username, password);

            if (hasil == null)
            {
                TampilkanError("Username atau password salah.");
                return;
            }

            SesiPengguna.Username = hasil.Username;
            SesiPengguna.Nama = hasil.Name;
            SesiPengguna.Token = hasil.Token;
            SesiPengguna.PegawaiId = hasil.PegawaiId;
            SesiPengguna.Role =
                string.Equals(hasil.Role, "admin", StringComparison.OrdinalIgnoreCase)
                    ? RolePengguna.Admin
                    : RolePengguna.User;

            if (Shell.Current is AppShell shell)
            {
                shell.TerapkanMenuSesuaiRole();
            }

            EntryUsername.Text = string.Empty;
            EntryPassword.Text = string.Empty;

            await Shell.Current.GoToAsync("//BerandaPage");
        }
        catch (Exception ex)
        {
            TampilkanError($"Gagal terhubung ke server: {ex.Message}");
        }
    }

    private void TampilkanError(string pesan)
    {
        LabelError.Text = pesan;
        LabelError.IsVisible = true;
    }
}