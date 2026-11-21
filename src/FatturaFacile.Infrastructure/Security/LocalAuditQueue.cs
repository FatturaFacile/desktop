// <copyright file="LocalAuditQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// EF Core implementation of the hash-chained "local_audit_queue" for the active tenant.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="keys">The device key provider.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class LocalAuditQueue(FatturaFacileDbContext db, ICurrentTenantService tenant, IAuditDeviceKeyProvider keys, TimeProvider timeProvider) : ILocalAuditQueue
{
    /// <summary>
    /// Serializes the appends of every scope so that two writers can never extend the chain from the same entry.
    /// </summary>
    private static readonly SemaphoreSlim _appendGate = new(1, 1);

    /// <inheritdoc/>
    public Task<LocalAuditEntry> AppendAsync(AuditEventInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        return AppendCoreAsync(input, cancellationToken);
    }

    /// <inheritdoc/>
    public Task MarkSyncedAsync(IReadOnlyList<AuditAcknowledgement> acknowledgements, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(acknowledgements);
        return MarkSyncedCoreAsync(acknowledgements, syncedAtUtc, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AuditChainVerification> VerifyChainAsync(CancellationToken cancellationToken)
    {
        var entries = await db.Set<LocalAuditEntry>()
            .AsNoTracking()
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return AuditChainVerifier.Verify(entries, keys.GetKey());
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LocalAuditEntry>> GetPendingAsync(int max, CancellationToken cancellationToken)
    {
        return await db.Set<LocalAuditEntry>()
            .AsNoTracking()
            .Where(x => x.Status == AuditQueueStatus.Pending)
            .OrderBy(x => x.SequenceNumber)
            .Take(max)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LocalAuditEntry>> GetRecentAsync(int max, CancellationToken cancellationToken)
    {
        return await db.Set<LocalAuditEntry>()
            .AsNoTracking()
            .OrderByDescending(x => x.SequenceNumber)
            .Take(max)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<AuditQueueSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var entries = db.Set<LocalAuditEntry>().AsNoTracking();
        var pending = await entries.CountAsync(x => x.Status == AuditQueueStatus.Pending, cancellationToken).ConfigureAwait(false);
        var synced = await entries.CountAsync(x => x.Status == AuditQueueStatus.Synced, cancellationToken).ConfigureAwait(false);
        var lastSequence = await entries.Select(x => (long?)x.SequenceNumber).MaxAsync(cancellationToken).ConfigureAwait(false) ?? 0;
        var lastSynced = await entries.Select(x => x.SyncedAt).MaxAsync(cancellationToken).ConfigureAwait(false);
        return new AuditQueueSummary(pending, synced, lastSequence, lastSynced);
    }

    private async Task<LocalAuditEntry> AppendCoreAsync(AuditEventInput input, CancellationToken cancellationToken)
    {
        await _appendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var last = await db.Set<LocalAuditEntry>()
                .AsNoTracking()
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            var entry = AuditEntryFactory.Create(last, input, keys.GetKey(), keys.GetDeviceId(), timeProvider.GetUtcNow().UtcDateTime, tenant.TenantId);
            _ = db.Set<LocalAuditEntry>().Add(entry);
            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry;
        }
        finally
        {
            _ = _appendGate.Release();
        }
    }

    private async Task MarkSyncedCoreAsync(IReadOnlyList<AuditAcknowledgement> acknowledgements, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        var sequences = acknowledgements.Select(static a => a.SequenceNumber).ToList();
        var entries = await db.Set<LocalAuditEntry>()
            .Where(x => sequences.Contains(x.SequenceNumber))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var byServer = acknowledgements.GroupBy(static a => a.SequenceNumber).ToDictionary(static g => g.Key, static g => g.First());

        // Only delivery fields change: the signed fields (and so the chain) stay untouched.
        foreach (var entry in entries)
        {
            var ack = byServer[entry.SequenceNumber];
            entry.Status = AuditQueueStatus.Synced;
            entry.SyncedAt = syncedAtUtc;
            entry.ServerReceivedAt = DateTime.SpecifyKind(ack.ServerReceivedAt, DateTimeKind.Utc);
            entry.ServerReference = ack.ServerReference;
        }

        // A single SaveChanges is one transaction: either every entry of the batch is marked or none.
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
