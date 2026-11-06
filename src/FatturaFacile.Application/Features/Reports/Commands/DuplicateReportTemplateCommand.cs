// <copyright file="DuplicateReportTemplateCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Commands;

/// <summary>
/// Copies a template under a new name, so that the user can customise it in Word.
/// </summary>
/// <param name="DocumentType">The document type.</param>
/// <param name="SourceName">The template to copy.</param>
/// <param name="NewName">The new template name.</param>
public sealed record DuplicateReportTemplateCommand(ReportDocumentType DocumentType, string SourceName, string NewName)
    : IRequest<ReportTemplateInfo>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => ReportPermissions.Write(DocumentType);
}
