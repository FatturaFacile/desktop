// <copyright file="ILocalAuditQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Security;

/// <summary>
/// The offline, hash-chained audit queue ("local_audit_queue") of the active tenant.
/// </summary>
public interface ILocalAuditQueue
{
    /// <summary>
    /// Appends a signed entry to the chain.
    /// </summary>
    /// <param name="input">The event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored entry.</returns>
    public Task<LocalAuditEntry> AppendAsync(AuditEventInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies every link and signature of the chain.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The verification result.</returns>
    public Task<AuditChainVerification> VerifyChainAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the oldest pending entries, in chain order.
    /// </summary>
    /// <param name="max">The maximum number of entries.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<LocalAuditEntry>> GetPendingAsync(int max, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the newest entries, newest first.
    /// </summary>
    /// <param name="max">The maximum number of entries.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<LocalAuditEntry>> GetRecentAsync(int max, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the queue counters.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary.</returns>
    public Task<AuditQueueSummary> GetSummaryAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Marks entries as synchronized in one transaction and records the server timestamps.
    /// Signed fields are never modified.
    /// </summary>
    /// <param name="acknowledgements">The server acknowledgements.</param>
    /// <param name="syncedAtUtc">The local instant of the acknowledgement.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when all entries are marked.</returns>
    public Task MarkSyncedAsync(IReadOnlyList<AuditAcknowledgement> acknowledgements, DateTime syncedAtUtc, CancellationToken cancellationToken);
}
