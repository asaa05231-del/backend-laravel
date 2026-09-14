<?php
namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Pegawai;
use Illuminate\Http\Request;

class PegawaiController extends Controller
{
    public function index()
    {
        return Pegawai::orderBy('nama')->get();
    }

    public function store(Request $request)
    {
        $data = $request->validate([
            'nama' => 'required|string',
            'jabatan' => 'required|in:sopir,kernet',
            'tanggal_masuk' => 'nullable|date',
            'upah_harian' => 'nullable|numeric',
            'upah_mingguan' => 'nullable|numeric',
            'tunjangan_transport' => 'nullable|numeric',
            'persen_jamsostek' => 'nullable|numeric|min:0|max:100',
            'ikut_jamsostek' => 'nullable|boolean',
        ]);

        return response()->json(Pegawai::create($data), 201);
    }

    public function update(Request $request, string $id)
    {
        $pegawai = Pegawai::findOrFail($id);

        $data = $request->validate([
            'nama' => 'sometimes|string',
            'jabatan' => 'sometimes|in:sopir,kernet',
            'tanggal_masuk' => 'nullable|date',
            'upah_harian' => 'nullable|numeric',
            'upah_mingguan' => 'nullable|numeric',
            'tunjangan_transport' => 'nullable|numeric',
            'persen_jamsostek' => 'nullable|numeric|min:0|max:100',
            'ikut_jamsostek' => 'nullable|boolean',
        ]);

        $pegawai->update($data);

        return response()->json($pegawai);
    }

    public function destroy(string $id)
    {
        Pegawai::findOrFail($id)->delete();

        return response()->json(['message' => 'Pegawai dihapus']);
    }
}