<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Pengiriman extends Model
{
    protected $table = 'pengirimans';

    protected $fillable = [
    'tanggal', 'supir_id', 'supir_manual', 'kernet_id', 'kernet_manual', 'wilayah_id',
    'muatan_berat', 'kir', 'bengkel', 'total_biaya',
];

    protected $casts = [
        'muatan_berat' => 'boolean',
        'kir' => 'boolean',
         'bengkel' => 'boolean',
    ];

    public function supir()
    {
        return $this->belongsTo(Pegawai::class, 'supir_id');
    }

    public function kernet()
    {
        return $this->belongsTo(Pegawai::class, 'kernet_id');
    }

    public function wilayah()
    {
        return $this->belongsTo(Wilayah::class);
    }
}