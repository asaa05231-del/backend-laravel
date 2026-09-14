<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;

class AuthController extends Controller
{
    // POST /api/login
    public function login(Request $request)
    {
        $request->validate([
            'username' => 'required|string',
            'password' => 'required|string',
        ]);

        $user = User::where('username', $request->username)->first();

        if (! $user || ! Hash::check($request->password, $user->password)) {
            return response()->json(['message' => 'Username atau password salah'], 401);
        }

        // token dipakai aplikasi MAUI untuk otentikasi request selanjutnya
        $token = $user->createToken('mobile-app')->plainTextToken;

        return response()->json([
            'token' => $token,
            'id' => $user->id,
            'username' => $user->username,
            'name' => $user->name,
            'role' => $user->role,             // 'admin' atau 'user' -> dipakai MAUI untuk routing dashboard
            'pegawai_id' => $user->pegawai_id, // null kalau akun admin
        ]);
    }

    // POST /api/logout  (butuh header Authorization: Bearer <token>)
    public function logout(Request $request)
    {
        $request->user()->currentAccessToken()->delete();

        return response()->json(['message' => 'Logout berhasil']);
    }

    // GET /api/me
    public function me(Request $request)
    {
        $user = $request->user();

        return response()->json([
            'id' => $user->id,
            'username' => $user->username,
            'name' => $user->name,
            'role' => $user->role,
            'pegawai_id' => $user->pegawai_id,
        ]);
    }

    // PATCH /api/me/password  (butuh header Authorization: Bearer <token>)
public function updatePassword(Request $request)
{
    $request->validate([
        'password_lama' => 'required|string',
        'password_baru' => 'required|string|min:6|confirmed',
    ]);

    $user = $request->user();

    if (! Hash::check($request->password_lama, $user->password)) {
        return response()->json(['message' => 'Password lama tidak sesuai'], 422);
    }

    $user->password = Hash::make($request->password_baru);
    $user->save();

    return response()->json(['message' => 'Password berhasil diubah']);
}

// PATCH /api/me  (ganti nama & username sendiri)
public function updateProfil(Request $request)
{
    $request->validate([
        'name' => 'required|string|max:255',
        'username' => 'required|string|max:255|unique:users,username,' . $request->user()->id,
    ]);

    $user = $request->user();
    $user->name = $request->name;
    $user->username = $request->username;
    $user->save();

    return response()->json(['message' => 'Profil berhasil diperbarui', 'name' => $user->name, 'username' => $user->username]);
}
}
