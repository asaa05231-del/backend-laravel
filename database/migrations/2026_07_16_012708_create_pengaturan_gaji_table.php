<?php
use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('pengaturan_gaji', function (Blueprint $table) {
            $table->id();
            $table->decimal('upah_harian_supir', 12, 2)->default(58000);
            $table->decimal('upah_harian_kernet', 12, 2)->default(53000);
            $table->decimal('tunjangan_makan_harian', 12, 2)->default(10500);
            $table->decimal('subsidi_transport_harian', 12, 2)->default(10000);
            $table->decimal('komisi_muatan_berat_supir', 12, 2)->default(5000);
            $table->decimal('komisi_muatan_berat_kernet', 12, 2)->default(4000);
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('pengaturan_gaji');
    }
};