using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

[QueryProperty(nameof(SlipId), "slipId")]
public partial class RincianSlipGajiPage : ContentPage
{
    private readonly ApiService _api;

    private SlipGaji? _slip;

    public string SlipId { get; set; } = "";

    public RincianSlipGajiPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Form edit potongan hanya untuk admin
        EntryPotonganCat.IsEnabled = SesiPengguna.IsAdmin;
        EntryPotonganPengobatan.IsEnabled = SesiPengguna.IsAdmin;
        EntryPotonganSpsi.IsEnabled = SesiPengguna.IsAdmin;
        ButtonSimpanPotongan.IsVisible = SesiPengguna.IsAdmin;

        // Sembunyikan tombol back bawaan Shell khusus untuk User.
        // Halaman ini diakses dari beberapa tempat berbeda (admin & user),
        // dan tombol back bawaan Shell bisa salah navigasi ke slip admin untuk User.
        if (!SesiPengguna.IsAdmin)
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
        }

        await MuatRincian();
    }

    private async Task MuatRincian()
    {
        if (!int.TryParse(SlipId, out int slipIdInt))
            return;

        var semuaPeriode = await _api.GetDaftarPeriodeAsync();

        foreach (var periode in semuaPeriode)
        {
            var daftarSlip = await _api.GetDetailPeriodeAsync(periode.Id);
            var ditemukan = daftarSlip.FirstOrDefault(s => s.Id == slipIdInt);

            if (ditemukan != null)
            {
                _slip = ditemukan;
                break;
            }
        }

        if (_slip == null)
            return;

        TampilkanData();
    }

    private void TampilkanData()
    {
        if (_slip == null) return;

        LabelNamaPegawai.Text = _slip.Pegawai?.Nama ?? "-";
        LabelJabatanPegawai.Text = _slip.Pegawai?.Jabatan switch
        {
            "sopir" => "sopir",
            "kernet" => "Kernet",
            _ => "-"
        };

        LabelPeriodeSlip.Text = _slip.Periode != null
            ? $"Periode: {_slip.Periode.TanggalMulai:dd MMM yyyy} - {_slip.Periode.TanggalSelesai:dd MMM yyyy}"
            : "";

        // Upah
        decimal rateUpahHarian = _slip.HariKerja > 0 ? _slip.Upah / _slip.HariKerja : 0;
        LabelUpah.Text = FormatAngka(_slip.Upah);
        LabelUpahKeterangan.Text = _slip.HariKerja > 0 ? $"{_slip.HariKerja} x {FormatRp(rateUpahHarian)}" : "";
        LabelUpahSamaDengan.Text = _slip.HariKerja > 0 ? "=Rp." : "";

        // Insentif sopir
        LabelInsentifSupir.Text = FormatAngka(_slip.InsentifSupir);
        LabelInsentifSupirKeterangan.Text = _slip.HariSbgSupir > 0 ? $"{_slip.HariSbgSupir}" : "";
        LabelInsentifSupirSamaDengan.Text = _slip.HariSbgSupir > 0 ? "=Rp." : "";

        // Insentif Kernet
        LabelInsentifKernet.Text = FormatAngka(_slip.InsentifKernet);
        LabelInsentifKernetKeterangan.Text = _slip.HariSbgKernet > 0 ? $"{_slip.HariSbgKernet}" : "";
        LabelInsentifKernetSamaDengan.Text = _slip.HariSbgKernet > 0 ? "=Rp." : "";

        // Tunjangan Makan
        decimal rateMakan = _slip.Hadir > 0 ? _slip.TunjanganMakan / _slip.Hadir : 0;
        LabelTunjanganMakan.Text = FormatAngka(_slip.TunjanganMakan);
        LabelTunjanganMakanKeterangan.Text = _slip.Hadir > 0 ? $"{_slip.Hadir} x {FormatRp(rateMakan)}" : "";
        LabelTunjanganMakanSamaDengan.Text = _slip.Hadir > 0 ? "=Rp." : "";

        // Subsidi Transport
        decimal rateTransport = _slip.Hadir > 0 ? _slip.SubsidiTransport / _slip.Hadir : 0;
        LabelSubsidiTransport.Text = FormatAngka(_slip.SubsidiTransport);
        LabelSubsidiTransportKeterangan.Text = _slip.Hadir > 0 ? $"{_slip.Hadir} x {FormatRp(rateTransport)}" : "";
        LabelSubsidiTransportSamaDengan.Text = _slip.Hadir > 0 ? "=Rp." : "";

        // Tanpa keterangan
        LabelInsentifKir.Text = FormatAngka(_slip.InsentifKir);
        LabelInsentifMuatanBerat.Text = FormatAngka(_slip.InsentifMuatanBerat);
        LabelPremi.Text = FormatAngka(_slip.Premi);
        LabelTotalUpah.Text = FormatAngka(_slip.TotalUpah);

        // Potongan
        LabelJamsostek.Text = FormatAngka(_slip.PotonganJamsostek);
        EntryPotonganCat.Text = _slip.PotonganCat.ToString("0");
        EntryPotonganPengobatan.Text = _slip.PotonganPengobatan.ToString("0");
        EntryPotonganSpsi.Text = _slip.PotonganSpsi.ToString("0");
        LabelTotalPotongan.Text = FormatAngka(_slip.TotalPotongan);
        LabelDibayar.Text = FormatAngka(_slip.Dibayar);
    }

    private static string FormatRp(decimal nilai) => $"Rp {nilai:N0}".Replace(",", ".");

    private static string FormatAngka(decimal nilai) => $"{nilai:N0}".Replace(",", ".");

    private async void OnSimpanPotonganClicked(object sender, EventArgs e)
    {
        if (_slip == null) return;

        decimal.TryParse(EntryPotonganCat.Text, out var cat);
        decimal.TryParse(EntryPotonganPengobatan.Text, out var pengobatan);
        decimal.TryParse(EntryPotonganSpsi.Text, out var spsi);

        _slip.PotonganCat = cat;
        _slip.PotonganPengobatan = pengobatan;
        _slip.PotonganSpsi = spsi;

        var (berhasil, pesan) = await _api.UpdateSlipAsync(_slip);
        if (berhasil)
        {
            await DisplayAlert("Berhasil", "Potongan berhasil disimpan.", "OK");
            await MuatRincian();
        }
        else
        {
            await DisplayAlert("Gagal", "Gagal menyimpan potongan.", "OK");
        }
    }

    private async void OnBagikanPdfClicked(object sender, EventArgs e)
    {
        if (_slip == null) return;

        try
        {
            string path = SlipGajiPdfService.BuatPdf(_slip);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Bagikan Slip Gaji",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Gagal", $"Gagal membuat/membagikan PDF.\n\n{ex.Message}", "OK");
        }
    }
}
