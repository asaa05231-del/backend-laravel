<?php
use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->decimal('tarif_kir', 12, 2)->default(30000)->after('komisi_muatan_berat_kernet');
        });
    }

    public function down(): void
    {
        Schema::table('pengaturan_gaji', function (Blueprint $table) {
            $table->dropColumn('tarif_kir');
        });
    }
};