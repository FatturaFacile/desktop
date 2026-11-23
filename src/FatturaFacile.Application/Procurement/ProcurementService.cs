// <copyright file="ProcurementService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Offline-first purchase order fulfilment (Mod 046): reads go to the server and fall back to the last cached answer; writes are
/// saved in the outbox first and replayed in order when the server is reachable. Every write carries its command id as idempotency
/// key, the outbox accepts a command id only once, so a command can never be queued twice.
/// </summary>
/// <param name="api">The procurement endpoints.</param>
/// <param name="store">The local cache and outbox.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class ProcurementService(IProcurementApiClient api, IProcurementStore store, ICurrentTenantService tenant, TimeProvider time) : IProcurementService
{
    /// <summary>
    /// The localization key of a missing order, dropshipment, sales order or supplier.
    /// </summary>
    public const string TargetKey = "Procurement_Err_Target";

    /// <summary>
    /// The localization key of a confirmation without its number, or with one that is too long.
    /// </summary>
    public const string NumberKey = "Procurement_Err_Number";

    /// <summary>
    /// The localization key of a line with a negative or missing value, or one that does not belong to the request.
    /// </summary>
    public const string LinesKey = "Procurement_Err_Lines";

    /// <summary>
    /// The localization key of a dispatch channel that does not exist.
    /// </summary>
    public const string ChannelKey = "Procurement_Err_Channel";

    /// <summary>
    /// The localization key of a missing or invalid recipient.
    /// </summary>
    public const string RecipientKey = "Procurement_Err_Recipient";

    /// <summary>
    /// The localization key of a text that is too long.
    /// </summary>
    public const string TextKey = "Procurement_Err_Text";

    /// <summary>
    /// The localization key of an incomplete delivery address.
    /// </summary>
    public const string AddressKey = "Procurement_Err_Address";

    /// <summary>
    /// The localization key of a quantity, weight or package count that is not positive.
    /// </summary>
    public const string QuantityKey = "Procurement_Err_Quantity";

    /// <summary>
    /// The longest free text in characters.
    /// </summary>
    public const int MaxText = 2000;

    /// <summary>
    /// The longest confirmation number in characters.
    /// </summary>
    public const int MaxNumber = 80;

    private const int MaxReference = 200;
    private const int MaxRecipient = 190;
    private const int CountryLength = 2;
    private const int CurrencyLength = 3;
    private const int ListKeep = 20;
    private const string Post = "POST";

    private enum SendOutcome
    {
        Sent = 0,
        Retry = 1,
        Refused = 2,
    }

    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<OverdueOrderView>>?> LoadOverdueAsync(CancellationToken cancellationToken) =>
        ReadAsync(ProcurementRoutes.Overdue, ProcurementParser.ParseOverdue, cancellationToken);

    /// <inheritdoc/>
    public async Task<ProcurementLoad<IReadOnlyList<PurchaseOrderView>>?> LoadOrdersAsync(string? status, CancellationToken cancellationToken)
    {
        var load = await ReadAsync(ProcurementRoutes.OrderList(status), ProcurementParser.ParseOrders, cancellationToken).ConfigureAwait(false);
        await TrimIfFreshAsync(load, ProcurementRoutes.OrdersPrefix, cancellationToken).ConfigureAwait(false);

        return load;
    }

    /// <inheritdoc/>
    public Task<ProcurementLoad<PurchaseOrderView>?> LoadOrderAsync(Guid id, CancellationToken cancellationToken) =>
        ReadAsync(ProcurementRoutes.Order(id), ProcurementParser.ParseOrder, cancellationToken);

    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<SupplierConfirmationView>>?> LoadConfirmationsAsync(Guid orderId, CancellationToken cancellationToken) =>
        ReadAsync(ProcurementRoutes.Confirmations(orderId), ProcurementParser.ParseConfirmations, cancellationToken);

    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<DispatchView>>?> LoadDispatchesAsync(Guid orderId, CancellationToken cancellationToken) =>
        ReadAsync(ProcurementRoutes.Dispatches(orderId), ProcurementParser.ParseDispatches, cancellationToken);

    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<ReminderView>>?> LoadRemindersAsync(Guid orderId, CancellationToken cancellationToken) =>
        ReadAsync(ProcurementRoutes.Reminders(orderId), ProcurementParser.ParseReminders, cancellationToken);

    /// <inheritdoc/>
    public async Task<ProcurementLoad<IReadOnlyList<DropshipmentView>>?> LoadDropshipmentsAsync(string? status, CancellationToken cancellationToken)
    {
        var load = await ReadAsync(ProcurementRoutes.DropshipList(status), ProcurementParser.ParseDropships, cancellationToken).ConfigureAwait(false);
        await TrimIfFreshAsync(load, ProcurementRoutes.DropshipsPrefix, cancellationToken).ConfigureAwait(false);

        return load;
    }

    /// <inheritdoc/>
    public Task<ProcurementLoad<DropshipmentView>?> LoadDropshipmentAsync(Guid id, CancellationToken cancellationToken) =>
        ReadAsync(ProcurementRoutes.Dropship(id), ProcurementParser.ParseDropship, cancellationToken);

    /// <inheritdoc/>
    public async Task<ProcurementLoad<PricingPreview>> PreviewPricingAsync(PricingPreviewRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (api.IsAuthenticated)
        {
            try
            {
                var body = new JsonObject
                {
                    ["quantity"] = Amount(request.Quantity),
                    ["unit_price"] = Amount(request.UnitPrice),
                    ["discount_cascade"] = new JsonArray([.. PurchasePricingCalculator.NormalizeCascade(request.DiscountCascade).Select(step => (JsonNode)JsonValue.Create(step))]),
                };
                PutNumber(body, "uom_conversion_factor", request.UomConversionFactor);
                PutNumber(body, "exchange_rate", request.ExchangeRate);
                PutNumber(body, "tax_rate", request.TaxRate);

                var reply = await api.SendAsync(Post, ProcurementRoutes.PricingPreview, body.ToJsonString(), cancellationToken).ConfigureAwait(false);
                if (reply.IsSuccess)
                {
                    return new ProcurementLoad<PricingPreview>(ProcurementParser.ParsePricing(reply.Body), FromCache: false, Now());
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                // Offline or a bad answer: the local estimate below.
            }
        }

        return new ProcurementLoad<PricingPreview>(PurchasePricingCalculator.Compute(request), FromCache: true, Now());
    }

    /// <inheritdoc/>
    public Task<ProcurementActionResult> DispatchOrderAsync(DispatchOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recipient = request.Recipient?.Trim();
        var invalid = Target(request.OrderId) ?? ValidateChannel(request.Channel, recipient);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = new() { ["channel"] = request.Channel };
        PutText(body, "recipient", recipient);

        return QueueAsync(NewEntry(ProcurementAction.DispatchOrder, ProcurementRoutes.Dispatch(request.OrderId), request.CommandId, request.OrderId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ProcurementActionResult> RecordConfirmationAsync(SupplierConfirmationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var number = (request.ConfirmationNumber ?? string.Empty).Trim();
        var invalid = Target(request.OrderId)
            ?? (number.Length is 0 or > MaxNumber ? NumberKey : null)
            ?? (TooLong(request.Notes) ? TextKey : null)
            ?? ValidateConfirmationLines(request.Lines);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = new() { ["confirmation_number"] = number };
        PutDay(body, "confirmed_at", request.ConfirmedAt);
        PutDay(body, "confirmed_delivery_date", request.ConfirmedDeliveryDate);
        PutText(body, "notes", request.Notes);
        if (request.Lines.Count > 0)
        {
            JsonArray lines = [];
            foreach (var line in request.Lines)
            {
                JsonObject row = new() { ["purchase_order_line_id"] = Id(line.PurchaseOrderLineId) };
                PutNumber(row, "confirmed_quantity", line.ConfirmedQuantity);
                PutNumber(row, "confirmed_unit_price", line.ConfirmedUnitPrice);
                PutDay(row, "confirmed_delivery_date", line.ConfirmedDeliveryDate);
                PutText(row, "variance_reason", line.VarianceReason);
                lines.Add(row);
            }

            body["lines"] = lines;
        }

        return QueueAsync(NewEntry(ProcurementAction.RecordConfirmation, ProcurementRoutes.Confirmations(request.OrderId), request.CommandId, request.OrderId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ProcurementActionResult> SendReminderAsync(OrderReminderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Target(request.OrderId) is { } invalid
            ? Rejected(invalid)
            : QueueAsync(NewEntry(ProcurementAction.SendReminder, ProcurementRoutes.Reminders(request.OrderId), request.CommandId, request.OrderId, request.Reference, [], request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ProcurementActionResult> CreateDropshipmentAsync(CreateDropshipmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var address = request.Address;
        var invalid = Target(request.SalesOrderId) ?? Target(request.SupplierId) ?? ValidateAddress(address) ?? ValidateDropship(request);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject addressBody = new()
        {
            ["street"] = address.Street.Trim(),
            ["postal_code"] = address.PostalCode.Trim(),
            ["city"] = address.City.Trim(),
            ["country_code"] = address.CountryCode.Trim().ToUpperInvariant(),
        };
        PutText(addressBody, "name", address.Name);
        PutText(addressBody, "house_number", address.HouseNumber);
        PutText(addressBody, "region", address.Region);
        PutText(addressBody, "phone", address.Phone);
        PutText(addressBody, "email", address.Email);
        PutText(addressBody, "delivery_notes", address.DeliveryNotes);

        JsonObject body = new() { ["sales_order_id"] = Id(request.SalesOrderId), ["supplier_id"] = Id(request.SupplierId), ["delivery_address"] = addressBody };
        PutDay(body, "expected_delivery_date", request.ExpectedDeliveryDate);
        PutText(body, "currency", request.Currency?.ToUpperInvariant());
        PutText(body, "notes", request.Notes);
        if (request.Lines.Count > 0)
        {
            JsonArray lines = [];
            foreach (var line in request.Lines)
            {
                JsonObject row = new() { ["sales_order_item_id"] = Id(line.SalesOrderItemId) };
                PutNumber(row, "unit_price", line.UnitPrice);
                PutText(row, "uom", line.Uom);
                if (line.DiscountCascade.Count > 0)
                {
                    row["discount_cascade"] = new JsonArray([.. line.DiscountCascade.Select(step => (JsonNode)JsonValue.Create(step))]);
                }

                lines.Add(row);
            }

            body["lines"] = lines;
        }

        return QueueAsync(NewEntry(ProcurementAction.CreateDropshipment, ProcurementRoutes.Dropships, request.CommandId, request.SalesOrderId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ProcurementActionResult> IssueDdtAsync(IssueDdtRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = Target(request.DropshipmentId) ?? ValidateDdt(request);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = [];
        PutText(body, "carrier", request.Carrier);
        PutText(body, "tracking_number", request.TrackingNumber);
        PutText(body, "transport_by", request.TransportBy);
        PutDay(body, "shipment_date", request.ShipmentDate);
        if (request.Packages is { } packages)
        {
            body["packages"] = packages;
        }

        PutNumber(body, "gross_weight", request.GrossWeight);

        return QueueAsync(NewEntry(ProcurementAction.IssueDropshipDdt, ProcurementRoutes.DropshipDdt(request.DropshipmentId), request.CommandId, request.DropshipmentId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ProcurementActionResult> RecordDeliveryAsync(RecordDeliveryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = Target(request.DropshipmentId)
            ?? (request.Lines.Any(line => line.PurchaseOrderLineId == Guid.Empty || line.QuantityDelivered <= 0m) ? QuantityKey : null)
            ?? (TooLong(request.DeliveryProofReference) || TooLong(request.SupplierShipmentReference) ? TextKey : null);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = [];
        PutDay(body, "delivered_at", request.DeliveredAt);
        PutText(body, "delivery_proof_reference", request.DeliveryProofReference);
        PutText(body, "supplier_shipment_reference", request.SupplierShipmentReference);
        if (request.Lines.Count > 0)
        {
            JsonArray lines = [];
            foreach (var line in request.Lines)
            {
                lines.Add(new JsonObject { ["purchase_order_line_id"] = Id(line.PurchaseOrderLineId), ["quantity_delivered"] = Amount(line.QuantityDelivered) });
            }

            body["lines"] = lines;
        }

        return QueueAsync(NewEntry(ProcurementAction.RecordDropshipDelivery, ProcurementRoutes.DropshipDeliveries(request.DropshipmentId), request.CommandId, request.DropshipmentId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcurementFlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        var entries = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return new ProcurementFlushResult(0, 0, 0, Offline: false);
        }

        if (!api.IsAuthenticated)
        {
            return new ProcurementFlushResult(0, 0, entries.Count, Offline: true);
        }

        var sent = 0;
        var conflicts = 0;
        var offline = false;
        foreach (var entry in entries)
        {
            if (entry.NextAttemptAt > Now())
            {
                offline = true;
                break;
            }

            var outcome = await SendAsync(entry, cancellationToken).ConfigureAwait(false);
            await store.SaveAsync(cancellationToken).ConfigureAwait(false);
            if (outcome == SendOutcome.Sent)
            {
                sent++;
            }
            else if (outcome == SendOutcome.Refused)
            {
                conflicts++;
            }
            else
            {
                // The network is most likely down: keep the order of the remaining entries.
                offline = true;
                break;
            }
        }

        var remaining = (await store.PendingAsync(cancellationToken).ConfigureAwait(false)).Count;

        return new ProcurementFlushResult(sent, conflicts, remaining, offline);
    }

    /// <inheritdoc/>
    public async Task<ProcurementQueue> LoadQueueAsync(CancellationToken cancellationToken)
    {
        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);

        return new ProcurementQueue([.. pending.Select(ToPending)], [.. conflicts.Select(ToPending)]);
    }

    /// <inheritdoc/>
    public async Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken)
    {
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);
        var entry = conflicts.FirstOrDefault(candidate => candidate.Id == entryId);
        if (entry is null)
        {
            return;
        }

        entry.Status = ProcurementOutboxStatus.Dismissed;
        entry.UpdatedAt = Now();
        await store.SaveAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Task<ProcurementActionResult> Rejected(string key) =>
        Task.FromResult(new ProcurementActionResult(ProcurementActionStatus.Rejected, key));

    private static string? Target(Guid id) => id == Guid.Empty ? TargetKey : null;

    private static bool TooLong(string? text) => text is { Length: > MaxText };

    private static string? ValidateChannel(string channel, string? recipient)
    {
        if (!ProcurementCodes.Channels.Contains(channel, StringComparer.Ordinal))
        {
            return ChannelKey;
        }

        var needsAddress = string.Equals(channel, ProcurementCodes.ChannelEmail, StringComparison.Ordinal);
        if (needsAddress && (string.IsNullOrEmpty(recipient) || !recipient.Contains('@', StringComparison.Ordinal)))
        {
            return RecipientKey;
        }

        return recipient is { Length: > MaxRecipient } ? RecipientKey : null;
    }

    private static string? ValidateAddress(DropshipAddress address)
    {
        var complete = !string.IsNullOrWhiteSpace(address.Street) && !string.IsNullOrWhiteSpace(address.PostalCode) && !string.IsNullOrWhiteSpace(address.City);
        if (!complete || address.CountryCode is not { Length: CountryLength })
        {
            return AddressKey;
        }

        return TooLong(address.DeliveryNotes) ? TextKey : null;
    }

    private static string? ValidateDropship(CreateDropshipmentRequest request)
    {
        if (TooLong(request.Notes) || (request.Currency is { Length: > 0 } currency && currency.Length != CurrencyLength))
        {
            return TextKey;
        }

        var badLine = request.Lines.Any(line => line.SalesOrderItemId == Guid.Empty || line.UnitPrice is < 0m || line.DiscountCascade.Any(step => step is < 0m or > 100m));

        return badLine ? LinesKey : null;
    }

    private static string? ValidateDdt(IssueDdtRequest request)
    {
        if (request.Packages is < 1 || request.GrossWeight is < 0m)
        {
            return QuantityKey;
        }

        return TooLong(request.Carrier) || TooLong(request.TrackingNumber) || TooLong(request.TransportBy) ? TextKey : null;
    }

    private static string? ValidateConfirmationLines(IReadOnlyList<ConfirmationLineRequest> lines)
    {
        if (lines.Any(line => line.PurchaseOrderLineId == Guid.Empty || line.ConfirmedQuantity is < 0m || line.ConfirmedUnitPrice is < 0m))
        {
            return LinesKey;
        }

        var duplicated = lines.GroupBy(line => line.PurchaseOrderLineId).Any(group => group.Skip(1).Any());
        if (duplicated)
        {
            return LinesKey;
        }

        return lines.Any(line => line.VarianceReason is { Length: > 255 }) ? TextKey : null;
    }

    private static string Id(Guid value) => value.ToString("D", CultureInfo.InvariantCulture);

    private static string Day(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Amount(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static void PutText(JsonObject body, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            body[name] = value.Trim();
        }
    }

    private static void PutDay(JsonObject body, string name, DateOnly? value)
    {
        if (value is { } day)
        {
            body[name] = Day(day);
        }
    }

    private static void PutNumber(JsonObject body, string name, decimal? value)
    {
        if (value is { } number)
        {
            body[name] = Amount(number);
        }
    }

    private static ProcurementPendingAction ToPending(ProcurementOutboxEntry entry) =>
        new(entry.Id, entry.Action, entry.Reference, DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc), entry.LastError);

    private static string Body(ProcurementOutboxEntry entry)
    {
        var body = JsonNode.Parse(entry.PayloadJson)?.AsObject() ?? [];
        body["idempotency_key"] = Id(entry.IdempotencyKey);

        return body.ToJsonString();
    }

    private static string ErrorText(string body, HttpStatusCode status)
    {
        try
        {
            if (JsonNode.Parse(body) is JsonObject root)
            {
                if (root["errors"] is JsonObject errors)
                {
                    var first = errors.Select(field => field.Value).OfType<JsonArray>().SelectMany(list => list).FirstOrDefault();
                    if (first is not null)
                    {
                        return first.ToString();
                    }
                }

                if (root["message"] is JsonValue message)
                {
                    return message.ToString();
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON: the status below describes it.
        }

        return "HTTP " + ((int)status).ToString(CultureInfo.InvariantCulture);
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status is 425 or >= 500;

    private DateTime Now() => time.GetUtcNow().UtcDateTime;

    private ProcurementOutboxEntry NewEntry(ProcurementAction action, string route, Guid commandId, Guid targetId, string? reference, JsonObject body, DateTime occurredAtUtc)
    {
        var label = (reference ?? string.Empty).Trim();

        return new ProcurementOutboxEntry
        {
            Id = commandId,
            TenantId = tenant.TenantId,
            Action = action,
            HttpMethod = Post,
            Route = route,
            TargetId = targetId,
            Reference = label.Length > MaxReference ? label[..MaxReference] : label,
            PayloadJson = body.ToJsonString(),
            IdempotencyKey = commandId,
            OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc),
            NextAttemptAt = Now(),
        };
    }

    private async Task TrimIfFreshAsync<T>(ProcurementLoad<T>? load, string prefix, CancellationToken cancellationToken)
    {
        if (load is { FromCache: false })
        {
            await store.TrimCacheAsync(prefix, ListKeep, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<ProcurementLoad<T>?> ReadAsync<T>(string route, Func<string, T> parse, CancellationToken cancellationToken)
    {
        if (api.IsAuthenticated)
        {
            try
            {
                var json = await api.GetAsync(route, cancellationToken).ConfigureAwait(false);
                var data = parse(json);
                var at = Now();
                await store.SaveCacheAsync(route, json, at, cancellationToken).ConfigureAwait(false);

                return new ProcurementLoad<T>(data, FromCache: false, at);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                // Offline or a bad answer: the cache below. A bad answer never replaces a good cache.
            }
        }

        var cached = await store.FindCacheAsync(route, cancellationToken).ConfigureAwait(false);

        return cached is null
            ? null
            : new ProcurementLoad<T>(parse(cached.Json), FromCache: true, DateTime.SpecifyKind(cached.SyncedAtUtc, DateTimeKind.Utc));
    }

    private async Task<ProcurementActionResult> QueueAsync(ProcurementOutboxEntry entry, CancellationToken cancellationToken)
    {
        var queued = await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);
        if (queued.Status == ProcurementOutboxStatus.Pending)
        {
            _ = await FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        return queued.Status switch
        {
            ProcurementOutboxStatus.Sent => new ProcurementActionResult(ProcurementActionStatus.Sent, ResultJson: queued.ResultJson),
            ProcurementOutboxStatus.Conflict => new ProcurementActionResult(ProcurementActionStatus.Conflict, Detail: queued.LastError),
            ProcurementOutboxStatus.Pending or ProcurementOutboxStatus.Dismissed => new ProcurementActionResult(ProcurementActionStatus.Queued),
            _ => new ProcurementActionResult(ProcurementActionStatus.Queued),
        };
    }

    private async Task<SendOutcome> SendAsync(ProcurementOutboxEntry entry, CancellationToken cancellationToken)
    {
        ProcurementApiReply reply;
        try
        {
            reply = await api.SendAsync(entry.HttpMethod, entry.Route, Body(entry), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return Retry(entry, ex.Message);
        }

        if (reply.IsSuccess)
        {
            entry.Status = ProcurementOutboxStatus.Sent;
            entry.ResultJson = reply.Body;
            entry.LastError = null;
            entry.UpdatedAt = Now();

            return SendOutcome.Sent;
        }

        if (reply.Status == HttpStatusCode.Unauthorized || IsTransient(reply.Status))
        {
            return Retry(entry, ErrorText(reply.Body, reply.Status));
        }

        entry.Status = ProcurementOutboxStatus.Conflict;
        entry.LastError = ErrorText(reply.Body, reply.Status);
        entry.UpdatedAt = Now();

        return SendOutcome.Refused;
    }

    private SendOutcome Retry(ProcurementOutboxEntry entry, string error)
    {
        entry.Attempts++;
        entry.LastError = error;
        entry.UpdatedAt = Now();
        if (!SyncRetryPolicy.CanRetry(entry.Attempts))
        {
            entry.Status = ProcurementOutboxStatus.Conflict;

            return SendOutcome.Refused;
        }

        entry.NextAttemptAt = Now() + SyncRetryPolicy.NextDelay(entry.Attempts);

        return SendOutcome.Retry;
    }
}
