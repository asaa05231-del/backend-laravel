using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;
public class PengirimanRequest
{
    [JsonPropertyName("tanggal")]
    public DateTime Tanggal { get; set; }
    [JsonPropertyName("supir_id")]
    public int? SopirId { get; set; }
    [JsonPropertyName("supir_manual")]
    public string? SupirManual { get; set; }
    [JsonPropertyName("kernet_id")]
    public int? KernetId { get; set; }
    [JsonPropertyName("kernet_manual")]
    public string? KernetManual { get; set; }
    [JsonPropertyName("wilayah_id")]
    public int? WilayahId { get; set; }
    [JsonPropertyName("muatan_berat")]
    public bool MuatanBerat { get; set; }
    [JsonPropertyName("kir")]
    public bool Kir { get; set; }
    [JsonPropertyName("total_biaya")]
    public decimal TotalBiaya { get; set; }
    [JsonPropertyName("bengkel")]
    public bool Bengkel { get; set; }
}