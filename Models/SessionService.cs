namespace SistemPengirimanApp.Services;

public static class SessionService
{
    public static bool IsLogin { get; set; }

    public static string Username { get; set; } = "";

    public static int UserId { get; set; }
}