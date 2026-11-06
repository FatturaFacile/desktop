// <copyright file="IReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Reporting;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Builds the merge data of one document type from the database.
/// </summary>
public interface IReportDataBuilder
{
    /// <summary>
    /// Gets the document type handled by the builder.
    /// </summary>
    public ReportDocumentType DocumentType { get; }

    /// <summary>
    /// Builds the merge data of a document.
    /// </summary>
    /// <param name="entityId">The identifier of the document (invoice, payslip, certificate).</param>
    /// <param name="culture">The culture used to format values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data, or null when the document does not exist.</returns>
    public Task<ReportData?> BuildAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken = default);
}
