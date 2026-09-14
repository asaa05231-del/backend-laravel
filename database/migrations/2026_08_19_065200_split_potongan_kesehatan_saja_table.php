<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->dropColumn('potongan_kesehatan_saja');
        });

        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->decimal('potongan_kesehatan_supir', 12, 2)->default(9400)->after('tarif_kir');
            $table->decimal('potongan_kesehatan_kernet', 12, 2)->default(9400)->after('potongan_kesehatan_supir');
        });
    }

    public function down(): void
    {
        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->dropColumn(['potongan_kesehatan_supir', 'potongan_kesehatan_kernet']);
            $table->decimal('potongan_kesehatan_saja', 12, 2)->default(9400)->after('tarif_kir');
        });
    }
};