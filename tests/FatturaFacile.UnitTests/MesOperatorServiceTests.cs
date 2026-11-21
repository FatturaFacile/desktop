// <copyright file="MesOperatorServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline-first MES service: validation, the outbox, idempotency keys, refusals and conflicts, retries and the
/// assignment of the phase after a queued take-over.
/// </summary>
public sealed class MesOperatorServiceTests
{
    private const string BoardTemplate = """{"server_time":"__AT__","work_center":{"id":"__WC__","work_center_code":"WC-01","name":"Milling"},"phases":[{"id":"22222222-2222-2222-2222-222222222222","version":2,"status":"running","machine_mode":"attended","production_order":{"order_number":"PO-100","item_code":"ITM-1","item_name":"Bracket","target_quantity":"100"},"operation":{"id":"__OP__","sequence":10,"name":"Milling"},"open_interval":{"category":"run","started_at":"__AT__","machine_attended":true},"times":{"setup_seconds":0,"attended_run_seconds":0,"unattended_run_seconds":0,"downtime_seconds":0}}],"available":[]}""";

    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2026, 6, 30, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IMesApiClient> api = new();
    private readonly Mock<ICurrentTenantService> tenant = new();
    private readonly InMemoryMesStore store = new();
    private readonly List<(string Route, string Body)> sent = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MesOperatorServiceTests"/> class.
    /// </summary>
    public MesOperatorServiceTests()
    {
        _ = tenant.SetupGet(t => t.TenantId).Returns(Guid.NewGuid());
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .Callback<string, string, CancellationToken>((route, body, _) => sent.Add((route, body)))
            .ReturnsAsync(new MesApiReply(HttpStatusCode.OK, """{"data":{"id":"99999999-9999-9999-9999-999999999999"}}"""));
    }

    /// <summary>
    /// Scrap without a defect reason is refused locally and never reaches the outbox.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_ScrapWithoutDefectReason_IsRejectedBeforeTheOutbox()
    {
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid());
        var command = MesCommand.Declare(row, 5m, 1m, defect: null, Now.UtcDateTime);

        var result = await Create().ExecuteAsync(command, row, CancellationToken.None);

