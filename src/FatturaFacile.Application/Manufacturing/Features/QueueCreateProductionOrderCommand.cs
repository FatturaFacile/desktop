// <copyright file="QueueCreateProductionOrderCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Creates a production order locally and queues its creation for the server.
/// </summary>
/// <param name="ItemId">The produced item identifier.</param>
/// <param name="ItemCode">The produced item code.</param>
/// <param name="ItemName">The produced item name.</param>
/// <param name="BomId">The BOM.</param>
/// <param name="RoutingId">The routing.</param>
/// <param name="TargetQuantity">The quantity to produce.</param>
/// <param name="PlannedStartDate">The planned start date.</param>
/// <param name="PlannedEndDate">The planned end date.</param>
public sealed record QueueCreateProductionOrderCommand(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    Guid? BomId,
    Guid? RoutingId,
    decimal TargetQuantity,
    DateTime PlannedStartDate,
    DateTime PlannedEndDate) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingWrite;
}
