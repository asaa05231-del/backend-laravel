namespace SistemPengirimanApp.Services;

public enum RolePengguna
{
    Admin,
    User
}

public static class SesiPengguna
{
    public static string Username { get; set; } = string.Empty;
    public static string Nama { get; set; } = string.Empty;
    public static RolePengguna Role { get; set; } = RolePengguna.User;
    public static string Token { get; set; } = string.Empty;
    public static int? PegawaiId { get; set; }

    public static bool IsAdmin => Role == RolePengguna.Admin;

    public static void Logout()
    {
        Username = string.Empty;
        Nama = string.Empty;
        Role = RolePengguna.User;
        Token = string.Empty;
        PegawaiId = null;
    }
}