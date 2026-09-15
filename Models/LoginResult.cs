using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;

public class LoginResult
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty; // "admin" atau "user" dari Laravel

    [JsonPropertyName("pegawai_id")]
    public int? PegawaiId { get; set; } // null kalau akun admin

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
