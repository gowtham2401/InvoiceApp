using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InvoiceApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceApp.Services
{
    public class PdfExportService
    {
        private readonly JsonDataService _dataService;

        public PdfExportService(JsonDataService dataService)
        {
            _dataService = dataService;
        }

        public async System.Threading.Tasks.Task<string> ExportAsync(Invoice invoice, Customer customer)
        {
            var settings = await _dataService.LoadSettingsAsync();
            var outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            Directory.CreateDirectory(outputDir);
            var filePath = Path.Combine(outputDir, $"Invoice_{invoice.InvoiceNumber}.pdf");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader(settings, invoice));
                    page.Content().Element(ComposeContent(invoice, customer));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            document.GeneratePdf(filePath);
            return filePath;
        }

        // Fixed header: now returns a single child (Column) containing the row and the line
        private Action<IContainer> ComposeHeader(AppSettings settings, Invoice invoice)
        {
            return container =>
            {
                container.Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(colLeft =>
                        {
                            colLeft.Item().Text(settings.CompanyName).Bold().FontSize(22).FontColor(Colors.Blue.Darken3);
                            if (!string.IsNullOrWhiteSpace(settings.CompanyAddress))
                                colLeft.Item().Text(settings.CompanyAddress).FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(settings.CompanyPhone))
                                colLeft.Item().Text($"Phone: {settings.CompanyPhone}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(settings.CompanyEmail))
                                colLeft.Item().Text($"Email: {settings.CompanyEmail}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(settings.CompanyGst))
                                colLeft.Item().Text($"GST: {settings.CompanyGst}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(150).Column(colRight =>
                        {
                            colRight.Item().AlignRight().Text("INVOICE").Bold().FontSize(24).FontColor(Colors.Blue.Darken3);
                            colRight.Item().AlignRight().Text($"#{invoice.InvoiceNumber}").FontSize(11).FontColor(Colors.Grey.Darken2);
                            colRight.Item().AlignRight().Text($"Date: {invoice.Date:dd MMM yyyy}").FontSize(9);
                            colRight.Item().AlignRight().Text($"Status: {invoice.Status}").Bold().FontSize(9)
                                .FontColor(invoice.Status == "Paid" ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                        });
                    });

                    col.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
                });
            };
        }

        private Action<IContainer> ComposeContent(Invoice invoice, Customer customer)
        {
            return container =>
            {
                container.Column(col =>
                {
                    col.Spacing(15);

                    // Bill To
                    col.Item().PaddingTop(10).Column(billTo =>
                    {
                        billTo.Item().Text("BILL TO").Bold().FontSize(10).FontColor(Colors.Grey.Darken2);
                        billTo.Item().Text(customer.Name).Bold().FontSize(13);
                        if (!string.IsNullOrWhiteSpace(customer.Address))
                            billTo.Item().Text(customer.Address).FontSize(9);
                        if (!string.IsNullOrWhiteSpace(customer.Phone))
                            billTo.Item().Text($"Phone: {customer.Phone}").FontSize(9);
                        if (!string.IsNullOrWhiteSpace(customer.Email))
                            billTo.Item().Text($"Email: {customer.Email}").FontSize(9);
                        if (!string.IsNullOrWhiteSpace(customer.GstNumber))
                            billTo.Item().Text($"GST: {customer.GstNumber}").FontSize(9);
                    });

                    // Items Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        // Header
                        table.Header(header =>
                        {
                            void HeaderCell(string text) =>
                                header.Cell().Background(Colors.Blue.Darken3).Padding(6)
                                    .Text(text).FontColor(Colors.White).Bold().FontSize(9);

                            HeaderCell("Item Description");
                            HeaderCell("Qty");
                            HeaderCell("Unit Price");
                            HeaderCell("Total");
                        });

                        // Rows (fixed cell content)
                        bool alternate = false;
                        foreach (var item in invoice.Items)
                        {
                            var bg = alternate ? Colors.Grey.Lighten4 : Colors.White;
                            alternate = !alternate;

                            // Local function to add a cell with proper text and alignment
                            void AddCell(string text, bool alignRight = false)
                            {
                                var cell = table.Cell()
                                    .Background(bg)
                                    .Padding(6)
                                    .Text(text)
                                    .FontSize(9);

                                if (alignRight)
                                    cell.AlignRight();
                            }

                            AddCell(item.ItemName);
                            AddCell(item.Quantity.ToString(), true);
                            AddCell($"₹{item.UnitPrice:N2}", true);
                            AddCell($"₹{item.Total:N2}", true);
                        }
                    });

                    // Totals
                    col.Item().AlignRight().Width(220).Column(totals =>
                    {
                        void TotalRow(string label, string value, bool bold = false)
                        {
                            totals.Item().Row(r =>
                            {
                                var left = r.RelativeItem().Text(label);
                                var right = r.ConstantItem(90).AlignRight().Text(value);

                                if (bold)
                                {
                                    left.Bold();
                                    right.Bold();
                                }
                            });
                        }

                        TotalRow("Subtotal:", $"₹{invoice.Subtotal:N2}");
                        TotalRow($"Tax ({invoice.TaxPercent}%):", $"₹{invoice.TaxAmount:N2}");
                        totals.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        TotalRow("GRAND TOTAL:", $"₹{invoice.GrandTotal:N2}", bold: true);
                    });

                    // Notes
                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        col.Item().Column(notes =>
                        {
                            notes.Item().Text("Notes").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                            notes.Item().Background(Colors.Grey.Lighten4).Padding(8).Text(invoice.Notes).FontSize(9);
                        });
                    }
                });
            };
        }
    }
}