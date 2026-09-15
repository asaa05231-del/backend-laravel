using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;

public class SlipGaji
{
    public int Id { get; set; }

    [JsonPropertyName("pegawai_id")]
    public int PegawaiId { get; set; }

    [JsonPropertyName("periode_gaji_id")]
    public int PeriodeGajiId { get; set; }

    [JsonPropertyName("hari_sbg_supir")]
    public int HariSbgSupir { get; set; }

    [JsonPropertyName("hari_sbg_kernet")]
    public int HariSbgKernet { get; set; }

    [JsonPropertyName("upah")]
    public decimal Upah { get; set; }

    [JsonPropertyName("insentif_supir")]
    public decimal InsentifSupir { get; set; }

    [JsonPropertyName("insentif_kernet")]
    public decimal InsentifKernet { get; set; }

    [JsonPropertyName("tunjangan_makan")]
    public decimal TunjanganMakan { get; set; }

    [JsonPropertyName("subsidi_transport")]
    public decimal SubsidiTransport { get; set; }

    [JsonPropertyName("insentif_kir")]
    public decimal InsentifKir { get; set; }

    [JsonPropertyName("insentif_muatan_berat")]
    public decimal InsentifMuatanBerat { get; set; }

    [JsonPropertyName("premi")]
    public decimal Premi { get; set; }

    [JsonPropertyName("total_upah")]
    public decimal TotalUpah { get; set; }

    [JsonPropertyName("potongan_jamsostek")]
    public decimal PotonganJamsostek { get; set; }

    [JsonPropertyName("potongan_cat")]
    public decimal PotonganCat { get; set; }

    [JsonPropertyName("potongan_pengobatan")]
    public decimal PotonganPengobatan { get; set; }

    [JsonPropertyName("potongan_spsi")]
    public decimal PotonganSpsi { get; set; }

    [JsonPropertyName("total_potongan")]
    public decimal TotalPotongan { get; set; }

    [JsonPropertyName("dibayar")]
    public decimal Dibayar { get; set; }

    [JsonPropertyName("pegawai")]
    public Pegawai? Pegawai { get; set; }

    [JsonPropertyName("periode")]
   public PeriodeGaji? Periode { get; set; }

    [JsonPropertyName("hadir")]
    public int Hadir { get; set; }

    [JsonPropertyName("hari_kerja")]
    public int HariKerja { get; set; }
}