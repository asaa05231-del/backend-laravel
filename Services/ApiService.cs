using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SistemPengirimanApp.Models;

namespace SistemPengirimanApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        private const string BaseUrl = "http://127.0.0.1:8000/api/";

        public ApiService()
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri(BaseUrl);
        }

        public void SetToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }

        // ===== AUTH =====
        public async Task<LoginResult?> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("login", new { username, password });

            if (!response.IsSuccessStatusCode)
                return null;

            var hasil = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (hasil is not null)
                SetToken(hasil.Token);

            return hasil;
        }

        public async Task<(bool berhasil, string pesan)> UpdatePasswordAsync(string passwordLama, string passwordBaru, string passwordBaruConfirmation)
        {
            var response = await _http.PatchAsJsonAsync("me/password", new
            {
                password_lama = passwordLama,
                password_baru = passwordBaru,
                password_baru_confirmation = passwordBaruConfirmation
            });

            if (response.IsSuccessStatusCode)
                return (true, "Password berhasil diubah.");

            try
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                var pesan = error != null && error.TryGetValue("message", out var m) ? m?.ToString() : "Gagal mengubah password.";
                return (false, pesan ?? "Gagal mengubah password.");
            }
            catch
            {
                return (false, "Gagal mengubah password.");
            }
        }

        // ===== sopir =====
        public async Task<List<Pegawai>> GetSopirAsync()
        {
            return await _http.GetFromJsonAsync<List<Pegawai>>("sopirs")
                   ?? new List<Pegawai>();
        }

        public async Task<bool> TambahsopirAsync(string nama, decimal tarif)
        {
            var response = await _http.PostAsJsonAsync("sopirs", new { nama, tarif });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HapussopirAsync(int id)
        {
            var response = await _http.DeleteAsync($"sopirs/{id}");
            return response.IsSuccessStatusCode;
        }

        // ===== KERNET =====
        public async Task<List<Kernet>> GetKernetAsync()
        {
            return await _http.GetFromJsonAsync<List<Kernet>>("kernets")
                   ?? new List<Kernet>();
        }

        public async Task<bool> TambahKernetAsync(string nama, decimal tarif)
        {
            var response = await _http.PostAsJsonAsync("kernets", new { nama, tarif });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HapusKernetAsync(int id)
        {
            var response = await _http.DeleteAsync($"kernets/{id}");
            return response.IsSuccessStatusCode;
        }

        // ===== WILAYAH =====
        public async Task<List<Wilayah>> GetWilayahAsync()
        {
            return await _http.GetFromJsonAsync<List<Wilayah>>("wilayah")
                   ?? new List<Wilayah>();
        }

        public async Task<bool> SimpanWilayahAsync(Wilayah wilayah)
        {
            var response = await _http.PutAsJsonAsync($"wilayah/{wilayah.Id}", new
            {
                nama_wilayah = wilayah.NamaWilayah,
                tarif_supir = wilayah.Tarifsopir,
                tarif_kernet = wilayah.TarifKernet
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TambahWilayahAsync(string namaWilayah, decimal tarifsopir, decimal tarifKernet)
        {
            var response = await _http.PostAsJsonAsync("wilayah", new
            {
                nama_wilayah = namaWilayah,
                tarif_supir = tarifsopir,
                tarif_kernet = tarifKernet
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HapusWilayahAsync(int id)
        {
            var response = await _http.DeleteAsync($"wilayah/{id}");
            return response.IsSuccessStatusCode;
        }

        // ===== PEGAWAI =====
        public async Task<List<Pegawai>> GetPegawaiAsync()
        {
            return await _http.GetFromJsonAsync<List<Pegawai>>("pegawai")
                   ?? new List<Pegawai>();
        }

        public async Task<bool> TambahPegawaiAsync(string nama, string jabatan, DateTime? tanggalMasuk, decimal upahHarian, decimal upahMingguan, decimal tunjanganTransport, decimal persenJamsostek, bool ikutJamsostek)
        {
            var response = await _http.PostAsJsonAsync("pegawai", new
            {
                nama,
                jabatan,
                tanggal_masuk = tanggalMasuk?.ToString("yyyy-MM-dd"),
                upah_harian = upahHarian,
                upah_mingguan = upahMingguan,
                tunjangan_transport = tunjanganTransport,
                persen_jamsostek = persenJamsostek,
                ikut_jamsostek = ikutJamsostek
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SimpanTarifPegawaiAsync(Pegawai pegawai)
        {
            var response = await _http.PutAsJsonAsync($"pegawai/{pegawai.Id}", new
            {
                nama = pegawai.Nama,
                jabatan = pegawai.Jabatan,
                tanggal_masuk = pegawai.TanggalMasuk?.ToString("yyyy-MM-dd"),
                upah_harian = pegawai.UpahHarian,
                upah_mingguan = pegawai.UpahMingguan,
                tunjangan_transport = pegawai.TunjanganTransport,
                persen_jamsostek = pegawai.PersenJamsostek,
                ikut_jamsostek = pegawai.IkutJamsostek
            });
            return response.IsSuccessStatusCode;
        }

        // ===== PENGIRIMAN / RIWAYAT =====
        public async Task<List<Pengiriman>> GetRiwayatLengkapAsync()
        {
            try
            {
                var hasil = await _http.GetFromJsonAsync<List<Pengiriman>>("pengiriman");
                return hasil ?? new List<Pengiriman>();
            }
            catch
            {
                return new List<Pengiriman>();
            }
        }

        public async Task<(bool Sukses, string Pesan)> SimpanPengirimanAsync(PengirimanRequest request)
        {
            var response = await _http.PostAsJsonAsync("pengiriman", request);

            if (response.IsSuccessStatusCode)
                return (true, "");

            var errorBody = await response.Content.ReadAsStringAsync();
            return (false, $"{(int)response.StatusCode} {response.StatusCode}\n{errorBody}");
        }

        public async Task<bool> HapusPengirimanAsync(int id)
        {
            var response = await _http.DeleteAsync($"pengiriman/{id}");
            return response.IsSuccessStatusCode;
        }

        // ===== PENGGAJIAN / SLIP GAJI =====
        public async Task<List<PeriodeGaji>> GetDaftarPeriodeAsync()
        {
            return await _http.GetFromJsonAsync<List<PeriodeGaji>>("penggajian/periode")
                   ?? new List<PeriodeGaji>();
        }

        public async Task<List<SlipGaji>> GetDetailPeriodeAsync(int periodeId)
        {
            return await _http.GetFromJsonAsync<List<SlipGaji>>($"penggajian/periode/{periodeId}")
                   ?? new List<SlipGaji>();
        }

        public async Task<bool> GenerateSlipGajiAsync(DateTime tanggalMulai, DateTime tanggalSelesai)
        {
            var response = await _http.PostAsJsonAsync("penggajian/generate", new
            {
                tanggal_mulai = tanggalMulai.ToString("yyyy-MM-dd"),
                tanggal_selesai = tanggalSelesai.ToString("yyyy-MM-dd")
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePotonganAsync(int slipId, decimal cat, decimal pengobatan, decimal spsi)
        {
            var response = await _http.PatchAsJsonAsync($"penggajian/slip/{slipId}/potongan", new
            {
                potongan_cat = cat,
                potongan_pengobatan = pengobatan,
                potongan_spsi = spsi
            });
            return response.IsSuccessStatusCode;
        }


        public async Task<List<TarifUpahTahunan>> GetTarifTahunanAsync()
        {
            return await _http.GetFromJsonAsync<List<TarifUpahTahunan>>("tarif-upah-tahunan")
                   ?? new List<TarifUpahTahunan>();
        }

        public async Task<bool> SimpanTarifTahunanAsync(TarifUpahTahunan tarif)
        {
            var response = await _http.PutAsJsonAsync($"tarif-upah-tahunan/{tarif.Id}", new
            {
                upah_harian_supir = tarif.UpahHariansupir,
                upah_harian_kernet = tarif.UpahHarianKernet,
                upah_mingguan_supir = tarif.UpahMingguansupir,
                upah_mingguan_kernet = tarif.UpahMingguanKernet,
                tunjangan_makan_harian = tarif.TunjanganMakanHarian,
                subsidi_transport_harian = tarif.SubsidiTransportHarian,
                jamsostek_persen = tarif.JamsostekPersen,
            });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TambahTarifTahunanAsync(int tahun, decimal UpahHariansupir, decimal upahHarianKernet, decimal UpahMingguansupir, decimal upahMingguanKernet, decimal tunjanganMakanHarian, decimal subsidiTransportHarian, decimal jamsostekPersen)
        {
            var response = await _http.PostAsJsonAsync("tarif-upah-tahunan", new
            {
                tahun,
                upah_harian_supir = UpahHariansupir,
                upah_harian_kernet = upahHarianKernet,
                upah_mingguan_supir = UpahMingguansupir,
                upah_mingguan_kernet = upahMingguanKernet,
                tunjangan_makan_harian = tunjanganMakanHarian,
                subsidi_transport_harian = subsidiTransportHarian,
                jamsostek_persen = jamsostekPersen,
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Sukses, string Pesan)> UpdateSlipAsync(SlipGaji slip)
        {
            var response = await _http.PatchAsJsonAsync($"penggajian/slip/{slip.Id}", new
            {
                hari_kerja = slip.HariKerja,
                upah = slip.Upah,
                hadir = slip.Hadir,
                tunjangan_makan = slip.TunjanganMakan,
                subsidi_transport = slip.SubsidiTransport,
                premi = slip.Premi,
                potongan_jamsostek = slip.PotonganJamsostek,
                potongan_cat = slip.PotonganCat,
                potongan_pengobatan = slip.PotonganPengobatan,
                potongan_spsi = slip.PotonganSpsi,
            });

            if (response.IsSuccessStatusCode)
                return (true, "");

            var errorBody = await response.Content.ReadAsStringAsync();
            return (false, $"{(int)response.StatusCode} {response.StatusCode}\n{errorBody}");
        }

        public async Task<PengaturanGaji?> GetPengaturanGajiAsync()
        {
            return await _http.GetFromJsonAsync<PengaturanGaji>("pengaturan-gaji");
        }

        public async Task<(bool berhasil, string pesan)> UpdateProfilAsync(string nama, string username)
        {
            var response = await _http.PatchAsJsonAsync("me/profil", new { name = nama, username });
            if (response.IsSuccessStatusCode) return (true, "Profil berhasil diperbarui.");
            var body = await response.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<bool> UpdatePengaturanGajiAsync(decimal jamsostekSupir, decimal jamsostekKernet, decimal potonganKesehatanSupir, decimal potonganKesehatanKernet)
        {
            var response = await _http.PutAsJsonAsync("pengaturan-gaji", new
            {
                jamsostek_supir = jamsostekSupir,
                jamsostek_kernet = jamsostekKernet,
                potongan_kesehatan_supir = potonganKesehatanSupir,
                potongan_kesehatan_kernet = potonganKesehatanKernet,
            });
            return response.IsSuccessStatusCode;
        }
    }
}