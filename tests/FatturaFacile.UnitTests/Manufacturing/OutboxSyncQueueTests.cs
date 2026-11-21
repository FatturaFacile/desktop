// <copyright file="OutboxSyncQueueTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Manufacturing;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// Tests of the offline outbox: queuing while offline, idempotent replay with a stable posting UUID, and conflict resolution on reconnect.
/// </summary>
public sealed class OutboxSyncQueueTests : IDisposable
{
    private readonly ManufacturingHarness harness = new();

    /// <inheritdoc/>
    public void Dispose() => harness.Dispose();

    /// <summary>
    /// A confirmation recorded offline is applied locally at once and waits in the outbox without touching the network.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_Offline_IsQueuedAndAppliedLocally()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.InProgress);
        var lastOperation = order.Operations.OrderBy(o => o.Sequence).Last();

        var uuid = await harness.Queue.QueueConfirmationAsync(Confirm(order, lastOperation, good: 8m, scrap: 1m), CancellationToken.None);

        var stored = await harness.OrderAsync(order.Id);
        Assert.Equal(8m, stored.CompletedQuantity);
        Assert.Equal(1m, stored.ScrappedQuantity);
        Assert.Equal(8m, stored.Operations.Single(o => o.Id == lastOperation.Id).ConfirmedGoodQuantity);
        Assert.True(stored.HasPendingChanges);

