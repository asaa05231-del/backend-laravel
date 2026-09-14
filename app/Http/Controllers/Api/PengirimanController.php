<?php
namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Pengiriman;
use App\Models\Pegawai;
use App\Models\PengaturanGaji;
use App\Models\Wilayah;
use Illuminate\Http\Request;

class PengirimanController extends Controller
{
    public function index()
    {
        return response()->json(
            Pengiriman::with(['supir', 'kernet', 'wilayah'])->get()
        );
    }

   public function store(Request $request)
{
    $isKir = $request->boolean('kir');
    $isBengkel = $request->boolean('bengkel');

    $rules = [
        'tanggal' => 'required|date',
        'supir_id' => 'nullable|exists:pegawais,id',
        'supir_manual' => 'nullable|string|max:255',
        'muatan_berat' => 'boolean',
        'kir' => 'boolean',
        'bengkel' => 'boolean',
    ];

    if ($isBengkel) {
        $rules['kernet_id'] = 'nullable|exists:pegawais,id';
        $rules['kernet_manual'] = 'nullable|string|max:255';
        $rules['wilayah_id'] = 'nullable';
    } elseif ($isKir) {
        $rules['kernet_id'] = 'nullable';
        $rules['kernet_manual'] = 'nullable';
        $rules['wilayah_id'] = 'nullable';
    } else {
        $rules['kernet_id'] = 'nullable|exists:pegawais,id';
        $rules['kernet_manual'] = 'nullable|string|max:255';
        $rules['wilayah_id'] = 'required|exists:wilayahs,id';
    }

    $request->validate($rules);

    if (!$request->filled('supir_id') && !$request->filled('supir_manual')) {
        return response()->json(['message' => 'Sopir wajib diisi (pilih pegawai atau isi manual).'], 422);
    }

    if (!$isKir && !$request->filled('kernet_id') && !$request->filled('kernet_manual')) {
        return response()->json(['message' => 'Kernet wajib diisi (pilih pegawai atau isi manual).'], 422);
    }

    $pengaturan = PengaturanGaji::first();

    if ($isBengkel) {
        $pengiriman = Pengiriman::create([
            'tanggal' => $request->tanggal,
            'supir_id' => $request->supir_id,
            'supir_manual' => $request->supir_manual,
            'kernet_id' => $request->kernet_id,
            'kernet_manual' => $request->kernet_manual,
            'wilayah_id' => null,
            'muatan_berat' => false,
            'kir' => false,
            'bengkel' => true,
            'total_biaya' => 0,
        ]);

        return response()->json(['message' => 'Data Bengkel berhasil disimpan', 'data' => $pengiriman], 201);
    }

    if ($isKir) {
        $pengiriman = Pengiriman::create([
            'tanggal' => $request->tanggal,
            'supir_id' => $request->supir_id,
            'supir_manual' => $request->supir_manual,
            'kernet_id' => null,
            'kernet_manual' => null,
            'wilayah_id' => null,
            'muatan_berat' => false,
            'kir' => true,
            'bengkel' => false,
            'total_biaya' => $pengaturan->tarif_kir,
        ]);

        return response()->json(['message' => 'Data KIR berhasil disimpan', 'data' => $pengiriman], 201);
    }

    $wilayah = Wilayah::findOrFail($request->wilayah_id);

    $pengiriman = Pengiriman::create([
        'tanggal' => $request->tanggal,
        'supir_id' => $request->supir_id,
        'supir_manual' => $request->supir_manual,
        'kernet_id' => $request->kernet_id,
        'kernet_manual' => $request->kernet_manual,
        'wilayah_id' => $wilayah->id,
        'muatan_berat' => $request->boolean('muatan_berat'),
        'kir' => false,
        'bengkel' => false,
        'total_biaya' => $wilayah->tarif_supir + $wilayah->tarif_kernet,
    ]);

    return response()->json(['message' => 'Data pengiriman berhasil disimpan', 'data' => $pengiriman], 201);
}

public function update(Request $request, string $id)
{
    $pengiriman = Pengiriman::findOrFail($id);
    $isKir = $request->boolean('kir');
    $isBengkel = $request->boolean('bengkel');

    $rules = [
        'tanggal' => 'required|date',
        'supir_id' => 'nullable|exists:pegawais,id',
        'supir_manual' => 'nullable|string|max:255',
        'muatan_berat' => 'boolean',
        'kir' => 'boolean',
        'bengkel' => 'boolean',
    ];

    if ($isBengkel) {
        $rules['kernet_id'] = 'nullable|exists:pegawais,id';
        $rules['kernet_manual'] = 'nullable|string|max:255';
        $rules['wilayah_id'] = 'nullable';
    } elseif ($isKir) {
        $rules['kernet_id'] = 'nullable';
        $rules['kernet_manual'] = 'nullable';
        $rules['wilayah_id'] = 'nullable';
    } else {
        $rules['kernet_id'] = 'nullable|exists:pegawais,id';
        $rules['kernet_manual'] = 'nullable|string|max:255';
        $rules['wilayah_id'] = 'required|exists:wilayahs,id';
    }

    $request->validate($rules);

    if (!$request->filled('supir_id') && !$request->filled('supir_manual')) {
        return response()->json(['message' => 'Sopir wajib diisi (pilih pegawai atau isi manual).'], 422);
    }

    if (!$isKir && !$request->filled('kernet_id') && !$request->filled('kernet_manual')) {
        return response()->json(['message' => 'Kernet wajib diisi (pilih pegawai atau isi manual).'], 422);
    }

    $pengaturan = PengaturanGaji::first();

    if ($isBengkel) {
        $pengiriman->update([
            'tanggal' => $request->tanggal,
            'supir_id' => $request->supir_id,
            'supir_manual' => $request->supir_manual,
            'kernet_id' => $request->kernet_id,
            'kernet_manual' => $request->kernet_manual,
            'wilayah_id' => null,
            'muatan_berat' => false,
            'kir' => false,
            'bengkel' => true,
            'total_biaya' => 0,
        ]);
    } elseif ($isKir) {
        $pengiriman->update([
            'tanggal' => $request->tanggal,
            'supir_id' => $request->supir_id,
            'supir_manual' => $request->supir_manual,
            'kernet_id' => null,
            'kernet_manual' => null,
            'wilayah_id' => null,
            'muatan_berat' => false,
            'kir' => true,
            'bengkel' => false,
            'total_biaya' => $pengaturan->tarif_kir,
        ]);
    } else {
        $wilayah = Wilayah::findOrFail($request->wilayah_id);

        $pengiriman->update([
            'tanggal' => $request->tanggal,
            'supir_id' => $request->supir_id,
            'supir_manual' => $request->supir_manual,
            'kernet_id' => $request->kernet_id,
            'kernet_manual' => $request->kernet_manual,
            'wilayah_id' => $wilayah->id,
            'muatan_berat' => $request->boolean('muatan_berat'),
            'kir' => false,
            'bengkel' => false,
            'total_biaya' => $wilayah->tarif_supir + $wilayah->tarif_kernet,
        ]);
    }

    return response()->json(['message' => 'Data berhasil diupdate', 'data' => $pengiriman]);
}

    public function destroy(string $id)
    {
        Pengiriman::findOrFail($id)->delete();

        return response()->json([
            'message' => 'Data berhasil dihapus'
        ]);
    }
}