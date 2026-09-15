using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemPengirimanApp.Models;
using Colors = QuestPDF.Helpers.Colors;

namespace SistemPengirimanApp.Services;

public static class SlipGajiPdfService
{
    static SlipGajiPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static string FormatRupiah(decimal nilai) => $"Rp {nilai:N0}".Replace(",", ".");

    private static string FormatAngka(decimal nilai) => $"{nilai:N0}".Replace(",", ".");

    public static string BuatPdf(SlipGaji slip)
    {
        string namaPegawai = slip.Pegawai?.Nama ?? "Pegawai";
        string jabatan = slip.Pegawai?.Jabatan switch
        {
            "sopir" => "sopir",
            "kernet" => "Kernet",
            _ => "-"
        };
        string periodeText = slip.Periode != null
            ? $"{slip.Periode.TanggalMulai:dd MMM yyyy} - {slip.Periode.TanggalSelesai:dd MMM yyyy}"
            : "-";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A5);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Slip Gaji").FontSize(18).Bold();
                    col.Item().Text("PT Bina Adidaya").FontSize(11).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Text(namaPegawai).FontSize(15).Bold();
                    col.Item().Text(jabatan).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Periode: {periodeText}").FontSize(10).FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(12).Text("Pendapatan").Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(165); // Label
                            columns.ConstantColumn(92);  // Keterangan (abu-abu)
                            columns.ConstantColumn(28);  // "=Rp."
                            columns.ConstantColumn(87);  // Angka
                        });

                        BarisNominalDetail(table, "Upah",
                             slip.HariKerja > 0 ? $"{slip.HariKerja} x {FormatRupiah(slip.HariKerja > 0 ? slip.Upah / slip.HariKerja : 0)}" : "",
                             slip.Upah);

                        BarisNominalDetail(table, "Insentif sopir",
                            slip.HariSbgSupir > 0 ? $"{slip.HariSbgSupir}" : "",
                            slip.InsentifSupir);

                        BarisNominalDetail(table, "Insentif Kernet",
                            slip.HariSbgKernet > 0 ? $"{slip.HariSbgKernet}" : "",
                            slip.InsentifKernet);

                        BarisNominalDetail(table, "Tunjangan Makan",
                            slip.Hadir > 0 ? $"{slip.Hadir} x {FormatRupiah(slip.Hadir > 0 ? slip.TunjanganMakan / slip.Hadir : 0)}" : "",
                            slip.TunjanganMakan);

                        BarisNominalDetail(table, "Subsidi Transport",
                            slip.Hadir > 0 ? $"{slip.Hadir} x {FormatRupiah(slip.Hadir > 0 ? slip.SubsidiTransport / slip.Hadir : 0)}" : "",
                            slip.SubsidiTransport);

                        BarisNominal(table, "Insentif KIR", slip.InsentifKir);
                        BarisNominal(table, "Insentif Muatan Berat", slip.InsentifMuatanBerat);

                        BarisNominal(table, "Total Upah", slip.TotalUpah, tebal: true);
                    });

                    col.Item().PaddingTop(12).Text("Potongan").Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(165);
                            columns.ConstantColumn(92);
                            columns.ConstantColumn(28);
                            columns.ConstantColumn(87);
                        });

                        BarisNominal(table, "Jamsostek", slip.PotonganJamsostek);
                        BarisNominal(table, "Cat", slip.PotonganCat);
                        BarisNominal(table, "Pengobatan", slip.PotonganPengobatan);
                        BarisNominal(table, "SPSI", slip.PotonganSpsi);

                        BarisNominal(table, "Total Potongan", slip.TotalPotongan, tebal: true);
                    });

                    col.Item().PaddingTop(16).Background(Colors.Blue.Darken2).Padding(12).Row(row =>
                    {
                        row.RelativeItem().Text("DIBAYAR").FontColor(Colors.White).Bold().FontSize(14);
                        row.AutoItem().Text(FormatAngka(slip.Dibayar)).FontColor(Colors.Orange.Medium).Bold().FontSize(16);
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Dicetak: ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        string namaFile = $"SlipGaji_{namaPegawai.Replace(" ", "")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        string path = Path.Combine(FileSystem.CacheDirectory, namaFile);

        document.GeneratePdf(path);

        return path;
    }

    private static void BarisNominal(QuestPDF.Fluent.TableDescriptor table, string label, decimal nominal, bool tebal = false)
    {
        BarisNominalDetail(table, label, "", nominal, tebal);
    }

    private static void BarisNominalDetail(QuestPDF.Fluent.TableDescriptor table, string label, string keterangan, decimal nominal, bool tebal = false)
    {
        var warnaLabel = tebal ? Colors.Black : Colors.Grey.Darken2;

        if (tebal)
            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(label).FontSize(11).Bold();
        else
            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(label).FontSize(11).FontColor(warnaLabel);

        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).AlignLeft()
            .Text(keterangan ?? "").FontSize(11).FontColor(warnaLabel);

        if (tebal)
        {
            // Baris Total: tanpa "=Rp.", angka polos
            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).AlignRight().Text(FormatAngka(nominal)).FontSize(11).Bold();
        }
        else
        {
            if (!string.IsNullOrEmpty(keterangan))
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).AlignLeft().Text("=Rp.").FontSize(11).FontColor(warnaLabel);
            else
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).AlignRight().Text(FormatAngka(nominal)).FontSize(11).FontColor(warnaLabel);
        }
    }
}