<?php

namespace Database\Seeders;

use App\Models\Wilayah;
use Illuminate\Database\Seeder;

class WilayahSeeder extends Seeder
{
    public function run(): void
    {
        $data = [
            ['nama_wilayah' => 'Wilayah I', 'tarif_supir' => 25000, 'tarif_kernet' => 20000],
            ['nama_wilayah' => 'Wilayah II', 'tarif_supir' => 35000, 'tarif_kernet' => 30000],
            ['nama_wilayah' => 'Wilayah III', 'tarif_supir' => 45000, 'tarif_kernet' => 40000],
            ['nama_wilayah' => 'Wilayah IV', 'tarif_supir' => 50000, 'tarif_kernet' => 45000],
            ['nama_wilayah' => 'Wilayah V', 'tarif_supir' => 100000, 'tarif_kernet' => 90000],
            ['nama_wilayah' => 'Wilayah VI', 'tarif_supir' => 65000, 'tarif_kernet' => 60000],
        ];

        foreach ($data as $w) {
            Wilayah::create($w);
        }
    }
}
