// <copyright file="SyncfusionTabularPdfRenderer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Drawing;
using System.Globalization;
using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// <see cref="ITabularPdfRenderer"/> over Syncfusion PDF: a landscape A4 table that paginates, with the page number printed on every
/// page, counted from the number the caller gives.
/// </summary>
public sealed class SyncfusionTabularPdfRenderer : ITabularPdfRenderer
{
    private const float Margin = 28f;
    private const float TitleSize = 14f;
    private const float BodySize = 8f;

    /// <inheritdoc/>
    public Task<byte[]> RenderAsync(TabularPdfDocument document, int firstPage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        return Task.Run(() => Render(document, firstPage), cancellationToken);
    }

    private static byte[] Render(TabularPdfDocument document, int firstPage)
    {
        using PdfDocument pdf = new();
        pdf.PageSettings.Orientation = PdfPageOrientation.Landscape;
        pdf.PageSettings.Margins.All = Margin;

        PdfStandardFont titleFont = new(PdfFontFamily.Helvetica, TitleSize, PdfFontStyle.Bold);
        PdfStandardFont headerFont = new(PdfFontFamily.Helvetica, BodySize, PdfFontStyle.Bold);
        PdfStandardFont bodyFont = new(PdfFontFamily.Helvetica, BodySize, PdfFontStyle.Regular);
        PdfSolidBrush ink = new(new PdfColor(40, 40, 40));

        var page = pdf.Pages.Add();
        var width = page.GetClientSize().Width;
        page.Graphics.DrawString(document.Title, titleFont, ink, new PointF(0, 0));
        page.Graphics.DrawString(document.Subtitle, bodyFont, ink, new PointF(0, 20));

        PdfGrid grid = new();
        grid.Columns.Add(document.Headers.Count);
        var headerRow = grid.Headers.Add(1)[0];
        for (var column = 0; column < document.Headers.Count; column++)
        {
            headerRow.Cells[column].Value = document.Headers[column];
        }

        headerRow.Style.BackgroundBrush = new PdfSolidBrush(new PdfColor(240, 243, 248));
        headerRow.Style.Font = headerFont;

        foreach (var cells in document.Rows)
        {
            var row = grid.Rows.Add();
            for (var column = 0; column < document.Headers.Count && column < cells.Count; column++)
            {
                row.Cells[column].Value = cells[column];
            }

            row.Style.Font = bodyFont;
        }

        PdfGridLayoutFormat format = new() { Layout = PdfLayoutType.Paginate, Break = PdfLayoutBreakType.FitPage };
        var result = grid.Draw(page, new PointF(0, 38), format);

        var y = (result?.Bounds.Bottom + 10) ?? 60f;
        var last = result?.Page ?? page;
        foreach (var line in document.Footer)
        {
            last.Graphics.DrawString(line, bodyFont, ink, new RectangleF(0, y, width, 24));
            y += 12;
        }

        for (var index = 0; index < pdf.Pages.Count; index++)
        {
            var current = pdf.Pages[index];
            var label = string.Create(CultureInfo.InvariantCulture, $"{firstPage + index}");
            var size = current.GetClientSize();
            current.Graphics.DrawString(label, bodyFont, ink, new PointF(size.Width - 20, size.Height - 10));
        }

        using MemoryStream stream = new();
        pdf.Save(stream);

        return stream.ToArray();
    }
}
