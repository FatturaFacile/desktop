// <copyright file="CustomerActivityOutbox.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.CustomerLifecycle;

/// <summary>
/// SQLite outbox of the customer quick actions, scoped to the active tenant by the query filter of the context.
/// </summary>
/// <param name="db">The database context.</param>
public sealed class CustomerActivityOutbox(FatturaFacileDbContext db) : ICustomerActivityOutbox
{
    /// <inheritdoc/>
    public Task EnqueueAsync(CustomerActivityOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _ = db.Set<CustomerActivityOutboxEntry>().Add(entry);

        return SaveAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerActivityOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<CustomerActivityOutboxEntry>()
            .Where(x => x.Status == CustomerActivityOutboxStatus.Pending)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public Task RemoveAsync(CustomerActivityOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _ = db.Set<CustomerActivityOutboxEntry>().Remove(entry);

        return SaveAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
