// <copyright file="ICustomerActivityOutbox.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// Local outbox of the customer quick actions saved while offline, scoped to the active tenant.
/// </summary>
public interface ICustomerActivityOutbox
{
    /// <summary>
    /// Saves a new entry.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the entry is saved.</returns>
    public Task EnqueueAsync(CustomerActivityOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entries waiting to be sent, oldest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<CustomerActivityOutboxEntry>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Removes an entry the server accepted.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the entry is removed.</returns>
    public Task RemoveAsync(CustomerActivityOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes of the tracked entries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the changes are saved.</returns>
    public Task SaveAsync(CancellationToken cancellationToken);
}
