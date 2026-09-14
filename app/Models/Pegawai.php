<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Pegawai extends Model
{
    protected $fillable = [
        'nama', 'jabatan', 'tanggal_masuk', 'upah_harian', 'upah_mingguan',
        'tunjangan_transport', 'persen_jamsostek', 'ikut_jamsostek',
    ];

    protected $casts = [
        'ikut_jamsostek' => 'boolean',
    ];

    public function sebagaiSupir()
    {
        return $this->hasMany(Pengiriman::class, 'supir_id');
    }

    public function sebagaiKernet()
    {
        return $this->hasMany(Pengiriman::class, 'kernet_id');
    }

    public function slipGajis()
    {
        return $this->hasMany(SlipGaji::class);
    }

    public function akun()
    {
        return $this->hasOne(User::class);
    }
}