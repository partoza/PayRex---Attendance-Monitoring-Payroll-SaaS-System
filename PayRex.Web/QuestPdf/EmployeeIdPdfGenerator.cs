using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PayRex.Web.QuestPdf
{
    public class EmployeeIdPdfGenerator
    {
        // Generates a compact employee ID-style PDF with highlighted QR code
        public byte[] Generate(string companyName, string employeeName, string employeeCode, string role, decimal salary, DateTime startDate, int age, byte[] qrImage, byte[] logoImage, string? tin, string? sss, string? philhealth, string? pagibig)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(12);

                    page.Content().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeColumn().Column(left =>
                            {
                                left.Item().Text(companyName).FontSize(10).Bold();
                                left.Item().Text("Employee ID").FontSize(8).SemiBold();
                            });

                            row.ConstantColumn(60).AlignRight().Element(el =>
                            {
                                if (logoImage != null && logoImage.Length > 0)
                                    el.Image(logoImage, ImageScaling.FitArea);
                                else
                                    el.Text(string.Empty);
                            });
                        });

                        col.Item().PaddingVertical(6).Row(r =>
                        {
                            r.ConstantColumn(90).AlignCenter().Element(e =>
                            {
                                if (qrImage != null && qrImage.Length > 0)
                                    e.Image(qrImage, ImageScaling.FitArea);
                                else
                                    e.Text("No QR").FontSize(8);
                            });

                            r.RelativeColumn().Column(info =>
                            {
                                info.Item().Text(employeeName).FontSize(10).Bold();
                                info.Item().Text($"ID: {employeeCode}").FontSize(8);
                                info.Item().Text(role).FontSize(8);
                                info.Item().Text($"Salary: ₱{salary:N2}").FontSize(8);
                                info.Item().Text($"Start: {startDate:yyyy-MM-dd}").FontSize(8);
                                info.Item().Text($"Age: {age}").FontSize(8);
                            });
                        });

                        col.Item().PaddingTop(6).Row(r2 =>
                        {
                            r2.RelativeColumn().Column(ids =>
                            {
                                ids.Item().Text("Government IDs").FontSize(8).Bold();
                                ids.Item().Text($"TIN: {tin}").FontSize(7);
                                ids.Item().Text($"SSS: {sss}").FontSize(7);
                                ids.Item().Text($"PhilHealth: {philhealth}").FontSize(7);
                                ids.Item().Text($"Pag-IBIG: {pagibig}").FontSize(7);
                            });
                        });
                    });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}
