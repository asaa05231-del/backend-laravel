<?php
namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\PengaturanGaji;
use Illuminate\Http\Request;

class PengaturanGajiController extends Controller
{
    public function index()
    {
        return PengaturanGaji::first();
    }

    public function update(Request $request)
    {
        $data = $request->validate([
            'jamsostek_supir' => 'nullable|numeric|min:0',
            'jamsostek_kernet' => 'nullable|numeric|min:0',
            'potongan_kesehatan_supir' => 'nullable|numeric|min:0',
            'potongan_kesehatan_kernet' => 'nullable|numeric|min:0',
        ]);

        $pengaturan = PengaturanGaji::first();
        $pengaturan->update($data);

        return response()->json($pengaturan);
    }
}