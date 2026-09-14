<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->decimal('jamsostek_supir', 12, 2)->default(12500)->after('potongan_kesehatan_kernet');
            $table->decimal('jamsostek_kernet', 12, 2)->default(11400)->after('jamsostek_supir');
        });
    }

    public function down(): void
    {
        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->dropColumn(['jamsostek_supir', 'jamsostek_kernet']);
        });
    }
};