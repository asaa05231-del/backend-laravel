<?php
use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('slip_gajis', function (Blueprint $table) {
            // Hapus kolom lama yang sudah tidak dipakai
            $table->dropColumn(['jumlah_hk', 'insentif', 'tunjangan_transport', 'potongan']);
        });

        Schema::table('slip_gajis', function (Blueprint $table) {
            // Rincian hari kerja
            $table->integer('hari_sbg_supir')->default(0)->after('periode_gaji_id');
            $table->integer('hari_sbg_kernet')->default(0)->after('hari_sbg_supir');

            // Komponen pendapatan
            $table->decimal('upah', 12, 2)->default(0)->after('hari_sbg_kernet');
            $table->decimal('insentif_supir', 12, 2)->default(0)->after('upah');
            $table->decimal('insentif_kernet', 12, 2)->default(0)->after('insentif_supir');
            $table->decimal('subsidi_transport', 12, 2)->default(0)->after('tunjangan_makan');
            $table->decimal('uang_lain_lain', 12, 2)->default(0)->after('subsidi_transport');

            // Komponen potongan
            $table->decimal('potongan_jamsostek', 12, 2)->default(0)->after('total_upah');
            $table->decimal('potongan_cat', 12, 2)->default(0)->after('potongan_jamsostek');
            $table->decimal('potongan_pengobatan', 12, 2)->default(0)->after('potongan_cat');
            $table->decimal('potongan_spsi', 12, 2)->default(0)->after('potongan_pengobatan');
            $table->decimal('total_potongan', 12, 2)->default(0)->after('potongan_spsi');

            // Hasil akhir
            $table->decimal('dibayar', 12, 2)->default(0)->after('total_potongan');
        });
    }

    public function down(): void
    {
        Schema::table('slip_gajis', function (Blueprint $table) {
            $table->dropColumn([
                'hari_sbg_supir', 'hari_sbg_kernet', 'upah',
                'insentif_supir', 'insentif_kernet',
                'subsidi_transport', 'uang_lain_lain',
                'potongan_jamsostek', 'potongan_cat', 'potongan_pengobatan', 'potongan_spsi',
                'total_potongan', 'dibayar',
            ]);
        });

        Schema::table('slip_gajis', function (Blueprint $table) {
            $table->integer('jumlah_hk')->default(0);
            $table->decimal('insentif', 12, 2)->default(0);
            $table->decimal('tunjangan_transport', 12, 2)->default(0);
            $table->decimal('potongan', 12, 2)->default(0);
        });
    }
};