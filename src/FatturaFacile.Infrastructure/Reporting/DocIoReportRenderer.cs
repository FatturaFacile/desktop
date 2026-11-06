// <copyright file="DocIoReportRenderer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Renders reports with Syncfusion DocIO: Word mail merge of the template, conversion to PDF and,
/// for Swiss invoices, the QR-bill payment part drawn as vector graphics on an extra A4 page.
/// </summary>
public sealed class DocIoReportRenderer : IReportRenderer
{
    /// <summary>
    /// Merges the data into a template and returns the merged Word document (used by the PDF conversion and by tests).
    /// </summary>
    /// <param name="templatePath">The template path.</param>
    /// <param name="data">The merge data.</param>
    /// <returns>The merged document; the caller disposes it.</returns>
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning", Justification = "Ownership of the document is transferred to the caller.")]
    public static WordDocument Merge(string templatePath, ReportData data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatePath);
        ArgumentNullException.ThrowIfNull(data);

        byte[] template;

        using (FileStream file = new(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (MemoryStream copy = new())
        {
            file.CopyTo(copy);
            template = copy.ToArray();
        }

        using MemoryStream templateStream = new(template);
        WordDocument? document = null;

        try
        {
            document = new WordDocument(templateStream, FormatType.Automatic);
            document.MailMerge.RemoveEmptyGroup = true;
            document.MailMerge.RemoveEmptyParagraphs = true;
            document.MailMerge.ClearFields = true;

            foreach (var (name, rows) in data.Groups)
            {
                using var table = ToDataTable(name, rows);
                document.MailMerge.ExecuteGroup(table);
            }

            var names = data.Fields.Keys.ToArray();
            var values = names.Select(n => data.Fields[n]).ToArray();
            document.MailMerge.Execute(names, values);

            var merged = document;
            document = null;
            return merged;
        }
        finally
        {
            document?.Dispose();
        }
    }

    /// <inheritdoc/>
    public Task<byte[]> RenderPdfAsync(string templatePath, ReportData data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatePath);
        ArgumentNullException.ThrowIfNull(data);

        return Task.Run(() => Render(templatePath, data), cancellationToken);
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "Columns are owned and disposed by their DataTable.")]
    private static DataTable ToDataTable(string name, IList<IDictionary<string, string>> rows)
    {
        DataTable table = new(name) { Locale = System.Globalization.CultureInfo.InvariantCulture };
        foreach (var column in rows.SelectMany(static r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _ = table.Columns.Add(column, typeof(string));
        }

        foreach (var row in rows)
        {
            var dataRow = table.NewRow();

            foreach (var (key, value) in row)
            {
                dataRow[key] = value;
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }

    private static byte[] Render(string templatePath, ReportData data)
    {
        using var document = Merge(templatePath, data);
        using DocToPDFConverter converter = new();
        using var pdf = converter.ConvertToPDF(document);

        if (!string.IsNullOrWhiteSpace(data.SwissQrBillSvg))
        {
            var section = pdf.Sections.Add();
            section.PageSettings.Size = PdfPageSize.A4;
            section.PageSettings.Margins.All = 0f;
            var page = section.Pages.Add();
            QrBillPdfPainter.Paint(page.Graphics, data.SwissQrBillSvg, offsetYMillimetres: 297f - 105f);
        }

        using MemoryStream output = new();
        pdf.Save(output);
        return output.ToArray();
    }
}
