<?php
namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\TarifUpahTahunan;
use Illuminate\Http\Request;

class TarifUpahTahunanController extends Controller
{
    public function index()
    {
        return TarifUpahTahunan::orderByDesc('tahun')->get();
    }

    public function store(Request $request)
    {
        $data = $request->validate([
            'tahun' => 'required|integer|min:2000|max:2100',
            'upah_harian_supir' => 'required|numeric|min:0',
            'upah_harian_kernet' => 'required|numeric|min:0',
            'upah_mingguan_supir' => 'required|numeric|min:0',
            'upah_mingguan_kernet' => 'required|numeric|min:0',
            'tunjangan_makan_harian' => 'required|numeric|min:0',
            'subsidi_transport_harian' => 'required|numeric|min:0',
        ]);

        $tarif = TarifUpahTahunan::updateOrCreate(['tahun' => $data['tahun']], $data);

        return response()->json($tarif, 201);
    }

    public function update(Request $request, string $id)
    {
        $tarif = TarifUpahTahunan::findOrFail($id);

        $data = $request->validate([
            'upah_harian_supir' => 'required|numeric|min:0',
            'upah_harian_kernet' => 'required|numeric|min:0',
            'upah_mingguan_supir' => 'required|numeric|min:0',
            'upah_mingguan_kernet' => 'required|numeric|min:0',
            'tunjangan_makan_harian' => 'required|numeric|min:0',
            'subsidi_transport_harian' => 'required|numeric|min:0',
        ]);

        $tarif->update($data);

        return response()->json($tarif);
    }
}