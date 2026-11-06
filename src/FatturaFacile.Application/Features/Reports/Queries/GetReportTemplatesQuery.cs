// <copyright file="GetReportTemplatesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Queries;

/// <summary>
/// Lists the templates available on the file system for a document type.
/// </summary>
/// <param name="DocumentType">The document type.</param>
public sealed record GetReportTemplatesQuery(ReportDocumentType DocumentType) : IRequest<IReadOnlyList<ReportTemplateInfo>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => ReportPermissions.Read(DocumentType);
}
