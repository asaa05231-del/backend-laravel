using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;

public class HasilHalaman<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("current_page")]
    public int HalamanSaatIni { get; set; }

    [JsonPropertyName("last_page")]
    public int HalamanTerakhir { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}