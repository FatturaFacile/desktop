// <copyright file="LocalAuditQueueTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Security;
using FatturaFacile.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the hash-chained local audit queue.
/// </summary>
public sealed class LocalAuditQueueTests : IDisposable
{
    private readonly AuditTestDatabase database = new();

    /// <summary>
    /// Appended entries form a verifiable chain with contiguous sequence numbers.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Append_BuildsVerifiableChain()
    {
        var first = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "update", new { name = "A" }), CancellationToken.None);
        database.Clock.Advance(TimeSpan.FromSeconds(5));
        var second = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "delete"), CancellationToken.None);

        Assert.Equal(1, first.SequenceNumber);
        Assert.Equal(2, second.SequenceNumber);
        Assert.Equal(first.RecordHash, second.PreviousHash);
        Assert.Equal(database.Keys.GetDeviceId(), second.DeviceId);
        Assert.True((await database.Queue.VerifyChainAsync(CancellationToken.None)).IsValid);
        Assert.Equal(new AuditQueueSummary(Pending: 2, Synced: 0, LastSequence: 2, LastSyncedAt: null), await database.Queue.GetSummaryAsync(CancellationToken.None));
    }

    /// <summary>
    /// Editing a stored entry directly in the database is caught by the verification.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Verify_AfterTamperingInDatabase_Fails()
    {
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "update"), CancellationToken.None);
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-2", "update"), CancellationToken.None);

        _ = await database.Db.Database.ExecuteSqlRawAsync("UPDATE local_audit_queue SET entity_id = 'forged' WHERE sequence_number = 1");
        database.Db.ChangeTracker.Clear();

        var result = await database.Queue.VerifyChainAsync(CancellationToken.None);
        Assert.False(result.IsValid);
        Assert.Equal(1, result.BrokenAtSequence);
    }

    /// <summary>
    /// The context refuses to modify signed fields or to delete entries.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SignedFields_CannotBeModifiedOrDeleted()
    {
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "update"), CancellationToken.None);

        var entry = await database.Db.LocalAuditQueue.SingleAsync();
        entry.Action = "forged";
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => database.Db.SaveChangesAsync());
        database.Db.ChangeTracker.Clear();

        var again = await database.Db.LocalAuditQueue.SingleAsync();
        _ = database.Db.LocalAuditQueue.Remove(again);
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => database.Db.SaveChangesAsync());
    }

    /// <summary>
    /// Marking entries synchronized records the server time and leaves every signed field, and so the chain, untouched.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MarkSynced_StoresServerTimeWithoutMutatingSignedFields()
    {
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "update", new { v = 1 }), CancellationToken.None);
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-2", "update", new { v = 2 }), CancellationToken.None);
        var before = (await database.Queue.GetPendingAsync(10, CancellationToken.None)).ToDictionary(static e => e.SequenceNumber);

        var serverTime = new DateTime(2026, 11, 21, 10, 30, 0, DateTimeKind.Utc);
        var syncedAt = new DateTime(2026, 11, 21, 10, 31, 0, DateTimeKind.Utc);
        await database.Queue.MarkSyncedAsync([new AuditAcknowledgement(1, serverTime, "srv-1"), new AuditAcknowledgement(2, serverTime.AddSeconds(1), ServerReference: null)], syncedAt, CancellationToken.None);
        database.Db.ChangeTracker.Clear();

        var after = (await database.Queue.GetRecentAsync(10, CancellationToken.None)).ToDictionary(static e => e.SequenceNumber);
        foreach (var sequence in before.Keys)
        {
            Assert.Equal(before[sequence].RecordHash, after[sequence].RecordHash);
            Assert.Equal(before[sequence].PayloadJson, after[sequence].PayloadJson);
            Assert.Equal(before[sequence].OccurredAt, after[sequence].OccurredAt);
            Assert.Equal(AuditQueueStatus.Synced, after[sequence].Status);
        }

        Assert.Equal(serverTime, after[1].ServerReceivedAt);
        Assert.Equal("srv-1", after[1].ServerReference);
        Assert.Equal(syncedAt, after[1].SyncedAt);
        Assert.True((await database.Queue.VerifyChainAsync(CancellationToken.None)).IsValid);
        Assert.Empty(await database.Queue.GetPendingAsync(10, CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose() => database.Dispose();
}
