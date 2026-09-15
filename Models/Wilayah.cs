using System.Text.Json.Serialization;
namespace SistemPengirimanApp.Models;
public class Wilayah
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nama_wilayah")]
    public string NamaWilayah { get; set; } = "";
    [JsonPropertyName("tarif_supir")]
    public decimal Tarifsopir { get; set; }
    [JsonPropertyName("tarif_kernet")]
    public decimal TarifKernet { get; set; }

    public override string ToString() => NamaWilayah;
}