<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('slip_gajis', function (Blueprint $table) {
            $table->id();
            $table->foreignId('pegawai_id')->constrained()->onDelete('cascade');
            $table->foreignId('periode_gaji_id')->constrained()->onDelete('cascade');
            $table->integer('jumlah_hk')->default(0);
            $table->decimal('insentif', 12, 2)->default(0);
            $table->decimal('tunjangan_makan', 12, 2)->default(0);
            $table->decimal('tunjangan_transport', 12, 2)->default(0);
            $table->decimal('potongan', 12, 2)->default(0);
            $table->decimal('total_upah', 12, 2)->default(0);
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('slip_gajis');
    }
};