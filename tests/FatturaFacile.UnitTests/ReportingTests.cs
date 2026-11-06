// <copyright file="ReportingTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Strategies.Swiss;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Reporting;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the file-system report templates and of the DocIO renderer.
/// </summary>
public sealed class ReportingTests
{
    /// <summary>
    /// Default templates are created per document type, listed, duplicated and protected against path traversal.
    /// </summary>
    [Fact]
    public void TemplateStore_CreatesListsAndDuplicatesTemplates()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"ff-templates-{Guid.NewGuid():N}");

        try
        {
            var store = new FileSystemReportTemplateStore(new DatabasePaths(Path.Combine(folder, "db.sqlite"), Path.Combine(folder, "k.key")));
            store.EnsureDefaultTemplates();

            var invoices = store.GetTemplates(ReportDocumentType.Invoice);
            Assert.Contains(invoices, static t => string.Equals(t.Name, "Fattura_IT", StringComparison.Ordinal));
            Assert.Contains(invoices, static t => string.Equals(t.Name, "Fattura_EN", StringComparison.Ordinal));
            Assert.NotEmpty(store.GetTemplates(ReportDocumentType.Payslip));
            Assert.NotEmpty(store.GetTemplates(ReportDocumentType.SalaryCertificate));

            var copy = store.Duplicate(ReportDocumentType.Invoice, "Fattura_IT", "Fattura_IT_Logo");
            Assert.True(File.Exists(copy.FilePath));
            _ = Assert.Throws<ArgumentException>(() => store.Duplicate(ReportDocumentType.Invoice, "Fattura_IT", "..\\..\\evil"));
            _ = Assert.Throws<IOException>(() => store.Duplicate(ReportDocumentType.Invoice, "Fattura_IT", "Fattura_IT_Logo"));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    /// <summary>
    /// The invoice template is merged (fields and repeating rows) and converted to PDF with the QR-bill page.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Renderer_MergesInvoiceAndAppendsQrBill()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"ff-render-{Guid.NewGuid():N}");

        try
        {
            var store = new FileSystemReportTemplateStore(new DatabasePaths(Path.Combine(folder, "db.sqlite"), Path.Combine(folder, "k.key")));
            store.EnsureDefaultTemplates();
            var template = store.GetTemplates(ReportDocumentType.Invoice).First(static t => string.Equals(t.Name, "Fattura_IT", StringComparison.Ordinal));

            ReportData data = new();
            data.Fields["Company_Name"] = "Muster AG";
            data.Fields["Invoice_Number"] = "2026-0042";
            data.AddRow("Items", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Item_Description"] = "Consulenza", ["Item_Total"] = "1'000.00" });
            data.AddRow("Items", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Item_Description"] = "Trasferta", ["Item_Total"] = "120.00" });

            using (var merged = DocIoReportRenderer.Merge(template.FilePath, data))
            {
                var text = merged.GetText();
                Assert.Contains("Muster AG", text, StringComparison.Ordinal);
                Assert.Contains("Consulenza", text, StringComparison.Ordinal);
                Assert.Contains("Trasferta", text, StringComparison.Ordinal);
                Assert.DoesNotContain("Item_Description", text, StringComparison.Ordinal);
            }

            var request = new SwissQrBillRequest(
                "CH4431999123000889012",
                new SwissQrBillAddress("Muster AG", "Bahnhofstrasse", "1", "8001", "Zürich", "CH"),
                1120m,
                "CHF",
                Debtor: null,
                "QRR",
                "210000000003139471430009017",
                UnstructuredMessage: null);
            QrCodeMatrix modules = new(25);
            modules.Set(0, 0, dark: true);
            data.SwissQrBillSvg = SwissQrBillSvgRenderer.Render(request, modules);

            var pdf = await new DocIoReportRenderer().RenderPdfAsync(template.FilePath, data);

            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(pdf, 0, 4), StringComparison.Ordinal);
            Assert.True(pdf.Length > 2000);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
