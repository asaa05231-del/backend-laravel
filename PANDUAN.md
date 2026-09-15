# Panduan Project: Sistem Transaksi Hasil Pengiriman (.NET MAUI)

Panduan ini disusun khusus supaya kamu bisa jalankan project dari nol di **D:\\**, tanpa Android Studio, tanpa XAMPP (database pakai **SQLite lokal**, otomatis dibuat oleh aplikasi).

---

## TAHAP 0 — Persiapan Aplikasi yang Harus Di-install

Download & install di laptop kamu (urut dari atas):

1. **Visual Studio 2022 (Community – gratis)**
   https://visualstudio.microsoft.com/vs/
   Saat instalasi, centang workload **".NET Multi-platform App UI development" (.NET MAUI)**.
2. Pastikan **.NET 8 SDK** ikut ter-install (biasanya otomatis lewat Visual Studio installer).
3. Tidak perlu XAMPP/MySQL — database pakai **SQLite** yang sudah otomatis lewat NuGet package `sqlite-net-pcl` (akan kita tambahkan).

Simpan seluruh folder project ini di: **`D:\SistemPengirimanApp\`**

---

## TAHAP 1 — Membuat Project Dasar

Karena kamu tidak pakai Android Studio, cara paling aman membuat project MAUI dari nol adalah lewat **terminal**, supaya semua file bawaan (ikon, splash screen, font) otomatis lengkap. Buka **Developer PowerShell** (cari di Start Menu: "Developer PowerShell for VS 2022"), lalu jalankan:

```
cd D:\
dotnet new maui -n SistemPengirimanApp
cd SistemPengirimanApp
```

Ini akan membuat folder `D:\SistemPengirimanApp\` berisi project MAUI standar (lengkap dengan `Resources\AppIcon`, `Resources\Splash`, `Resources\Fonts`, dll — **jangan dihapus**).

---

## TAHAP 2 — Tambahkan Package NuGet untuk Database

Masih di folder yang sama, jalankan:

```
dotnet add package sqlite-net-pcl
dotnet add package SQLitePCLRaw.bundle_green
```

---

## TAHAP 3 — Salin File-File dari Paket Ini

Dari paket yang saya berikan, salin/timpa (copy-paste, replace bila ditanya) ke folder `D:\SistemPengirimanApp\` sesuai struktur berikut:

```
D:\SistemPengirimanApp\
│
├── MauiProgram.cs              (TIMPA file bawaan)
├── App.xaml / App.xaml.cs      (TIMPA file bawaan)
├── AppShell.xaml / .cs         (TIMPA file bawaan)
│
├── Models\                     (folder BARU, buat manual lalu isi filenya)
│   ├── Orang.cs
│   ├── Wilayah.cs
│   ├── Transaksi.cs
│   └── Akun.cs
│
├── Services\                   (folder BARU)
│   ├── DatabaseService.cs
│   └── SesiPengguna.cs
│
├── Views\                      (folder BARU — semua halaman UI)
│   ├── LoginPage.xaml / .cs
│   ├── BerandaPage.xaml / .cs
│   ├── InputPengirimanPage.xaml / .cs
│   ├── RiwayatPage.xaml / .cs
│   ├── LaporanPage.xaml / .cs
│   └── MasterDataPage.xaml / .cs
│
└── Resources\Styles\
    ├── Colors.xaml   (TIMPA file bawaan — isi sudah beda temanya)
    └── Styles.xaml   (TIMPA file bawaan)
```

> ⚠️ **Jangan** menimpa folder `Resources\AppIcon`, `Resources\Splash`, `Resources\Fonts`, `Resources\Images` — biarkan bawaan template, supaya project tetap bisa build (ikon & font sudah otomatis tersedia dari `dotnet new maui`).

Buka `SistemPengirimanApp.sln` dengan double click — otomatis kebuka di Visual Studio 2022.

---

## TAHAP 4 — Jalankan Aplikasi

1. Di Visual Studio, pada dropdown target di toolbar atas, pilih **Windows Machine** (paling gampang untuk development/testing di laptop, tanpa emulator Android).
2. Tekan **F5** atau klik tombol ▶️ hijau.
3. Tunggu proses build pertama kali (agak lama, wajar).
4. Aplikasi akan terbuka → halaman **Login**.

**Akun demo (sudah otomatis dibuat sistem saat pertama kali jalan):**
- Admin → username: `admin` / password: `admin123`
- User biasa → username: `user` / password: `user123`

---

## TAHAP 5 — Struktur Alur Sistem (sudah diimplementasikan di code)

1. **Login** → cek role (Admin/User) → menu yang muncul berbeda.
2. **Beranda** → ringkasan jumlah pengiriman & total biaya.
3. **Input Pengiriman** → isi tanggal, pilih wilayah → tarif sopir & kernet otomatis muncul → pilih sopir & kernet → total otomatis terhitung → simpan.
4. **Riwayat Pengiriman** → daftar semua transaksi tersimpan.
5. **Master Data** *(khusus Admin)* → kelola daftar nama sopir/kernet, dan edit tarif per wilayah.
6. **Laporan** *(khusus Admin)* → total pengiriman per hari, total biaya per wilayah, total pendapatan per sopir/kernet.

### Hak Akses (Role)
| Menu               | Admin | User |
|---------------------|:-----:|:----:|
| Beranda              | ✅ | ✅ |
| Input Pengiriman      | ✅ | ✅ |
| Riwayat Pengiriman    | ✅ | ✅ |
| Master Data           | ✅ | ❌ |
| Laporan               | ✅ | ❌ |

---

## TAHAP 6 — Data Awal yang Sudah Otomatis Diisi Sistem

- **21 nama** (dipakai bersama untuk pilihan sopir maupun Kernet).
- **6 wilayah** beserta tarif sopir & kernet, dan daftar kota masing-masing (sesuai data yang kamu berikan).
- Data ini otomatis masuk ke database SQLite saat aplikasi pertama kali dijalankan (lihat method `SeedDataAwal()` di `DatabaseService.cs`). Kamu bisa menambah/menghapus data lewat menu **Master Data** tanpa perlu edit code lagi.

---

## TAHAP 7 — Untuk Laporan PKL (opsional, saran susunan bab)

1. **BAB 1** — Latar belakang, rumusan masalah, tujuan, batasan masalah.
2. **BAB 2** — Landasan teori: .NET MAUI, SQLite, konsep role-based access.
3. **BAB 3** — Analisis & perancangan: use case (Admin vs User), ERD/struktur tabel (Orang, Wilayah, Transaksi, Akun), flowchart alur input pengiriman (persis seperti yang kamu tulis di deskripsi).
4. **BAB 4** — Implementasi: screenshot tiap halaman (Login, Beranda, Input Pengiriman, Riwayat, Laporan, Master Data) + potongan code penting (perhitungan tarif otomatis).
5. **BAB 5** — Pengujian (black-box testing tiap fitur) & kesimpulan.

---

## Troubleshooting Umum

- **Error build "Resources not found"** → pastikan tidak menghapus folder `Resources\AppIcon` / `Splash` / `Fonts` bawaan template.
- **Package sqlite-net-pcl tidak ketemu** → pastikan laptop terkoneksi internet saat `dotnet add package` / saat build pertama (NuGet perlu download).
- **Data tidak berubah setelah edit tarif** → pastikan menekan tombol "Simpan Perubahan" di kartu wilayah masing-masing (di Master Data).
