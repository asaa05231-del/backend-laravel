<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class TarifUpahTahunan extends Model
{
    protected $fillable = [
        'tahun', 'upah_harian_supir', 'upah_harian_kernet',
        'upah_mingguan_supir', 'upah_mingguan_kernet',
        'tunjangan_makan_harian', 'subsidi_transport_harian',
    ];
}