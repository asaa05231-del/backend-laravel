<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PeriodeGaji extends Model
{
    protected $fillable = ['tanggal_mulai', 'tanggal_selesai'];

    public function slipGajis()
    {
        return $this->hasMany(SlipGaji::class);
    }
}