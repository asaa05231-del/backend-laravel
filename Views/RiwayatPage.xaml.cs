using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class RiwayatPage : ContentPage
{
    private readonly ApiService _api;

    private List<Pengiriman> _semuaData = new();
    private List<Pegawai> _semuaPegawai = new();

    private DateTime _tanggalKamis;
    private DateTime _tanggalRabu;

    // ===== Pasangan tetap sopir-kernet, sesuai data ledger fisik =====
    // Isi null di salah satu sisi kalau memang tidak ada pasangannya di ledger.
    private static readonly (string? Sopir, string? Kernet)[] PasanganTetap = new (string?, string?)[]
 {
    ("Agus Rohman", "Helirianto"),
    ("Budi Bahtiar", "Prawito"),
    ("Enjang Kartono", "Indra Sutrisno"),
    ("Imang Supriana", "Suratno"),
    (null, "Endriyanto"),
    ("Kamdani", "Saliman"),
    ("Kusna", "Surja"),
    (null, "Andri Suhendra"),
    ("Sarmili", "Agus Susanto"),
    ("Surahman", "Yunidar Sigit"),
    ("Tatang Komarudin", null),
    ("Udi Prasetyo", "Hepriandi"),
 };

    public RiwayatPage(ApiService api)
    {
        InitializeComponent();
        _api = api;

        HitungPeriodeDariTanggal(DateTime.Today);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool admin = SesiPengguna.IsAdmin;
        GridHeaderKategori.IsVisible = admin;
        GridKolomAdmin.IsVisible = admin;
        StackIndividual.IsVisible = !admin;

        await MuatSemuaData();
        TampilkanPeriode();
    }

    private async Task MuatSemuaData()
    {
        _semuaData = await _api.GetRiwayatLengkapAsync();
        _semuaPegawai = await _api.GetPegawaiAsync();
    }

    private void HitungPeriodeDariTanggal(DateTime tanggalDipilih)
    {
        int selisihKeKamis = ((int)tanggalDipilih.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
        _tanggalKamis = tanggalDipilih.AddDays(-selisihKeKamis).Date;
        _tanggalRabu = _tanggalKamis.AddDays(6);
    }

    private void TampilkanPeriode()
    {
        LabelPeriode.Text = $"{_tanggalKamis:dd/MM/yyyy} - {_tanggalRabu:dd/MM/yyyy}";
        TampilkanData();
    }

    private void TampilkanData()
    {
        var tanggalDataMulai = _tanggalKamis.AddDays(-1);
        var tanggalDataSelesai = _tanggalRabu.AddDays(-1);

        var dataPeriodeIni = _semuaData
         .Where(p => p.Tanggal.Date >= tanggalDataMulai && p.Tanggal.Date <= tanggalDataSelesai)
         .Where(p => p.Wilayah != null || p.Bengkel || p.Kir)
        .ToList();

        var (listsopir, listKernet) = BentukListPerJabatan(dataPeriodeIni);

        bool admin = SesiPengguna.IsAdmin;

        if (admin)
        {
            BindableLayout.SetItemsSource(StackSopir, listsopir);
            BindableLayout.SetItemsSource(StackKernet, listKernet);
            LabelKosong.IsVisible = listsopir.Count == 0 && listKernet.Count == 0;
        }
        else
        {
            BindableLayout.SetItemsSource(StackIndividual, listsopir);
            LabelKosong.IsVisible = listsopir.Count == 0;
        }

        LabelJumlah.Text = $"{dataPeriodeIni.Count} transaksi tercatat di periode ini";
    }

    private (List<object> sopir, List<object> kernet) BentukListPerJabatan(List<Pengiriman> data)
    {
        var tanggalDataMulai = _tanggalKamis.AddDays(-1);
        var semuaTanggal = Enumerable.Range(0, 7)
            .Select(i => tanggalDataMulai.AddDays(i))
            .Where(t => t.DayOfWeek != DayOfWeek.Saturday && t.DayOfWeek != DayOfWeek.Sunday)
            .ToList();

        var pegawaiUnik = _semuaPegawai
            .Where(pg => !string.IsNullOrWhiteSpace(pg.Nama))
            .OrderBy(pg => pg.Nama)
            .ToList();

        if (!SesiPengguna.IsAdmin && SesiPengguna.PegawaiId.HasValue)
        {
            pegawaiUnik = pegawaiUnik
                .Where(pg => pg.Id == SesiPengguna.PegawaiId.Value)
                .ToList();
        }

        var listsopir = new List<object>();
        var listKernet = new List<object>();

        // Usual partner (buat catatan "partner beda dari biasanya" di dalam trip) diambil dari tabel tetap
        var usualPartnerOf = new Dictionary<string, string>();
        foreach (var pasangan in PasanganTetap)
        {
            if (pasangan.Sopir != null && pasangan.Kernet != null)
            {
                usualPartnerOf[pasangan.Sopir] = pasangan.Kernet;
                usualPartnerOf[pasangan.Kernet] = pasangan.Sopir;
            }
        }

        if (!SesiPengguna.IsAdmin)
        {
            // User biasa: cuma satu pegawai (dirinya sendiri), tidak perlu logika pemasangan kolom
            foreach (var pegawai in pegawaiUnik)
                listsopir.AddRange(BuatBlokPegawai(pegawai, data, semuaTanggal, usualPartnerOf));

            return (listsopir, listKernet);
        }

        var daftarSopirPegawai = pegawaiUnik.Where(pg => pg.Jabatan != "kernet").ToList();
        var daftarKernetPegawai = pegawaiUnik.Where(pg => pg.Jabatan == "kernet").ToList();

        var sopirByName = daftarSopirPegawai.ToDictionary(p => p.Nama!, p => p);
        var kernetByName = daftarKernetPegawai.ToDictionary(p => p.Nama!, p => p);

        var sopirDipakai = new HashSet<string>();
        var kernetDipakai = new HashSet<string>();

        var pasanganUrutan = new List<(Pegawai? Sopir, Pegawai? Kernet)>();

        // 1) Susun sesuai urutan tabel PasanganTetap
        foreach (var pasangan in PasanganTetap)
        {
            Pegawai? sopirPeg = null;
            Pegawai? kernetPeg = null;

            if (pasangan.Sopir != null && sopirByName.TryGetValue(pasangan.Sopir, out var sp))
            {
                sopirPeg = sp;
                sopirDipakai.Add(sp.Nama!);
            }

            if (pasangan.Kernet != null && kernetByName.TryGetValue(pasangan.Kernet, out var kp))
            {
                kernetPeg = kp;
                kernetDipakai.Add(kp.Nama!);
            }

            // Kalau dua-duanya tidak ketemu di data pegawai (misal nama typo/belum ada), lewati barisnya
            if (sopirPeg == null && kernetPeg == null) continue;

            pasanganUrutan.Add((sopirPeg, kernetPeg));
        }

        // 2) Sisanya (pegawai yang tidak ada di tabel pasangan tetap) ditambahkan di bawah, sejajar sebisanya
        var sisaSopir = daftarSopirPegawai.Where(p => !sopirDipakai.Contains(p.Nama!)).OrderBy(p => p.Nama).ToList();
        var sisaKernet = daftarKernetPegawai.Where(p => !kernetDipakai.Contains(p.Nama!)).OrderBy(p => p.Nama).ToList();

        int maxSisa = Math.Max(sisaSopir.Count, sisaKernet.Count);
        for (int i = 0; i < maxSisa; i++)
        {
            Pegawai? s = i < sisaSopir.Count ? sisaSopir[i] : null;
            Pegawai? k = i < sisaKernet.Count ? sisaKernet[i] : null;
            pasanganUrutan.Add((s, k));
        }

        // 3) Bangun blok kolom sopir & kolom kernet, sejajar per baris pasangan
        foreach (var (sopirPeg, kernetPeg) in pasanganUrutan)
        {
            listsopir.AddRange(sopirPeg != null
                ? BuatBlokPegawai(sopirPeg, data, semuaTanggal, usualPartnerOf)
                : BuatBlokKosong(semuaTanggal));

            listKernet.AddRange(kernetPeg != null
                ? BuatBlokPegawai(kernetPeg, data, semuaTanggal, usualPartnerOf)
                : BuatBlokKosong(semuaTanggal));
        }

        return (listsopir, listKernet);
    }

    private List<object> BuatBlokPegawai(Pegawai pegawai, List<Pengiriman> data, List<DateTime> semuaTanggal, Dictionary<string, string> usualPartnerOf)
    {
        var nama = pegawai.Nama;
        var item = new List<object>
        {
            new RiwayatItemNama { Nama = nama },
            new RiwayatItemKolomHeader()
        };

        int nomor = 1;
        foreach (var tanggal in semuaTanggal)
        {
            var pengirimanHariItu = data
                .Where(p => p.Tanggal.Date == tanggal &&
                    (p.Sopir?.Nama == nama || p.Kernet?.Nama == nama))
                .ToList();

            List<RiwayatTripHarian> trips;

            if (pengirimanHariItu.Count == 0)
            {
                trips = new List<RiwayatTripHarian>
                {
                    new RiwayatTripHarian { IsVirtual = true, BukanTripPertama = false }
                };
            }
            else
            {
                trips = pengirimanHariItu.Select((p, idx) =>
                {
                    return new RiwayatTripHarian
                    {
                        Id = p.Id,
                        Wilayah = p.Kir
                            ? "KIR"
                            : p.Bengkel
                                ? ""
                                : (p.Wilayah?.NamaWilayah ?? ""),

                        IsBengkel = p.Bengkel,
                        IsKir = p.Kir,

                        NominalSupir =
                            p.Kir
                                ? (p.Sopir?.Nama == nama ? p.TotalBiaya : null)
                                : p.Bengkel
                                    ? null
                                    : (p.Sopir?.Nama == nama ? p.Wilayah?.Tarifsopir : null),

                        NominalKernet =
                            p.Kir
                                ? null
                                : p.Bengkel
                                    ? null
                                    : (p.Kernet?.Nama == nama ? p.Wilayah?.TarifKernet : null),

                        PasanganSopir = (!p.Kir && !p.Bengkel && p.Kernet?.Nama == nama)
                            ? HitungTeksPasangan(nama, p.Sopir?.Nama ?? p.SupirManual, usualPartnerOf)
                            : null,

                        PasanganKernet = (!p.Kir && !p.Bengkel && p.Sopir?.Nama == nama)
                            ? HitungTeksPasangan(nama, p.Kernet?.Nama ?? p.KernetManual, usualPartnerOf)
                            : null,

                        BukanTripPertama = idx > 0,
                    };
                }).ToList();
            }

            int nomorBaris = nomor++;
            trips[0].Nomor = nomorBaris;
            trips[0].TanggalTextRaw = tanggal.ToString("dd/MM/yyyy");

            item.Add(new RiwayatBaris
            {
                Nomor = nomorBaris,
                Tanggal = tanggal,
                Trips = trips,
            });
        }

        decimal totalSupir = item.OfType<RiwayatBaris>().SelectMany(b => b.Trips).Sum(t => t.NominalSupir ?? 0);
        decimal totalKernet = item.OfType<RiwayatBaris>().SelectMany(b => b.Trips).Sum(t => t.NominalKernet ?? 0);

        item.Add(new RiwayatItemTotal { TotalSupir = totalSupir, TotalKernet = totalKernet });

        return item;
    }

    // Blok kosong: dipakai kalau di posisi pasangan ini, sisi sopir/kernet-nya memang tidak ada
    // (biar tetap sejajar sama sisi yang isi, sesuai posisi aslinya di ledger)
    private List<object> BuatBlokKosong(List<DateTime> semuaTanggal)
    {
        var item = new List<object>
        {
            new RiwayatItemNama { Nama = "" },
            new RiwayatItemKolomHeader()
        };

        int nomor = 1;
        foreach (var tanggal in semuaTanggal)
        {
            var trip = new RiwayatTripHarian
            {
                IsVirtual = true,
                BukanTripPertama = false,
                Nomor = nomor,
                TanggalTextRaw = tanggal.ToString("dd/MM/yyyy"),
            };
            nomor++;

            item.Add(new RiwayatBaris { Nomor = trip.Nomor, Tanggal = tanggal, Trips = new List<RiwayatTripHarian> { trip } });
        }

        item.Add(new RiwayatItemTotal { TotalSupir = 0, TotalKernet = 0 });

        return item;
    }

    private static string? HitungTeksPasangan(string namaPegawai, string? namaPartnerAsli, Dictionary<string, string> usualPartnerOf)
    {
        if (string.IsNullOrEmpty(namaPartnerAsli))
            return null;

        if (usualPartnerOf.TryGetValue(namaPegawai, out var pasanganUtama) && pasanganUtama == namaPartnerAsli)
        {
            return "";
        }

        return namaPartnerAsli;
    }

    private async void OnBarisTapped(object sender, EventArgs e)
    {
        if (!SesiPengguna.IsAdmin) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not RiwayatBaris baris) return;

        var tripsAsli = baris.Trips.Where(t => !t.IsVirtual).ToList();
        if (tripsAsli.Count == 0) return;

        if (tripsAsli.Count == 1)
        {
            await KonfirmasiDanHapus(tripsAsli[0]);
            return;
        }

        var opsi = tripsAsli
            .Select((t, i) =>
            {
                string label = t.IsBengkel
                    ? "Bengkel"
                    : t.IsKir
                        ? "KIR"
                        : $"{t.Wilayah} (sopir: {(t.NominalSupir.HasValue ? t.NominalSupir.Value.ToString("N0") : "-")}, Kernet: {(t.NominalKernet.HasValue ? t.NominalKernet.Value.ToString("N0") : "-")})";
                return $"{i + 1}. {label}";
            })
            .ToArray();

        string pilihan = await DisplayActionSheet("Pilih data yang mau dihapus", "Batal", null, opsi);
        if (string.IsNullOrEmpty(pilihan) || pilihan == "Batal") return;

        int idx = Array.IndexOf(opsi, pilihan);
        if (idx < 0) return;

        await KonfirmasiDanHapus(tripsAsli[idx]);
    }

    private async Task KonfirmasiDanHapus(RiwayatTripHarian trip)
    {
        string namaData = trip.IsBengkel ? "Bengkel" : trip.IsKir ? "KIR" : (string.IsNullOrEmpty(trip.Wilayah) ? "data ini" : trip.Wilayah);
        bool konfirmasi = await DisplayAlert("Hapus Data", $"Hapus \"{namaData}\"? Tindakan ini tidak bisa dibatalkan.", "Hapus", "Batal");
        if (!konfirmasi) return;

        bool berhasil = await _api.HapusPengirimanAsync(trip.Id);

        if (berhasil)
        {
            await MuatSemuaData();
            TampilkanData();
        }
        else
        {
            await DisplayAlert("Gagal", "Gagal menghapus data. Coba lagi.", "OK");
        }
    }

    private void OnPeriodeSebelumnyaClicked(object sender, EventArgs e)
    {
        _tanggalKamis = _tanggalKamis.AddDays(-7);
        _tanggalRabu = _tanggalRabu.AddDays(-7);
        TampilkanPeriode();
    }

    private void OnPeriodeBerikutnyaClicked(object sender, EventArgs e)
    {
        _tanggalKamis = _tanggalKamis.AddDays(7);
        _tanggalRabu = _tanggalRabu.AddDays(7);
        TampilkanPeriode();
    }

    private void OnPilihTanggal(object sender, DateChangedEventArgs e)
    {
        HitungPeriodeDariTanggal(e.NewDate);
        TampilkanPeriode();
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await MuatSemuaData();
        TampilkanData();
        RefreshViewRiwayat.IsRefreshing = false;
    }
}