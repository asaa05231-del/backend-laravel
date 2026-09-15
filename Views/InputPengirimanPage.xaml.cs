using SistemPengirimanApp.Models;
using SistemPengirimanApp.Services;

namespace SistemPengirimanApp.Views;

public partial class InputPengirimanPage : ContentPage
{
    private const decimal TarifKir = 30000; // pastikan sama dengan pengaturan_gajis.tarif_kir di backend

    private readonly ApiService _apiService;

    private List<Pegawai> _pegawaiList = new();
    private List<Wilayah> _wilayahList = new();

    private Wilayah? _wilayahTerpilih;
    private Pegawai? _sopirTerpilih;
    private Pegawai? _kernetTerpilih;
    private bool _modeKirAktif = false;
    private bool _modeBengkelAktif = false;
    private string? _sopirManual;
    private string? _kernetManual;

    public InputPengirimanPage(ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;

        PickerTanggal.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            LabelError.IsVisible = false;

            _pegawaiList = await _apiService.GetPegawaiAsync();
            _wilayahList = await _apiService.GetWilayahAsync();

            ResetForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal mengambil data.\n\n{ex.Message}", "OK");
        }
    }

    private async void OnPilihWilayahTapped(object sender, EventArgs e)
    {
        if (_modeKirAktif || _modeBengkelAktif) return;

        if (_wilayahList.Count == 0)
        {
            await DisplayAlert("Info", "Data wilayah belum berhasil dimuat.", "OK");
            return;
        }

        var namaList = _wilayahList.Select(w => w.NamaWilayah).ToArray();
        string pilihan = await DisplayActionSheet("Pilih Wilayah Tujuan", "Batal", null, namaList);

        if (string.IsNullOrEmpty(pilihan) || pilihan == "Batal") return;

        int index = Array.IndexOf(namaList, pilihan);
        if (index < 0) return;

        _wilayahTerpilih = _wilayahList[index];
        LabelWilayahTerpilih.Text = _wilayahTerpilih.NamaWilayah;
        LabelWilayahTerpilih.TextColor = (Color)Application.Current!.Resources["HitamTeks"];

        RefreshRincianBiaya();
    }

    private async void OnPilihsopirTapped(object sender, EventArgs e)
    {
        if (_pegawaiList.Count == 0)
        {
            await DisplayAlert("Info", "Data pegawai belum berhasil dimuat.", "OK");
            return;
        }

        const string opsiManual = "+ Isi manual (bukan pegawai)";
        var namaList = _pegawaiList.Select(p => p.Nama).Prepend(opsiManual).ToArray();
        string pilihan = await DisplayActionSheet("Pilih sopir", "Batal", null, namaList);

        if (string.IsNullOrEmpty(pilihan) || pilihan == "Batal") return;

        if (pilihan == opsiManual)
        {
            string nama = await DisplayPromptAsync("Isi Manual", "Nama sopir (bukan pegawai):", "Simpan", "Batal");
            if (string.IsNullOrWhiteSpace(nama)) return;

            _sopirTerpilih = null;
            _sopirManual = nama.Trim();
            LabelsopirTerpilih.Text = _sopirManual;
            LabelsopirTerpilih.TextColor = (Color)Application.Current!.Resources["HitamTeks"];
            return;
        }

        int index = Array.IndexOf(namaList, pilihan) - 1; // -1 karena ada opsiManual di depan
        if (index < 0) return;

        _sopirManual = null;
        _sopirTerpilih = _pegawaiList[index];
        LabelsopirTerpilih.Text = _sopirTerpilih.Nama;
        LabelsopirTerpilih.TextColor = (Color)Application.Current!.Resources["HitamTeks"];
    }

    private async void OnPilihKernetTapped(object sender, EventArgs e)
    {
        if (_pegawaiList.Count == 0)
        {
            await DisplayAlert("Info", "Data pegawai belum berhasil dimuat.", "OK");
            return;
        }

        const string opsiManual = "+ Isi manual (bukan pegawai)";
        var namaList = _pegawaiList.Select(p => p.Nama).Prepend(opsiManual).ToArray();
        string pilihan = await DisplayActionSheet("Pilih Kernet", "Batal", null, namaList);

        if (string.IsNullOrEmpty(pilihan) || pilihan == "Batal") return;

        if (pilihan == opsiManual)
        {
            string nama = await DisplayPromptAsync("Isi Manual", "Nama kernet (bukan pegawai):", "Simpan", "Batal");
            if (string.IsNullOrWhiteSpace(nama)) return;

            _kernetTerpilih = null;
            _kernetManual = nama.Trim();
            LabelKernetTerpilih.Text = _kernetManual;
            LabelKernetTerpilih.TextColor = (Color)Application.Current!.Resources["HitamTeks"];
            return;
        }

        int index = Array.IndexOf(namaList, pilihan) - 1;
        if (index < 0) return;

        _kernetManual = null;
        _kernetTerpilih = _pegawaiList[index];
        LabelKernetTerpilih.Text = _kernetTerpilih.Nama;
        LabelKernetTerpilih.TextColor = (Color)Application.Current!.Resources["HitamTeks"];
    }

    private void OnCheckboxKirChanged(object sender, CheckedChangedEventArgs e)
    {
        if (CheckboxKir.IsChecked)
        {
            CheckboxBengkel.IsChecked = false;
            CheckboxBengkel.IsEnabled = false;
        }
        else
        {
            CheckboxBengkel.IsEnabled = true;
        }

        AturModeKir(CheckboxKir.IsChecked);
    }

    private void OnCheckboxBengkelChanged(object sender, CheckedChangedEventArgs e)
    {
        if (CheckboxBengkel.IsChecked)
        {
            CheckboxKir.IsChecked = false;
            CheckboxKir.IsEnabled = false;
        }
        else
        {
            CheckboxKir.IsEnabled = true;
        }

        AturModeBengkel(CheckboxBengkel.IsChecked);
    }

    private void OnCheckboxMuatanBeratChanged(object sender, CheckedChangedEventArgs e)
    {
        // Muatan Berat tidak memengaruhi Rincian Biaya di halaman ini
    }

    private void AturModeKir(bool aktif)
    {
        _modeKirAktif = aktif;

        var abuTeks = (Color)Application.Current!.Resources["AbuTeks"];

        if (aktif)
        {
            _wilayahTerpilih = null;
            _kernetTerpilih = null;

            LabelWilayahTerpilih.Text = "-";
            LabelKernetTerpilih.Text = "-";
            LabelWilayahTerpilih.TextColor = abuTeks;
            LabelKernetTerpilih.TextColor = abuTeks;

            BorderWilayah.Opacity = 0.5;
            BorderKernet.Opacity = 0.5;

            CheckboxMuatanBerat.IsChecked = false;
            CheckboxMuatanBerat.IsEnabled = false;
        }
        else
        {
            LabelWilayahTerpilih.Text = "Pilih wilayah tujuan";
            LabelKernetTerpilih.Text = "Pilih kernet";

            BorderWilayah.Opacity = 1;
            BorderKernet.Opacity = 1;

            CheckboxMuatanBerat.IsEnabled = true;
        }

        RefreshRincianBiaya();
    }

    private void AturModeBengkel(bool aktif)
    {
        _modeBengkelAktif = aktif;

        var abuTeks = (Color)Application.Current!.Resources["AbuTeks"];

        if (aktif)
        {
            // Bengkel gak butuh Wilayah, tapi tetap butuh Kernet
            _wilayahTerpilih = null;
            LabelWilayahTerpilih.Text = "-";
            LabelWilayahTerpilih.TextColor = abuTeks;
            BorderWilayah.Opacity = 0.5;

            CheckboxMuatanBerat.IsChecked = false;
            CheckboxMuatanBerat.IsEnabled = false;
        }
        else
        {
            LabelWilayahTerpilih.Text = "Pilih wilayah tujuan";
            BorderWilayah.Opacity = 1;

            CheckboxMuatanBerat.IsEnabled = true;
        }

        RefreshRincianBiaya();
    }

    private void RefreshRincianBiaya()
    {
        if (CheckboxBengkel.IsChecked)
        {
            LabelTarifsopir.Text = "Rp 0";
            LabelTarifKernet.Text = "Rp 0";
            LabelTotalBiaya.Text = "Rp 0";
            return;
        }

        if (CheckboxKir.IsChecked)
        {
            LabelTarifsopir.Text = $"Rp {TarifKir:N0}";
            LabelTarifKernet.Text = "Rp 0";
            LabelTotalBiaya.Text = $"Rp {TarifKir:N0}";
            return;
        }

        if (_wilayahTerpilih == null)
        {
            ResetBiaya();
            return;
        }

        LabelTarifsopir.Text = $"Rp {_wilayahTerpilih.Tarifsopir:N0}";
        LabelTarifKernet.Text = $"Rp {_wilayahTerpilih.TarifKernet:N0}";

        decimal total = _wilayahTerpilih.Tarifsopir + _wilayahTerpilih.TarifKernet;
        LabelTotalBiaya.Text = $"Rp {total:N0}";
    }

    private void ResetBiaya()
    {
        LabelTarifsopir.Text = "Rp 0";
        LabelTarifKernet.Text = "Rp 0";
        LabelTotalBiaya.Text = "Rp 0";
    }

    private decimal GetTotalBiaya()
    {
        if (CheckboxBengkel.IsChecked)
            return 0;

        if (CheckboxKir.IsChecked)
            return TarifKir;

        if (_wilayahTerpilih == null)
            return 0;

        return _wilayahTerpilih.Tarifsopir + _wilayahTerpilih.TarifKernet;
    }

    private async void OnSimpanClicked(object sender, EventArgs e)
    {
        bool konfirmasi = await DisplayAlert("Konfirmasi", "Simpan pengiriman ini?", "Simpan", "Batal");
        if (!konfirmasi) return;

        try
        {
            LabelError.IsVisible = false;

            if (_sopirTerpilih == null && string.IsNullOrWhiteSpace(_sopirManual))
            {
                TampilkanError("Silakan pilih atau isi sopir.");
                return;
            }

            bool modeKir = CheckboxKir.IsChecked;
            bool modeBengkel = CheckboxBengkel.IsChecked;

            if (modeBengkel)
            {
                if (_kernetTerpilih == null && string.IsNullOrWhiteSpace(_kernetManual))
                {
                    TampilkanError("Silakan pilih atau isi kernet.");
                    return;
                }
            }
            else if (!modeKir)
            {
                if (_kernetTerpilih == null && string.IsNullOrWhiteSpace(_kernetManual))
                {
                    TampilkanError("Silakan pilih atau isi kernet.");
                    return;
                }

                if (_wilayahTerpilih == null)
                {
                    TampilkanError("Silakan pilih wilayah.");
                    return;
                }
            }

            var request = new PengirimanRequest
            {
                Tanggal = PickerTanggal.Date,
                SopirId = _sopirTerpilih?.Id,
                SupirManual = _sopirManual,
                KernetId = modeKir ? null : _kernetTerpilih?.Id,
                KernetManual = modeKir ? null : _kernetManual,
                WilayahId = (modeKir || modeBengkel) ? null : _wilayahTerpilih!.Id,
                MuatanBerat = (modeKir || modeBengkel) ? false : CheckboxMuatanBerat.IsChecked,
                Kir = modeKir,
                Bengkel = modeBengkel,
                TotalBiaya = GetTotalBiaya()
            };

            var (sukses, pesan) = await _apiService.SimpanPengirimanAsync(request);

            if (sukses)
            {
                string pesanSukses = modeBengkel
                    ? "Data Bengkel berhasil disimpan."
                    : (modeKir ? "Data KIR berhasil disimpan." : "Data pengiriman berhasil disimpan.");

                await DisplayAlert("Berhasil", pesanSukses, "OK");
                ResetForm();
            }
            else
            {
                await DisplayAlert("Gagal", pesan, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void ResetForm()
    {
        PickerTanggal.Date = DateTime.Today;

        _wilayahTerpilih = null;
        _sopirTerpilih = null;
        _kernetTerpilih = null;
        _sopirManual = null;
        _kernetManual = null;

        LabelsopirTerpilih.Text = "Pilih sopir";
        LabelsopirTerpilih.TextColor = (Color)Application.Current!.Resources["AbuTeks"];

        CheckboxKir.IsChecked = false;
        CheckboxBengkel.IsChecked = false;
        CheckboxKir.IsEnabled = true;
        CheckboxBengkel.IsEnabled = true;

        AturModeKir(false);
        AturModeBengkel(false);

        ResetBiaya();
        LabelError.IsVisible = false;
    }

    private void TampilkanError(string pesan)
    {
        LabelError.Text = pesan;
        LabelError.IsVisible = true;
    }
}