using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;

public class PengaturanGaji
{
    [JsonPropertyName("tunjangan_makan_harian")]
    public decimal TunjanganMakanHarian { get; set; }

    [JsonPropertyName("subsidi_transport_harian")]
    public decimal SubsidiTransportHarian { get; set; }

    [JsonPropertyName("potongan_kesehatan_supir")]
    public decimal PotonganKesehatanSupir { get; set; }

    [JsonPropertyName("potongan_kesehatan_kernet")]
    public decimal PotonganKesehatanKernet { get; set; }

    [JsonPropertyName("jamsostek_supir")]
    public decimal JamsostekSupir { get; set; }

    [JsonPropertyName("jamsostek_kernet")]
    public decimal JamsostekKernet { get; set; }
}