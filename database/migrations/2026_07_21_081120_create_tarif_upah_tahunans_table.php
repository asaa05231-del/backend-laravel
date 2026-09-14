<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('tarif_upah_tahunans', function (Blueprint $table) {
            $table->id();
            $table->unsignedSmallInteger('tahun')->unique();
            $table->decimal('upah_harian_supir', 12, 2)->default(0);
            $table->decimal('upah_harian_kernet', 12, 2)->default(0);
            $table->decimal('upah_mingguan_supir', 12, 2)->default(0);
            $table->decimal('upah_mingguan_kernet', 12, 2)->default(0);
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('tarif_upah_tahunans');
    }
};