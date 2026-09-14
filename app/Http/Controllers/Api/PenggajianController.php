<?php
namespace App\Http\Controllers\Api;

use App\Models\TarifUpahTahunan;
use App\Http\Controllers\Controller;
use App\Models\Pegawai;
use App\Models\Pengiriman;
use App\Models\PengaturanGaji;
use App\Models\PeriodeGaji;
use App\Models\SlipGaji;
use Illuminate\Http\Request;

class PenggajianController extends Controller
{

    // Hitung & buat slip gaji otomatis untuk semua pegawai dalam 1 periode
    // (admin saja -- dibatasi lewat route middleware role:admin)
  public function generate(Request $request)
{
    $request->validate([
        'tanggal_mulai' => 'required|date',
        'tanggal_selesai' => 'required|date|after_or_equal:tanggal_mulai',
    ]);

    $pengaturan = PengaturanGaji::first();
    $tahunPeriode = \Carbon\Carbon::parse($request->tanggal_mulai)->year;
    $tarifTahunan = TarifUpahTahunan::where('tahun', $tahunPeriode)->first();

    if (! $tarifTahunan) {
        return response()->json([
            'message' => "Tarif upah untuk tahun {$tahunPeriode} belum diatur. Silakan atur dulu di Master Data > Tarif Upah Tahunan."
        ], 422);
    }

    // Cari atau buat periode -- TIDAK dihapus lagi kalau sudah ada,
    // supaya slip yang sudah diedit manual (Hadir, Premi, Potongan, Jamsostek) tidak ikut hilang.
    $periode = PeriodeGaji::firstOrCreate([
        'tanggal_mulai' => $request->tanggal_mulai,
        'tanggal_selesai' => $request->tanggal_selesai,
    ]);

    $tanggalDataMulai = \Carbon\Carbon::parse($request->tanggal_mulai)->subDay()->toDateString();
    $tanggalDataSelesai = \Carbon\Carbon::parse($request->tanggal_selesai)->subDay()->toDateString();

    $pengirimanPeriode = Pengiriman::with('wilayah')
        ->whereBetween('tanggal', [$tanggalDataMulai, $tanggalDataSelesai])
        ->get();

    $pegawaiSemua = Pegawai::all();
    $slipYangSudahAda = SlipGaji::where('periode_gaji_id', $periode->id)->get()->keyBy('pegawai_id');

    foreach ($pegawaiSemua as $pegawai) {
        $sebagaiSupir = $pengirimanPeriode->where('supir_id', $pegawai->id)->where('bengkel', false);
        $sebagaiKernet = $pengirimanPeriode->where('kernet_id', $pegawai->id)->where('bengkel', false);

        $hariSbgSupir = $sebagaiSupir->pluck('tanggal')->map(fn ($t) => (string) $t)->unique()->count();
        $hariSbgKernet = $sebagaiKernet->pluck('tanggal')->map(fn ($t) => (string) $t)->unique()->count();

        $tanggalUnik = $sebagaiSupir->pluck('tanggal')
            ->merge($sebagaiKernet->pluck('tanggal'))
            ->map(fn ($t) => (string) $t)
            ->unique();
        $hariKerjaUnikDefault = $tanggalUnik->count();

        $insentifSupir = $sebagaiSupir->sum(fn ($p) => $p->wilayah->tarif_supir ?? 0);
        $insentifKernet = $sebagaiKernet->sum(fn ($p) => $p->wilayah->tarif_kernet ?? 0);
        $insentifKir = $sebagaiSupir->filter(fn ($p) => (bool) $p->kir)->count() * $pengaturan->tarif_kir;

        $insentifMuatanBerat = 0;
        $insentifMuatanBerat += $sebagaiSupir->filter(fn ($p) => (bool) $p->muatan_berat)->count()
            * $pengaturan->komisi_muatan_berat_supir;
        $insentifMuatanBerat += $sebagaiKernet->filter(fn ($p) => (bool) $p->muatan_berat)->count()
            * $pengaturan->komisi_muatan_berat_kernet;

        $slipLama = $slipYangSudahAda->get($pegawai->id);

        if ($slipLama) {
    // Sudah ada slip -- refresh angka yang berasal dari data Pengiriman DAN Jamsostek
    // (Jamsostek selalu ikut status ikut_jamsostek pegawai terkini).
    // Hari Kerja, Hadir, Premi, dan Potongan lain (Cat/Pengobatan/SPSI) yang sudah diedit manual TIDAK disentuh.
    $rateUpah = $pegawai->jabatan === 'sopir' ? $tarifTahunan->upah_harian_supir : $tarifTahunan->upah_harian_kernet;
    $upah = $slipLama->hari_kerja * $rateUpah;

    $tunjanganMakan = $slipLama->hadir * $tarifTahunan->tunjangan_makan_harian;
    $subsidiTransport = $slipLama->hadir * $tarifTahunan->subsidi_transport_harian;

    $totalUpah = $upah + $insentifSupir + $insentifKernet
        + $tunjanganMakan + $subsidiTransport + $insentifKir + $insentifMuatanBerat + $slipLama->premi;

    if ($pegawai->ikut_jamsostek) {
        $potonganJamsostek = $pegawai->jabatan === 'sopir'
            ? $pengaturan->jamsostek_supir
            : $pengaturan->jamsostek_kernet;
    } else {
        $potonganJamsostek = $pegawai->jabatan === 'sopir'
            ? $pengaturan->potongan_kesehatan_supir
            : $pengaturan->potongan_kesehatan_kernet;
    }

    $totalPotongan = $potonganJamsostek + $slipLama->potongan_cat
        + $slipLama->potongan_pengobatan + $slipLama->potongan_spsi;

    $slipLama->update([
        'hari_sbg_supir' => $hariSbgSupir,
        'hari_sbg_kernet' => $hariSbgKernet,
        'upah' => $upah,
        'insentif_supir' => $insentifSupir,
        'insentif_kernet' => $insentifKernet,
        'tunjangan_makan' => $tunjanganMakan,
        'subsidi_transport' => $subsidiTransport,
        'insentif_kir' => $insentifKir,
        'insentif_muatan_berat' => $insentifMuatanBerat,
        'total_upah' => $totalUpah,
        'potongan_jamsostek' => $potonganJamsostek,
        'total_potongan' => $totalPotongan,
        'dibayar' => $totalUpah - $totalPotongan,
    ]);

    continue;
}

        // Belum ada slip -- buat baru dengan nilai default penuh (termasuk Jamsostek otomatis).
        $hariKerja = 5;
        $rateUpah = $pegawai->jabatan === 'sopir' ? $tarifTahunan->upah_harian_supir : $tarifTahunan->upah_harian_kernet;
        $upah = $hariKerja * $rateUpah;

        $tunjanganMakan = $hariKerjaUnikDefault * $tarifTahunan->tunjangan_makan_harian;
        $subsidiTransport = $hariKerjaUnikDefault * $tarifTahunan->subsidi_transport_harian;

        $totalUpah = $upah + $insentifSupir + $insentifKernet
            + $tunjanganMakan + $subsidiTransport + $insentifKir + $insentifMuatanBerat;

    if ($pegawai->ikut_jamsostek) {
    $potonganJamsostek = $pegawai->jabatan === 'sopir'
        ? $pengaturan->jamsostek_supir
        : $pengaturan->jamsostek_kernet;
} else {
    $potonganJamsostek = $pegawai->jabatan === 'sopir'
        ? $pengaturan->potongan_kesehatan_supir
        : $pengaturan->potongan_kesehatan_kernet;
}

        SlipGaji::create([
            'pegawai_id' => $pegawai->id,
            'periode_gaji_id' => $periode->id,
            'hari_kerja' => $hariKerja,
            'hadir' => $hariKerjaUnikDefault,
            'hari_sbg_supir' => $hariSbgSupir,
            'hari_sbg_kernet' => $hariSbgKernet,
            'upah' => $upah,
            'insentif_supir' => $insentifSupir,
            'insentif_kernet' => $insentifKernet,
            'tunjangan_makan' => $tunjanganMakan,
            'subsidi_transport' => $subsidiTransport,
            'insentif_kir' => $insentifKir,
            'insentif_muatan_berat' => $insentifMuatanBerat,
            'premi' => 0,
            'total_upah' => $totalUpah,
            'potongan_jamsostek' => $potonganJamsostek,
            'potongan_cat' => 0,
            'potongan_pengobatan' => 0,
            'potongan_spsi' => 0,
            'total_potongan' => $potonganJamsostek,
            'dibayar' => $totalUpah - $potonganJamsostek,
        ]);
    }

    return response()->json([
        'message' => 'Slip gaji berhasil di-generate/diperbarui',
        'periode' => $periode,
        'slip' => SlipGaji::with('pegawai')->where('periode_gaji_id', $periode->id)->get(),
    ], 201);
}

