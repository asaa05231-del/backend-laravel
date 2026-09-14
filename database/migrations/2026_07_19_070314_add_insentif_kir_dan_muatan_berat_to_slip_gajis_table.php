<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('slip_gajis', function (Blueprint $table) {
            $table->decimal('insentif_kir', 12, 2)->default(0)->after('subsidi_transport');
            $table->decimal('insentif_muatan_berat', 12, 2)->default(0)->after('insentif_kir');
        });
    }

    public function down(): void
    {
        Schema::table('slip_gajis', function (Blueprint $table) {
            $table->dropColumn(['insentif_kir', 'insentif_muatan_berat']);
        });
    }
};