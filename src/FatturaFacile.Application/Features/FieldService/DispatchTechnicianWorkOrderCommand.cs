// <copyright file="DispatchTechnicianWorkOrderCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Dispatches a work order to a technician in a time window. The server checks the schedule overlap, the required skills and the van stock, and returns every blocker together. Needs a connection.
/// </summary>
/// <param name="WorkOrderId">The work order identifier.</param>
/// <param name="TechnicianId">The technician identifier.</param>
/// <param name="Start">The scheduled start.</param>
/// <param name="End">The scheduled end.</param>
public sealed record DispatchTechnicianWorkOrderCommand(string WorkOrderId, string TechnicianId, DateTimeOffset Start, DateTimeOffset End) : IRequest<FieldServiceDispatchResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
