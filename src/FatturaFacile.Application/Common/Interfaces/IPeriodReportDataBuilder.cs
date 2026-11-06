// <copyright file="IPeriodReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Reporting;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// A report data builder that also accepts a period (e.g. monthly time sheets).
/// </summary>
public interface IPeriodReportDataBuilder : IReportDataBuilder
{
    /// <summary>
    /// Builds the merge data of a document for a period.
    /// </summary>
    /// <param name="entityId">The identifier of the subject (e.g. the employee).</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="culture">The culture used to format values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data, or null when the subject does not exist.</returns>
    public Task<ReportData?> BuildAsync(Guid entityId, DateTime from, DateTime to, CultureInfo culture, CancellationToken cancellationToken = default);
}
