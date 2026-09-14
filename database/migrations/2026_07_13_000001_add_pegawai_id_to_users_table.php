<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('users', function (Blueprint $table) {
            // Penghubung ke pegawai. Null untuk akun admin (admin tidak
            // mewakili satu pegawai tertentu).
            $table->foreignId('pegawai_id')->nullable()->after('role')
                ->constrained('pegawais')->nullOnDelete();
        });
    }

    public function down(): void
    {
        Schema::table('users', function (Blueprint $table) {
            $table->dropConstrainedForeignId('pegawai_id');
        });
    }
};
