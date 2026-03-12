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
    public class CompensationExportRow
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DateOrFrequency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CompensationPdfGeneratorOptions
    {
        public string Title { get; set; } = "Compensation Report";
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyTagline { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }
        public string IssuerName { get; set; } = string.Empty;
        public string IssuerPosition { get; set; } = string.Empty;
        public string? IssuerSignatureUrl { get; set; }
        public string FilterDescription { get; set; } = string.Empty;
        public List<CompensationExportRow> Rows { get; set; } = new();
        public byte[]? LogoBytes { get; set; }
        public bool IsDeduction { get; set; }
    }

    public class CompensationPdfGenerator
    {
        private readonly QuestPDF.Infrastructure.Color PrimaryColor = QuestPDF.Infrastructure.Color.FromHex("#1E3A8A");
        private readonly QuestPDF.Infrastructure.Color SecondaryColor = QuestPDF.Infrastructure.Color.FromHex("#2563EB");
        private readonly QuestPDF.Infrastructure.Color TextDark = QuestPDF.Infrastructure.Color.FromHex("#1F2937");
        private readonly QuestPDF.Infrastructure.Color TextGray = QuestPDF.Infrastructure.Color.FromHex("#6B7280");

        public byte[] Generate(CompensationPdfGeneratorOptions opts)
        {
            var now = DateTime.Now;
            byte[]? logoBytes = opts.LogoBytes ?? FetchImage(opts.CompanyLogoUrl);
            byte[]? sigBytes = FetchImage(opts.IssuerSignatureUrl);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(28);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(TextDark).FontFamily("Helvetica"));

                    page.Header().Element(c => ComposeHeader(c, now, opts, logoBytes));
                    page.Content().PaddingHorizontal(0).PaddingTop(8).Element(c => ComposeContent(c, opts, sigBytes));
                    page.Footer().Element(c => ComposeFooter(c, opts, now));
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, DateTime now, CompensationPdfGeneratorOptions opts, byte[]? logoBytes)
        {
            var addressText = string.IsNullOrWhiteSpace(opts.CompanyAddress) ? "N/A" : opts.CompanyAddress;
            var contactText = string.IsNullOrWhiteSpace(opts.CompanyPhone) ? "N/A" : opts.CompanyPhone;

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
                        c.Item().Text(string.IsNullOrWhiteSpace(opts.CompanyName) ? "PayRex" : opts.CompanyName).FontSize(18).Bold().FontColor(PrimaryColor);
                        c.Item().PaddingTop(4).Text($"Address: {addressText}").FontSize(9).FontColor(TextGray);
                        c.Item().Text($"Contact Number: {contactText}").FontSize(9).FontColor(TextGray);

                        if (!string.IsNullOrWhiteSpace(opts.CompanyEmail))
                        {
                            c.Item().Text($"Email: {opts.CompanyEmail}").FontSize(9).FontColor(TextGray);
                        }

                        if (!string.IsNullOrWhiteSpace(opts.CompanyTagline))
                        {
                            c.Item().PaddingTop(2).Text(opts.CompanyTagline).FontSize(9).Italic().FontColor(TextGray);
                        }
                    });

                    row.ConstantItem(260).Column(c =>
                    {
                        c.Item().Text(opts.Title.ToUpper()).FontSize(14).Bold().FontColor(TextDark);

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Report Period:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text("All Time").FontSize(9).Bold().FontColor(PrimaryColor);
                        });

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Generated Date:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text(now.ToString("MM/dd/yyyy")).FontSize(9);
                        });

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Records:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text($"{opts.Rows.Count:N0}").FontSize(9).Bold().FontColor(PrimaryColor);
                        });
                        
                        var totalAmount = opts.Rows.Sum(r => r.Amount);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Amount:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text($"PHP {totalAmount:N2}").FontSize(9).Bold().FontColor(opts.IsDeduction ? Colors.Red.Medium : Colors.Green.Medium);
                        });
                    });
                });

                col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, CompensationPdfGeneratorOptions opts, byte[]? sigBytes)
        {
            container.Column(c =>
            {
                c.Item().PaddingTop(12).Element(cont => ComposeTable(cont, opts));
                c.Item().PaddingTop(10).Element(cont => ComposeNotesSection(cont, opts.IssuerName, opts.IssuerPosition, sigBytes));
            });
        }

        private void ComposeTable(IContainer container, CompensationPdfGeneratorOptions opts)
        {
            container.Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(opts.FilterDescription))
                {
                    col.Item().PaddingTop(4).PaddingBottom(8).Text(opts.FilterDescription)
                        .FontSize(8.5f).Italic().FontColor(TextGray);
                }

                if (opts.Rows.Any())
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4); // Employee
                            cols.RelativeColumn(3); // Type
                            cols.RelativeColumn(2); // Amount
                            cols.RelativeColumn(3); // Date/Freq
                            cols.RelativeColumn(2); // Status
                        });

                        table.Header(header =>
                        {
                            void H(string text)
                            {
                                header.Cell().Background(PrimaryColor)
                                    .PaddingLeft(4).PaddingRight(4).PaddingTop(6).PaddingBottom(6)
                                    .AlignLeft().Text(text).FontColor(Colors.White).SemiBold().FontSize(8);
                            }
                            H("Employee Name"); H("Type / Name"); H("Amount"); H(opts.IsDeduction ? "Date" : "Frequency"); H("Status");
                        });

                        for (int i = 0; i < opts.Rows.Count; i++)
                        {
                            var r = opts.Rows[i];
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            void D(string text, bool right = false, bool bold = false, QuestPDF.Infrastructure.Color? color = null)
                            {
                                var cell = table.Cell().Background(bg)
                                    .PaddingLeft(4).PaddingRight(4).PaddingTop(5).PaddingBottom(5);
                                
                                if (right) cell = cell.AlignRight();
                                else cell = cell.AlignLeft();

                                var textStyle = cell.Text(text ?? "-").FontSize(7);
                                if (bold) textStyle.Bold();
                                if (color.HasValue) textStyle.FontColor(color.Value);
                            }

                            D(r.EmployeeName);
                            D(r.Type);
                            D($"{(opts.IsDeduction ? "-" : "+")}{r.Amount:N2}", right: true, bold: true, color: opts.IsDeduction ? Colors.Red.Medium : Colors.Green.Medium);
                            D(r.DateOrFrequency);
                            D(r.Status);
                        }
                    });
                }
                else
                {
                    col.Item().PaddingTop(8).Background(Colors.White).Padding(10)
                        .Text("No records found.")
                        .FontColor(TextGray).FontSize(10);
                }
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

        private void ComposeFooter(IContainer container, CompensationPdfGeneratorOptions opts, DateTime now)
        {
            container.Column(c =>
            {
                c.Item().Height(2).Background(Colors.Grey.Lighten2);

                c.Item().PaddingTop(6).Row(r =>
                {
                    r.RelativeItem().Column(col =>
                    {
                        col.Item().Text(string.IsNullOrWhiteSpace(opts.CompanyName) ? "PayRex" : opts.CompanyName).FontSize(9).Bold().FontColor(PrimaryColor);
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
