// <copyright file="ProcurementServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Features.Procurement;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline-first purchase order fulfilment service (Mod 046): validation, the outbox, idempotent replay, ordered flush,
/// retries, refusals, the cache fallback of the reads and the local pricing estimate.
/// </summary>
public sealed class ProcurementServiceTests
{
    private const string OrdersJson = """{"data":[{"id":"11111111-1111-1111-1111-111111111111","po_number":"PO-1","status":"sent","lines":[]}]}""";

    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid OrderId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LineId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DropId = new("88888888-8888-8888-8888-888888888888");

    private readonly Mock<IProcurementApiClient> api = new();
    private readonly Mock<ICurrentTenantService> tenant = new();
    private readonly InMemoryProcurementStore store = new();
    private readonly ManualTimeProvider time = new();
    private readonly List<(string Method, string Route, string? Body)> sent = [];
    private bool online = true;
    private ProcurementApiReply reply = new(HttpStatusCode.Created, """{"data":{}}""");

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcurementServiceTests"/> class.
    /// </summary>
    public ProcurementServiceTests()
    {
        _ = tenant.SetupGet(t => t.TenantId).Returns(Guid.NewGuid());
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(() => online);
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), Any))
            .Callback<string, string, string?, CancellationToken>((method, route, body, _) => sent.Add((method, route, body)))
            .ReturnsAsync(() => reply);
    }

    /// <summary>
    /// An email dispatch needs a recipient with an address, and an unknown channel is refused; nothing reaches the outbox.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_InvalidRequests_AreRejectedBeforeTheOutbox()
    {
        var service = Create();

        Assert.Equal(ProcurementService.RecipientKey, (await service.DispatchOrderAsync(Dispatch("email", null), CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.RecipientKey, (await service.DispatchOrderAsync(Dispatch("email", "no-address"), CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.ChannelKey, (await service.DispatchOrderAsync(Dispatch("fax", null), CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.TargetKey, (await service.DispatchOrderAsync(Dispatch("pdf", null) with { OrderId = Guid.Empty }, CancellationToken.None)).ErrorKey);
        Assert.Empty(store.Entries);
        Assert.Empty(sent);
    }

    /// <summary>
    /// A valid dispatch goes to the server at once with its channel and idempotency key.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_Online_IsSentWithTheIdempotencyKey()
    {
        var command = Dispatch("email", " buyer@example.ch ");

        var result = await Create().DispatchOrderAsync(command, CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Sent, result.Status);
        var call = Assert.Single(sent);
        Assert.Equal("POST", call.Method);
        Assert.Equal("procurement/purchase-orders/" + OrderId + "/dispatch", call.Route);
        using var body = JsonDocument.Parse(call.Body!);
        Assert.Equal("email", body.RootElement.GetProperty("channel").GetString());
        Assert.Equal("buyer@example.ch", body.RootElement.GetProperty("recipient").GetString());
        Assert.Equal(command.CommandId.ToString("D"), body.RootElement.GetProperty("idempotency_key").GetString());
        Assert.Equal(ProcurementOutboxStatus.Sent, Assert.Single(store.Entries).Status);
    }

    /// <summary>
    /// A supplier confirmation sends its number, dates and only the lines the user filled in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_Valid_SendsNumberDatesAndLines()
    {
        var request = Confirmation("AB-7") with
        {
            ConfirmedAt = new DateOnly(2027, 3, 2),
            ConfirmedDeliveryDate = new DateOnly(2027, 3, 25),
            Lines = [new ConfirmationLineRequest(LineId, 8m, 101.5m, new DateOnly(2027, 3, 26), "Short stock")],
        };

        var result = await Create().RecordConfirmationAsync(request, CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Sent, result.Status);
        var call = Assert.Single(sent);
        Assert.Equal("procurement/purchase-orders/" + OrderId + "/confirmations", call.Route);
        using var body = JsonDocument.Parse(call.Body!);
        var root = body.RootElement;
        Assert.Equal("AB-7", root.GetProperty("confirmation_number").GetString());
        Assert.Equal("2027-03-02", root.GetProperty("confirmed_at").GetString());
        Assert.Equal("2027-03-25", root.GetProperty("confirmed_delivery_date").GetString());
        var line = Assert.Single(root.GetProperty("lines").EnumerateArray());
        Assert.Equal(LineId.ToString("D"), line.GetProperty("purchase_order_line_id").GetString());
        Assert.Equal("8", line.GetProperty("confirmed_quantity").GetString());
        Assert.Equal("101.5", line.GetProperty("confirmed_unit_price").GetString());
        Assert.Equal("2027-03-26", line.GetProperty("confirmed_delivery_date").GetString());
        Assert.Equal("Short stock", line.GetProperty("variance_reason").GetString());
    }

    /// <summary>
    /// A confirmation without a number, with a duplicated line or a negative quantity is refused locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_InvalidRequests_AreRejected()
    {
        var service = Create();
        var line = new ConfirmationLineRequest(LineId, 1m, 1m, null, null);

        Assert.Equal(ProcurementService.NumberKey, (await service.RecordConfirmationAsync(Confirmation("  "), CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.NumberKey, (await service.RecordConfirmationAsync(Confirmation(new string('x', 81)), CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.LinesKey, (await service.RecordConfirmationAsync(Confirmation("A") with { Lines = [line, line] }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.LinesKey, (await service.RecordConfirmationAsync(Confirmation("A") with { Lines = [line with { ConfirmedQuantity = -1m }] }, CancellationToken.None)).ErrorKey);
        Assert.Empty(sent);
    }

    /// <summary>
    /// A reminder written offline waits in the outbox and is sent in order once the server is reachable again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Offline_Writes_AreQueuedAndFlushedInOrder()
    {
        online = false;
        var service = Create();

        var dispatch = await service.DispatchOrderAsync(Dispatch("pdf", null), CancellationToken.None);
        var reminder = await service.SendReminderAsync(new OrderReminderRequest(Guid.NewGuid(), OrderId, "PO-1", time.GetUtcNow().UtcDateTime.AddMinutes(1)), CancellationToken.None);
        var queue = await service.LoadQueueAsync(CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Queued, dispatch.Status);
        Assert.Equal(ProcurementActionStatus.Queued, reminder.Status);
        Assert.Empty(sent);
        Assert.Equal([ProcurementAction.DispatchOrder, ProcurementAction.SendReminder], queue.Pending.Select(p => p.Action));

        online = true;
        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(2, flush.Sent);
        Assert.Equal(0, flush.Remaining);
        Assert.False(flush.Offline);
        Assert.EndsWith("/dispatch", sent[0].Route, StringComparison.Ordinal);
        Assert.EndsWith("/reminders", sent[1].Route, StringComparison.Ordinal);
    }

    /// <summary>
    /// The same command id is queued once: a double click never sends twice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SameCommandId_IsQueuedOnce()
    {
        online = false;
        var service = Create();
        var command = Dispatch("pdf", null);

        _ = await service.DispatchOrderAsync(command, CancellationToken.None);
        _ = await service.DispatchOrderAsync(command, CancellationToken.None);

        _ = Assert.Single(store.Entries);
    }

    /// <summary>
    /// A server error that is not transient becomes a conflict with the message of the server; dismissing it removes it from the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refused_ByTheServer_BecomesAConflictThatCanBeDismissed()
    {
        reply = new ProcurementApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"Order not overdue"}""");
        var service = Create();

        var result = await service.SendReminderAsync(new OrderReminderRequest(Guid.NewGuid(), OrderId, "PO-1", time.GetUtcNow().UtcDateTime), CancellationToken.None);
        var queue = await service.LoadQueueAsync(CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Conflict, result.Status);
        Assert.Equal("Order not overdue", result.Detail);
        var conflict = Assert.Single(queue.Conflicts);
        Assert.Equal("Order not overdue", conflict.Error);

        await service.DismissConflictAsync(conflict.Id, CancellationToken.None);

        Assert.Empty((await service.LoadQueueAsync(CancellationToken.None)).Conflicts);
    }

    /// <summary>
    /// A network failure keeps the write pending and counts the attempt.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task NetworkFailure_KeepsTheWritePending()
    {
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), Any)).ThrowsAsync(new HttpRequestException("down"));

        var result = await Create().DispatchOrderAsync(Dispatch("pdf", null), CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Queued, result.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(ProcurementOutboxStatus.Pending, entry.Status);
        Assert.Equal(1, entry.Attempts);
        Assert.Equal("down", entry.LastError);
    }

    /// <summary>
    /// A dropshipment needs a complete delivery address; the country code is sent in upper case.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateDropshipment_ChecksTheAddressAndSendsTheLines()
    {
        var service = Create();
        var address = new DropshipAddress("Bianchi SA", "Via Roma", "5", "6900", "Lugano", null, "ch", null, null, null);
        var request = new CreateDropshipmentRequest(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid(), null, null, null, address, [new DropshipLineRequest(Guid.NewGuid(), 7.5m, "BOX", [10m, 5m])], time.GetUtcNow().UtcDateTime);

        Assert.Equal(ProcurementService.AddressKey, (await service.CreateDropshipmentAsync(request with { Address = address with { City = " " } }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.AddressKey, (await service.CreateDropshipmentAsync(request with { Address = address with { CountryCode = "CHE" } }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ProcurementService.TargetKey, (await service.CreateDropshipmentAsync(request with { SupplierId = Guid.Empty }, CancellationToken.None)).ErrorKey);
        Assert.Empty(sent);

        var result = await service.CreateDropshipmentAsync(request, CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Sent, result.Status);
        var call = Assert.Single(sent);
        Assert.Equal("procurement/dropshipments", call.Route);
        using var body = JsonDocument.Parse(call.Body!);
        Assert.Equal("CH", body.RootElement.GetProperty("delivery_address").GetProperty("country_code").GetString());
        var line = Assert.Single(body.RootElement.GetProperty("lines").EnumerateArray());
        Assert.Equal("BOX", line.GetProperty("uom").GetString());
        Assert.Equal(2, line.GetProperty("discount_cascade").GetArrayLength());
    }

    /// <summary>
    /// The transport document is issued through its route; a package count below one is refused.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task IssueDdt_SendsTheCarrierDataToTheDropshipment()
    {
        var service = Create();
        var request = new IssueDdtRequest(Guid.NewGuid(), DropId, "Bianchi SA", "DHL", "TRK1", 3, 12.5m, "supplier", new DateOnly(2027, 3, 10), time.GetUtcNow().UtcDateTime);

        Assert.Equal(ProcurementService.QuantityKey, (await service.IssueDdtAsync(request with { Packages = 0 }, CancellationToken.None)).ErrorKey);

        var result = await service.IssueDdtAsync(request, CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Sent, result.Status);
        var call = Assert.Single(sent);
        Assert.Equal("procurement/dropshipments/" + DropId + "/ddt", call.Route);
        using var body = JsonDocument.Parse(call.Body!);
        Assert.Equal("DHL", body.RootElement.GetProperty("carrier").GetString());
        Assert.Equal(3, body.RootElement.GetProperty("packages").GetInt32());
        Assert.Equal("12.5", body.RootElement.GetProperty("gross_weight").GetString());
        Assert.Equal("2027-03-10", body.RootElement.GetProperty("shipment_date").GetString());
    }

    /// <summary>
    /// A delivery notice sends the delivered quantity of each line; a quantity that is not positive is refused.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RecordDelivery_SendsTheLinesAndTheProof()
    {
        var service = Create();
        var request = new RecordDeliveryRequest(Guid.NewGuid(), DropId, "Bianchi SA", new DateOnly(2027, 3, 12), "POD-1", "S-1", [new DeliveryLineRequest(LineId, 6m)], time.GetUtcNow().UtcDateTime);

        Assert.Equal(ProcurementService.QuantityKey, (await service.RecordDeliveryAsync(request with { Lines = [new DeliveryLineRequest(LineId, 0m)] }, CancellationToken.None)).ErrorKey);

        var result = await service.RecordDeliveryAsync(request, CancellationToken.None);

        Assert.Equal(ProcurementActionStatus.Sent, result.Status);
        var call = Assert.Single(sent);
        Assert.Equal("procurement/dropshipments/" + DropId + "/deliveries", call.Route);
        using var body = JsonDocument.Parse(call.Body!);
        Assert.Equal("POD-1", body.RootElement.GetProperty("delivery_proof_reference").GetString());
        Assert.Equal("6", Assert.Single(body.RootElement.GetProperty("lines").EnumerateArray()).GetProperty("quantity_delivered").GetString());
    }

    /// <summary>
    /// A read goes to the server and caches the answer; offline it falls back to the cache, and without a cache it answers null.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reads_FallBackToTheCache()
    {
        var service = Create();
        _ = api.Setup(a => a.GetAsync(ProcurementRoutes.OrderList(null), Any)).ReturnsAsync(OrdersJson);

        var live = await service.LoadOrdersAsync(null, CancellationToken.None);
        online = false;
        var cached = await service.LoadOrdersAsync(null, CancellationToken.None);
        var missing = await service.LoadOrdersAsync("closed", CancellationToken.None);

        Assert.False(live!.FromCache);
        Assert.Equal("PO-1", Assert.Single(live.Data).Number);
        Assert.True(cached!.FromCache);
        Assert.Equal(live.SyncedAtUtc, cached.SyncedAtUtc);
        Assert.Null(missing);
    }

    /// <summary>
    /// An answer that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reads_BadAnswer_KeepsTheGoodCache()
    {
        var service = Create();
        _ = api.SetupSequence(a => a.GetAsync(ProcurementRoutes.Overdue, Any)).ReturnsAsync("""{"data":[]}""").ReturnsAsync("<html>");

        _ = await service.LoadOverdueAsync(CancellationToken.None);
        var second = await service.LoadOverdueAsync(CancellationToken.None);

        Assert.True(second!.FromCache);
        Assert.Equal("""{"data":[]}""", store.Cache[ProcurementRoutes.Overdue].Json);
    }

    /// <summary>
    /// The pricing preview comes from the server when it can be reached and is a local estimate otherwise.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task PreviewPricing_UsesTheServerThenTheLocalEstimate()
    {
        var service = Create();
        var request = new PricingPreviewRequest(10m, 100m, [10m], 12m, null, null);
        reply = new ProcurementApiReply(HttpStatusCode.OK, """{"data":{"discount_cascade":["10.00"],"effective_discount_percent":"10.00","net_unit_price":"90.0000","line_net_amount":"900.00","line_tax_amount":"0.00","line_total_amount":"900.00","quantity_base":"120.0000","net_price_per_base_unit":"7.5000","line_net_amount_company_currency":"900.00"}}""");

        var server = await service.PreviewPricingAsync(request, CancellationToken.None);
        online = false;
        var local = await service.PreviewPricingAsync(request, CancellationToken.None);

        Assert.False(server.FromCache);
        Assert.Equal("procurement/purchase-orders/pricing-preview", Assert.Single(sent).Route);
        Assert.Equal(900m, server.Data.LineNetAmount);
        Assert.True(local.FromCache);
        Assert.Equal(900m, local.Data.LineNetAmount);
        Assert.Equal(120m, local.Data.QuantityBase);
    }

    /// <summary>
    /// A handler hands its request to the service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Handler_DelegatesToTheService()
    {
        var service = new Mock<IProcurementService>();
        var request = Dispatch("pdf", null);
        var expected = new ProcurementActionResult(ProcurementActionStatus.Queued);
        _ = service.Setup(s => s.DispatchOrderAsync(request, Any)).ReturnsAsync(expected);

        var result = await new DispatchPurchaseOrderCommandHandler(service.Object).Handle(new DispatchPurchaseOrderCommand(request), CancellationToken.None);

        Assert.Same(expected, result);
    }

    private static DispatchOrderRequest Dispatch(string channel, string? recipient) =>
        new(Guid.NewGuid(), OrderId, "PO-1", channel, recipient, new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc));

    private static SupplierConfirmationRequest Confirmation(string number) =>
        new(Guid.NewGuid(), OrderId, "PO-1", number, null, null, null, [], new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc));

    private ProcurementService Create() => new(api.Object, store, tenant.Object, time);

    private sealed class InMemoryProcurementStore : IProcurementStore
    {
        public Dictionary<string, LocalProcurementCache> Cache { get; } = new(StringComparer.Ordinal);

        public List<ProcurementOutboxEntry> Entries { get; } = [];

        public Task<LocalProcurementCache?> FindCacheAsync(string key, CancellationToken cancellationToken) =>
            Task.FromResult(Cache.GetValueOrDefault(key));

        public Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
        {
            Cache[key] = new LocalProcurementCache { CacheKey = key, Json = json, SyncedAtUtc = syncedAtUtc };

            return Task.CompletedTask;
        }

        public Task TrimCacheAsync(string prefix, int keep, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ProcurementOutboxEntry> EnqueueAsync(ProcurementOutboxEntry entry, CancellationToken cancellationToken)
        {
            var existing = Entries.FirstOrDefault(candidate => candidate.IdempotencyKey == entry.IdempotencyKey);
            if (existing is not null)
            {
                return Task.FromResult(existing);
            }

            Entries.Add(entry);

            return Task.FromResult(entry);
        }

        public Task<IReadOnlyList<ProcurementOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProcurementOutboxEntry>>([.. Entries.Where(e => e.Status == ProcurementOutboxStatus.Pending).OrderBy(e => e.OccurredAtUtc)]);

        public Task<IReadOnlyList<ProcurementOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProcurementOutboxEntry>>([.. Entries.Where(e => e.Status == ProcurementOutboxStatus.Conflict)]);

        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
