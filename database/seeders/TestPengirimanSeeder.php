<?php
namespace Database\Seeders;

use App\Models\Pengiriman;
use Illuminate\Database\Seeder;

class TestPengirimanSeeder extends Seeder
{
    public function run(): void
    {
        // Agus Rohman (id 1) sbg supir, Kamdani (id 5) numpang jadi kernet (placeholder teknis)
        $tanggalAgus = ['2026-07-01', '2026-07-02', '2026-07-03', '2026-07-06', '2026-07-07'];
        foreach ($tanggalAgus as $tgl) {
            Pengiriman::create([
                'tanggal' => $tgl,
                'supir_id' => 1,
                'kernet_id' => 5,
                'wilayah_id' => 3,
                'muatan_berat' => false,
                'total_biaya' => 45000,
            ]);
        }

        // Budi Bahtiar (id 2) sbg supir DAN kernet sekaligus, semua muatan berat
        $budiData = [
            ['2026-07-01', 6],
            ['2026-07-01', 3],
            ['2026-07-02', 4],
            ['2026-07-03', 6],
            ['2026-07-06', 4],
            ['2026-07-07', 3],
        ];
        foreach ($budiData as [$tgl, $wilayahId]) {
            Pengiriman::create([
                'tanggal' => $tgl,
                'supir_id' => 2,
                'kernet_id' => 2,
                'wilayah_id' => $wilayahId,
                'muatan_berat' => true,
                'total_biaya' => 0,
            ]);
        }
    }
}