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
    public class PayslipReportRow
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string BasicPay { get; set; } = string.Empty;
        public string GrossPay { get; set; } = string.Empty;
        public string TotalDeductions { get; set; } = string.Empty;
        public string NetPay { get; set; } = string.Empty;
        public string Status { get; set; } = "Paid";
    }

    public class PayslipReportOptions
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyAddress { get; set; }
        public string PeriodFilter { get; set; } = "All Periods";
        public string IssuerName { get; set; } = string.Empty;
        public string IssuerPosition { get; set; } = string.Empty;
        public string? IssuerSignatureUrl { get; set; }
        public IEnumerable<PayslipReportRow> Rows { get; set; } = Array.Empty<PayslipReportRow>();
        public byte[]? LogoBytes { get; set; }
    }

    public class PayslipReportPdfGenerator
    {
        private readonly QuestPDF.Infrastructure.Color PrimaryColor = QuestPDF.Infrastructure.Color.FromHex("#1E3A8A");
        private readonly QuestPDF.Infrastructure.Color SecondaryColor = QuestPDF.Infrastructure.Color.FromHex("#2563EB");
        private readonly QuestPDF.Infrastructure.Color TextDark    = QuestPDF.Infrastructure.Color.FromHex("#1F2937");
        private readonly QuestPDF.Infrastructure.Color TextGray    = QuestPDF.Infrastructure.Color.FromHex("#6B7280");

        public byte[] Generate(PayslipReportOptions opts)
        {
            var rows = (opts.Rows ?? Array.Empty<PayslipReportRow>()).ToList();
            var now  = DateTime.Now;
            byte[]? sigBytes  = FetchImage(opts.IssuerSignatureUrl);
            byte[]? logoBytes = opts.LogoBytes;

            var totalNet  = rows.Sum(r => decimal.TryParse(r.NetPay.Replace("₱","").Replace(",",""), out var v) ? v : 0);
            var totalGross = rows.Sum(r => decimal.TryParse(r.GrossPay.Replace("₱","").Replace(",",""), out var v) ? v : 0);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(TextDark).FontFamily("Helvetica"));

                    page.Header().Element(c => ComposeHeader(c, opts, now, rows.Count, logoBytes));
                    page.Content().PaddingHorizontal(0).PaddingTop(8).Element(c => ComposeContent(c, opts, rows, sigBytes, totalGross, totalNet));
                    page.Footer().Element(c => ComposeFooter(c, opts, now));
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, PayslipReportOptions opts, DateTime now, int totalRows, byte[]? logoBytes)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(90).AlignLeft().AlignTop().Element(cont =>
                    {
                        if (logoBytes != null)
                        {
                            cont.Height(80).Width(80).Image(logoBytes);
                        }
                        else
                        {
                            cont.Border(1).BorderColor(PrimaryColor).Width(80).Height(80).AlignCenter().AlignMiddle()
                                .Text("LOGO").FontSize(12).Bold().FontColor(PrimaryColor);
                        }
                    });

                    row.RelativeItem().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text(opts.CompanyName).FontSize(18).Bold().FontColor(PrimaryColor);
                        if (!string.IsNullOrEmpty(opts.CompanyAddress))
                            c.Item().PaddingTop(4).Text($"Address: {opts.CompanyAddress}").FontSize(9).FontColor(TextGray);
                    });

                    row.ConstantItem(260).Column(c =>
                    {
                        c.Item().Text("PAYSLIPS REPORT").FontSize(14).Bold().FontColor(TextDark);

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Report Period:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text(opts.PeriodFilter).FontSize(9).Bold().FontColor(PrimaryColor);
                        });

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Generated Date:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text(now.ToString("MM/dd/yyyy")).FontSize(9);
                        });

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Records:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text($"{totalRows:N0}").FontSize(9).Bold().FontColor(PrimaryColor);
                        });
                    });
                });

                col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, PayslipReportOptions opts, List<PayslipReportRow> rows, byte[]? sigBytes, decimal totalGross, decimal totalNet)
        {
            container.Column(c =>
            {
                c.Item().PaddingTop(10).Element(cont => ComposeTable(cont, rows));
                c.Item().PaddingTop(6).Element(cont => ComposeTotals(cont, totalGross, totalNet));
                c.Item().PaddingTop(14).Element(cont => ComposeSignature(cont, opts.IssuerName, opts.IssuerPosition, sigBytes));
            });
        }

        private void ComposeTable(IContainer container, List<PayslipReportRow> rows)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3.5f); // Employee
                    cols.RelativeColumn(2.5f); // Period
                    cols.RelativeColumn(1.8f); // Basic Pay
                    cols.RelativeColumn(1.8f); // Gross Pay
                    cols.RelativeColumn(1.8f); // Deductions
                    cols.RelativeColumn(1.8f); // Net Pay
                    cols.RelativeColumn(1.2f); // Status
                });

                table.Header(header =>
                {
                    void H(string text, bool alignRight = false)
                    {
                        var cell = header.Cell().Background(PrimaryColor)
                            .PaddingLeft(5).PaddingRight(5).PaddingTop(7).PaddingBottom(7);
                        if (alignRight)
                            cell.AlignRight().Text(text).FontColor(Colors.White).SemiBold().FontSize(8);
                        else
                            cell.AlignLeft().Text(text).FontColor(Colors.White).SemiBold().FontSize(8);
                    }
                    H("Employee"); H("Period"); H("Basic Pay", true); H("Gross Pay", true);
                    H("Deductions", true); H("Net Pay", true); H("Status");
                });

                if (!rows.Any())
                {
                    table.Cell().ColumnSpan(7).PaddingTop(16).PaddingBottom(16)
                        .AlignCenter().Text("No payslip records found.").FontColor(TextGray).FontSize(9);
                    return;
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    var r   = rows[i];
                    var bg  = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    void D(string text, bool alignRight = false)
                    {
                        var cell = table.Cell().Background(bg)
                            .PaddingLeft(5).PaddingRight(5).PaddingTop(5).PaddingBottom(5);
                        if (alignRight)
                            cell.AlignRight().Text(text ?? "-").FontSize(8);
                        else
                            cell.AlignLeft().Text(text ?? "-").FontSize(8);
                    }

                    D(r.EmployeeName); D(r.Period);
                    D(r.BasicPay, true); D(r.GrossPay, true);
                    D(r.TotalDeductions, true); D(r.NetPay, true);

                    // Status badge — green "Paid"
                    table.Cell().Background(bg)
                        .PaddingLeft(5).PaddingRight(5).PaddingTop(4).PaddingBottom(4)
                        .AlignLeft().Text(r.Status ?? "Paid")
                        .FontSize(7.5f).SemiBold()
                        .FontColor(QuestPDF.Infrastructure.Color.FromHex("#15803D"));
                }
            });
        }

        private void ComposeTotals(IContainer container, decimal totalGross, decimal totalNet)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem();
                    row.ConstantItem(340).Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Total Gross Pay:").FontSize(9).SemiBold().FontColor(TextGray);
                        r.ConstantItem(100).AlignRight().Text($"₱{totalGross:N2}").FontSize(9).Bold().FontColor(TextDark);
                    });
                    row.ConstantItem(20);
                    row.ConstantItem(200).Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Total Net Pay:").FontSize(10).SemiBold().FontColor(TextDark);
                        r.ConstantItem(110).AlignRight().Text($"₱{totalNet:N2}").FontSize(10).Bold().FontColor(PrimaryColor);
                    });
                });
            });
        }

        private void ComposeSignature(IContainer container, string issuerName, string issuerPosition, byte[]? sigBytes)
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

        private void ComposeFooter(IContainer container, PayslipReportOptions opts, DateTime now)
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
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            catch { return null; }
        }
    }
}
