<?php

namespace Database\Seeders;

use App\Models\Pegawai;
use Illuminate\Database\Seeder;

class PegawaiSeeder extends Seeder
{
    public function run(): void
    {
        $namaOrang = [
            'Agus Rochman', 'Budi Bahtiar', 'Enjang Kartona', 'Emang Supriana', 'Kamdani', 'Kusna',
            'Helirianto', 'Prawito', 'Indra Sutrisno', 'Suratno', 'Endriyanto', 'Saliman', 'Surja',
            'Andri Suhendra', 'Sarmili', 'Surahman', 'Tatang Komarudin', 'Udi Prasetyo', 'Agus Susanto',
            'Sigit P', 'Hepriandi',
        ];

        foreach ($namaOrang as $nama) {
            Pegawai::firstOrCreate(
                ['nama' => $nama],
                ['upah_harian' => 0, 'upah_mingguan' => 0, 'tunjangan_transport' => 0]
            );
        }
    }
}