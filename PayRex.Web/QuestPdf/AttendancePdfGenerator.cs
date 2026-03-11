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
    public class AttendanceRecordExport
    {
        public string Date { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string TimeIn { get; set; } = string.Empty;
        public string TimeOut { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }

    public class AttendancePdfGeneratorOptions
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
        
        public string ActiveFiltersDescription { get; set; } = string.Empty;

        public IEnumerable<AttendanceRecordExport> Records { get; set; } = Array.Empty<AttendanceRecordExport>();
    }

    public class AttendancePdfGenerator
    {
        /*
        * New Blue Theme
        * Primary: Deep Blue #1E3A8A
        * Secondary: Blue #2563EB
        */
        private readonly QuestPDF.Infrastructure.Color PrimaryColor = QuestPDF.Infrastructure.Color.FromHex("#1E3A8A");
        private readonly QuestPDF.Infrastructure.Color TextDark = QuestPDF.Infrastructure.Color.FromHex("#1F2937");
        private readonly QuestPDF.Infrastructure.Color TextGray = QuestPDF.Infrastructure.Color.FromHex("#6B7280");

        public byte[] Generate(AttendancePdfGeneratorOptions opts)
        {
            var list = (opts.Records ?? Array.Empty<AttendanceRecordExport>()).ToList();
            var now = DateTime.Now;

            byte[]? logoBytes = FetchImage(opts.CompanyLogoUrl);
            byte[]? sigBytes = FetchImage(opts.IssuerSignatureUrl);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, opts, logoBytes, now, list.Count));
                    page.Content().Element(c => ComposeContent(c, opts, list, sigBytes));
                    
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(9).FontColor(TextGray);
                        x.CurrentPageNumber().FontSize(9).FontColor(TextGray);
                        x.Span(" of ").FontSize(9).FontColor(TextGray);
                        x.TotalPages().FontSize(9).FontColor(TextGray);
                    });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, AttendancePdfGeneratorOptions opts, byte[]? logoBytes, DateTime now, int totalRecords)
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
                        c.Item().Text("ATTENDANCE RECORDS REPORT").FontSize(14).Bold().FontColor(TextDark);

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Generated Date:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text(now.ToString("MM/dd/yyyy")).FontSize(9);
                        });

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Logged:").FontSize(9).SemiBold().FontColor(TextGray);
                            r.RelativeItem().AlignRight().Text($"{totalRecords:N0}").FontSize(9).Bold().FontColor(PrimaryColor);
                        });
                    });
                });

                col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, AttendancePdfGeneratorOptions opts, List<AttendanceRecordExport> list, byte[]? sigBytes)
        {
            container.Column(c =>
            {
                c.Item().PaddingTop(12).Element(cont => ComposeMovementsTable(cont, list, opts));
                c.Item().PaddingTop(10).Element(cont => ComposeNotesSection(cont, opts.IssuerName, opts.IssuerPosition, sigBytes));
            });
        }

        private void ComposeMovementsTable(IContainer container, List<AttendanceRecordExport> list, AttendancePdfGeneratorOptions opts)
        {
            container.Column(col =>
            {
                col.Item().Text("ATTENDANCE LIST").FontSize(11).Bold().FontColor(PrimaryColor);
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Filter sentence shown before the table
                if (!string.IsNullOrWhiteSpace(opts.ActiveFiltersDescription))
                {
                    col.Item().PaddingTop(6).PaddingBottom(2).Text(opts.ActiveFiltersDescription)
                        .FontSize(8.5f).Italic().FontColor(TextGray);
                }

                if (list.Any())
                {
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(2); // Employee ID
                            columns.RelativeColumn(3); // Employee Name
                            columns.RelativeColumn(2); // Time In
                            columns.RelativeColumn(2); // Time Out
                            columns.RelativeColumn(2); // Hours
                            columns.RelativeColumn(2); // Status
                            columns.RelativeColumn(2); // Remarks
                        });

                        table.Header(header =>
                        {
                            void HeaderCell(string text, bool rightAlign = false)
                            {
                                var cell = header.Cell().Background(PrimaryColor).PaddingLeft(4).PaddingRight(4).PaddingTop(6).PaddingBottom(6);
                                var alignedCell = rightAlign ? cell.AlignRight() : cell.AlignLeft();
                                alignedCell.Text(text).FontColor(Colors.White).SemiBold().FontSize(8);
                            }

                            HeaderCell("Date");
                            HeaderCell("Employee ID");
                            HeaderCell("Employee Name");
                            HeaderCell("Time In");
                            HeaderCell("Time Out");
                            HeaderCell("Hours", true);
                            HeaderCell("Status");
                            HeaderCell("Remarks");
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

                            DataCell(e.Date);
                            DataCell(e.EmployeeId);
                            DataCell(e.EmployeeName);
                            DataCell(e.TimeIn);
                            DataCell(e.TimeOut);
                            DataCell(e.Hours, true);
                            DataCell(e.Status);
                            DataCell(e.Remarks);
                        }
                    });
                }
                else
                {
                    col.Item().PaddingTop(8).Background(Colors.White).Padding(10)
                        .Text("No attendance records found for the selected company.")
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
                                cn.Height(50).Image(sigBytes);
                            }
                            else
                            {
                                cn.Height(50).AlignBottom().Text("_________________________").FontColor(Colors.Grey.Medium);
                            }
                        });
                    });

                    sig.Item().PaddingTop(2).Text($"Issued by: {issuerName}").FontSize(10).Bold().FontColor(TextDark);
                    sig.Item().Text(string.IsNullOrWhiteSpace(issuerPosition) ? "Authorized Signatory" : issuerPosition)
                        .FontSize(9).FontColor(TextGray);
                });
            });
        }

        // Helper method to fetch image from URL as byte array
        private byte[]? FetchImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            try
            {
                using var client = new HttpClient();
                // synchronous wait is generally discouraged in async pipelines, 
                // but QuestPDF synchronous doc generation requires available bytes.
                return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            catch
            {
                return null; // Ignore errors, let it fallback to default graphic
            }
        }
    }
}
