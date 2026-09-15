using System.Globalization;
using System.Text.RegularExpressions;
using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class SummaryPage : ContentPage
{
    private readonly ApiService _api;

    private DateTime _tanggalKamis;
    private DateTime _tanggalRabu;
    private PeriodeGaji? _periode;
    private TarifUpahTahunan? _tarifAktif;
    private PengaturanGaji? _pengaturan;

    private List<SlipGaji> _daftarSlip = new();
    private bool _jamsostekAktif = false;
    private List<(SlipGaji Slip, Label LabelJamsostek, Action Refresh)> _barisJamsostek = new();

    private Label? _labelGTotalDibayar;

    private static readonly CultureInfo BudayaId = new("id-ID");
    private static string FormatRp(decimal nilai) => nilai.ToString("#,##0", BudayaId);
    private static string FormatRp(long nilai) => nilai.ToString("#,##0", BudayaId);

    private static readonly (string Header, double Width)[] KolomBeku = new[]
    {
        ("No", 40.0),
        ("Nama", 160.0),
    };

    private static readonly (string Header, double Width)[] KolomScroll = new[]
    {
        ("Unit", 100.0),
        ("Masuk", 90.0),
        ("Hk(h)", 65.0),
        ("Upah/Mgg", 100.0),
        ("Upah/Hari", 90.0),
        ("Upah", 100.0),
        ("Hk sopir", 75.0),
        ("sopir", 100.0),
        ("Hk Kernet", 80.0),
        ("Kernet", 100.0),
        ("Total Ins.\nPengiriman", 105.0),
        ("Hadir", 65.0),
        ("Tunj.\nMakan", 100.0),
        ("Tunj.\nTransport", 105.0),
        ("Ins. KIR", 90.0),
        ("Muatan\nBerat", 105.0),
        ("Premi", 95.0),
        ("Total\nUpah", 110.0),
        ("Jamsostek", 95.0),
        ("Cat", 80.0),
        ("Pengobatan", 100.0),
        ("SPSI", 80.0),
        ("Total\nPotongan", 110.0),
        ("Dibayar", 115.0),
    };

    private const double TinggiBaris = 52.0;
    private static readonly Color GarisAbu = Color.FromArgb("#E5E7EB");
 
    public SummaryPage(ApiService api)
    {
        InitializeComponent();
        _api = api;

        HitungPeriodeDariTanggal(DateTime.Today);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MuatData();
    }

 

    private void HitungPeriodeDariTanggal(DateTime tanggalDipilih)
    {
        int selisihKeKamis = ((int)tanggalDipilih.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
        _tanggalKamis = tanggalDipilih.AddDays(-selisihKeKamis).Date;
        _tanggalRabu = _tanggalKamis.AddDays(6);
    }

    private async Task MuatData()
    {
        LabelPeriode.Text = $"{_tanggalKamis:dd/MM/yyyy} - {_tanggalRabu:dd/MM/yyyy}";

        var semuaPeriode = await _api.GetDaftarPeriodeAsync();
        _periode = semuaPeriode.FirstOrDefault(p =>
            p.TanggalMulai.Date == _tanggalKamis && p.TanggalSelesai.Date == _tanggalRabu);

        var semuaTarif = await _api.GetTarifTahunanAsync();
        _tarifAktif = semuaTarif.FirstOrDefault(t => t.Tahun == _tanggalKamis.Year);

        _pengaturan = await _api.GetPengaturanGajiAsync();

        if (_periode == null)
        {
            _daftarSlip = new List<SlipGaji>();
            LabelInfo.Text = "Belum ada slip gaji untuk periode ini. Tekan Generate dulu.";
            BangunTabel();
            return;
        }

        _daftarSlip = await _api.GetDetailPeriodeAsync(_periode.Id);

        LabelInfo.Text = _tarifAktif == null
            ? $"{_daftarSlip.Count} pegawai — PERHATIAN: Tarif Upah tahun {_tanggalKamis.Year} belum diatur di Master Data."
            : $"{_daftarSlip.Count} pegawai dalam periode ini";

        BangunTabel();
    }

    private void OnPeriodeSebelumnyaClicked(object sender, EventArgs e)
    {
        _tanggalKamis = _tanggalKamis.AddDays(-7);
        _tanggalRabu = _tanggalRabu.AddDays(-7);
        _ = MuatData();
    }

    private void OnPeriodeBerikutnyaClicked(object sender, EventArgs e)
    {
        _tanggalKamis = _tanggalKamis.AddDays(7);
        _tanggalRabu = _tanggalRabu.AddDays(7);
        _ = MuatData();
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        bool berhasil = await _api.GenerateSlipGajiAsync(_tanggalKamis, _tanggalRabu);

        if (berhasil)
            await MuatData();
        else
            await DisplayAlert("Gagal", "Gagal generate. Pastikan ada data pengiriman di periode ini dan Tarif Upah Tahunan sudah diatur.", "OK");
    }

    private async void OnSimpanSemuaClicked(object sender, EventArgs e)
    {
        if (_daftarSlip.Count == 0) return;

        ButtonSimpanSemua.IsEnabled = false;
        ButtonSimpanSemua.Text = "Menyimpan...";

        int gagal = 0;
        string pesanErrorPertama = "";
        foreach (var slip in _daftarSlip)
        {
            var (ok, pesan) = await _api.UpdateSlipAsync(slip);
            if (!ok)
            {
                gagal++;
                if (pesanErrorPertama == "") pesanErrorPertama = $"{slip.Pegawai?.Nama}: {pesan}";
            }
        }

        ButtonSimpanSemua.IsEnabled = true;
        ButtonSimpanSemua.Text = "Simpan Semua Perubahan";

        if (gagal == 0)
            await DisplayAlert("Berhasil", "Semua perubahan berhasil disimpan.", "OK");
        else
            await DisplayAlert("Sebagian Gagal", $"{gagal} data gagal disimpan.\n\n{pesanErrorPertama}", "OK");

        await MuatData();
    }

    private static long ParseAngka(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        var bersih = Regex.Replace(text, "[^0-9]", "");
        return long.TryParse(bersih, out var hasil) ? hasil : 0;
    }

    private void BangunTabel()
    {
        GridBeku.Children.Clear();
        GridBeku.RowDefinitions.Clear();
        GridBeku.ColumnDefinitions.Clear();

        GridTabel.Children.Clear();
        GridTabel.RowDefinitions.Clear();
        GridTabel.ColumnDefinitions.Clear();

        _labelGTotalDibayar = null;
        _barisJamsostek.Clear();
        _jamsostekAktif = _daftarSlip.Count > 0 && _daftarSlip.All(s => s.PotonganJamsostek > 0);

        foreach (var k in KolomBeku)
            GridBeku.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(k.Width)));
        foreach (var k in KolomScroll)
            GridTabel.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(k.Width)));

        int baris = 0;
        TambahBarisHeader(ref baris);
    
        var listsopir = _daftarSlip.Where(s => s.Pegawai?.Jabatan == "sopir").OrderBy(s => s.Pegawai?.Nama).ToList();
        var listKernet = _daftarSlip.Where(s => s.Pegawai?.Jabatan == "kernet").OrderBy(s => s.Pegawai?.Nama).ToList();

        TambahGrupPegawai(ref baris, "sopir", listsopir);
        TambahGrupPegawai(ref baris, "KERNET", listKernet);

        TambahBarisGTotal(ref baris, _daftarSlip);
    }

    private void TambahRowKedua(ref int baris)
    {
        GridBeku.RowDefinitions.Add(new RowDefinition(new GridLength(TinggiBaris)));
        GridTabel.RowDefinitions.Add(new RowDefinition(new GridLength(TinggiBaris)));
        baris++;
    }

    private Border SelBeku(View isi, int row, int col, Color? bg = null)
    {
        var border = new Border { Stroke = GarisAbu, StrokeThickness = 1, Padding = 0, BackgroundColor = bg ?? Colors.White, Content = isi };
        Grid.SetRow(border, row);
        Grid.SetColumn(border, col);
        GridBeku.Children.Add(border);
        return border;
    }

    private Border SelScroll(View isi, int row, int col, Color? bg = null)
    {
        var border = new Border { Stroke = GarisAbu, StrokeThickness = 1, Padding = 0, BackgroundColor = bg ?? Colors.White, Content = isi };
        Grid.SetRow(border, row);
        Grid.SetColumn(border, col);
        GridTabel.Children.Add(border);
        return border;
    }

    private Label LabelSel(string text, bool bold = false, Color? warna = null)
    {
        return new Label
        {
            Text = text,
            FontSize = 14,
            FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
            TextColor = warna ?? (Color)Application.Current!.Resources["HitamTeks"],
            Padding = new Thickness(8, 6),
            LineBreakMode = LineBreakMode.WordWrap,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };
    }

    private Entry EntrySel(long nilai)
    {
        return new Entry
        {
            Text = FormatRp(nilai),
            FontSize = 14,
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 2),
        };
    }

    private void TambahBarisHeader(ref int baris)
    {
        int row = baris;
        TambahRowKedua(ref baris);
        var biru = (Color)Application.Current!.Resources["BiruUtama"];

        for (int c = 0; c < KolomBeku.Length; c++)
            SelBeku(LabelSel(KolomBeku[c].Header, bold: true, warna: Colors.White), row, c, biru);
        for (int c = 0; c < KolomScroll.Length; c++)
        {
            if (c == 18) // kolom Jamsostek
            {
                var checkboxJamsostek = new CheckBox
                {
                    Color = Colors.White,
                    IsChecked = _jamsostekAktif,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = 24,
                    HeightRequest = 24
                };
                checkboxJamsostek.CheckedChanged += (s, e) =>
                {
                    _jamsostekAktif = e.Value;
                    foreach (var (slip, label, refresh) in _barisJamsostek)
                        refresh();
                    RefreshGTotal();
                };

                var wrapperCheckbox = new HorizontalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { checkboxJamsostek }
                };

                var isiHeader = new VerticalStackLayout
                {
                    Spacing = 2,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
    {
        new Label { Text = "Jamsostek", FontAttributes = FontAttributes.Bold, TextColor = Colors.White, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center },
        wrapperCheckbox
    }
                };
                SelScroll(isiHeader, row, c, biru);
                continue;
            }

            SelScroll(LabelSel(KolomScroll[c].Header, bold: true, warna: Colors.White), row, c, biru);
        }
    }

    private void TambahGrupPegawai(ref int baris, string judulGrup, List<SlipGaji> daftar)
    {
        int no = 1;
        foreach (var slip in daftar)
            TambahBarisPegawai(ref baris, no++, slip);

        TambahBarisTotalGrup(ref baris, daftar, "TOTAL");
    }

    private void TambahBarisPegawai(ref int baris, int no, SlipGaji slip)
    {
        int row = baris;
        TambahRowKedua(ref baris);

        var pegawai = slip.Pegawai;
        bool sopir = pegawai?.Jabatan != "kernet";
        string unit = sopir ? "HRD sopir" : "HRD Kernet";
        string masuk = pegawai?.TanggalMasuk != null ? pegawai.TanggalMasuk.Value.ToString("dd MMM yy") : "-";

        decimal upahHariRef = _tarifAktif == null ? 0 : (sopir ? _tarifAktif.UpahHariansupir : _tarifAktif.UpahHarianKernet);
        decimal upahMingguRef = _tarifAktif == null ? 0 : (sopir ? _tarifAktif.UpahMingguansupir : _tarifAktif.UpahMingguanKernet);

        SelBeku(LabelSel(no.ToString()), row, 0);
        SelBeku(LabelSel(pegawai?.Nama ?? "-"), row, 1);

        SelScroll(LabelSel(unit), row, 0);
        SelScroll(LabelSel(masuk), row, 1);

        // Hk(h): bisa diedit admin (misal cuti jadi 4), Upah otomatis ikut berubah
        var entryHariKerja = EntrySel(slip.HariKerja);
        SelScroll(entryHariKerja, row, 2);

        SelScroll(LabelSel(FormatRp(upahMingguRef)), row, 3);
        SelScroll(LabelSel(FormatRp(upahHariRef)), row, 4);

        var labelUpah = LabelSel(FormatRp(slip.Upah));
        SelScroll(labelUpah, row, 5);

        // Ttl Hk & Insentif sopir/Kernet: read-only, otomatis dari Riwayat Pengiriman
        SelScroll(LabelSel(slip.HariSbgSupir.ToString()), row, 6);
        SelScroll(LabelSel(FormatRp(slip.InsentifSupir)), row, 7);
        SelScroll(LabelSel(slip.HariSbgKernet.ToString()), row, 8);
        SelScroll(LabelSel(FormatRp(slip.InsentifKernet)), row, 9);

        var labelTotalIns = LabelSel(FormatRp(slip.InsentifSupir + slip.InsentifKernet));
        SelScroll(labelTotalIns, row, 10);

        var entryHadir = EntrySel(slip.Hadir);
        SelScroll(entryHadir, row, 11);

        // Tunjangan Makan & Transport: otomatis dari Hadir x rate harian
        var rateMakan = _tarifAktif?.TunjanganMakanHarian ?? 0;
        var rateTransport = _tarifAktif?.SubsidiTransportHarian ?? 0;

        var entryTunjanganMakan = EntrySel((long)slip.TunjanganMakan);
        var entryTunjanganTransport = EntrySel((long)slip.SubsidiTransport);

        var entryPremi = EntrySel((long)slip.Premi);
        var entryCat = EntrySel((long)slip.PotonganCat);
        var entryPengobatan = EntrySel((long)slip.PotonganPengobatan);
        var entrySpsi = EntrySel((long)slip.PotonganSpsi);

        var labelTotalUpah = LabelSel(FormatRp(slip.TotalUpah), bold: true);
        var labelTotalPotongan = LabelSel(FormatRp(slip.TotalPotongan));
        var labelDibayar = LabelSel(FormatRp(slip.Dibayar), bold: true, warna: (Color)Application.Current!.Resources["AksenOranye"]);
        var labelJamsostek = LabelSel(FormatRp(slip.PotonganJamsostek));

        bool sedangSinkronTunjangan = false;

        void HitungUlangTotal()
        {
            long premi = ParseAngka(entryPremi.Text);
            long cat = ParseAngka(entryCat.Text);
            long pengobatan = ParseAngka(entryPengobatan.Text);
            long spsi = ParseAngka(entrySpsi.Text);

            slip.Premi = premi;
            slip.PotonganCat = cat;
            slip.PotonganPengobatan = pengobatan;
            slip.PotonganSpsi = spsi;

            decimal totalIns = slip.InsentifSupir + slip.InsentifKernet;
            decimal totalUpah = slip.Upah + totalIns + slip.TunjanganMakan + slip.SubsidiTransport
                + slip.InsentifKir + slip.InsentifMuatanBerat + premi;

            long jamsostek = _jamsostekAktif
                ? (sopir ? (long)(_pengaturan?.JamsostekSupir ?? 0) : (long)(_pengaturan?.JamsostekKernet ?? 0))
                : 0; slip.PotonganJamsostek = jamsostek;
            decimal totalPotongan = slip.PotonganJamsostek + cat + pengobatan + spsi;
            decimal dibayar = totalUpah - totalPotongan;

            slip.TotalUpah = totalUpah;
            slip.TotalPotongan = totalPotongan;
            slip.Dibayar = dibayar;

            labelTotalUpah.Text = FormatRp(totalUpah);
            labelTotalPotongan.Text = FormatRp(totalPotongan);
            labelDibayar.Text = FormatRp(dibayar);
            labelJamsostek.Text = FormatRp(jamsostek);

            RefreshGTotal();
        }

        void RefreshRow()
        {
            long hariKerja = ParseAngka(entryHariKerja.Text);
            long hadir = ParseAngka(entryHadir.Text);
            decimal upahBaru = hariKerja * upahHariRef;

            slip.HariKerja = (int)hariKerja;
            slip.Hadir = (int)hadir;
            slip.Upah = upahBaru;
            labelUpah.Text = FormatRp(upahBaru);

            // Tunjangan Makan & Transport otomatis ikut Hadir
            decimal tunjMakan = hadir * rateMakan;
            decimal tunjTransport = hadir * rateTransport;

            sedangSinkronTunjangan = true;
            entryTunjanganMakan.Text = FormatRp((long)tunjMakan);
            entryTunjanganTransport.Text = FormatRp((long)tunjTransport);
            sedangSinkronTunjangan = false;

            slip.TunjanganMakan = tunjMakan;
            slip.SubsidiTransport = tunjTransport;

            HitungUlangTotal();
        }

        void OnTunjanganDieditManual()
        {
            if (sedangSinkronTunjangan) return; // ini perubahan otomatis dari Hadir, bukan ketikan admin

            slip.TunjanganMakan = ParseAngka(entryTunjanganMakan.Text);
            slip.SubsidiTransport = ParseAngka(entryTunjanganTransport.Text);
            HitungUlangTotal();
        }

        entryHariKerja.TextChanged += (s, e) => RefreshRow();
        entryHadir.TextChanged += (s, e) => RefreshRow();
        entryTunjanganMakan.TextChanged += (s, e) => OnTunjanganDieditManual();
        entryTunjanganTransport.TextChanged += (s, e) => OnTunjanganDieditManual();
        entryPremi.TextChanged += (s, e) => HitungUlangTotal();
        entryCat.TextChanged += (s, e) => HitungUlangTotal();
        entryPengobatan.TextChanged += (s, e) => HitungUlangTotal();
        entrySpsi.TextChanged += (s, e) => HitungUlangTotal();
        _barisJamsostek.Add((slip, labelJamsostek, HitungUlangTotal));

        SelScroll(entryTunjanganMakan, row, 12);
        SelScroll(entryTunjanganTransport, row, 13);
        SelScroll(LabelSel(FormatRp(slip.InsentifKir)), row, 14);
        SelScroll(LabelSel(FormatRp(slip.InsentifMuatanBerat)), row, 15);
        SelScroll(entryPremi, row, 16);
        SelScroll(labelTotalUpah, row, 17);
        SelScroll(labelJamsostek, row, 18);
        SelScroll(entryCat, row, 19);
        SelScroll(entryPengobatan, row, 20);
        SelScroll(entrySpsi, row, 21);
        SelScroll(labelTotalPotongan, row, 22);
        SelScroll(labelDibayar, row, 23);
    }

    private void RefreshGTotal()
    {
        if (_labelGTotalDibayar == null) return;
        _labelGTotalDibayar.Text = FormatRp(_daftarSlip.Sum(s => s.Dibayar));
    }

    private void TambahBarisGTotal(ref int baris, List<SlipGaji> semua)
    {
        if (semua.Count == 0) return;

        int row = baris;
        TambahRowKedua(ref baris);

        var biru = (Color)Application.Current!.Resources["BiruUtama"];

        Grid.SetColumnSpan(SelBeku(LabelSel("G.TOTAL", bold: true, warna: Colors.White), row, 0, biru), KolomBeku.Length);

        long totalHariKerja = semua.Sum(s => (long)s.HariKerja);
        decimal totalUpah = semua.Sum(s => s.Upah);
        long totalHkSupir = semua.Sum(s => (long)s.HariSbgSupir);
        decimal totalInsSupir = semua.Sum(s => s.InsentifSupir);
        long totalHkKernet = semua.Sum(s => (long)s.HariSbgKernet);
        decimal totalInsKernet = semua.Sum(s => s.InsentifKernet);
        decimal totalInsPengiriman = totalInsSupir + totalInsKernet;
        long totalHadir = semua.Sum(s => (long)s.Hadir);
        decimal totalTunjMakan = semua.Sum(s => s.TunjanganMakan);
        decimal totalTunjTransport = semua.Sum(s => s.SubsidiTransport);
        decimal totalInsKir = semua.Sum(s => s.InsentifKir);
        decimal totalMuatanBerat = semua.Sum(s => s.InsentifMuatanBerat);
        decimal totalPremi = semua.Sum(s => s.Premi);
        decimal totalUpahAkhir = semua.Sum(s => s.TotalUpah);
        decimal totalJamsostek = semua.Sum(s => s.PotonganJamsostek);
        decimal totalCat = semua.Sum(s => s.PotonganCat);
        decimal totalPengobatan = semua.Sum(s => s.PotonganPengobatan);
        decimal totalSpsi = semua.Sum(s => s.PotonganSpsi);
        decimal totalPotongan = semua.Sum(s => s.TotalPotongan);
        decimal totalDibayar = semua.Sum(s => s.Dibayar);

        SelScroll(LabelSel(""), row, 0, biru);
        SelScroll(LabelSel(""), row, 1, biru);
        SelScroll(LabelSel(totalHariKerja.ToString(), bold: true, warna: Colors.White), row, 2, biru);
        SelScroll(LabelSel(""), row, 3, biru);
        SelScroll(LabelSel(""), row, 4, biru);
        SelScroll(LabelSel(FormatRp(totalUpah), bold: true, warna: Colors.White), row, 5, biru);
        SelScroll(LabelSel(totalHkSupir.ToString(), bold: true, warna: Colors.White), row, 6, biru);
        SelScroll(LabelSel(FormatRp(totalInsSupir), bold: true, warna: Colors.White), row, 7, biru);
        SelScroll(LabelSel(totalHkKernet.ToString(), bold: true, warna: Colors.White), row, 8, biru);
        SelScroll(LabelSel(FormatRp(totalInsKernet), bold: true, warna: Colors.White), row, 9, biru);
        SelScroll(LabelSel(FormatRp(totalInsPengiriman), bold: true, warna: Colors.White), row, 10, biru);
        SelScroll(LabelSel(totalHadir.ToString(), bold: true, warna: Colors.White), row, 11, biru);
        SelScroll(LabelSel(FormatRp(totalTunjMakan), bold: true, warna: Colors.White), row, 12, biru);
        SelScroll(LabelSel(FormatRp(totalTunjTransport), bold: true, warna: Colors.White), row, 13, biru);
        SelScroll(LabelSel(FormatRp(totalInsKir), bold: true, warna: Colors.White), row, 14, biru);
        SelScroll(LabelSel(FormatRp(totalMuatanBerat), bold: true, warna: Colors.White), row, 15, biru);
        SelScroll(LabelSel(FormatRp(totalPremi), bold: true, warna: Colors.White), row, 16, biru);
        SelScroll(LabelSel(FormatRp(totalUpahAkhir), bold: true, warna: Colors.White), row, 17, biru);
        SelScroll(LabelSel(FormatRp(totalJamsostek), bold: true, warna: Colors.White), row, 18, biru);
        SelScroll(LabelSel(FormatRp(totalCat), bold: true, warna: Colors.White), row, 19, biru);
        SelScroll(LabelSel(FormatRp(totalPengobatan), bold: true, warna: Colors.White), row, 20, biru);
        SelScroll(LabelSel(FormatRp(totalSpsi), bold: true, warna: Colors.White), row, 21, biru);
        SelScroll(LabelSel(FormatRp(totalPotongan), bold: true, warna: Colors.White), row, 22, biru);

        var labelDibayarGTotal = LabelSel(FormatRp(totalDibayar), bold: true, warna: (Color)Application.Current!.Resources["AksenOranye"]);
        SelScroll(labelDibayarGTotal, row, 23, biru);

        _labelGTotalDibayar = labelDibayarGTotal;
    }
   
    private void TambahBarisTotalGrup(ref int baris, List<SlipGaji> daftar, string label)
    {
        if (daftar.Count == 0) return;

        int row = baris;
        TambahRowKedua(ref baris);

        var bgAbu = Color.FromArgb("#E3E8F2");
        var biruUtama = (Color)Application.Current!.Resources["BiruUtama"];

        Grid.SetColumnSpan(SelBeku(LabelSel(label, bold: true, warna: biruUtama), row, 0, bgAbu), KolomBeku.Length);

        long totalHariKerja = daftar.Sum(s => (long)s.HariKerja);
        decimal totalUpah = daftar.Sum(s => s.Upah);
        long totalHkSupir = daftar.Sum(s => (long)s.HariSbgSupir);
        decimal totalInsSupir = daftar.Sum(s => s.InsentifSupir);
        long totalHkKernet = daftar.Sum(s => (long)s.HariSbgKernet);
        decimal totalInsKernet = daftar.Sum(s => s.InsentifKernet);
        decimal totalInsPengiriman = totalInsSupir + totalInsKernet;
        long totalHadir = daftar.Sum(s => (long)s.Hadir);
        decimal totalTunjMakan = daftar.Sum(s => s.TunjanganMakan);
        decimal totalTunjTransport = daftar.Sum(s => s.SubsidiTransport);
        decimal totalInsKir = daftar.Sum(s => s.InsentifKir);
        decimal totalMuatanBerat = daftar.Sum(s => s.InsentifMuatanBerat);
        decimal totalPremi = daftar.Sum(s => s.Premi);
        decimal totalUpahAkhir = daftar.Sum(s => s.TotalUpah);
        decimal totalJamsostek = daftar.Sum(s => s.PotonganJamsostek);
        decimal totalCat = daftar.Sum(s => s.PotonganCat);
        decimal totalPengobatan = daftar.Sum(s => s.PotonganPengobatan);
        decimal totalSpsi = daftar.Sum(s => s.PotonganSpsi);
        decimal totalPotongan = daftar.Sum(s => s.TotalPotongan);
        decimal totalDibayar = daftar.Sum(s => s.Dibayar);

        SelScroll(LabelSel(""), row, 0, bgAbu);
        SelScroll(LabelSel(""), row, 1, bgAbu);
        SelScroll(LabelSel(totalHariKerja.ToString(), bold: true), row, 2, bgAbu);
        SelScroll(LabelSel(""), row, 3, bgAbu);
        SelScroll(LabelSel(""), row, 4, bgAbu);
        SelScroll(LabelSel(FormatRp(totalUpah), bold: true), row, 5, bgAbu);
        SelScroll(LabelSel(totalHkSupir.ToString(), bold: true), row, 6, bgAbu);
        SelScroll(LabelSel(FormatRp(totalInsSupir), bold: true), row, 7, bgAbu);
        SelScroll(LabelSel(totalHkKernet.ToString(), bold: true), row, 8, bgAbu);
        SelScroll(LabelSel(FormatRp(totalInsKernet), bold: true), row, 9, bgAbu);
        SelScroll(LabelSel(FormatRp(totalInsPengiriman), bold: true), row, 10, bgAbu);
        SelScroll(LabelSel(totalHadir.ToString(), bold: true), row, 11, bgAbu);
        SelScroll(LabelSel(FormatRp(totalTunjMakan), bold: true), row, 12, bgAbu);
        SelScroll(LabelSel(FormatRp(totalTunjTransport), bold: true), row, 13, bgAbu);
        SelScroll(LabelSel(FormatRp(totalInsKir), bold: true), row, 14, bgAbu);
        SelScroll(LabelSel(FormatRp(totalMuatanBerat), bold: true), row, 15, bgAbu);
        SelScroll(LabelSel(FormatRp(totalPremi), bold: true), row, 16, bgAbu);
        SelScroll(LabelSel(FormatRp(totalUpahAkhir), bold: true), row, 17, bgAbu);
        SelScroll(LabelSel(FormatRp(totalJamsostek), bold: true), row, 18, bgAbu);
        SelScroll(LabelSel(FormatRp(totalCat), bold: true), row, 19, bgAbu);
        SelScroll(LabelSel(FormatRp(totalPengobatan), bold: true), row, 20, bgAbu);
        SelScroll(LabelSel(FormatRp(totalSpsi), bold: true), row, 21, bgAbu);
        SelScroll(LabelSel(FormatRp(totalPotongan), bold: true), row, 22, bgAbu);
        SelScroll(LabelSel(FormatRp(totalDibayar), bold: true, warna: (Color)Application.Current!.Resources["AksenOranye"]), row, 23, bgAbu);
    }
}