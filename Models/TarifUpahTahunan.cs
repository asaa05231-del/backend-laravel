using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;

public class TarifUpahTahunan
{
    public int Id { get; set; }

    [JsonPropertyName("tahun")]
    public int Tahun { get; set; }

    [JsonPropertyName("upah_harian_supir")]
    public decimal UpahHariansupir { get; set; }

    [JsonPropertyName("upah_harian_kernet")]
    public decimal UpahHarianKernet { get; set; }

    [JsonPropertyName("upah_mingguan_supir")]
    public decimal UpahMingguansupir { get; set; }

    [JsonPropertyName("upah_mingguan_kernet")]
    public decimal UpahMingguanKernet { get; set; }

    [JsonPropertyName("tunjangan_makan_harian")]
    public decimal TunjanganMakanHarian { get; set; }

    [JsonPropertyName("subsidi_transport_harian")]
    public decimal SubsidiTransportHarian { get; set; }

}