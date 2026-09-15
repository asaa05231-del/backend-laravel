using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

[QueryProperty(nameof(PeriodeId), "periodeId")]
public partial class DetailSlipGajiPage : ContentPage
{
    private readonly ApiService _api;

    private List<SlipGaji> _daftarSlip = new();

    public string PeriodeId { get; set; } = "";

    public DetailSlipGajiPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MuatDetailSlip();
    }

    private async Task MuatDetailSlip()
    {
        if (!int.TryParse(PeriodeId, out int periodeIdInt))
            return;

        _daftarSlip = await _api.GetDetailPeriodeAsync(periodeIdInt);

        // User biasa: langsung ke rincian slip miliknya sendiri, GANTIKAN halaman ini di stack
        if (!SesiPengguna.IsAdmin)
        {
            var slipMilikSendiri = _daftarSlip.FirstOrDefault();
            if (slipMilikSendiri != null)
            {
                await Shell.Current.GoToAsync($"../RincianSlipGajiPage?slipId={slipMilikSendiri.Id}");
            }
            return;
        }

        var daftarsopir = _daftarSlip
            .Where(s => s.Pegawai?.Jabatan == "sopir")
            .OrderBy(s => s.Pegawai?.Nama)
            .ToList();

        var daftarKernet = _daftarSlip
            .Where(s => s.Pegawai?.Jabatan == "kernet")
            .OrderBy(s => s.Pegawai?.Nama)
            .ToList();

        Listsopir.ItemsSource = daftarsopir;
        ListKernet.ItemsSource = daftarKernet;

        LabelPeriode.Text = _daftarSlip.Count > 0
            ? $"{_daftarSlip.Count} pegawai dalam periode ini"
            : "";
    }

    private void OnTabsopirClicked(object sender, EventArgs e)
    {
        Listsopir.IsVisible = true;
        ListKernet.IsVisible = false;
        TombolTabsopir.Style = (Style)Application.Current!.Resources["TombolUtama"];
        TombolTabKernet.Style = (Style)Application.Current!.Resources["TombolSekunder"];
    }

    private void OnTabKernetClicked(object sender, EventArgs e)
    {
        Listsopir.IsVisible = false;
        ListKernet.IsVisible = true;
        TombolTabKernet.Style = (Style)Application.Current!.Resources["TombolUtama"];
        TombolTabsopir.Style = (Style)Application.Current!.Resources["TombolSekunder"];
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await MuatDetailSlip();
        RefreshViewSlip.IsRefreshing = false;
    }

    private async void OnSlipDipilih(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not SlipGaji slip)
            return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        await Shell.Current.GoToAsync($"RincianSlipGajiPage?slipId={slip.Id}");
    }
}