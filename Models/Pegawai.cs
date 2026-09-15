using System.Text.Json.Serialization;
namespace SistemPengirimanApp.Models;
public class Pegawai
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nama")]
    public string Nama { get; set; } = string.Empty;

    [JsonPropertyName("jabatan")]
    public string Jabatan { get; set; } = "sopir"; // "sopir" atau "kernet"
    [JsonPropertyName("tanggal_masuk")]
    public DateTime? TanggalMasuk { get; set; }
    [JsonPropertyName("upah_harian")]
    public decimal UpahHarian { get; set; }
    [JsonPropertyName("upah_mingguan")]
    public decimal UpahMingguan { get; set; }
    [JsonPropertyName("tunjangan_transport")]
    public decimal TunjanganTransport { get; set; }
    [JsonPropertyName("persen_jamsostek")]
    public decimal PersenJamsostek { get; set; } = 4;
    [JsonPropertyName("ikut_jamsostek")]
    public bool IkutJamsostek { get; set; } = true;

    public override string ToString() => Nama;
}