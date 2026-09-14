<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        // 1. Lepas foreign key lama supaya bisa update bebas
        Schema::table('pengirimans', function (Blueprint $table) {
            $table->dropForeign(['supir_id']);
            $table->dropForeign(['kernet_id']);
        });

        // 2. Pindahkan data supir ke pegawais
        $petaSupirKeNama = [];
        foreach (DB::table('supirs')->get() as $s) {
            $pegawaiId = DB::table('pegawais')->insertGetId([
                'nama' => $s->nama,
                'upah_harian' => 0,
                'upah_mingguan' => 0,
                'tunjangan_transport' => 0,
                'created_at' => now(),
                'updated_at' => now(),
            ]);
            $petaSupirKeNama[$s->id] = $pegawaiId;
        }

        // 3. Cocokkan kernet ke pegawai yang sudah ada (berdasarkan nama)
        $petaKernetKeNama = [];
        foreach (DB::table('kernets')->get() as $k) {
            $pegawai = DB::table('pegawais')->where('nama', $k->nama)->first();
            if ($pegawai) {
                $petaKernetKeNama[$k->id] = $pegawai->id;
            } else {
                $pegawaiId = DB::table('pegawais')->insertGetId([
                    'nama' => $k->nama,
                    'upah_harian' => 0,
                    'upah_mingguan' => 0,
                    'tunjangan_transport' => 0,
                    'created_at' => now(),
                    'updated_at' => now(),
                ]);
                $petaKernetKeNama[$k->id] = $pegawaiId;
            }
        }

        // 4. Update pengirimans ke id pegawai baru
        foreach (DB::table('pengirimans')->get() as $p) {
            DB::table('pengirimans')->where('id', $p->id)->update([
                'supir_id' => $petaSupirKeNama[$p->supir_id] ?? null,
                'kernet_id' => $petaKernetKeNama[$p->kernet_id] ?? null,
            ]);
        }

        // 5. Pasang foreign key baru merujuk ke pegawais
        Schema::table('pengirimans', function (Blueprint $table) {
            $table->foreign('supir_id')->references('id')->on('pegawais')->onDelete('cascade');
            $table->foreign('kernet_id')->references('id')->on('pegawais')->onDelete('cascade');
        });
    }

    public function down(): void
    {
        // Tidak perlu rollback otomatis untuk migrasi data
    }
};