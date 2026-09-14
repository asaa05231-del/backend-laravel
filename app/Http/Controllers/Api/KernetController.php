<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Kernet;
use Illuminate\Http\Request;

class KernetController extends Controller
{
    /**
     * Menampilkan semua data kernet.
     */
    public function index()
    {
        return response()->json(Kernet::all(), 200);
    }

    /**
     * Menyimpan data kernet baru.
     */
    public function store(Request $request)
    {
        $request->validate([
            'nama' => 'required|string|max:255',
            'tarif' => 'required|numeric'
        ]);

        $kernet = Kernet::create([
            'nama' => $request->nama,
            'tarif' => $request->tarif
        ]);

        return response()->json([
            'message' => 'Data kernet berhasil ditambahkan',
            'data' => $kernet
        ], 201);
    }

    /**
     * Menampilkan satu data kernet.
     */
    public function show(string $id)
    {
        $kernet = Kernet::find($id);

        if (!$kernet) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        return response()->json($kernet, 200);
    }

    /**
     * Mengubah data kernet.
     */
    public function update(Request $request, string $id)
    {
        $kernet = Kernet::find($id);

        if (!$kernet) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        $request->validate([
            'nama' => 'required|string|max:255',
            'tarif' => 'required|numeric'
        ]);

        $kernet->update([
            'nama' => $request->nama,
            'tarif' => $request->tarif
        ]);

        return response()->json([
            'message' => 'Data kernet berhasil diubah',
            'data' => $kernet
        ], 200);
    }

    /**
     * Menghapus data kernet.
     */
    public function destroy(string $id)
    {
        $kernet = Kernet::find($id);

        if (!$kernet) {
            return response()->json([
                'message' => 'Data tidak ditemukan'
            ], 404);
        }

        $kernet->delete();

        return response()->json([
            'message' => 'Data kernet berhasil dihapus'
        ], 200);
    }
}