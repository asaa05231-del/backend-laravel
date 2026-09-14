<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Wilayah;
use Illuminate\Http\Request;

class WilayahController extends Controller
{
    public function index()
    {
        return response()->json(Wilayah::all(), 200);
    }

    public function store(Request $request)
    {
        $request->validate([
            'nama_wilayah' => 'required',
            'tarif_supir' => 'required|numeric',
            'tarif_kernet' => 'required|numeric',
        ]);

        $wilayah = Wilayah::create([
            'nama_wilayah' => $request->nama_wilayah,
            'tarif_supir' => $request->tarif_supir,
            'tarif_kernet' => $request->tarif_kernet,
        ]);

        return response()->json([
            'message' => 'Data wilayah berhasil ditambahkan',
            'data' => $wilayah
        ], 201);
    }

    public function show(string $id)
    {
        $wilayah = Wilayah::find($id);

        if (!$wilayah) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        return response()->json($wilayah);
    }

    public function update(Request $request, string $id)
    {
        $wilayah = Wilayah::find($id);

        if (!$wilayah) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        $request->validate([
            'nama_wilayah' => 'required',
            'tarif_supir' => 'required|numeric',
            'tarif_kernet' => 'required|numeric',
        ]);

        $wilayah->update([
            'nama_wilayah' => $request->nama_wilayah,
            'tarif_supir' => $request->tarif_supir,
            'tarif_kernet' => $request->tarif_kernet,
        ]);

        return response()->json([
            'message' => 'Data wilayah berhasil diubah',
            'data' => $wilayah
        ]);
    }

    public function destroy(string $id)
    {
        $wilayah = Wilayah::find($id);

        if (!$wilayah) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        $wilayah->delete();

        return response()->json([
            'message' => 'Data wilayah berhasil dihapus'
        ]);
    }
}