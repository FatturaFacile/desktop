// <copyright file="ReportData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Reporting;

/// <summary>
/// The values merged into a template: simple merge fields and repeating groups
/// (Word regions «TableStart:Name» … «TableEnd:Name»).
/// </summary>
public sealed class ReportData
{
    /// <summary>
    /// Gets the simple merge fields, already formatted for the template language.
    /// </summary>
    public IDictionary<string, string> Fields { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the repeating groups (e.g. "Items", "VatSummary"), each a list of rows of formatted values.
    /// </summary>
    public IDictionary<string, IList<IDictionary<string, string>>> Groups { get; } =
        new Dictionary<string, IList<IDictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the Swiss QR-bill SVG (receipt and payment part) to append on a separate page, if any.
    /// </summary>
    public string? SwissQrBillSvg { get; set; }

    /// <summary>
    /// Gets or sets the suggested file name of the generated PDF (without folder).
    /// </summary>
    public string SuggestedFileName { get; set; } = "documento.pdf";

    /// <summary>
    /// Adds a row to a group, creating the group when missing.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <param name="row">The row values.</param>
    public void AddRow(string group, IDictionary<string, string> row)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        ArgumentNullException.ThrowIfNull(row);

        if (!Groups.TryGetValue(group, out var rows))
        {
            rows = [];
            Groups[group] = rows;
        }

        rows.Add(row);
    }
}
