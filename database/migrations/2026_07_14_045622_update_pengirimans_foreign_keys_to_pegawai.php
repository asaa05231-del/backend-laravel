<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('pengirimans', function (Blueprint $table) {
            // Hapus foreign key lama yang menunjuk ke tabel supirs & kernets
            $table->dropForeign(['supir_id']);
            $table->dropForeign(['kernet_id']);
        });

        Schema::table('pengirimans', function (Blueprint $table) {
            // Pasang foreign key baru: supir_id dan kernet_id sama-sama
            // menunjuk ke tabel pegawais (1 pegawai bisa jadi supir maupun kernet)
            $table->foreign('supir_id')
                  ->references('id')->on('pegawais')
                  ->cascadeOnDelete();

            $table->foreign('kernet_id')
                  ->references('id')->on('pegawais')
                  ->cascadeOnDelete();
        });
    }

    public function down(): void
    {
        Schema::table('pengirimans', function (Blueprint $table) {
            $table->dropForeign(['supir_id']);
            $table->dropForeign(['kernet_id']);
        });

        Schema::table('pengirimans', function (Blueprint $table) {
            $table->foreign('supir_id')
                  ->references('id')->on('supirs')
                  ->cascadeOnDelete();

            $table->foreign('kernet_id')
                  ->references('id')->on('kernets')
                  ->cascadeOnDelete();
        });
    }
};
