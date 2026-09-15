using Microsoft.Maui.Controls;

namespace SistemPengirimanApp.Views;

public class RiwayatItemNama
{
    public string Nama { get; set; } = "";
}

public class RiwayatItemKolomHeader
{
}

public class RiwayatItemTotal
{
    public decimal TotalSupir { get; set; }
    public decimal TotalKernet { get; set; }
    public decimal GrandTotal => TotalSupir + TotalKernet;

    public string TotalSupirText => $"{TotalSupir:N0}";
    public string TotalKernetText => $"{TotalKernet:N0}";
    public string GrandTotalText => $"{GrandTotal:N0}";
}

public class RiwayatTripHarian
{
    public int Id { get; set; }
    public string Wilayah { get; set; } = "";
    public bool IsBengkel { get; set; }
    public bool IsKir { get; set; }
    public decimal? NominalSupir { get; set; }
    public decimal? NominalKernet { get; set; }
    public string? PasanganSopir { get; set; }
    public string? PasanganKernet { get; set; }

    public string NominalSupirText => IsVirtual ? "" : (NominalSupir.HasValue ? $"{NominalSupir:N0}" : (Singkat(PasanganSopir) ?? "-"));
    public string NominalKernetText => IsVirtual ? "" : (NominalKernet.HasValue ? $"{NominalKernet:N0}" : (Singkat(PasanganKernet) ?? "-"));
    public bool BukanTripPertama { get; set; }
    public bool IsVirtual { get; set; }

    public int Nomor { get; set; }
    public string TanggalTextRaw { get; set; } = "";

    public string NomorText => BukanTripPertama ? "" : Nomor.ToString();
    public string TanggalDisplayText => BukanTripPertama ? "" : TanggalTextRaw;

    public string BengkelText => IsBengkel ? "Ya" : "";
    public string KirText => IsKir ? "Ya" : "";

    private static string? Singkat(string? nama)
    {
        if (string.IsNullOrWhiteSpace(nama)) return nama;

        var bagian = nama.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (bagian.Length <= 1) return nama;

        return $"{bagian[0]} {bagian[1][0]}.";
    }
}

public class RiwayatBaris
{
    public int Nomor { get; set; }
    public DateTime Tanggal { get; set; }
    public List<RiwayatTripHarian> Trips { get; set; } = new();

    public string TanggalText => Tanggal.ToString("dd/MM/yyyy");
}

public class RiwayatTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TemplateNama { get; set; }
    public DataTemplate? TemplateKolomHeader { get; set; }
    public DataTemplate? TemplateBaris { get; set; }
    public DataTemplate? TemplateTotal { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item switch
        {
            RiwayatItemNama => TemplateNama!,
            RiwayatItemKolomHeader => TemplateKolomHeader!,
            RiwayatItemTotal => TemplateTotal!,
            RiwayatBaris => TemplateBaris!,
            _ => TemplateBaris!
        };
    }
}