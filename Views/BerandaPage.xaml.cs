using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public class AktivitasItem
{
    public string Judul { get; set; } = "";
    public string TanggalText { get; set; } = "";
    public string Status { get; set; } = "";
    public string Inisial { get; set; } = "";
}

public partial class BerandaPage : ContentPage
{
    private readonly ApiService _api;

    public BerandaPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var jam = DateTime.Now.Hour;
        string sapaan = jam < 11 ? "Selamat Pagi" : jam < 15 ? "Selamat Siang" : jam < 19 ? "Selamat Sore" : "Selamat Malam";
        string nama = string.IsNullOrEmpty(SesiPengguna.Nama) ? SesiPengguna.Username : SesiPengguna.Nama;

        bool isAdmin = SesiPengguna.IsAdmin;

        HeaderAdmin.IsVisible = isAdmin;
        HeaderUser.IsVisible = !isAdmin;

        GridStatistik.IsVisible = isAdmin;

        BtnInputPengiriman.IsVisible = isAdmin;
        BtnRiwayatPengiriman.IsVisible = isAdmin;

        SectionAktivitas.IsVisible = !isAdmin;

        if (isAdmin)
        {
            LabelSapaanWaktuAdmin.Text = sapaan;
            LabelNamaUserAdmin.Text = nama;
            await MuatRingkasan();
        }
        else
        {
            LabelSapaanWaktu.Text = sapaan;
            LabelNamaUser.Text = nama;
            await MuatProgresDanAktivitas();
        }
    }

    private async Task MuatRingkasan()
    {
        var daftarPengiriman = await _api.GetRiwayatLengkapAsync();

        var hariIni = DateTime.Today;
        var totalHariIni = daftarPengiriman.Count(p => p.Tanggal.Date == hariIni);
        var totalBiayaSemua = daftarPengiriman.Sum(p => p.TotalBiaya);

        LabelTotalTransaksi.Text = daftarPengiriman.Count.ToString();
        LabelTotalHariIni.Text = totalHariIni.ToString();
        LabelTotalBiaya.Text = $"Rp {totalBiayaSemua:N0}";
    }

    private async Task MuatProgresDanAktivitas()
    {
        if (!SesiPengguna.PegawaiId.HasValue) return;

        var semuaData = await _api.GetRiwayatLengkapAsync();
        int myId = SesiPengguna.PegawaiId.Value;

        var punyaSaya = semuaData
            .Where(p => (p.Sopir?.Id == myId) || (p.Kernet?.Id == myId))
            .ToList();

        var hariIni = DateTime.Today;
        int selisihKeKamis = ((int)hariIni.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
        var kamis = hariIni.AddDays(-selisihKeKamis).Date;
        var rabu = kamis.AddDays(6);

        var periodeIni = punyaSaya.Where(p => p.Tanggal.Date >= kamis && p.Tanggal.Date <= rabu).ToList();
        int hariKerjaUnik = periodeIni.Select(p => p.Tanggal.Date).Distinct().Count();
        if (hariKerjaUnik > 5) hariKerjaUnik = 5;

        LabelHariKerjaText.Text = $"{hariKerjaUnik} dari 5 hari";
        GridBarHariKerja.ColumnDefinitions[0].Width = new GridLength(hariKerjaUnik, GridUnitType.Star);
        GridBarHariKerja.ColumnDefinitions[1].Width = new GridLength(5 - hariKerjaUnik, GridUnitType.Star);

        var terakhir = punyaSaya
            .OrderByDescending(p => p.Tanggal)
            .Take(2)
            .Select(p => new AktivitasItem
            {
                Judul = p.Bengkel ? "Bengkel" : (p.Wilayah?.NamaWilayah ?? "-"),
                TanggalText = p.Tanggal.ToString("dd MMM yyyy"),
                Status = p.Bengkel ? "servis" : "selesai",
                Inisial = p.Bengkel ? "B" : (string.IsNullOrEmpty(p.Wilayah?.NamaWilayah) ? "-" : p.Wilayah!.NamaWilayah.Substring(0, 1))
            })
            .ToList();

        ListAktivitas.ItemsSource = terakhir;
    }

    private async void OnInputPengirimanClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//InputPengirimanPage");
    }

    private async void OnRiwayatClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RiwayatPage");
    }

    private async void OnSlipGajiClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SlipGajiPage");
    }

    private async void OnKartuHariIniTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DetailPengirimanPage?mode=hariini");
    }

    private async void OnKartuTotalTransaksiTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DetailPengirimanPage?mode=semua");
    }

    private async void OnKartuTotalBiayaTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DetailPengirimanPage?mode=semua");
    }
}