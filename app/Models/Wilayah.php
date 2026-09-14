<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Wilayah extends Model
{
    protected $fillable = ['nama_wilayah', 'tarif_supir', 'tarif_kernet'];
}