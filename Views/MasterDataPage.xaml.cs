using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class MasterDataPage : ContentPage
{
    public MasterDataPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!SesiPengguna.IsAdmin)
        {
            await DisplayAlert("Akses Ditolak", "Halaman ini khusus untuk Admin.", "OK");
            await Shell.Current.GoToAsync("//BerandaPage");
        }
    }

    private async void OnKartuPegawaiTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MasterDataPegawaiPage");
    }

    private async void OnKartuWilayahTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MasterDataWilayahPage");
    }

    private async void OnKartuTarifTahunanTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MasterDataTarifTahunanPage");
    }
}