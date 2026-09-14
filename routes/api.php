<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\PegawaiController;
use App\Http\Controllers\Api\PengirimanController;
use App\Http\Controllers\Api\PenggajianController;
use App\Http\Controllers\Api\WilayahController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\TarifUpahTahunanController;
use App\Http\Controllers\Api\PengaturanGajiController;

// Login publik, tidak butuh token. Body: { username, password }
Route::post('/login', [AuthController::class, 'login']);

// Semua route di bawah ini wajib pakai header:
// Authorization: Bearer <token>
Route::middleware('auth:sanctum')->group(function () {
    Route::post('/logout', [AuthController::class, 'logout']);
    Route::get('/me', [AuthController::class, 'me']);
    Route::patch('/me', [AuthController::class, 'updateProfile']);

    Route::get('/pengiriman', [PengirimanController::class, 'index']);
    Route::get('/pengiriman/{pengiriman}', [PengirimanController::class, 'show']);

    Route::get('/penggajian/periode', [PenggajianController::class, 'daftarPeriode']);
    Route::get('/penggajian/periode/{periodeId}', [PenggajianController::class, 'detailPeriode']);

    Route::get('/wilayah', [WilayahController::class, 'index']);
    Route::get('/wilayah/{wilayah}', [WilayahController::class, 'show']);
    Route::get('/pegawai', [PegawaiController::class, 'index']);

    // ---- Khusus admin ----
    Route::middleware('role:admin')->group(function () {
        Route::post('/pengiriman', [PengirimanController::class, 'store']);
        Route::delete('/pengiriman/{pengiriman}', [PengirimanController::class, 'destroy']);

        Route::post('/penggajian/generate', [PenggajianController::class, 'generate']);
       Route::patch('/penggajian/slip/{slipId}', [PenggajianController::class, 'updateSlip']);
        Route::post('/wilayah', [WilayahController::class, 'store']);
        Route::put('/wilayah/{wilayah}', [WilayahController::class, 'update']);
        Route::delete('/wilayah/{wilayah}', [WilayahController::class, 'destroy']);

        Route::post('/pegawai', [PegawaiController::class, 'store']);
        Route::put('/pegawai/{pegawai}', [PegawaiController::class, 'update']);
        Route::delete('/pegawai/{pegawai}', [PegawaiController::class, 'destroy']);
        Route::apiResource('tarif-upah-tahunan', TarifUpahTahunanController::class)->only(['index', 'store', 'update']);
        Route::get('pengaturan-gaji', [PengaturanGajiController::class, 'index']);
        Route::put('pengaturan-gaji', [PengaturanGajiController::class, 'update']);       
         });
});