    // Lihat semua periode yang pernah dibuat
    public function daftarPeriode()
    {
        return PeriodeGaji::orderByDesc('tanggal_mulai')->get();
    }

    // Lihat detail slip gaji dalam 1 periode
    // Admin: lihat semua pegawai. User: otomatis difilter ke pegawai_id miliknya sendiri.
    public function detailPeriode(Request $request, string $periodeId)
    {
        $pengguna = $request->user();
        $query = SlipGaji::with(['pegawai', 'periode'])->where('periode_gaji_id', $periodeId);
        if (! $pengguna->isAdmin()) {
            $query->where('pegawai_id', $pengguna->pegawai_id);
        }

        return $query->get();
    }

    // Update potongan manual (Cat, Pengobatan, SPSI) per slip -- admin saja
   public function updatePotongan(Request $request, string $slipId)
{
    $request->validate([
        'potongan_cat' => 'nullable|numeric|min:0',
        'potongan_pengobatan' => 'nullable|numeric|min:0',
        'potongan_spsi' => 'nullable|numeric|min:0',
        'premi' => 'nullable|numeric|min:0',
        'hari_sbg_supir' => 'nullable|integer|min:0',
        'hari_sbg_kernet' => 'nullable|integer|min:0',
    ]);

    $slip = SlipGaji::findOrFail($slipId);

    $slip->potongan_cat = $request->input('potongan_cat', $slip->potongan_cat);
    $slip->potongan_pengobatan = $request->input('potongan_pengobatan', $slip->potongan_pengobatan);
    $slip->potongan_spsi = $request->input('potongan_spsi', $slip->potongan_spsi);
    $slip->premi = $request->input('premi', $slip->premi);
    $slip->hari_sbg_supir = $request->input('hari_sbg_supir', $slip->hari_sbg_supir);
    $slip->hari_sbg_kernet = $request->input('hari_sbg_kernet', $slip->hari_sbg_kernet);

    $slip->total_upah = $slip->upah + $slip->insentif_supir + $slip->insentif_kernet
        + $slip->tunjangan_makan + $slip->subsidi_transport
        + $slip->insentif_kir + $slip->insentif_muatan_berat + $slip->premi;

    $slip->total_potongan = $slip->potongan_jamsostek
        + $slip->potongan_cat + $slip->potongan_pengobatan + $slip->potongan_spsi;
    $slip->dibayar = $slip->total_upah - $slip->total_potongan;

    $slip->save();

    return response()->json($slip);
}

