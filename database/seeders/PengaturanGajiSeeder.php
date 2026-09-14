<?php
namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\DB;

class PengaturanGajiSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        // Hanya isi kalau tabel masih kosong, supaya tidak dobel
        // kalau seeder ini dijalankan lebih dari sekali
        if (DB::table('pengaturan_gaji')->count() === 0) {
            DB::table('pengaturan_gaji')->insert([
                'upah_harian_supir' => 58000,
                'upah_harian_kernet' => 53000,
                'tunjangan_makan_harian' => 10500,
                'subsidi_transport_harian' => 10000,
                'komisi_muatan_berat_supir' => 5000,
                'komisi_muatan_berat_kernet' => 4000,
                'created_at' => now(),
                'updated_at' => now(),
            ]);
        }
    }
}