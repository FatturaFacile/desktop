// <copyright file="SyncfusionDocumentExportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Drawing;
using System.Globalization;
using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.XlsIO;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Service generating non-visual PDF documents and Excel exports via Syncfusion engines.
/// </summary>
public sealed class SyncfusionDocumentExportService : IDocumentExportService
{
    /// <inheritdoc/>
    public Task<byte[]> GenerateInvoicePdfAsync(
        InvoiceDto invoice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return GenerateInvoicePdf(invoice);
    }

    /// <inheritdoc/>
    public Task<byte[]> ExportInvoicesToExcelAsync(
        IEnumerable<InvoiceDto> invoices,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoices);

        return ExportInvoicesToExcelInternalAsync(invoices);
    }

    private static async Task<byte[]> ExportInvoicesToExcelInternalAsync(IEnumerable<InvoiceDto> invoices)
    {
        using var excelEngine = new ExcelEngine();

        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];

        worksheet.Name = "Fatture";

        string[] headers =
        [
            "Numero",
            "Data",
            "Destinatario",
            "Giurisdizione",
            "Valuta",
            "Imponibile",
            "IVA",
            "Totale Lordo",
            "Stato",
        ];

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Range[1, i + 1].Text = headers[i];
            worksheet.Range[1, i + 1].CellStyle.Font.Bold = true;
            worksheet.Range[1, i + 1].CellStyle.Color = Color.FromArgb(240, 243, 248);
        }

        var rowIndex = 2;

        foreach (var invoice in invoices)
        {
            worksheet.Range[rowIndex, 1].Text = invoice.InvoiceNumber;
            worksheet.Range[rowIndex, 2].DateTime = invoice.Date;
            worksheet.Range[rowIndex, 3].Text = invoice.ContactName;
            worksheet.Range[rowIndex, 4].Text = invoice.Jurisdiction;
            worksheet.Range[rowIndex, 5].Text = invoice.Currency;
            worksheet.Range[rowIndex, 6].Number = (double)invoice.TotalNet;
            worksheet.Range[rowIndex, 7].Number = (double)invoice.TotalTax;
            worksheet.Range[rowIndex, 8].Number = (double)invoice.TotalGross;
            worksheet.Range[rowIndex, 9].Text = invoice.Status;

            rowIndex++;
        }

        worksheet.UsedRange.AutofitColumns();

        await using var memoryStream = new MemoryStream();

        workbook.SaveAs(memoryStream);
        workbook.Close();

        return await Task.FromResult(memoryStream.ToArray());
    }

    private static async Task<byte[]> GenerateInvoicePdf(InvoiceDto invoice)
    {
        using var document = new PdfDocument();

        var page = document.Pages.Add();
        var graphics = page.Graphics;

        var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
        var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Regular);
        var smallFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8, PdfFontStyle.Regular);

        var primaryBrush = new PdfSolidBrush(new PdfColor(20, 35, 60));
        var textBrush = new PdfSolidBrush(new PdfColor(40, 40, 40));

        // Header Title
        graphics.DrawString("FATTURA FACILE ERP", titleFont, primaryBrush, new PointF(0, 0));
        graphics.DrawString(
            $"Documento: {invoice.InvoiceNumber}",
            headerFont,
            primaryBrush,
            new PointF(0, 25));
        graphics.DrawString(
            string.Create(CultureInfo.InvariantCulture, $"Data: {invoice.Date:dd/MM/yyyy}"),
            bodyFont,
            textBrush,
            new PointF(0, 42));
        graphics.DrawString(
            $"Scadenza: {(invoice.DueDate.HasValue ? invoice.DueDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "A vista")}",
            bodyFont,
            textBrush,
            new PointF(0, 56));

        // Customer details
        graphics.DrawString("Destinatario / Spett.le:", headerFont, primaryBrush, new PointF(300, 25));
        graphics.DrawString(invoice.ContactName, bodyFont, textBrush, new PointF(300, 42));
        graphics.DrawString(
            $"Giurisdizione: {invoice.Jurisdiction} | Valuta: {invoice.Currency}",
            smallFont,
            textBrush,
            new PointF(300, 56));

        // Draw dividing rule
        graphics.DrawLine(
            new PdfPen(new PdfColor(200, 200, 200), 1),
            new PointF(0, 75),
            new PointF(500, 75));

        // Invoice Items Table via PdfGrid
        var grid = new PdfGrid();
        grid.Columns.Add(4);
        grid.Columns[0].Width = 240;
        grid.Columns[1].Width = 60;
        grid.Columns[2].Width = 100;
        grid.Columns[3].Width = 100;

        var headerRow = grid.Headers.Add(1)[0];
        headerRow.Cells[0].Value = "Descrizione";
        headerRow.Cells[1].Value = "Q.tà";
        headerRow.Cells[2].Value = "Prezzo Unit.";
        headerRow.Cells[3].Value = "Totale Riga";

        headerRow.Style.BackgroundBrush = new PdfSolidBrush(new PdfColor(240, 243, 248));
        headerRow.Style.Font = headerFont;

        foreach (var item in invoice.Items)
        {
            var row = grid.Rows.Add();
            row.Cells[0].Value = item.Description;
            row.Cells[1].Value = item.Quantity.ToString("F2", CultureInfo.InvariantCulture);
            row.Cells[2].Value = string.Create(
                CultureInfo.InvariantCulture,
                $"{item.UnitPrice:F2} {invoice.Currency}");
            row.Cells[3].Value = string.Create(
                CultureInfo.InvariantCulture,
                $"{item.LineTotal:F2} {invoice.Currency}");
            row.Style.Font = bodyFont;
        }

        var result = grid.Draw(page, new PointF(0, 90));

        // Totals Box
        var totalY = result.Bounds.Bottom + 20;
        graphics.DrawString(
            string.Create(
                CultureInfo.InvariantCulture,
                $"Imponibile Netto: {invoice.TotalNet:F2} {invoice.Currency}"),
            headerFont,
            textBrush,
            new PointF(300, totalY));
        graphics.DrawString(
            string.Create(
                CultureInfo.InvariantCulture,
                $"Totale IVA: {invoice.TotalTax:F2} {invoice.Currency}"),
            headerFont,
            textBrush,
            new PointF(300, totalY + 16));

        if (invoice.RoundingAmount != 0m)
        {
            graphics.DrawString(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"Arrotondamento 0.05: {invoice.RoundingAmount:F2} {invoice.Currency}"),
                smallFont,
                textBrush,
                new PointF(300, totalY + 32));
        }

        graphics.DrawString(
            string.Create(
                CultureInfo.InvariantCulture,
                $"TOTALE DOVUTO: {invoice.TotalGross:F2} {invoice.Currency}"),
            titleFont,
            primaryBrush,
            new PointF(300, totalY + 48));

        // Country Specific Compliance Section
        if (string.Equals(invoice.Jurisdiction, "CH", StringComparison.OrdinalIgnoreCase) &&
            invoice.QrBillData != null)
        {
            var qrY = totalY + 80;
            graphics.DrawString(
                "Swiss QR-Bill Payment Slip (ISO 20022)",
                headerFont,
                primaryBrush,
                new PointF(0, qrY));
            graphics.DrawString(
                $"QR-IBAN: {invoice.QrBillData.QrIban}",
                bodyFont,
                textBrush,
                new PointF(0, qrY + 18));
            graphics.DrawString(
                $"Riferimento {invoice.QrBillData.ReferenceType}: {invoice.QrBillData.PaymentReference}",
                bodyFont,
                textBrush,
                new PointF(0, qrY + 34));
        }
        else if (string.Equals(invoice.Jurisdiction, "IT", StringComparison.OrdinalIgnoreCase) &&
                 invoice.SdiMetadata != null)
        {
            var sdiY = totalY + 80;
            graphics.DrawString(
                "Fattura Elettronica SDI (FatturaPA)",
                headerFont,
                primaryBrush,
                new PointF(0, sdiY));
            graphics.DrawString(
                $"File SDI: {invoice.SdiMetadata.FileName} | Formato: {invoice.SdiMetadata.TransmissionFormat}",
                bodyFont,
                textBrush,
                new PointF(0, sdiY + 18));
            graphics.DrawString(
                $"Stato Trasmissione SDI: {invoice.SdiMetadata.SdiStatus.ToUpperInvariant()}",
                bodyFont,
                textBrush,
                new PointF(0, sdiY + 34));
        }

        await using var memoryStream = new MemoryStream();

        document.Save(memoryStream);
        document.Close(completely: true);

        return memoryStream.ToArray();
    }
}
