// <copyright file="AuditQueueSynchronizerTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the audit queue sync adapter and of the server timestamp reconciliation.
/// </summary>
public sealed class AuditQueueSynchronizerTests : IDisposable
{
    private readonly AuditTestDatabase database = new();
    private readonly FakeSecurityApiClient api = new();

    /// <summary>
    /// Pending entries are uploaded in order in one batch, then marked synchronized with the server timestamps.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_Success_UploadsInOrderAndReconcilesTimestamps()
    {
        await Seed(3);
        var batchTime = new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc);
        var entryTime = batchTime.AddMinutes(1);
        api.OnBatch = _ => new AuditBatchOutcome(SyncSendResult.Success, batchTime, [new AuditAcknowledgement(2, entryTime, "srv-2")]);

        var result = await Synchronizer().RunAsync(CancellationToken.None);

        var batch = Assert.Single(api.Batches);
        Assert.Equal(new long[] { 1, 2, 3 }, batch.Entries.Select(static e => e.SequenceNumber));
        Assert.Equal(database.Keys.GetDeviceId(), batch.DeviceId);
        Assert.Equal(new AuditSyncResult(Uploaded: 3, Remaining: 0, ChainBroken: false, AuthRequired: false, Error: null), result);

        database.Db.ChangeTracker.Clear();
        var entries = (await database.Queue.GetRecentAsync(10, CancellationToken.None)).OrderBy(static e => e.SequenceNumber).ToList();
        Assert.All(entries, static e => Assert.Equal(AuditQueueStatus.Synced, e.Status));
        Assert.Equal(batchTime, entries[0].ServerReceivedAt);
        Assert.Equal(entryTime, entries[1].ServerReceivedAt);
        Assert.Equal("srv-2", entries[1].ServerReference);
        Assert.Equal(batchTime, entries[2].ServerReceivedAt);
        Assert.True((await database.Queue.VerifyChainAsync(CancellationToken.None)).IsValid);
    }

    /// <summary>
    /// Without any server time the local clock is used for the reconciliation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_ServerReportsNoTime_FallsBackToLocalClock()
    {
        await Seed(1);

        _ = await Synchronizer().RunAsync(CancellationToken.None);

        database.Db.ChangeTracker.Clear();
        var entry = Assert.Single(await database.Queue.GetRecentAsync(10, CancellationToken.None));
        Assert.Equal(database.Clock.GetUtcNow().UtcDateTime, entry.ServerReceivedAt);
    }

    /// <summary>
    /// A failed upload marks nothing, so the entries are retried in the same order later.
    /// </summary>
    /// <param name="failure">The failing result.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(SyncSendResult.Transient)]
    [InlineData(SyncSendResult.Rejected)]
    [InlineData(SyncSendResult.Conflict)]
    public async Task Run_FailedBatch_LeavesEntriesPending(SyncSendResult failure)
    {
        await Seed(2);
        api.OnBatch = _ => new AuditBatchOutcome(failure, Error: "boom");

        var result = await Synchronizer().RunAsync(CancellationToken.None);

        Assert.Equal(0, result.Uploaded);
        Assert.Equal(2, result.Remaining);
        Assert.Equal("boom", result.Error);
        Assert.Equal(2, (await database.Queue.GetPendingAsync(10, CancellationToken.None)).Count);

        api.OnBatch = static _ => new AuditBatchOutcome(SyncSendResult.Success);
        var retry = await Synchronizer().RunAsync(CancellationToken.None);

        Assert.Equal(2, retry.Uploaded);
        Assert.Equal(api.Batches[0].BatchId, api.Batches[1].BatchId);
    }

    /// <summary>
    /// An expired session is reported and nothing is marked.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_Unauthorized_RequiresSignIn()
    {
        await Seed(1);
        api.OnBatch = static _ => new AuditBatchOutcome(SyncSendResult.Unauthorized);

        var result = await Synchronizer().RunAsync(CancellationToken.None);

        Assert.True(result.AuthRequired);
        Assert.Equal(1, result.Remaining);
    }

    /// <summary>
    /// When the user is signed out nothing is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_NotAuthenticated_DoesNotCallTheServer()
    {
        await Seed(1);
        api.IsAuthenticated = false;

        var result = await Synchronizer().RunAsync(CancellationToken.None);

        Assert.True(result.AuthRequired);
        Assert.Empty(api.Batches);
    }

    /// <summary>
    /// A tampered chain is never uploaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_TamperedChain_IsNotUploaded()
    {
        await Seed(2);
        _ = await database.Db.Database.ExecuteSqlRawAsync("UPDATE local_audit_queue SET action = 'forged' WHERE sequence_number = 2");
        database.Db.ChangeTracker.Clear();

        var result = await Synchronizer().RunAsync(CancellationToken.None);

        Assert.True(result.ChainBroken);
        Assert.Equal("record_hash_mismatch", result.Error);
        Assert.Empty(api.Batches);
    }

    /// <summary>
    /// More entries than the batch size are sent as several ordered batches.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_MoreThanBatchSize_SendsOrderedBatches()
    {
        await Seed(AuditQueueSynchronizer.BatchSize + 5);

        var result = await Synchronizer().RunAsync(CancellationToken.None);

        Assert.Equal(2, api.Batches.Count);
        Assert.Equal(AuditQueueSynchronizer.BatchSize, api.Batches[0].Entries.Count);
        Assert.Equal(AuditQueueSynchronizer.BatchSize + 1, api.Batches[1].Entries[0].SequenceNumber);
        Assert.Equal(AuditQueueSynchronizer.BatchSize + 5, result.Uploaded);
    }

    /// <summary>
    /// Reconcile prefers the per-entry server time and never invents data for unknown sequence numbers.
    /// </summary>
    [Fact]
    public void Reconcile_PrefersPerEntryTime()
    {
        var chain = new[] { Entry(1), Entry(2) };
        var batch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var entry = batch.AddHours(1);

        var acks = AuditQueueSynchronizer.Reconcile(chain, new AuditBatchOutcome(SyncSendResult.Success, batch, [new AuditAcknowledgement(1, entry, "x"), new AuditAcknowledgement(99, entry, "ignored")]), batch.AddDays(1));

        Assert.Equal(2, acks.Count);
        Assert.Equal(entry, acks[0].ServerReceivedAt);
        Assert.Equal(batch, acks[1].ServerReceivedAt);
    }

    /// <inheritdoc/>
    public void Dispose() => database.Dispose();

    private static LocalAuditEntry Entry(long sequence) =>
        new() { SequenceNumber = sequence, DeviceId = "d", RecordHash = new string('a', 64) };

    private async Task Seed(int count)
    {
        for (var i = 0; i < count; i++)
        {
            database.Clock.Advance(TimeSpan.FromSeconds(1));
            _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-" + i.ToString(CultureInfo.InvariantCulture), "update", new { index = i }), CancellationToken.None);
        }
    }

    private AuditQueueSynchronizer Synchronizer() => new(database.Queue, api, database.Clock);
}
