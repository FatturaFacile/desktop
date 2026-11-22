// <copyright file="AdvanceFieldWorkOrderStatusCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Moves a dispatched work order to en route or on site. Queued offline like every technician operation.
/// </summary>
/// <param name="WorkOrderId">The work order.</param>
/// <param name="Status">The new status: en_route or on_site.</param>
public sealed record AdvanceFieldWorkOrderStatusCommand(Guid WorkOrderId, string Status) : IRequest<FieldServiceQueueResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
