// <copyright file="ArchiveReportCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Commands;

/// <summary>
/// Saves a generated PDF in the document archive of the current company.
/// </summary>
/// <param name="Report">The generated report.</param>
public sealed record ArchiveReportCommand(RenderedReport Report) : IRequest<string>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => ReportPermissions.Read(Report.DocumentType);
}
