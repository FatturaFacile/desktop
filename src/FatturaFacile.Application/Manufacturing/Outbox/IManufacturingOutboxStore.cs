// <copyright file="IManufacturingOutboxStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// Local persistence of the manufacturing outbox. Every method is atomic.
/// </summary>
public interface IManufacturingOutboxStore
{
    /// <summary>
    /// Queues an entry together with its optimistic local effect, in one transaction.
    /// </summary>
    /// <param name="entry">The entry; the store assigns the FIFO sequence.</param>
    /// <param name="log">The shop-floor log to insert (confirmations only).</param>
    /// <param name="change">The optimistic change of the local order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task QueueAsync(ManufacturingOutboxEntry entry, ShopFloorLog? log, OptimisticOrderChange change, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts an order created offline (with its operations) and queues its creation, in one transaction.
    /// </summary>
    /// <param name="order">The new local order.</param>
    /// <param name="entry">The creation entry; the store assigns the FIFO sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task QueueNewOrderAsync(ProductionOrder order, ManufacturingOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entries in the Pending state ordered by sequence.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pending entries.</returns>
    public Task<IReadOnlyList<ManufacturingOutboxEntry>> GetPendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the orders that have a Conflict or Failed entry; their later entries are held back.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The blocked local order identifiers.</returns>
    public Task<IReadOnlySet<Guid>> GetBlockedOrderIdsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Persists the new state of an entry and recomputes the pending flag of its order.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(ManufacturingOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the placeholders of the entry route with the server order identifier.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The route, or null while the order creation is not yet synchronised.</returns>
    public Task<ResolvedRoute?> ResolveRouteAsync(ManufacturingOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Applies the effect of a delivered or discarded entry to the local rows (server identifier of a created order, confirmation number, log state).
    /// </summary>
    /// <param name="entry">The entry, already carrying its final status.</param>
    /// <param name="responseBody">The server response body, when any.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task ApplyOutcomeAsync(ManufacturingOutboxEntry entry, string? responseBody, CancellationToken cancellationToken);
}
