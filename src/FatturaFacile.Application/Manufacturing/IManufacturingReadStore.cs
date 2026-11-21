// <copyright file="IManufacturingReadStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Features;
using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Read access to the offline manufacturing store of the current tenant.
/// </summary>
public interface IManufacturingReadStore
{
    /// <summary>
    /// Loads everything the workbench shows.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The workbench data.</returns>
    public Task<ManufacturingWorkbench> GetWorkbenchAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads one order with its operations.
    /// </summary>
    /// <param name="orderId">The local order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order, or null.</returns>
    public Task<ProductionOrder?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the shop-floor logs of one order.
    /// </summary>
    /// <param name="orderId">The local order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The logs.</returns>
    public Task<IReadOnlyList<ShopFloorLog>> GetLogsAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the routing operations of one routing.
    /// </summary>
    /// <param name="routingId">The routing identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operations.</returns>
    public Task<IReadOnlyList<RoutingOperation>> GetRoutingOperationsAsync(Guid routingId, CancellationToken cancellationToken);
}