        var entry = Assert.Single(await harness.OutboxAsync());
        Assert.Equal(ManufacturingOutboxStatus.Pending, entry.Status);
        Assert.Equal(uuid, entry.IdempotencyKey);
        Assert.Equal(uuid, Assert.Single(await harness.LogsAsync()).OperationPostingUuid);
        harness.Gateway.Verify(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// A retry after a lost response reuses the stored payload and the same operation_posting_uuid, so the server answers 200 (replay) and nothing is posted twice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_RetryAfterLostResponse_ReusesPostingUuidAndCountsServerReplay()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.InProgress);
        var lastOperation = order.Operations.OrderBy(o => o.Sequence).Last();
        var uuid = await harness.Queue.QueueConfirmationAsync(Confirm(order, lastOperation, good: 8m, scrap: 0m), CancellationToken.None);

        _ = harness.Gateway
            .SetupSequence(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Transient, 0, error: "timeout"))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 200, "{\"data\":{\"confirmation_number\":\"C-1\"}}"));

        var first = await harness.Queue.FlushAsync(CancellationToken.None);
        Assert.Equal(0, first.Sent);
        _ = Assert.Single(first.Errors);
        var pending = Assert.Single(await harness.OutboxAsync());
        Assert.Equal(ManufacturingOutboxStatus.Pending, pending.Status);
        Assert.Equal(1, pending.Attempts);
        Assert.True(pending.NextAttemptAt > harness.Clock.GetUtcNow().UtcDateTime);

        // Still inside the back-off window: nothing is sent.
        var held = await harness.Queue.FlushAsync(CancellationToken.None);
        Assert.Equal(1, held.Deferred);
        _ = Assert.Single(harness.SentRequests());

        harness.Clock.Advance(TimeSpan.FromMinutes(1));
        var second = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, second.Sent);
        Assert.Equal(1, second.Replayed);
        var requests = harness.SentRequests();
        Assert.Equal(2, requests.Count);
        Assert.All(requests, r => Assert.Equal(uuid, r.IdempotencyKey));
        Assert.Equal(requests[0].JsonPayload, requests[1].JsonPayload);
        using (var payload = JsonDocument.Parse(requests[1].JsonPayload!))
        {
            Assert.Equal(uuid, payload.RootElement.GetProperty("operation_posting_uuid").GetGuid());
        }

        var log = Assert.Single(await harness.LogsAsync());
        Assert.Equal(ManufacturingOutboxStatus.Synced, log.SyncStatus);
        Assert.Equal("C-1", log.ServerConfirmationNumber);
        Assert.Equal(8m, (await harness.OrderAsync(order.Id)).CompletedQuantity);
    }

    /// <summary>
    /// A synchronised entry is never sent again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Twice_DoesNotResendSyncedEntries()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Released);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Start, CancellationToken.None);
        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 200));

        var first = await harness.Queue.FlushAsync(CancellationToken.None);
        var second = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, first.Sent);
        Assert.Equal(0, second.Sent);
        _ = Assert.Single(harness.SentRequests());
        Assert.False((await harness.OrderAsync(order.Id)).HasPendingChanges);
    }

    /// <summary>
    /// When the session expired the flush stops and leaves the entry untouched.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Unauthorized_StopsWithoutChangingTheEntry()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Released);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Start, CancellationToken.None);
        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Unauthorized, 401));

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.True(result.AuthRequired);
        var entry = Assert.Single(await harness.OutboxAsync());
        Assert.Equal(ManufacturingOutboxStatus.Pending, entry.Status);
        Assert.Equal(0, entry.Attempts);
    }

    /// <summary>
    /// A rejected entry fails and holds back the later entries of the same order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Rejected_FailsAndHoldsLaterEntriesOfTheOrder()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Released);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Start, CancellationToken.None);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Complete, CancellationToken.None);
        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Rejected, 400, error: "bad"));

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, result.Failed);
        Assert.Equal(1, result.Deferred);
        _ = Assert.Single(harness.SentRequests());
        var entries = await harness.OutboxAsync();
        Assert.Equal([ManufacturingOutboxStatus.Failed, ManufacturingOutboxStatus.Pending], entries.Select(e => e.Status));
    }

    /// <summary>
    /// An ambiguous conflict blocks that order only: the entries of other orders still go out, in order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ManualReviewConflict_BlocksOnlyThatOrder()
    {
        var a = await harness.SeedOrderAsync(ProductionOrderStatus.Released);
        var b = await harness.SeedOrderAsync(ProductionOrderStatus.Released);
        _ = await harness.Queue.QueueTransitionAsync(a.Id, ManufacturingMutationKind.Start, CancellationToken.None);
        _ = await harness.Queue.QueueTransitionAsync(a.Id, ManufacturingMutationKind.Complete, CancellationToken.None);
        _ = await harness.Queue.QueueTransitionAsync(b.Id, ManufacturingMutationKind.Start, CancellationToken.None);

        _ = harness.Gateway
            .Setup(g => g.SendAsync(It.Is<ManufacturingRequest>(r => r.Route.Contains(a.Id.ToString("D"), StringComparison.Ordinal)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Conflict, 409, error: "state"));
        _ = harness.Gateway
            .Setup(g => g.SendAsync(It.Is<ManufacturingRequest>(r => r.Route.Contains(b.Id.ToString("D"), StringComparison.Ordinal)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 200));
        _ = harness.Gateway
            .Setup(g => g.GetJsonAsync("production-orders/" + a.Id.ToString("D"), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{\"status\":\"planned\"}");

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, result.Sent);
        Assert.Equal(1, result.Conflicts);
        Assert.Equal(1, result.Deferred);
        var entries = await harness.OutboxAsync();
        Assert.Equal(ManufacturingOutboxStatus.Conflict, entries[0].Status);
        Assert.Equal(nameof(ConflictDecision.ManualReview), entries[0].Resolution);
        Assert.Equal(ManufacturingOutboxStatus.Pending, entries[1].Status);
        Assert.Equal(ManufacturingOutboxStatus.Synced, entries[2].Status);
        Assert.True((await harness.OrderAsync(a.Id)).HasPendingChanges);
        Assert.False((await harness.OrderAsync(b.Id)).HasPendingChanges);
    }

    /// <summary>
    /// When the server already moved the order to the intended state, the entry is closed as already applied.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ConflictServerAlreadyAtTarget_IsMarkedAlreadyApplied()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Planned);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Release, CancellationToken.None);
        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Conflict, 409, error: "already released"));
        _ = harness.Gateway.Setup(g => g.GetJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("{\"status\":\"released\"}");

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, result.Sent);
        var entry = Assert.Single(await harness.OutboxAsync());
        Assert.Equal(ManufacturingOutboxStatus.Synced, entry.Status);
        Assert.Equal(nameof(ConflictDecision.AlreadyApplied), entry.Resolution);
        Assert.False((await harness.OrderAsync(order.Id)).HasPendingChanges);
    }

    /// <summary>
    /// When the server state now allows the transition it is applied once more on the next flush; a second conflict would need a person.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ConflictReapply_SucceedsOnNextFlush()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Planned);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Release, CancellationToken.None);
        _ = harness.Gateway
            .SetupSequence(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Conflict, 409, error: "locked"))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 200));
        _ = harness.Gateway.Setup(g => g.GetJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("{\"status\":\"planned\"}");

        var first = await harness.Queue.FlushAsync(CancellationToken.None);
        var afterFirst = Assert.Single(await harness.OutboxAsync());
        Assert.Equal(0, first.Sent);
        Assert.Equal(ManufacturingOutboxStatus.Pending, afterFirst.Status);
        Assert.Equal(1, afterFirst.ConflictRetries);

        var second = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, second.Sent);
        Assert.Equal(ManufacturingOutboxStatus.Synced, Assert.Single(await harness.OutboxAsync()).Status);
    }

    /// <summary>
    /// A posting on an order the server already completed is dropped (server wins) and its optimistic effect is taken back out of the local order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ConfirmationAfterServerCompleted_IsDiscardedAndRevertsTheLocalEffect()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.InProgress);
        var lastOperation = order.Operations.OrderBy(o => o.Sequence).Last();
        _ = await harness.Queue.QueueConfirmationAsync(Confirm(order, lastOperation, good: 8m, scrap: 2m), CancellationToken.None);
        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Conflict, 422, error: "order completed"));
        _ = harness.Gateway.Setup(g => g.GetJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("{\"status\":\"completed\"}");

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(1, result.Discarded);
        var stored = await harness.OrderAsync(order.Id);
        Assert.Equal(0m, stored.CompletedQuantity);
        Assert.Equal(0m, stored.ScrappedQuantity);
        Assert.Equal(0m, stored.Operations.Single(o => o.Id == lastOperation.Id).ConfirmedGoodQuantity);
        Assert.False(stored.HasPendingChanges);
        Assert.Equal(ManufacturingOutboxStatus.Discarded, Assert.Single(await harness.LogsAsync()).SyncStatus);
        Assert.Equal(nameof(ConflictDecision.ServerWins), Assert.Single(await harness.OutboxAsync()).Resolution);
    }

    /// <summary>
    /// A conflict whose server state cannot be read because the network dropped is retried later, not decided.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ConflictWhileServerStateUnreachable_IsTreatedAsTransient()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Planned);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Release, CancellationToken.None);
        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Conflict, 409));
        _ = harness.Gateway.Setup(g => g.GetJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("offline"));

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        _ = Assert.Single(result.Errors);
        var entry = Assert.Single(await harness.OutboxAsync());
        Assert.Equal(ManufacturingOutboxStatus.Pending, entry.Status);
        Assert.Equal(1, entry.Attempts);
    }

    /// <summary>
    /// An order created offline is sent first; the entries queued behind it use the server identifier the creation returned.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateOrder_ThenRelease_UsesTheServerIdentifierIssuedByTheCreation()
    {
        var serverId = Guid.NewGuid();
        ProductionOrder local = new()
        {
            OrderNumber = "LOCAL-1",
            ItemId = Guid.NewGuid(),
            Status = ProductionOrderStatus.Planned,
            TargetQuantity = 4m,
            PlannedStartDate = harness.Clock.GetUtcNow().Date,
            PlannedEndDate = harness.Clock.GetUtcNow().Date.AddDays(2),
            HasPendingChanges = true,
        };

        _ = await harness.Queue.QueueCreateOrderAsync(local, CancellationToken.None);
        _ = await harness.Queue.QueueTransitionAsync(local.Id, ManufacturingMutationKind.Release, CancellationToken.None);
        _ = harness.Gateway
            .Setup(g => g.SendAsync(It.Is<ManufacturingRequest>(r => r.Route == "production-orders"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 201, "{\"data\":{\"id\":\"" + serverId.ToString("D") + "\",\"order_number\":\"MO-77\"}}"));
        _ = harness.Gateway
            .Setup(g => g.SendAsync(It.Is<ManufacturingRequest>(r => r.Route.EndsWith("/release", StringComparison.Ordinal)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 200));

        var result = await harness.Queue.FlushAsync(CancellationToken.None);

        Assert.Equal(2, result.Sent);
        var requests = harness.SentRequests();
        Assert.Equal("production-orders", requests[0].Route);
        Assert.Equal("production-orders/" + serverId.ToString("D") + "/release", requests[1].Route);
        var stored = await harness.OrderAsync(local.Id);
        Assert.Equal(serverId, stored.ServerId);
        Assert.Equal("MO-77", stored.OrderNumber);
        Assert.Equal(ProductionOrderStatus.Released, stored.Status);
        Assert.False(stored.HasPendingChanges);
    }

    /// <summary>
    /// The queue refuses mutations the local order cannot take, before anything is stored.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_InvalidMutations_AreRefused()
    {
        var planned = await harness.SeedOrderAsync(ProductionOrderStatus.Planned);
        var unsynced = await harness.SeedOrderAsync(ProductionOrderStatus.Released, synced: false);
        var other = await harness.SeedOrderAsync(ProductionOrderStatus.Released);

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Queue.QueueTransitionAsync(planned.Id, ManufacturingMutationKind.Start, CancellationToken.None));
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Queue.QueueConfirmationAsync(Confirm(unsynced, unsynced.Operations.First(), 1m, 0m), CancellationToken.None));
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Queue.QueueConfirmationAsync(Confirm(planned, planned.Operations.First(), 1m, 0m), CancellationToken.None));
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Queue.QueueConfirmationAsync(Confirm(other, planned.Operations.First(), 1m, 0m), CancellationToken.None));
        Assert.Empty(await harness.OutboxAsync());
    }

    private static OperationConfirmationInput Confirm(ProductionOrder order, ProductionOperation operation, decimal good, decimal scrap) =>
        new(order.Id, operation.Id, SetupMinutes: 5m, RunMinutes: 20m, LaborMinutes: 0m, DowntimeMinutes: 0m, good, scrap, scrap > 0m ? "burr" : null, IsFinal: false, new DateTime(2026, 11, 21));
}
