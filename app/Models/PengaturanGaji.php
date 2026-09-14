<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PengaturanGaji extends Model
{
    protected $table = 'pengaturan_gaji';

    protected $fillable = [
        'upah_harian_supir',
        'upah_harian_kernet',
        'tunjangan_makan_harian',
        'subsidi_transport_harian',
        'komisi_muatan_berat_supir',
        'komisi_muatan_berat_kernet',
        'tarif_kir',
        'potongan_kesehatan_supir',
        'potongan_kesehatan_kernet',
        'jamsostek_supir',
        'jamsostek_kernet',
    ];
}