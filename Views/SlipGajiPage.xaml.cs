using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class SlipGajiPage : ContentPage
{
    private readonly ApiService _api;

    private DateTime _tanggalKamis;
    private DateTime _tanggalRabu;

    private PeriodeGaji? _periodeAdaDiDb;

    public SlipGajiPage(ApiService api)
    {
        InitializeComponent();
        _api = api;

        HitungPeriodeDariTanggal(DateTime.Today);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await TampilkanPeriode();
    }

    private void HitungPeriodeDariTanggal(DateTime tanggalDipilih)
    {
        // Periode gaji: Kamis s/d Rabu (7 hari kalender)
        int selisihKeKamis = ((int)tanggalDipilih.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
        _tanggalKamis = tanggalDipilih.AddDays(-selisihKeKamis).Date;
        _tanggalRabu = _tanggalKamis.AddDays(6);
    }

    private async Task TampilkanPeriode()
    {
        LabelPeriode.Text = $"{_tanggalKamis:dd/MM/yyyy} - {_tanggalRabu:dd/MM/yyyy}";
        var semuaPeriode = await _api.GetDaftarPeriodeAsync();
        _periodeAdaDiDb = semuaPeriode.FirstOrDefault(p =>
            p.TanggalMulai.Date == _tanggalKamis && p.TanggalSelesai.Date == _tanggalRabu);

        bool isAdmin = SesiPengguna.IsAdmin;

        // Cari tanggal selesai paling baru dari seluruh periode yang ada di database
        DateTime? tglSelesaiTerbaru = semuaPeriode.Count > 0
            ? semuaPeriode.Max(p => p.TanggalSelesai.Date)
            : (DateTime?)null;

        // Periode ini dianggap "terbaru" kalau tanggal selesainya >= periode paling baru di DB
        // (atau belum ada periode sama sekali di DB)
        bool iniPeriodeTerbaru = tglSelesaiTerbaru == null || _tanggalRabu.Date >= tglSelesaiTerbaru.Value;

        // Boleh lihat kalau: admin, ATAU bukan periode terbaru (periode lama selalu boleh), ATAU hari ini Jumat
        bool bolehLihatHariIni = isAdmin || !iniPeriodeTerbaru || DateTime.Today.DayOfWeek == DayOfWeek.Friday;

        if (!bolehLihatHariIni)
        {
            LabelStatusPeriode.Text = "Slip gaji periode terbaru hanya bisa dibuka pada hari Jumat. Periode sebelumnya tetap bisa dilihat kapan saja.";
            ButtonGenerate.IsVisible = false;
            ButtonLihatDetail.IsVisible = false;
            CardTotalGaji.IsVisible = false;
            return;
        }


        if (_periodeAdaDiDb != null)
        {
            LabelStatusPeriode.Text = "Slip gaji untuk periode ini sudah pernah dibuat. Generate ulang akan memperbarui datanya dengan data pengiriman terbaru.";
            ButtonGenerate.Text = "Generate / Perbarui Slip Gaji";
            ButtonLihatDetail.IsVisible = true;
        }
        else
        {
            LabelStatusPeriode.Text = isAdmin
                ? "Slip gaji untuk periode ini belum dibuat."
                : "Slip gaji untuk periode ini belum tersedia.";
            ButtonGenerate.Text = "Generate Slip Gaji Periode Ini";
            ButtonLihatDetail.IsVisible = false;
        }

        ButtonGenerate.IsVisible = isAdmin;

        await TampilkanTotalGaji(isAdmin);
    }

    private async Task TampilkanTotalGaji(bool isAdmin)
    {
        if (_periodeAdaDiDb == null)
        {
            CardTotalGaji.IsVisible = false;
            return;
        }

        var daftarSlip = await _api.GetDetailPeriodeAsync(_periodeAdaDiDb.Id);
        decimal totalGaji = daftarSlip.Sum(s => s.Dibayar);

        LabelJudulTotalGaji.Text = isAdmin
            ? "Total Gaji Periode Ini (Semua Pegawai)"
            : "Gaji Kamu Periode Ini";

        LabelTotalGaji.Text = FormatRp(totalGaji);
        CardTotalGaji.IsVisible = true;
    }

    private static string FormatRp(decimal nilai) => $"Rp {nilai:N0}".Replace(",", ".");

    private void OnPeriodeSebelumnyaClicked(object sender, EventArgs e)
    {
        _tanggalKamis = _tanggalKamis.AddDays(-7);
        _tanggalRabu = _tanggalRabu.AddDays(-7);
        _ = TampilkanPeriode();
    }

    private void OnPeriodeBerikutnyaClicked(object sender, EventArgs e)
    {
        _tanggalKamis = _tanggalKamis.AddDays(7);
        _tanggalRabu = _tanggalRabu.AddDays(7);
        _ = TampilkanPeriode();
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        bool berhasil = await _api.GenerateSlipGajiAsync(_tanggalKamis, _tanggalRabu);

        if (berhasil)
        {
            await DisplayAlert("Berhasil", $"Slip gaji berhasil digenerate untuk periode {_tanggalKamis:dd/MM/yyyy} - {_tanggalRabu:dd/MM/yyyy}.", "OK");
            await TampilkanPeriode();
        }
        else
        {
            await DisplayAlert("Gagal", "Gagal generate slip gaji. Pastikan ada data pengiriman di periode ini.", "OK");
        }
    }

    private async void OnLihatDetailClicked(object sender, EventArgs e)
    {
        if (_periodeAdaDiDb == null) return;

        bool isAdmin = SesiPengguna.IsAdmin;

        if (!isAdmin)
        {
            var semuaPeriode = await _api.GetDaftarPeriodeAsync();
            DateTime? tglSelesaiTerbaru = semuaPeriode.Count > 0
                ? semuaPeriode.Max(p => p.TanggalSelesai.Date)
                : (DateTime?)null;

            bool iniPeriodeTerbaru = tglSelesaiTerbaru == null || _tanggalRabu.Date >= tglSelesaiTerbaru.Value;
            bool bolehLihatHariIni = !iniPeriodeTerbaru || DateTime.Today.DayOfWeek == DayOfWeek.Friday;

            if (!bolehLihatHariIni)
            {
                await DisplayAlert("Belum Bisa Dibuka", "Slip gaji periode terbaru hanya bisa dibuka pada hari Jumat. Periode sebelumnya tetap bisa dilihat kapan saja.", "OK");
                return;
            }
        }

        if (isAdmin)
        {
            await Shell.Current.GoToAsync($"DetailSlipGajiPage?periodeId={_periodeAdaDiDb.Id}");
            return;
        }

        var daftarSlip = await _api.GetDetailPeriodeAsync(_periodeAdaDiDb.Id);
        var slipMilikSendiri = daftarSlip.FirstOrDefault();
        if (slipMilikSendiri == null)
        {
            await DisplayAlert("Info", "Slip gaji kamu belum tersedia di periode ini.", "OK");
            return;
        }
        await Shell.Current.GoToAsync($"RincianSlipGajiPage?slipId={slipMilikSendiri.Id}");
    }
}