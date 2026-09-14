<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('tarif_upah_tahunans', function (Blueprint $table) {
            $table->decimal('tunjangan_makan_harian', 12, 2)->default(0);
            $table->decimal('subsidi_transport_harian', 12, 2)->default(0);
        });
    }

    public function down(): void
    {
        Schema::table('tarif_upah_tahunans', function (Blueprint $table) {
            $table->dropColumn(['tunjangan_makan_harian', 'subsidi_transport_harian']);
        });
    }
};