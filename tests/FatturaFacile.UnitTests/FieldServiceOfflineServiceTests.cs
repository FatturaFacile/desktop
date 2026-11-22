// <copyright file="FieldServiceOfflineServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.FieldService;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline-first writes of the technician over a real SQLite outbox: queueing while offline, ordered replay in
/// batches, the idempotent replay after a lost answer (same client identifier, reported as a duplicate), conflicts, back-off and
/// the cases where the session or the server fails.
/// </summary>
public sealed class FieldServiceOfflineServiceTests : IDisposable
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid Tenant = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly FieldServiceStore store;
    private readonly ManualTimeProvider clock = new();
    private readonly Mock<IFieldServiceApiClient> api = new();
    private readonly List<string> bodies = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceOfflineServiceTests"/> class.
    /// </summary>
    public FieldServiceOfflineServiceTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(Tenant, "Alpha", "CH", "CHF");
        store = new FieldServiceStore(db, tenant);
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
    }

    /// <summary>
    /// Without a session the operation is saved locally and nothing is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_WithoutSession_SavesLocallyAndSendsNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        Assert.Null(result.ErrorKey);
        Assert.False(result.Sent);
        Assert.False(result.Conflict);
        Assert.NotEmpty(result.ClientUuid);
        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(FieldServiceOperationType.PartConsumption, entry.Operation);
        Assert.Equal(FieldServiceTestData.OrderGuid, entry.WorkOrderId);
        Assert.Equal(result.ClientUuid, entry.ClientUuid);
        Assert.Contains(FieldServiceTestData.ProductId, entry.PayloadJson, StringComparison.Ordinal);
        api.Verify(a => a.SendAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any), Times.Never);
    }

    /// <summary>
    /// With a session the operation is sent at once in a batch, and the server answer marks it as sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_Online_SendsAtOnceAndMarksItSent()
    {
        Answer(_ => "applied");

        var result = await Create().QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        Assert.True(result.Sent);
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
        var body = Assert.Single(bodies);
        Assert.Contains(result.ClientUuid, body, StringComparison.Ordinal);
        Assert.Contains("part_consumption", body, StringComparison.Ordinal);
    }

    /// <summary>
    /// Operations queued offline are replayed together, in the order the technician acted, when the connection returns.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The array enumerator is a struct whose Dispose is a no-op.")]
    public async Task Flush_ReplaysQueuedOperationsInOrderInOneBatch()
    {
        var service = Create();
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        _ = await service.QueueStatusAsync(FieldServiceTestData.OrderGuid, "on_site", CancellationToken.None);
        clock.Advance(TimeSpan.FromMinutes(5));
        _ = await service.QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);
        clock.Advance(TimeSpan.FromMinutes(5));
        _ = await service.QueueReportAsync(FieldServiceTestData.ReportInput(), CancellationToken.None);
        Assert.Empty(bodies);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        Answer(_ => "applied");
        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(new FieldServiceFlushResult(3, 0, 0, Offline: false), flush);
        using var body = JsonDocument.Parse(Assert.Single(bodies));
        var types = body.RootElement.GetProperty("operations").EnumerateArray().Select(op => op.GetProperty("type").GetString()).ToList();
        Assert.Equal(["status_transition", "part_consumption", "service_report"], types);
    }

    /// <summary>
    /// A lost answer is harmless: the operation stays pending, is replayed later with the same client identifier and the server
    /// reports it as a duplicate.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterALostAnswer_ReplaysWithTheSameClientIdentifier()
    {
        var service = Create();
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any))
            .Callback((HttpMethod _, string _, string? body, CancellationToken _) => bodies.Add(body ?? string.Empty))
            .ThrowsAsync(new HttpRequestException("connection lost"));

        var queued = await service.QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        Assert.False(queued.Sent);
        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(1, entry.Attempts);
        Assert.Equal(clock.GetUtcNow().UtcDateTime + SyncRetryPolicy.NextDelay(1), entry.NextAttemptAt);
        Assert.Equal("connection lost", entry.LastError);

        // Still backing off: nothing is sent and the queue reports offline.
        var waiting = await service.FlushAsync(CancellationToken.None);
        Assert.True(waiting.Offline);
        Assert.Equal(1, waiting.Remaining);
        _ = Assert.Single(bodies);

        // The server did apply the first send: it answers duplicate to the replay.
        clock.Advance(TimeSpan.FromMinutes(1));
        api.Reset();
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        Answer(_ => "duplicate");
        var replay = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(new FieldServiceFlushResult(1, 0, 0, Offline: false), replay);
        Assert.Equal(queued.ClientUuid, ClientUuids(bodies[^1]).Single());
        Assert.Equal(ClientUuids(bodies[0]).Single(), ClientUuids(bodies[^1]).Single());
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
    }

    /// <summary>
    /// An operation the server rejects becomes a conflict with the field errors, and the user can dismiss it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_RejectedOperation_BecomesAConflictThatCanBeDismissed()
    {
        var service = Create();
        Answer(_ => "rejected");

        var result = await service.QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        Assert.True(result.Conflict);
        Assert.False(result.Sent);
        var summary = await service.SummaryAsync(CancellationToken.None);
        Assert.Equal(0, summary.Pending);
        var conflict = Assert.Single(summary.Conflicts);
        Assert.Equal(FieldServiceOperationType.PartConsumption, conflict.Operation);
        Assert.Contains("quantity", conflict.Error, StringComparison.Ordinal);
        Assert.Contains("Not enough stock", conflict.Error, StringComparison.Ordinal);

        await service.DismissConflictAsync(conflict.Id, CancellationToken.None);

        Assert.Empty((await service.SummaryAsync(CancellationToken.None)).Conflicts);
    }

    /// <summary>
    /// An operation missing from the server answer stays pending and is retried later.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_OperationMissingFromTheAnswer_StaysPending()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.OK, FieldServiceTestData.Envelope("{\"results\":[]}")));

        var result = await Create().QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        Assert.False(result.Sent);
        Assert.False(result.Conflict);
        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(1, entry.Attempts);
    }

    /// <summary>
    /// A refused session (401 or 403) leaves the operation pending without counting an attempt.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Flush_RefusedSession_KeepsTheOperationWithoutAnAttempt(HttpStatusCode status)
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any)).ReturnsAsync(new FieldServiceApiReply(status, "{}"));

        _ = await Create().QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(0, entry.Attempts);
    }

    /// <summary>
    /// A server error keeps the operation pending with a back-off, while a validation refusal of the whole batch is a conflict.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ServerErrorBacksOff_AndBatchRefusalIsAConflict()
    {
        var service = Create();
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any)).ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.InternalServerError, "oops"));

        _ = await service.QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(1, entry.Attempts);

        clock.Advance(TimeSpan.FromMinutes(1));
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.UnprocessableEntity, "{\"message\":\"The operations field is invalid.\"}"));
        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(1, flush.Conflicts);
        Assert.Contains("invalid", Assert.Single((await service.SummaryAsync(CancellationToken.None)).Conflicts).Error, StringComparison.Ordinal);
    }

    /// <summary>
    /// After too many failed attempts the operation stops retrying and is shown as a conflict.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterTooManyAttempts_BecomesAConflict()
    {
        var service = Create();
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        _ = await service.QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);

        for (var attempt = 1; attempt < SyncRetryPolicy.MaxAttempts; attempt++)
        {
            clock.Advance(TimeSpan.FromHours(2));
            _ = await service.FlushAsync(CancellationToken.None);
        }

        var summary = await service.SummaryAsync(CancellationToken.None);
        Assert.Equal(0, summary.Pending);
        Assert.Contains("Too many", Assert.Single(summary.Conflicts).Error, StringComparison.Ordinal);
    }

    /// <summary>
    /// More operations than the server accepts in one batch are sent in several batches, in order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_SplitsLargeQueuesIntoBatches()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        for (var i = 0; i < FieldServiceBatch.MaxOperations + 1; i++)
        {
            _ = await service.QueuePartAsync(FieldServiceTestData.Part(), CancellationToken.None);
            clock.Advance(TimeSpan.FromSeconds(1));
        }

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        Answer(_ => "applied");
        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(FieldServiceBatch.MaxOperations + 1, flush.Sent);
        Assert.Equal(2, bodies.Count);
        Assert.Equal(FieldServiceBatch.MaxOperations, ClientUuids(bodies[0]).Count);
        _ = Assert.Single(ClientUuids(bodies[1]));
    }

    /// <summary>
    /// An operation the server would reject is refused on the spot and never enters the outbox.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_InvalidOperation_IsRefusedAndNotStored()
    {
        var service = Create();

        var part = await service.QueuePartAsync(FieldServiceTestData.Part(0m), CancellationToken.None);
        var status = await service.QueueStatusAsync(FieldServiceTestData.OrderGuid, "completed", CancellationToken.None);
        var report = await service.QueueReportAsync(FieldServiceTestData.ReportInput() with { SignerName = " " }, CancellationToken.None);

        Assert.Equal(FieldServiceValidation.QuantityKey, part.ErrorKey);
        Assert.Equal(FieldServiceValidation.StatusKey, status.ErrorKey);
        Assert.Equal(FieldServiceValidation.SignerKey, report.ErrorKey);
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
        api.Verify(a => a.SendAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any), Times.Never);
    }

    /// <summary>
    /// An unsupported report language is replaced by Italian before the report is queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task QueueReport_UnsupportedLocale_FallsBackToItalian()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        _ = await Create().QueueReportAsync(FieldServiceTestData.ReportInput() with { Locale = "en-US" }, CancellationToken.None);

        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        using var payload = JsonDocument.Parse(entry.PayloadJson);
        Assert.Equal("it-IT", payload.RootElement.GetProperty("locale").GetString());
        Assert.Equal(clock.GetUtcNow().UtcDateTime, entry.OccurredAtUtc);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The array enumerator is a struct whose Dispose is a no-op.")]
    private static List<string> ClientUuids(string body)
    {
        using var document = JsonDocument.Parse(body);

        return [.. document.RootElement.GetProperty("operations").EnumerateArray().Select(op => op.GetProperty("client_uuid").GetString() ?? string.Empty)];
    }

    private FieldServiceOfflineService Create() => new(api.Object, store, clock);

    private void Answer(Func<string, string> statusOf)
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "mobile-sync", It.IsAny<string?>(), Any))
            .ReturnsAsync((HttpMethod _, string _, string? body, CancellationToken _) =>
            {
                bodies.Add(body ?? string.Empty);
                var results = ClientUuids(body ?? string.Empty).Select(uuid => (ClientUuid: uuid, Status: statusOf(uuid))).ToArray();

                return new FieldServiceApiReply(HttpStatusCode.OK, FieldServiceTestData.SyncAnswer(results));
            });
    }
}
