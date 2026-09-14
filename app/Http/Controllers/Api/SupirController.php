<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Supir;
use Illuminate\Http\Request;

class SupirController extends Controller
{
    /**
     * Menampilkan semua data supir.
     */
    public function index()
    {
        return response()->json(Supir::all(), 200);
    }

    /**
     * Menyimpan data supir baru.
     */
    public function store(Request $request)
    {
        $request->validate([
            'nama' => 'required|string|max:255',
            'tarif' => 'required|numeric'
        ]);

        $supir = Supir::create([
            'nama' => $request->nama,
            'tarif' => $request->tarif
        ]);

        return response()->json([
            'message' => 'Data supir berhasil ditambahkan',
            'data' => $supir
        ], 201);
    }

    /**
     * Menampilkan satu data supir.
     */
    public function show(string $id)
    {
        $supir = Supir::find($id);

        if (!$supir) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        return response()->json($supir, 200);
    }

    /**
     * Mengubah data supir.
     */
    public function update(Request $request, string $id)
    {
        $supir = Supir::find($id);

        if (!$supir) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        $request->validate([
            'nama' => 'required|string|max:255',
            'tarif' => 'required|numeric'
        ]);

        $supir->update([
            'nama' => $request->nama,
            'tarif' => $request->tarif
        ]);

        return response()->json([
            'message' => 'Data supir berhasil diubah',
            'data' => $supir
        ], 200);
    }

    /**
     * Menghapus data supir.
     */
    public function destroy(string $id)
    {
        $supir = Supir::find($id);

        if (!$supir) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        $supir->delete();

        return response()->json([
            'message' => 'Data supir berhasil dihapus'
        ], 200);
    }
}