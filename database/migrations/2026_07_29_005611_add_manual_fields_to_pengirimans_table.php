<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('pengirimans', function (Blueprint $table) {
            $table->string('supir_manual')->nullable()->after('supir_id');
            $table->string('kernet_manual')->nullable()->after('kernet_id');
        });

        Schema::table('pengirimans', function (Blueprint $table) {
            $table->foreignId('supir_id')->nullable()->change();
        });
    }

    public function down(): void
    {
        Schema::table('pengirimans', function (Blueprint $table) {
            $table->dropColumn(['supir_manual', 'kernet_manual']);
            $table->foreignId('supir_id')->nullable(false)->change();
        });
    }
};