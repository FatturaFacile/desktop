// <copyright file="FieldServiceWorkOrderParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Reads the field work order payloads of `/api/v1/fsm/work-orders` and the service report payloads into the application models.
/// </summary>
public static class FieldServiceWorkOrderParser
{
    /// <summary>
    /// Parses the list of work orders (without parts and report).
    /// </summary>
    /// <param name="json">The `data` array of the list answer.</param>
    /// <returns>The work orders.</returns>
    /// <exception cref="JsonException">The payload is not a list.</exception>
    public static IReadOnlyList<FieldServiceWorkOrder> ParseList(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Array);

        return [.. FieldServiceJson.Elements(document.RootElement).Select(WorkOrder)];
    }

    /// <summary>
    /// Parses one work order with its parts and its service report.
    /// </summary>
    /// <param name="json">The `data` object of the detail answer.</param>
    /// <returns>The work order.</returns>
    /// <exception cref="JsonException">The payload is not a work order.</exception>
    public static FieldServiceWorkOrder ParseDetail(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Object);

        return WorkOrder(document.RootElement);
    }

    /// <summary>
    /// Parses a service report, as returned by the sign endpoint.
    /// </summary>
    /// <param name="json">The `data` object of the answer.</param>
    /// <returns>The report.</returns>
    /// <exception cref="JsonException">The payload is not a report.</exception>
    public static FieldServiceReport ParseReport(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Object);

        return Report(document.RootElement);
    }

    private static FieldServiceWorkOrder WorkOrder(JsonElement order)
    {
        var site = FieldServiceJson.Child(order, "site") ?? default;
        var report = FieldServiceJson.Child(order, "service_report");

        return new FieldServiceWorkOrder(
            FieldServiceJson.Text(order, "id"),
            FieldServiceJson.Text(order, "work_order_number"),
            FieldServiceJson.Text(order, "work_order_type"),
            FieldServiceJson.Text(order, "status"),
            FieldServiceJson.Text(order, "priority"),
            FieldServiceJson.Text(order, "title"),
            FieldServiceJson.NullableText(order, "problem_description"),
            FieldServiceJson.NullableText(order, "customer_id"),
            FieldServiceJson.NullableText(order, "equipment_passport_id"),
            FieldServiceJson.NullableText(order, "service_contract_id"),
            FieldServiceJson.NullableText(order, "assigned_technician_id"),
            FieldServiceJson.Moment(order, "scheduled_start"),
            FieldServiceJson.Moment(order, "scheduled_end"),
            FieldServiceJson.NullableText(site, "address"),
            FieldServiceJson.Strings(order, "required_skills"),
            [.. FieldServiceJson.Items(order, "parts").Select(Part)],
            report is { ValueKind: JsonValueKind.Object } value ? Report(value) : null,
            FieldServiceJson.NullableText(order, "invoice_id"));
    }

    private static FieldServicePart Part(JsonElement item) => new(
        FieldServiceJson.Text(item, "id"),
        FieldServiceJson.Text(item, "product_id"),
        FieldServiceJson.Text(item, "description"),
        FieldServiceJson.Amount(item, "quantity"),
        FieldServiceJson.Amount(item, "unit_price"),
        FieldServiceJson.Flag(item, "covered"),
        IsPending: false);

    private static FieldServiceReport Report(JsonElement item) => new(
        FieldServiceJson.Text(item, "id"),
        FieldServiceJson.Text(item, "work_order_id"),
        FieldServiceJson.Text(item, "report_number"),
        FieldServiceJson.Text(item, "work_performed_summary"),
        [.. FieldServiceJson.Items(item, "checklist").Select(ChecklistItem)],
        FieldServiceJson.Amount(item, "labor_hours"),
        FieldServiceJson.Amount(item, "travel_hours"),
        FieldServiceJson.Amount(item, "travel_km"),
        FieldServiceJson.Flag(item, "apply_callout_fee"),
        FieldServiceJson.NullableAmount(item, "meter_reading_hours"),
        FieldServiceJson.NullableWhole(item, "meter_reading_cycles"),
        FieldServiceJson.Text(item, "customer_signer_name"),
        FieldServiceJson.NullableText(item, "customer_signer_role"),
        FieldServiceJson.Flag(item, "signature_intact"),
        FieldServiceJson.Moment(item, "signed_at") ?? DateTimeOffset.MinValue,
        FieldServiceJson.Text(item, "locale"),
        FieldServiceJson.Flag(item, "has_pdf"),
        FieldServiceJson.NullableText(item, "invoice_id"),
        Billing(FieldServiceJson.Child(item, "billing") ?? default),
        IsPending: false);

    private static FieldServiceChecklistItem ChecklistItem(JsonElement item) => new(
        FieldServiceJson.Text(item, "item"),
        FieldServiceJson.Text(item, "result"),
        FieldServiceJson.NullableText(item, "notes"));

    private static FieldServiceBilling Billing(JsonElement item) => new(
        [.. FieldServiceJson.Items(item, "lines").Select(BillingLine)],
        FieldServiceJson.Amount(item, "billable_total"),
        FieldServiceJson.Amount(item, "covered_total"),
        FieldServiceJson.Text(item, "currency"));

    private static FieldServiceBillingLine BillingLine(JsonElement line) => new(
        FieldServiceJson.Text(line, "kind"),
        FieldServiceJson.Text(line, "description"),
        FieldServiceJson.Amount(line, "quantity"),
        FieldServiceJson.Amount(line, "unit_price"),
        FieldServiceJson.Amount(line, "list_amount"),
        FieldServiceJson.Amount(line, "charged_amount"),
        FieldServiceJson.Flag(line, "covered"));
}
