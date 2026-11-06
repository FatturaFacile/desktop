// <copyright file="RenderReportQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Queries;

/// <summary>
/// Generates the PDF of a document with the chosen template.
/// </summary>
/// <param name="DocumentType">The document type.</param>
/// <param name="TemplateName">The template name (as listed by <see cref="GetReportTemplatesQuery"/>).</param>
/// <param name="EntityId">The identifier of the document to print.</param>
/// <param name="From">The first day of the period, for period reports (time sheets).</param>
/// <param name="To">The last day of the period, for period reports.</param>
public sealed record RenderReportQuery(ReportDocumentType DocumentType, string TemplateName, Guid EntityId, DateTime? From = null, DateTime? To = null)
    : IRequest<RenderedReport>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => ReportPermissions.Read(DocumentType);
}
