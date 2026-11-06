// <copyright file="IReportPrintService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Prints a document: lets the user choose one of the templates of the document type and shows the PDF preview.
/// </summary>
internal interface IReportPrintService
{
    /// <summary>
    /// Prints a document.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="entityId">The document identifier.</param>
    /// <returns>True when a preview was opened, false when the user cancelled.</returns>
    public Task<bool> PrintAsync(ReportDocumentType documentType, Guid entityId);

    /// <summary>
    /// Prints a period document (e.g. the time sheet of an employee).
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="entityId">The subject identifier.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <returns>True when a preview was opened, false when the user cancelled.</returns>
    public Task<bool> PrintAsync(ReportDocumentType documentType, Guid entityId, DateTime from, DateTime to);
}
