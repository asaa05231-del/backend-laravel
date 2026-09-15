
using System.Text.Json.Serialization;

namespace SistemPengirimanApp.Models;

public class PeriodeGaji
{
    public int Id { get; set; }

    [JsonPropertyName("tanggal_mulai")]
    public DateTime TanggalMulai { get; set; }

    [JsonPropertyName("tanggal_selesai")]
    public DateTime TanggalSelesai { get; set; }
}