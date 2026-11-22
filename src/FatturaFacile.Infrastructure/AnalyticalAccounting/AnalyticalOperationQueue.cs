// <copyright file="AnalyticalOperationQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.AnalyticalAccounting;

/// <summary>
/// EF Core implementation of <see cref="IAnalyticalOperationQueue"/> for the active tenant.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class AnalyticalOperationQueue(FatturaFacileDbContext db, ICurrentTenantService tenant, TimeProvider time) : IAnalyticalOperationQueue
{
    private const int MaxErrorLength = 500;

    /// <inheritdoc/>
    public async Task<Guid> EnqueueAsync(string payloadJson, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        var row = new LocalAnalyticalOperation { TenantId = tenant.TenantId, PayloadJson = payloadJson, CreatedAt = time.GetUtcNow().UtcDateTime };
        _ = db.Set<LocalAnalyticalOperation>().Add(row);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return row.Id;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PendingAnalyticalOperation>> PendingAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<LocalAnalyticalOperation>().AsNoTracking()
            .Where(x => x.Status == LocalAnalyticalOperation.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. rows.Select(x => new PendingAnalyticalOperation(x.Id, x.PayloadJson))];
    }

    /// <inheritdoc/>
    public Task<int> CountPendingAsync(CancellationToken cancellationToken) =>
        db.Set<LocalAnalyticalOperation>().CountAsync(x => x.Status == LocalAnalyticalOperation.Pending, cancellationToken);

    /// <inheritdoc/>
    public Task MarkSentAsync(Guid id, CancellationToken cancellationToken) =>
        UpdateAsync(id, LocalAnalyticalOperation.Sent, reason: null, cancellationToken);

    /// <inheritdoc/>
    public Task MarkFailedAsync(Guid id, string? reason, CancellationToken cancellationToken) =>
        UpdateAsync(id, LocalAnalyticalOperation.Failed, reason, cancellationToken);

    private async Task UpdateAsync(Guid id, string status, string? reason, CancellationToken cancellationToken)
    {
        var row = await db.Set<LocalAnalyticalOperation>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            return;
        }

        row.Status = status;
        row.Attempts++;
        row.LastError = reason is null ? null : reason[..Math.Min(reason.Length, MaxErrorLength)];
        row.UpdatedAt = time.GetUtcNow().UtcDateTime;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