        Assert.Equal(MesExecuteStatus.Rejected, result.Status);
        Assert.Equal(MesOperatorService.DefectReasonKey, result.ErrorKey);
        Assert.Empty(store.Entries);
        Assert.Empty(sent);
    }

    /// <summary>
    /// An empty declaration, an action the state does not allow and a malformed or missing lot are refused locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_InvalidCommands_AreRejectedWithTheirKey()
    {
        var service = Create();
        var running = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid());
        var itemId = Guid.NewGuid();
        var withLot = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), components: [new MesComponent(itemId, "STEEL", "Steel", 10m, 0m, LotTracked: true)]);

        var empty = await service.ExecuteAsync(MesCommand.Declare(running, 0m, 0m, defect: null, Now.UtcDateTime), running, CancellationToken.None);
        var notAllowed = await service.ExecuteAsync(MesCommand.StartSetup(running, Now.UtcDateTime), running, CancellationToken.None);
        var missingLot = await service.ExecuteAsync(MesCommand.ConsumeMaterial(withLot, itemId, lotCode: null, 1m, overConsumptionConfirmed: false, Now.UtcDateTime), withLot, CancellationToken.None);
        var badLot = await service.ExecuteAsync(MesCommand.ConsumeMaterial(withLot, itemId, "no spaces/allowed here!", 1m, overConsumptionConfirmed: false, Now.UtcDateTime), withLot, CancellationToken.None);
        var noQuantity = await service.ExecuteAsync(MesCommand.ConsumeMaterial(withLot, itemId, "LOT-1", 0m, overConsumptionConfirmed: false, Now.UtcDateTime), withLot, CancellationToken.None);

        Assert.Equal(MesOperatorService.QuantityKey, empty.ErrorKey);
        Assert.Equal(MesOperatorService.NotAllowedKey, notAllowed.ErrorKey);
        Assert.Equal(MesOperatorService.LotCodeKey, missingLot.ErrorKey);
        Assert.Equal(MesOperatorService.LotCodeKey, badLot.ErrorKey);
        Assert.Equal(MesOperatorService.QuantityKey, noQuantity.ErrorKey);
        Assert.Empty(store.Entries);
    }

    /// <summary>
    /// Offline, the command is saved with its idempotency key and the instant the operator acted, and nothing is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_Offline_QueuesTheCommandWithItsTimestampAndKey()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, Guid.NewGuid());
        var acted = Now.UtcDateTime.AddMinutes(-7);
        var command = MesCommand.StartSetup(row, acted);

        var result = await Create().ExecuteAsync(command, row, CancellationToken.None);

        Assert.Equal(MesExecuteStatus.Queued, result.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(MesOutboxStatus.Pending, entry.Status);
        Assert.Equal(command.Id, entry.IdempotencyKey);
        Assert.Equal(acted, entry.OccurredAtUtc);
        Assert.Empty(sent);
    }

    /// <summary>
    /// Online, the command is sent to its route with the idempotency key and the real instant in the body, and marked sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_Online_SendsToTheRouteWithKeyAndOccurredAt()
    {
        var phaseId = Guid.NewGuid();
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, phaseId);
        var acted = Now.UtcDateTime.AddSeconds(-30);
        var command = MesCommand.StartProduction(row, MesMachineMode.Unattended, acted);

        var result = await Create().ExecuteAsync(command, row, CancellationToken.None);

        Assert.Equal(MesExecuteStatus.Sent, result.Status);
        var (route, body) = Assert.Single(sent);
        Assert.Equal($"manufacturing/mes/phases/{phaseId}/start-production", route);
        using var document = JsonDocument.Parse(body);
        Assert.Equal(command.Id.ToString("D"), document.RootElement.GetProperty("idempotency_key").GetString());
        Assert.Equal("unattended", document.RootElement.GetProperty("machine_mode").GetString());
        Assert.Equal(acted, document.RootElement.GetProperty("occurred_at").GetDateTime().ToUniversalTime());
        Assert.Equal(MesOutboxStatus.Sent, store.Entries.Single().Status);
    }

    /// <summary>
    /// A sent command is never sent again: flushing twice sends it once.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterSent_DoesNotSendAgain()
    {
        var service = Create();
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, Guid.NewGuid());
        _ = await service.ExecuteAsync(MesCommand.StartSetup(row, Now.UtcDateTime), row, CancellationToken.None);

        var again = await service.FlushAsync(CancellationToken.None);

        _ = Assert.Single(sent);
        Assert.Equal(0, again.Sent);
        Assert.Equal(0, again.Remaining);
    }

    /// <summary>
    /// A refusal becomes a conflict with the server's message, and later commands of the same operation are refused with it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_RefusedCommand_BlocksTheCommandsThatDependOnIt()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        var phaseId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, phaseId, operationId);
        var setup = MesCommand.StartSetup(row, Now.UtcDateTime.AddSeconds(-20));
        var production = MesCommand.StartProduction(row, MesMachineMode.Attended, Now.UtcDateTime.AddSeconds(-10));
        _ = await service.ExecuteAsync(setup, row, CancellationToken.None);
        _ = await service.ExecuteAsync(production, row, CancellationToken.None);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .Callback<string, string, CancellationToken>((route, body, _) => sent.Add((route, body)))
            .ReturnsAsync(new MesApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"Phase already closed."}"""));

        var result = await service.FlushAsync(CancellationToken.None);

        _ = Assert.Single(sent);
        Assert.Equal(2, result.Conflicts);
        Assert.All(store.Entries, entry => Assert.Equal(MesOutboxStatus.Conflict, entry.Status));
        Assert.Equal("Phase already closed.", store.Entries.Single(e => e.IdempotencyKey == setup.Id).LastError);
        Assert.Equal(MesOperatorService.BlockedKey, store.Entries.Single(e => e.IdempotencyKey == production.Id).LastError);
    }

    /// <summary>
    /// The first validation message of the server is the error text of a refused command.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_ValidationRefusal_ShowsTheFirstFieldError()
    {
        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .ReturnsAsync(new MesApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"The given data was invalid.","errors":{"lot_code":["Lot not found."]}}"""));
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid());
        var command = MesCommand.Declare(row, 3m, 0m, defect: null, Now.UtcDateTime);

        var result = await Create().ExecuteAsync(command, row, CancellationToken.None);

        Assert.Equal(MesExecuteStatus.Conflict, result.Status);
        Assert.Equal("Lot not found.", result.Detail);
        _ = Assert.Single(store.Conflicts);
    }

    /// <summary>
    /// A server error keeps the command pending with a retry time and an attempt count; a network failure does the same.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_TransientFailures_KeepTheCommandPendingForRetry()
    {
        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .ReturnsAsync(new MesApiReply(HttpStatusCode.ServiceUnavailable, string.Empty));
        var service = Create();
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, Guid.NewGuid());

        var first = await service.ExecuteAsync(MesCommand.StartSetup(row, Now.UtcDateTime), row, CancellationToken.None);

        Assert.Equal(MesExecuteStatus.Queued, first.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(MesOutboxStatus.Pending, entry.Status);
        Assert.Equal(1, entry.Attempts);
        Assert.True(entry.NextAttemptAt > Now.UtcDateTime);

        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any)).ThrowsAsync(new HttpRequestException("no route"));
        entry.NextAttemptAt = Now.UtcDateTime;

        var offline = await service.FlushAsync(CancellationToken.None);

        Assert.True(offline.Offline);
        Assert.Equal(2, entry.Attempts);
        Assert.Equal(MesOutboxStatus.Pending, entry.Status);
    }

    /// <summary>
    /// After too many attempts the command stops retrying and is shown as a conflict.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_TooManyAttempts_BecomesAConflict()
    {
        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .ReturnsAsync(new MesApiReply(HttpStatusCode.BadGateway, string.Empty));
        var service = Create();
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, Guid.NewGuid());
        _ = await service.ExecuteAsync(MesCommand.StartSetup(row, Now.UtcDateTime), row, CancellationToken.None);
        var entry = store.Entries.Single();

        for (var attempt = 1; attempt < 20 && entry.Status == MesOutboxStatus.Pending; attempt++)
        {
            entry.NextAttemptAt = Now.UtcDateTime;
            _ = await service.FlushAsync(CancellationToken.None);
        }

        Assert.Equal(MesOutboxStatus.Conflict, entry.Status);
        Assert.Equal(FatturaFacile.Application.Sync.SyncRetryPolicy.MaxAttempts, entry.Attempts);
    }

    /// <summary>
    /// A command queued behind an offline take-over waits for the take-over, then goes to the phase the server created.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_QueuedTakeOver_GivesItsPhaseToTheNextCommands()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        var operationId = Guid.NewGuid();
        var available = MesTestRows.Row(MesPhaseStatus.Available, operationId: operationId);
        var take = MesCommand.TakeOver(operationId, phaseId: null, Guid.NewGuid(), Now.UtcDateTime.AddSeconds(-30));
        _ = await service.ExecuteAsync(take, available, CancellationToken.None);
        var assigned = available with { Status = MesPhaseStatus.Assigned };
        _ = await service.ExecuteAsync(MesCommand.StartSetup(assigned, Now.UtcDateTime.AddSeconds(-20)), assigned, CancellationToken.None);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        var result = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(2, result.Sent);
        Assert.Equal("manufacturing/mes/phases/take-over", sent[0].Route);
        Assert.Equal("manufacturing/mes/phases/99999999-9999-9999-9999-999999999999/start-setup", sent[1].Route);
        using var document = JsonDocument.Parse(sent[0].Body);
        Assert.Equal(operationId.ToString("D"), document.RootElement.GetProperty("production_order_operation_id").GetString());
    }

    /// <summary>
    /// Commands are sent in the order the operator acted, whatever the order they were saved in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_SendsInTheOrderTheOperatorActed()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        var phaseId = Guid.NewGuid();
        var running = MesTestRows.Row(MesPhaseStatus.Running, phaseId);
        var suspended = running with { Status = MesPhaseStatus.Suspended };
        var resume = MesCommand.Resume(suspended, Now.UtcDateTime.AddSeconds(-10));
        var suspend = MesCommand.Suspend(running, MesSuspensionReason.Break, Now.UtcDateTime.AddSeconds(-60));
        _ = await service.ExecuteAsync(resume, suspended, CancellationToken.None);
        _ = await service.ExecuteAsync(suspend, running, CancellationToken.None);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = await service.FlushAsync(CancellationToken.None);

        Assert.EndsWith("/suspend", sent[0].Route, StringComparison.Ordinal);
        Assert.EndsWith("/resume", sent[1].Route, StringComparison.Ordinal);
    }

    /// <summary>
    /// Dismissing a conflict removes it from the list the operator sees.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DismissConflict_RemovesItFromTheConflicts()
    {
        _ = api.Setup(a => a.SendCommandAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .ReturnsAsync(new MesApiReply(HttpStatusCode.Conflict, """{"message":"Version conflict."}"""));
        var service = Create();
        var row = MesTestRows.Row(MesPhaseStatus.Assigned, Guid.NewGuid());
        var command = MesCommand.StartSetup(row, Now.UtcDateTime);
        _ = await service.ExecuteAsync(command, row, CancellationToken.None);
        _ = Assert.Single(store.Conflicts);

        await service.DismissConflictAsync(command.Id, CancellationToken.None);

        Assert.Empty(store.Conflicts);
        Assert.Equal(MesOutboxStatus.Dismissed, store.Entries.Single().Status);
    }

    /// <summary>
    /// Offline, the board comes from the cache, catches up to now and shows the effect of the commands still pending.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadBoard_Offline_UsesTheCacheAndProjectsPendingCommands()
    {
        var workCenterId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var json = BoardJson(workCenterId, operationId, Now.UtcDateTime.AddMinutes(-5));
        await store.SaveBoardAsync(workCenterId, json, Now.UtcDateTime.AddMinutes(-5), CancellationToken.None);
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var phaseId = new Guid(0x22222222, 0x2222, 0x2222, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22);
        var row = MesTestRows.Row(MesPhaseStatus.Running, phaseId, operationId);
        _ = await Create().ExecuteAsync(MesCommand.Suspend(row, MesSuspensionReason.Break, Now.UtcDateTime.AddMinutes(-2)), row, CancellationToken.None);

        var view = await Create().LoadBoardAsync(workCenterId, CancellationToken.None);

        Assert.NotNull(view);
        Assert.True(view.FromCache);
        Assert.Equal(1, view.PendingCount);
        var phase = Assert.Single(view.Rows);
        Assert.Equal(MesPhaseStatus.Suspended, phase.Status);
        Assert.Equal(180, phase.Times.AttendedRunSeconds);
        Assert.Equal(120, phase.Times.DowntimeSeconds);
        api.Verify(a => a.GetBoardAsync(It.IsAny<Guid>(), Any), Times.Never);
    }

    /// <summary>
    /// Without a cache and without a connection there is no board to show.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadBoard_OfflineWithoutCache_ReturnsNull()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        Assert.Null(await Create().LoadBoardAsync(Guid.NewGuid(), CancellationToken.None));
    }

    /// <summary>
    /// A board read from the server is cached for offline use and is not marked as cached.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadBoard_Online_SavesTheBoardForOfflineUse()
    {
        var workCenterId = Guid.NewGuid();
        var json = BoardJson(workCenterId, Guid.NewGuid(), Now.UtcDateTime);
        _ = api.Setup(a => a.GetBoardAsync(workCenterId, Any)).ReturnsAsync(json);

        var view = await Create().LoadBoardAsync(workCenterId, CancellationToken.None);

        Assert.NotNull(view);
        Assert.False(view.FromCache);
        Assert.Equal(Now.UtcDateTime, view.SyncedAtUtc);
        Assert.NotNull(await store.FindBoardAsync(workCenterId, CancellationToken.None));
    }

    /// <summary>
    /// A server answer that does not parse never replaces the good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadBoard_BadAnswer_KeepsTheCachedBoard()
    {
        var workCenterId = Guid.NewGuid();
        var good = BoardJson(workCenterId, Guid.NewGuid(), Now.UtcDateTime.AddMinutes(-1));
        await store.SaveBoardAsync(workCenterId, good, Now.UtcDateTime.AddMinutes(-1), CancellationToken.None);
        _ = api.Setup(a => a.GetBoardAsync(workCenterId, Any)).ReturnsAsync("""{"server_time":"2026-06-30T12:00:00+00:00"}""");

        var view = await Create().LoadBoardAsync(workCenterId, CancellationToken.None);

        Assert.NotNull(view);
        Assert.True(view.FromCache);
        Assert.Equal(good, (await store.FindBoardAsync(workCenterId, CancellationToken.None))?.BoardJson);
    }

    private static string BoardJson(Guid workCenterId, Guid operationId, DateTime serverTime) =>
        BoardTemplate
            .Replace("__AT__", serverTime.ToString("O", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("__WC__", workCenterId.ToString("D"), StringComparison.Ordinal)
            .Replace("__OP__", operationId.ToString("D"), StringComparison.Ordinal);

    private MesOperatorService Create() => new(api.Object, store, tenant.Object, new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InMemoryMesStore : IMesStore
    {
        private readonly Dictionary<Guid, LocalMesBoard> boards = [];

        public List<MesOutboxEntry> Entries { get; } = [];

        public IReadOnlyList<MesOutboxEntry> Conflicts => [.. Entries.Where(e => e.Status == MesOutboxStatus.Conflict)];

        public Task<LocalMesBoard?> FindBoardAsync(Guid workCenterId, CancellationToken cancellationToken) =>
            Task.FromResult(boards.GetValueOrDefault(workCenterId));

        public Task SaveBoardAsync(Guid workCenterId, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
        {
            boards[workCenterId] = new LocalMesBoard { WorkCenterId = workCenterId, BoardJson = json, SyncedAtUtc = syncedAtUtc };

            return Task.CompletedTask;
        }

        public Task EnqueueAsync(MesOutboxEntry entry, CancellationToken cancellationToken)
        {
            Entries.Add(entry);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MesOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MesOutboxEntry>>([.. Entries.Where(e => e.Status == MesOutboxStatus.Pending).OrderBy(e => e.OccurredAtUtc)]);

        public Task<IReadOnlyList<MesOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) => Task.FromResult(Conflicts);

        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task AssignPhaseAsync(Guid operationId, Guid phaseId, CancellationToken cancellationToken)
        {
            foreach (var entry in Entries.Where(e => e.OperationId == operationId && e.PhaseId is null && e.Status == MesOutboxStatus.Pending))
            {
                entry.PhaseId = phaseId;
            }

            return Task.CompletedTask;
        }
    }
}
