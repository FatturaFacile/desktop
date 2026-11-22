// <copyright file="SignAndCloseServiceReportCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Signs and closes the service report of a work order (checklist, labor and travel metrics, meter readings, customer signature). Queued offline; the server seals it with SHA-256, completes the work order and generates the localized PDF.
/// </summary>
/// <param name="Report">The report.</param>
public sealed record SignAndCloseServiceReportCommand(FieldServiceReportInput Report) : IRequest<FieldServiceQueueResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
