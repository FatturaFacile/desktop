// <copyright file="AuditQueueSynchronizer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Security;

/// <summary>
/// Sync adapter of the local audit queue: uploads pending entries to the web-api in chain order, one atomic batch at a time,
/// and marks them synchronized only after the server accepted the batch.
/// </summary>
/// <param name="queue">The local audit queue.</param>
/// <param name="api">The web-api security client.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class AuditQueueSynchronizer(ILocalAuditQueue queue, ISecurityApiClient api, TimeProvider timeProvider)
{
    /// <summary>
    /// The maximum number of entries per batch.
    /// </summary>
    public const int BatchSize = 200;

    /// <summary>
    /// Derives the idempotency key of a batch: identical entries always produce the same key, so a retry after a lost response is not applied twice.
    /// </summary>
    /// <param name="entries">The entries of the batch (at least one).</param>
    /// <returns>The batch identifier.</returns>
    /// <exception cref="ArgumentException">The list is empty.</exception>
    public static Guid BatchIdOf(IReadOnlyList<LocalAuditEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries.Count == 0)
        {
            throw new ArgumentException("A batch needs at least one entry.", nameof(entries));
        }

        var first = entries[0];
        var last = entries[^1];
        var text = string.Join('|', first.DeviceId, first.SequenceNumber.ToString(CultureInfo.InvariantCulture), last.SequenceNumber.ToString(CultureInfo.InvariantCulture), last.RecordHash);
        return new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(text)).AsSpan(0, 16));
    }

    /// <summary>
    /// Pairs every uploaded entry with the server timestamp: the per-entry value when reported, otherwise the batch value, otherwise the local time.
    /// Only delivery data is produced: the signed fields of the entries are never part of the result.
    /// </summary>
    /// <param name="sent">The uploaded entries.</param>
    /// <param name="outcome">The accepted outcome.</param>
    /// <param name="fallback">The local instant used when the server reported nothing.</param>
    /// <returns>One acknowledgement per uploaded entry.</returns>
    public static IReadOnlyList<AuditAcknowledgement> Reconcile(IReadOnlyList<LocalAuditEntry> sent, AuditBatchOutcome outcome, DateTime fallback)
    {
        ArgumentNullException.ThrowIfNull(sent);
        ArgumentNullException.ThrowIfNull(outcome);

        Dictionary<long, AuditAcknowledgement> byServer = [];
        foreach (var ack in outcome.Entries ?? [])
        {
            _ = byServer.TryAdd(ack.SequenceNumber, ack);
        }

        var batchTime = outcome.ReceivedAt ?? fallback;
        List<AuditAcknowledgement> result = [];

        foreach (var entry in sent)
        {
            result.Add(byServer.TryGetValue(entry.SequenceNumber, out var reported)
                ? reported
                : new AuditAcknowledgement(entry.SequenceNumber, batchTime, ServerReference: null));
        }

        return result;
    }

    /// <summary>
    /// Uploads every pending entry; stops at the first failed batch so the order is preserved.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary of the run.</returns>
    public async Task<AuditSyncResult> RunAsync(CancellationToken cancellationToken)
    {
        var verification = await queue.VerifyChainAsync(cancellationToken).ConfigureAwait(false);
        if (!verification.IsValid)
        {
            var broken = await queue.GetSummaryAsync(cancellationToken).ConfigureAwait(false);
            return new AuditSyncResult(0, broken.Pending, ChainBroken: true, AuthRequired: false, verification.Reason);
        }

        if (!api.IsAuthenticated)
        {
            var summary = await queue.GetSummaryAsync(cancellationToken).ConfigureAwait(false);
            return new AuditSyncResult(0, summary.Pending, ChainBroken: false, AuthRequired: summary.Pending > 0, Error: null);
        }

        var uploaded = 0;
        var authRequired = false;
        string? error = null;

        while (true)
        {
            var pending = await queue.GetPendingAsync(BatchSize, cancellationToken).ConfigureAwait(false);
            if (pending.Count == 0)
            {
                break;
            }

            var outcome = await api.UploadAuditBatchAsync(new AuditBatchRequest(BatchIdOf(pending), pending[0].DeviceId, pending), cancellationToken).ConfigureAwait(false);
            if (outcome.Result != SyncSendResult.Success)
            {
                authRequired = outcome.Result == SyncSendResult.Unauthorized;
                error = outcome.Error;
                break;
            }

            var now = timeProvider.GetUtcNow().UtcDateTime;
            await queue.MarkSyncedAsync(Reconcile(pending, outcome, now), now, cancellationToken).ConfigureAwait(false);
            uploaded += pending.Count;
        }

        var after = await queue.GetSummaryAsync(cancellationToken).ConfigureAwait(false);
        return new AuditSyncResult(uploaded, after.Pending, ChainBroken: false, authRequired, error);
    }
}
