// <copyright file="DownloadServiceReportPdfCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Downloads the signed service report PDF from the server and saves it in the service report folder of the user. Needs a connection.
/// </summary>
/// <param name="ReportId">The service report identifier.</param>
public sealed record DownloadServiceReportPdfCommand(string ReportId) : IRequest<FieldServicePdfResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
