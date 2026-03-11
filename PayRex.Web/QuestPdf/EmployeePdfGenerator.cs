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
    public class EmployeeRecord
    {
        public string IdCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public int? Age { get; set; }
        public string? CivilStatus { get; set; }
        public DateTime? StartDate { get; set; }
        
        // Salary fields
        public decimal Salary { get; set; }
        public string PayType { get; set; } = string.Empty;

        // Government IDs
        public string? Tin { get; set; }
        public string? Sss { get; set; }
        public string? PhilHealth { get; set; }
        public string? PagIbig { get; set; }
    }

    public class EmployeePdfGeneratorOptions
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyTagline { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }
        
        public string IssuerName { get; set; } = string.Empty;
        public string IssuerPosition { get; set; } = string.Empty;
        public string? IssuerSignatureUrl { get; set; }
        
        public bool IncludeSalary { get; set; }
        public bool IncludeGovIds { get; set; }
        public string FilterSentence { get; set; } = string.Empty;

        public IEnumerable<EmployeeRecord> Employees { get; set; } = Array.Empty<EmployeeRecord>();
    }

    public class EmployeePdfGenerator
    {
        /*
        * New Blue Theme
        * Primary: Deep Blue #1E3A8A
        * Secondary: Blue #2563EB
        */
        private readonly QuestPDF.Infrastructure.Color PrimaryColor = QuestPDF.Infrastructure.Color.FromHex("#1E3A8A");
        private readonly QuestPDF.Infrastructure.Color SecondaryColor = QuestPDF.Infrastructure.Color.FromHex("#2563EB");
        private readonly QuestPDF.Infrastructure.Color TextDark = QuestPDF.Infrastructure.Color.FromHex("#1F2937");
        private readonly QuestPDF.Infrastructure.Color TextGray = QuestPDF.Infrastructure.Color.FromHex("#6B7280");
        private readonly QuestPDF.Infrastructure.Color SuccessColor = Colors.Green.Darken2;
        private readonly QuestPDF.Infrastructure.Color WarningColor = Colors.Orange.Darken2;

        public byte[] Generate(EmployeePdfGeneratorOptions opts)
        {
            var list = (opts.Employees ?? Array.Empty<EmployeeRecord>()).ToList();
            var now = DateTime.Now;

            byte[]? logoBytes = FetchImage(opts.CompanyLogoUrl);
            byte[]? sigBytes = FetchImage(opts.IssuerSignatureUrl);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(TextDark).FontFamily("Helvetica"));

                    page.Header().Element(c => ComposeHeader(c, opts, logoBytes, now, list.Count));
                    page.Content().PaddingHorizontal(0).PaddingTop(8).Element(c => ComposeContent(c, opts, list, sigBytes));
                    page.Footer().Element(c => ComposeFooter(c, opts, now));
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, EmployeePdfGeneratorOptions opts, byte[]? logoBytes, DateTime now, int totalEmployees)
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
                        c.Item().Text(opts.CompanyName).FontSize(18).Bold().FontColor(PrimaryColor);
                        
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
                        c.Item().Text("EMPLOYEE RECORDS REPORT").FontSize(14).Bold().FontColor(TextDark);

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
                            r.RelativeItem().Text("Total Employees:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text($"{totalEmployees:N0}").FontSize(9).Bold().FontColor(PrimaryColor);
                        });
                    });
                });

                col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, EmployeePdfGeneratorOptions opts, List<EmployeeRecord> list, byte[]? sigBytes)
        {
            container.Column(c =>
            {
                // KPI Cards removed
                c.Item().PaddingTop(12).Element(cont => ComposeMovementsTable(cont, list, opts));
                c.Item().PaddingTop(10).Element(cont => ComposeNotesSection(cont, opts.IssuerName, opts.IssuerPosition, sigBytes));
            });
        }

        private void ComposeMovementsTable(IContainer container, List<EmployeeRecord> list, EmployeePdfGeneratorOptions opts)
        {
            container.Column(col =>
            {
                col.Item().Text("EMPLOYEE LIST").FontSize(11).Bold().FontColor(PrimaryColor);
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Filter sentence shown before the table
                if (!string.IsNullOrWhiteSpace(opts.FilterSentence))
                {
                    col.Item().PaddingTop(6).PaddingBottom(2).Text(opts.FilterSentence)
                        .FontSize(8.5f).Italic().FontColor(TextGray);
                }

                if (list.Any())
                {
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // ID Code
                            columns.RelativeColumn(3); // Name
                            columns.RelativeColumn(2); // Position
                            columns.RelativeColumn(3); // Email
                            columns.RelativeColumn(2); // Contact
                            columns.ConstantColumn(30); // Age
                            columns.RelativeColumn(2); // Civil Status
                            columns.RelativeColumn(2); // Start Date

                            if (opts.IncludeSalary)
                            {
                                columns.RelativeColumn(2); // Salary Rate
                                columns.RelativeColumn(2); // Pay Type
                            }

                            if (opts.IncludeGovIds)
                            {
                                columns.RelativeColumn(2); // TIN
                                columns.RelativeColumn(2); // SSS
                                columns.RelativeColumn(2); // PhilHealth
                                columns.RelativeColumn(2); // PagIbig
                            }
                        });

                        table.Header(header =>
                        {
                            void HeaderCell(string text, bool rightAlign = false)
                            {
                                var cell = header.Cell().Background(PrimaryColor).PaddingLeft(4).PaddingRight(4).PaddingTop(6).PaddingBottom(6);
                                var alignedCell = rightAlign ? cell.AlignRight() : cell.AlignLeft();
                                alignedCell.Text(text).FontColor(Colors.White).SemiBold().FontSize(8);
                            }

                            HeaderCell("ID");
                            HeaderCell("Name");
                            HeaderCell("Role");
                            HeaderCell("Email");
                            HeaderCell("Contact");
                            HeaderCell("Age", true);
                            HeaderCell("Civil Status");
                            HeaderCell("Start Date");

                            if (opts.IncludeSalary)
                            {
                                HeaderCell("Salary Rate", true);
                                HeaderCell("Pay Type");
                            }

                            if (opts.IncludeGovIds)
                            {
                                HeaderCell("TIN");
                                HeaderCell("SSS");
                                HeaderCell("PhilHealth");
                                HeaderCell("Pag-Ibig");
                            }
                        });


                        for (int i = 0; i < list.Count; i++)
                        {
                            var e = list[i];
                            var backgroundColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            void DataCell(string text, bool rightAlign = false)
                            {
                                var cell = table.Cell().Background(backgroundColor).PaddingLeft(4).PaddingRight(4).PaddingTop(5).PaddingBottom(5);
                                var alignedCell = rightAlign ? cell.AlignRight() : cell.AlignLeft();
                                alignedCell.Text(text ?? "-").FontSize(7);
                            }

                            DataCell(e.IdCode);
                            DataCell(e.Name);
                            DataCell(e.Position);
                            DataCell(e.Email);
                            DataCell(e.Contact);
                            DataCell(e.Age?.ToString(), true);
                            DataCell(e.CivilStatus);
                            DataCell(e.StartDate?.ToString("MM/dd/yy") ?? "-");

                            if (opts.IncludeSalary)
                            {
                                DataCell($"₱{e.Salary:N2}", true);
                                DataCell(e.PayType);
                            }

                            if (opts.IncludeGovIds)
                            {
                                DataCell(e.Tin);
                                DataCell(e.Sss);
                                DataCell(e.PhilHealth);
                                DataCell(e.PagIbig);
                            }
                        }
                    });
                }
                else
                {
                    col.Item().PaddingTop(8).Background(Colors.White).Padding(10)
                        .Text("No employees found for the selected company.")
                        .FontColor(TextGray).FontSize(10);
                }

                col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
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

        private void ComposeFooter(IContainer container, EmployeePdfGeneratorOptions opts, DateTime now)
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
