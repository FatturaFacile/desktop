// <copyright file="ITabularPdfRenderer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Renders a <see cref="TabularPdfDocument"/> to a PDF file.
/// </summary>
public interface ITabularPdfRenderer
{
    /// <summary>
    /// Renders the document, numbering its pages from a given number.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="firstPage">The number of the first page, so that a register continues the numbering of the year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The PDF bytes.</returns>
    public Task<byte[]> RenderAsync(TabularPdfDocument document, int firstPage, CancellationToken cancellationToken);
}
