// <copyright file="TabularPdfDocument.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The content of a printable table: a title, a table and the lines under it. The registers and the Certificazioni Uniche are
/// rendered through it.
/// </summary>
/// <param name="Title">The title of the document.</param>
/// <param name="Subtitle">The line under the title.</param>
/// <param name="Headers">The column headers.</param>
/// <param name="Rows">The rows; each has one cell per header.</param>
/// <param name="Footer">The lines printed under the table.</param>
public sealed record TabularPdfDocument(
    string Title,
    string Subtitle,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<string> Footer);
