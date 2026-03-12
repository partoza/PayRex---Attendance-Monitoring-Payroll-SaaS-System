using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PayRex.Web.QuestPdf
{
    public class FinanceExportRow
    {
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
    }

    public class FinancePdfGeneratorOptions
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyTagline { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }

        public string Period { get; set; } = "All Time";
        public decimal TotalIncome { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalVat { get; set; }
        public decimal NetProfit { get; set; }
        
        public string ForecastText { get; set; } = string.Empty;
        public string SummaryConclusion { get; set; } = string.Empty;

        public List<FinanceExportRow> Entries { get; set; } = new();
        
        public string IssuerName { get; set; } = string.Empty;
        public string IssuerPosition { get; set; } = string.Empty;
        public string? IssuerSignatureUrl { get; set; }
        public byte[]? LogoBytes { get; set; }
    }

    public class FinancePdfGenerator
    {
        private readonly QuestPDF.Infrastructure.Color PrimaryColor = QuestPDF.Infrastructure.Color.FromHex("#1E3A8A");
        private readonly QuestPDF.Infrastructure.Color SecondaryColor = QuestPDF.Infrastructure.Color.FromHex("#2563EB");
        private readonly QuestPDF.Infrastructure.Color TextDark = QuestPDF.Infrastructure.Color.FromHex("#1F2937");
        private readonly QuestPDF.Infrastructure.Color TextGray = QuestPDF.Infrastructure.Color.FromHex("#6B7280");
        private readonly QuestPDF.Infrastructure.Color IncomeColor = QuestPDF.Infrastructure.Color.FromHex("#15803D");
        private readonly QuestPDF.Infrastructure.Color CostColor = QuestPDF.Infrastructure.Color.FromHex("#B91C1C");

        public byte[] Generate(FinancePdfGeneratorOptions opts)
        {
            var now = DateTime.Now;
            byte[]? sigBytes = FetchImage(opts.IssuerSignatureUrl);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(28);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(TextDark).FontFamily("Helvetica"));

                    page.Header().Element(c => ComposeHeader(c, opts, now));
                    page.Content().PaddingTop(10).Element(c => ComposeContent(c, opts, sigBytes));
                    page.Footer().Element(c => ComposeFooter(c, opts, now));
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, FinancePdfGeneratorOptions opts, DateTime now)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(90).AlignLeft().AlignTop().Element(cont =>
                    {
                        if (opts.LogoBytes != null)
                            cont.Height(80).Width(80).Image(opts.LogoBytes);
                        else
                            cont.Border(1).BorderColor(PrimaryColor).Width(80).Height(80).AlignCenter().AlignMiddle()
                                .Text("LOGO").FontSize(12).Bold().FontColor(PrimaryColor);
                    });

                    row.RelativeItem().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text(opts.CompanyName).FontSize(18).Bold().FontColor(PrimaryColor);
                        c.Item().PaddingTop(2).Text($"Address: {opts.CompanyAddress ?? "N/A"}").FontSize(8).FontColor(TextGray);
                        c.Item().Text($"Contact: {opts.CompanyPhone ?? "N/A"} | Email: {opts.CompanyEmail ?? "N/A"}").FontSize(8).FontColor(TextGray);
                        if (!string.IsNullOrWhiteSpace(opts.CompanyTagline))
                            c.Item().PaddingTop(2).Text(opts.CompanyTagline).FontSize(8).Italic().FontColor(TextGray);
                    });

                    row.ConstantItem(200).Column(c =>
                    {
                        c.Item().Text("FINANCIAL REPORT").FontSize(14).Bold().FontColor(TextDark);
                        c.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("Period:").FontSize(8).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text(opts.Period).FontSize(8).Bold().FontColor(PrimaryColor);
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Generated:").FontSize(8).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text(now.ToString("MM/dd/yyyy")).FontSize(8);
                        });
                    });
                });

                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, FinancePdfGeneratorOptions opts, byte[]? sigBytes)
        {
            container.Column(c =>
            {
                // KPI Section
                c.Item().Row(row =>
                {
                    row.RelativeItem().Element(cont => ComposeKpi(cont, "Total Income", opts.TotalIncome, IncomeColor));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(cont => ComposeKpi(cont, "Total Cost", opts.TotalCost, CostColor));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(cont => ComposeKpi(cont, "Net Profit", opts.NetProfit, opts.NetProfit >= 0 ? SecondaryColor : CostColor));
                });

                // Analytics Section
                c.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Column(coll =>
                    {
                        coll.Item().Text("REVENUE FORECAST").FontSize(9).Bold().FontColor(PrimaryColor);
                        coll.Item().PaddingTop(4).PaddingBottom(4).LineHorizontal(0.5f).LineColor(PrimaryColor);
                        coll.Item().Text(opts.ForecastText).FontSize(8.5f).LineHeight(1.4f);
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(coll =>
                    {
                        coll.Item().Text("SUMMARY CONCLUSION").FontSize(9).Bold().FontColor(PrimaryColor);
                        coll.Item().PaddingTop(4).PaddingBottom(4).LineHorizontal(0.5f).LineColor(PrimaryColor);
                        coll.Item().Text(opts.SummaryConclusion).FontSize(8.5f).LineHeight(1.4f);
                    });
                });

                // Table Section
                c.Item().PaddingTop(20).Column(col =>
                {
                    col.Item().Text("TRANSACTION DETAILS").FontSize(10).Bold().FontColor(TextDark);
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(1.5f); // Type
                            columns.RelativeColumn(4); // Description
                            columns.RelativeColumn(2); // Category
                            columns.RelativeColumn(2); // Amount
                        });

                        table.Header(header =>
                        {
                            void H(string text) => header.Cell().Background(PrimaryColor).Padding(5).Text(text).FontColor(Colors.White).Bold().FontSize(8);
                            H("Date"); H("Type"); H("Description"); H("Category"); header.Cell().Background(PrimaryColor).Padding(5).AlignRight().Text("Amount").FontColor(Colors.White).Bold().FontSize(8);
                        });

                        foreach (var entry in opts.Entries)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(entry.Date).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(entry.Type).FontSize(7.5f).FontColor(entry.Type == "Income" ? IncomeColor : CostColor);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(entry.Description).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(entry.Category).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"₱{entry.Amount:N2}").FontSize(7.5f).Bold();
                        }
                    });
                });

                // Signatures
                c.Item().PaddingTop(30).Element(cont => ComposeNotesSection(cont, opts.IssuerName, opts.IssuerPosition, sigBytes));
            });
        }

        private void ComposeNotesSection(IContainer container, string issuerName, string issuerPosition, byte[]? sigBytes)
        {
            container.Column(c =>
            {
                c.Item().PaddingTop(12).Column(sig =>
                {
                    sig.Item().Row(r =>
                    {
                        r.ConstantItem(160).AlignLeft().Element(cn =>
                        {
                            if (sigBytes != null)
                            {
                                cn.Height(60).Width(160).Image(sigBytes);
                            }
                            else
                            {
                                cn.Width(160).Height(60).AlignLeft().AlignBottom()
                                    .Text("Signature").FontSize(9).FontColor(TextGray);
                            }
                        });

                        r.RelativeItem();
                    });

                    var issuedBy = string.IsNullOrWhiteSpace(issuerName) ? "" : issuerName;
                    sig.Item().PaddingTop(4).Text($"Issued by: {issuedBy}").FontSize(10).SemiBold().FontColor(TextDark);
                    sig.Item().Text(issuerPosition).FontSize(9).FontColor(TextGray);
                });
            });
        }

        private void ComposeKpi(IContainer container, string label, decimal value, QuestPDF.Infrastructure.Color color)
        {
            container.Background(Colors.Grey.Lighten5).Border(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(10).Column(c =>
            {
                c.Item().Text(label.ToUpper()).FontSize(7).Bold().FontColor(TextGray);
                c.Item().PaddingTop(2).Text($"₱{value:N2}").FontSize(14).Bold().FontColor(color);
            });
        }

        private void ComposeFooter(IContainer container, FinancePdfGeneratorOptions opts, DateTime now)
        {
            container.Column(c =>
            {
                c.Item().Height(2).Background(Colors.Grey.Lighten2);

                c.Item().PaddingTop(6).Row(r =>
                {
                    r.RelativeItem().Column(col =>
                    {
                        col.Item().Text(opts.CompanyName).FontSize(9).Bold().FontColor(PrimaryColor);
                        col.Item().Text($"Generated: {now:MM/dd/yyyy hh:mm tt}").FontSize(8).FontColor(TextGray);
                        col.Item().Text("Document is confidential and intended for internal use only.")
                            .FontSize(8).FontColor(TextGray);
                    });

                    r.RelativeItem().AlignRight().AlignMiddle().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(TextGray);
                        text.CurrentPageNumber().FontSize(8).Bold().FontColor(PrimaryColor);
                        text.Span(" of ").FontSize(8).FontColor(TextGray);
                        text.TotalPages().FontSize(8).Bold().FontColor(PrimaryColor);
                    });
                });
            });
        }

        private static byte[]? FetchImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }
    }
}
