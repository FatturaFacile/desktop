// <copyright file="IReportRenderer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Merges report data into a Word template and converts the result to PDF.
/// </summary>
public interface IReportRenderer
{
    /// <summary>
    /// Renders a template to PDF.
    /// </summary>
    /// <param name="templatePath">The .docx template path.</param>
    /// <param name="data">The merge data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The PDF document.</returns>
    public Task<byte[]> RenderPdfAsync(string templatePath, ReportData data, CancellationToken cancellationToken = default);
}
