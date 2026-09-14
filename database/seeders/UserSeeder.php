<?php

namespace Database\Seeders;

use App\Models\Pegawai;
use App\Models\User;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Str;

class UserSeeder extends Seeder
{
    public function run(): void
    {
        // Username dibuat dari nama, huruf kecil & tanpa spasi.
        // Contoh: "Agus Rochman" -> "agus.rochman"
        Pegawai::all()->each(function (Pegawai $pegawai) {
            $usernameDasar = Str::slug($pegawai->nama, '.');
            $username = $usernameDasar;
            $i = 1;
            while (User::where('username', $username)->exists()) {
                $username = $usernameDasar.$i++;
            }

            User::create([
                'name' => $pegawai->nama,
                'username' => $username,
                'email' => $username.'@pengiriman.local', // dummy, tidak dipakai login
                'password' => Hash::make('user123'),
                'role' => 'user',
                'pegawai_id' => $pegawai->id,
            ]);
        });
    }
}
