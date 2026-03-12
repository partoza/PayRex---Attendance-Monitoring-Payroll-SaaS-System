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
    public class UserExportRow
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class UsersPdfGeneratorOptions
    {
        public string IssuerName { get; set; } = string.Empty;
        public string IssuerPosition { get; set; } = string.Empty;
        public string? IssuerSignatureUrl { get; set; }
        public string FilterDescription { get; set; } = string.Empty;
        public IEnumerable<UserExportRow> Users { get; set; } = Array.Empty<UserExportRow>();
        public byte[]? LogoBytes { get; set; }
    }

    public class UsersPdfGenerator
    {
        private readonly QuestPDF.Infrastructure.Color PrimaryColor = QuestPDF.Infrastructure.Color.FromHex("#1E3A8A");
        private readonly QuestPDF.Infrastructure.Color SecondaryColor = QuestPDF.Infrastructure.Color.FromHex("#2563EB");
        private readonly QuestPDF.Infrastructure.Color TextDark = QuestPDF.Infrastructure.Color.FromHex("#1F2937");
        private readonly QuestPDF.Infrastructure.Color TextGray = QuestPDF.Infrastructure.Color.FromHex("#6B7280");

        public byte[] Generate(UsersPdfGeneratorOptions opts)
        {
            var list = (opts.Users ?? Array.Empty<UserExportRow>()).ToList();
            var now = DateTime.Now;
            byte[]? sigBytes = FetchImage(opts.IssuerSignatureUrl);
            byte[]? logoBytes = opts.LogoBytes;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(TextDark).FontFamily("Helvetica"));

                    page.Header().Element(c => ComposeHeader(c, now, list.Count, logoBytes));
                    page.Content().PaddingHorizontal(0).PaddingTop(8).Element(c => ComposeContent(c, opts, list, sigBytes));
                    page.Footer().Element(c => ComposeFooter(c, now));
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, DateTime now, int totalRecords, byte[]? logoBytes = null)
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
                        c.Item().Text("PayRex").FontSize(18).Bold().FontColor(PrimaryColor);
                        c.Item().PaddingTop(4).Text("Administration Platform").FontSize(9).FontColor(TextGray);
                        c.Item().Text("User Management System").FontSize(9).FontColor(TextGray);
                    });

                    row.ConstantItem(260).Column(c =>
                    {
                        c.Item().Text("USERS REPORT").FontSize(14).Bold().FontColor(TextDark);

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
                            r.RelativeItem().Text("Total Users:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text($"{totalRecords:N0}").FontSize(9).Bold().FontColor(PrimaryColor);
                        });
                    });
                });

                col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, UsersPdfGeneratorOptions opts, List<UserExportRow> list, byte[]? sigBytes)
        {
            container.Column(c =>
            {
                c.Item().PaddingTop(12).Element(cont => ComposeTable(cont, list, opts.FilterDescription));
                c.Item().PaddingTop(10).Element(cont => ComposeSignatureSection(cont, opts.IssuerName, opts.IssuerPosition, sigBytes));
            });
        }

        private void ComposeTable(IContainer container, List<UserExportRow> list, string filterDesc)
        {
            container.Column(col =>
            {
                col.Item().Text("USER LIST").FontSize(11).Bold().FontColor(PrimaryColor);
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                if (!string.IsNullOrWhiteSpace(filterDesc))
                {
                    col.Item().PaddingTop(6).PaddingBottom(2).Text(filterDesc)
                        .FontSize(8.5f).Italic().FontColor(TextGray);
                }

                if (list.Any())
                {
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3); // Full Name
                            cols.RelativeColumn(4); // Email
                            cols.RelativeColumn(2); // Role
                            cols.RelativeColumn(3); // Company
                            cols.RelativeColumn(1); // Status
                            cols.RelativeColumn(2); // Created
                        });

                        table.Header(header =>
                        {
                            void H(string text)
                            {
                                header.Cell().Background(PrimaryColor)
                                    .PaddingLeft(4).PaddingRight(4).PaddingTop(6).PaddingBottom(6)
                                    .AlignLeft().Text(text).FontColor(Colors.White).SemiBold().FontSize(8);
                            }
                            H("Full Name"); H("Email"); H("Role"); H("Company"); H("Status"); H("Registered");
                        });

                        for (int i = 0; i < list.Count; i++)
                        {
                            var u = list[i];
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            void D(string text)
                            {
                                table.Cell().Background(bg)
                                    .PaddingLeft(4).PaddingRight(4).PaddingTop(5).PaddingBottom(5)
                                    .AlignLeft().Text(text ?? "-").FontSize(7);
                            }

                            D(u.FullName);
                            D(u.Email);
                            D(u.Role);
                            D(u.Company);
                            D(u.Status);
                            D(u.CreatedAt);
                        }
                    });
                }
                else
                {
                    col.Item().PaddingTop(8).Background(Colors.White).Padding(10)
                        .Text("No users found for the selected filters.")
                        .FontColor(TextGray).FontSize(10);
                }

                col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeSignatureSection(IContainer container, string issuerName, string issuerPosition, byte[]? sigBytes)
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

        private void ComposeFooter(IContainer container, DateTime now)
        {
            container.Column(c =>
            {
                c.Item().Height(2).Background(Colors.Grey.Lighten2);

                c.Item().PaddingTop(6).Row(r =>
                {
                    r.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PayRex").FontSize(9).Bold().FontColor(PrimaryColor);
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

        private byte[]? FetchImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                using var client = new HttpClient();
                return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            catch { return null; }
        }
    }
}
