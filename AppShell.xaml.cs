using SistemPengirimanApp.Views;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp;

public partial class AppShell : Shell
{
    private Border? _itemAktifSaatIni;
    private static readonly Color WarnaHover = Color.FromArgb("#2A4FBF");
    private static readonly Color WarnaAktif = Color.FromArgb("#F97316"); // AksenOranye

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("MasterDataPegawaiPage", typeof(MasterDataPegawaiPage));
        Routing.RegisterRoute("MasterDataWilayahPage", typeof(MasterDataWilayahPage));
        Routing.RegisterRoute("MasterDataTarifTahunanPage", typeof(MasterDataTarifTahunanPage));
        Routing.RegisterRoute("DetailSlipGajiPage", typeof(DetailSlipGajiPage));
        Routing.RegisterRoute("MasterDataPegawaiPage", typeof(MasterDataPegawaiPage));
        Routing.RegisterRoute("MasterDataWilayahPage", typeof(MasterDataWilayahPage));
        Routing.RegisterRoute("MasterDataTarifTahunanPage", typeof(MasterDataTarifTahunanPage));
        Routing.RegisterRoute("DetailSlipGajiPage", typeof(DetailSlipGajiPage));
        Routing.RegisterRoute("RincianSlipGajiPage", typeof(RincianSlipGajiPage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MuatDataUserFlyout();
        TerapkanMenuSesuaiRole();
    }

    public void TerapkanMenuSesuaiRole()
    {
        bool admin = SesiPengguna.IsAdmin;

        ItemInput.IsVisible = admin;
        ItemMasterData.IsVisible = admin;
        ItemLaporan.IsVisible = admin;
        ItemSummary.IsVisible = admin;
        ItemPengaturan.IsVisible = admin;

        if (!admin)
            StackSubMasterData.IsVisible = false;
    }

    private void MuatDataUserFlyout()
    {
        string nama = !string.IsNullOrWhiteSpace(SesiPengguna.Nama) ? SesiPengguna.Nama : (SesiPengguna.IsAdmin ? "Admin" : "Pegawai");

        LabelNamaUserFlyout.Text = nama;
        LabelPeranUserFlyout.Text = SesiPengguna.IsAdmin ? "Admin" : "Pegawai";
        LabelInisialUser.Text = nama.Substring(0, 1).ToUpper();
    }

    private void OnToggleMasterDataTapped(object sender, EventArgs e)
    {
        bool tampil = !StackSubMasterData.IsVisible;
        StackSubMasterData.IsVisible = tampil;
       
    }

    // ===== Hover (mengikuti kursor, khusus platform dengan mouse) =====
    private void OnItemPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border && border != _itemAktifSaatIni)
            border.BackgroundColor = WarnaHover;
    }

    private void OnItemPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border && border != _itemAktifSaatIni)
            border.BackgroundColor = Color.FromArgb("#01000000");
    }

    // ===== Aktif (persisten, oranye) =====
    private void SetItemAktif(Border item)
    {
        if (_itemAktifSaatIni != null)
            _itemAktifSaatIni.BackgroundColor = Color.FromArgb("#01000000");

        item.BackgroundColor = WarnaAktif;
        _itemAktifSaatIni = item;
    }

    private async Task GoTo(string route, Border itemDiklik)
    {
        SetItemAktif(itemDiklik);
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync(route);
    }

    private async void OnMenuBerandaTapped(object sender, EventArgs e) => await GoTo("//BerandaPage", ItemBeranda);
    private async void OnMenuInputTapped(object sender, EventArgs e) => await GoTo("//InputPengirimanPage", ItemInput);
    private async void OnMenuRiwayatTapped(object sender, EventArgs e) => await GoTo("//RiwayatPage", ItemRiwayat);
    private async void OnMenuSummaryTapped(object sender, EventArgs e) => await GoTo("//SummaryPage", ItemSummary);
    private async void OnMenuSlipGajiTapped(object sender, EventArgs e) => await GoTo("//SlipGajiPage", ItemSlipGaji);
    private async void OnMenuLaporanTapped(object sender, EventArgs e) => await GoTo("//LaporanPage", ItemLaporan);
    private async void OnMenuProfilTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//ProfilPage");

    private async void OnMenuDataPegawaiTapped(object sender, EventArgs e) => await GoTo("MasterDataPegawaiPage", ItemDataPegawai);
    private async void OnMenuWilayahTarifTapped(object sender, EventArgs e) => await GoTo("MasterDataWilayahPage", ItemWilayahTarif);
    private async void OnMenuTarifTahunanTapped(object sender, EventArgs e) => await GoTo("MasterDataTarifTahunanPage", ItemTarifTahunan);

    private async void OnMenuPengaturanTapped(object sender, EventArgs e)
    {
        SetItemAktif(ItemPengaturan);
        await DisplayAlert("Segera Hadir", "Halaman Pengaturan belum tersedia.", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        SesiPengguna.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}