   // Update slip gaji lengkap dari halaman Summary -- admin saja
   public function updateSlip(Request $request, string $slipId)
{
    $request->validate([
        'hari_kerja' => 'nullable|integer|min:0|max:31',
        'hadir' => 'nullable|integer|min:0|max:31',
        'upah' => 'nullable|numeric|min:0',
        'tunjangan_makan' => 'nullable|numeric|min:0',
        'subsidi_transport' => 'nullable|numeric|min:0',
        'premi' => 'nullable|numeric|min:0',
        'potongan_jamsostek' => 'nullable|numeric|min:0',
        'potongan_cat' => 'nullable|numeric|min:0',
        'potongan_pengobatan' => 'nullable|numeric|min:0',
        'potongan_spsi' => 'nullable|numeric|min:0',
    ]);

    $slip = SlipGaji::findOrFail($slipId);

    $slip->hari_kerja = $request->input('hari_kerja', $slip->hari_kerja);
    $slip->hadir = $request->input('hadir', $slip->hadir);
    $slip->upah = $request->input('upah', $slip->upah);
    $slip->tunjangan_makan = $request->input('tunjangan_makan', $slip->tunjangan_makan);
    $slip->subsidi_transport = $request->input('subsidi_transport', $slip->subsidi_transport);
    $slip->premi = $request->input('premi', $slip->premi);
    $slip->potongan_jamsostek = $request->input('potongan_jamsostek', $slip->potongan_jamsostek);
    $slip->potongan_cat = $request->input('potongan_cat', $slip->potongan_cat);
    $slip->potongan_pengobatan = $request->input('potongan_pengobatan', $slip->potongan_pengobatan);
    $slip->potongan_spsi = $request->input('potongan_spsi', $slip->potongan_spsi);

    $slip->total_upah = $slip->upah + $slip->insentif_supir + $slip->insentif_kernet
        + $slip->tunjangan_makan + $slip->subsidi_transport
        + $slip->insentif_kir + $slip->insentif_muatan_berat + $slip->premi;

    $slip->total_potongan = $slip->potongan_jamsostek
        + $slip->potongan_cat + $slip->potongan_pengobatan + $slip->potongan_spsi;
    $slip->dibayar = $slip->total_upah - $slip->total_potongan;

    $slip->save();

    return response()->json($slip);
}
}
