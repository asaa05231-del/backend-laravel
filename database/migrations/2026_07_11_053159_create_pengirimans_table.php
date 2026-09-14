<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('pengirimans', function (Blueprint $table) {

            $table->id();

            $table->date('tanggal');

            $table->foreignId('supir_id')
                  ->constrained('supirs')
                  ->cascadeOnDelete();

            $table->foreignId('kernet_id')
                  ->constrained('kernets')
                  ->cascadeOnDelete();

            $table->foreignId('wilayah_id')
                  ->constrained('wilayahs')
                  ->cascadeOnDelete();

            $table->decimal('total_biaya', 12, 2);

            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('pengirimans');
    }
};