// <copyright file="FieldServiceLocalProjection.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Lays the operations still waiting in the outbox over a cached work order, so a technician offline sees the part he took, the
/// status he set and the report he signed straight away, marked as pending until the server confirms and prices them.
/// </summary>
public static class FieldServiceLocalProjection
{
    private static readonly Dictionary<string, int> JourneyRank = new(StringComparer.Ordinal)
    {
        ["dispatched"] = 0,
        ["en_route"] = 1,
        ["on_site"] = 2,
        ["in_progress"] = 3,
        ["completed"] = 3,
        ["closed"] = 3,
        ["invoiced"] = 3,
    };

    /// <summary>
    /// Applies the pending operations of the work order, oldest first.
    /// </summary>
    /// <param name="order">The cached work order.</param>
    /// <param name="pending">The pending outbox entries (of any work order).</param>
    /// <returns>The work order as the technician left it.</returns>
    public static FieldServiceWorkOrder Apply(FieldServiceWorkOrder order, IEnumerable<FieldServiceOutboxEntry> pending)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(pending);

        if (!Guid.TryParse(order.Id, out var id))
        {
            return order;
        }

        var result = order;
        foreach (var entry in pending.Where(e => e.WorkOrderId == id && e.Status == FieldServiceOutboxStatus.Pending).OrderBy(e => e.OccurredAtUtc))
        {
            result = Apply(result, entry);
        }

        return result;
    }

    private static FieldServiceWorkOrder Apply(FieldServiceWorkOrder order, FieldServiceOutboxEntry entry)
    {
        using var document = JsonDocument.Parse(entry.PayloadJson);
        var payload = document.RootElement;

        return entry.Operation switch
        {
            FieldServiceOperationType.StatusTransition => WithStatus(order, FieldServiceJson.Text(payload, "status")),
            FieldServiceOperationType.PartConsumption => order with { Parts = [.. order.Parts, PendingPart(payload)] },
            _ => order with { Status = "completed", Report = PendingReport(order, payload) },
        };
    }

    private static FieldServiceWorkOrder WithStatus(FieldServiceWorkOrder order, string status)
    {
        var current = JourneyRank.GetValueOrDefault(order.Status, -1);

        return JourneyRank.TryGetValue(status, out var rank) && rank > current ? order with { Status = status } : order;
    }

    private static FieldServicePart PendingPart(JsonElement payload) => new(
        string.Empty,
        FieldServiceJson.Text(payload, "product_id"),
        FieldServiceJson.Text(payload, "description"),
        FieldServiceJson.Amount(payload, "quantity"),
        FieldServiceJson.Amount(payload, "unit_price"),
        Covered: false,
        IsPending: true);

    private static FieldServiceReport PendingReport(FieldServiceWorkOrder order, JsonElement payload) => new(
        string.Empty,
        order.Id,
        string.Empty,
        FieldServiceJson.Text(payload, "work_performed_summary"),
        [.. FieldServiceJson.Items(payload, "checklist").Select(line => new FieldServiceChecklistItem(FieldServiceJson.Text(line, "item"), FieldServiceJson.Text(line, "result"), FieldServiceJson.NullableText(line, "notes")))],
        FieldServiceJson.Amount(payload, "labor_hours"),
        FieldServiceJson.Amount(payload, "travel_hours"),
        FieldServiceJson.Amount(payload, "travel_km"),
        FieldServiceJson.Flag(payload, "apply_callout_fee"),
        FieldServiceJson.NullableAmount(payload, "meter_reading_hours"),
        FieldServiceJson.NullableWhole(payload, "meter_reading_cycles"),
        FieldServiceJson.Text(payload, "customer_signer_name"),
        FieldServiceJson.NullableText(payload, "customer_signer_role"),
        SignatureIntact: true,
        FieldServiceJson.Moment(payload, "signed_at") ?? DateTimeOffset.MinValue,
        FieldServiceJson.Text(payload, "locale"),
        HasPdf: false,
        InvoiceId: null,
        new FieldServiceBilling([], 0m, 0m, string.Empty),
        IsPending: true);
}
