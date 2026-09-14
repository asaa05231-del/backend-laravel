<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SlipGaji extends Model
{
   protected $fillable = [
    'pegawai_id', 'periode_gaji_id',
    'hari_sbg_supir', 'hari_sbg_kernet', 'hadir',
    'upah', 'insentif_supir', 'insentif_kernet',
    'tunjangan_makan', 'subsidi_transport',
    'insentif_kir', 'insentif_muatan_berat', 'premi',
    'total_upah',
    'potongan_jamsostek', 'potongan_cat', 'potongan_pengobatan', 'potongan_spsi',
    'total_potongan', 'dibayar',
    'pegawai_id', 'hari_kerja', 'periode_gaji_id',
];
    public function pegawai()
    {
        return $this->belongsTo(Pegawai::class);
    }

    public function periode()
    {
        return $this->belongsTo(PeriodeGaji::class, 'periode_gaji_id');
    }
}