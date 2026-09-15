using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;
public class Pengiriman
{
    public int Id { get; set; }
    [JsonPropertyName("tanggal")]
    public DateTime Tanggal { get; set; }
    [JsonPropertyName("muatan_berat")]
    public bool MuatanBerat { get; set; }
    [JsonPropertyName("kir")]
    public bool Kir { get; set; }
    [JsonPropertyName("total_biaya")]
    public decimal TotalBiaya { get; set; }
    [JsonPropertyName("supir")]
    public Pegawai? Sopir { get; set; }
    [JsonPropertyName("kernet")]
    public Pegawai? Kernet { get; set; }

    [JsonPropertyName("wilayah")]
    public Wilayah? Wilayah { get; set; }

    [JsonPropertyName("bengkel")]
    public bool Bengkel { get; set; }

    public string JudulTampilan =>
    Kir ? "KIR" :
    Bengkel ? "Bengkel" :
    (Wilayah?.NamaWilayah ?? "");

    [JsonPropertyName("supir_manual")]
    public string? SupirManual { get; set; }

    [JsonPropertyName("kernet_manual")]
    public string? KernetManual { get; set; }

    // computed, bukan dari API — fallback tampilan
    public string NamaSupirTampilan => Sopir?.Nama ?? SupirManual ?? "";
    public string NamaKernetTampilan => Kernet?.Nama ?? KernetManual ?? "";
}