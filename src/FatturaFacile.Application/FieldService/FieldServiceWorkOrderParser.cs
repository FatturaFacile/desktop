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

        return [.. document.RootElement.EnumerateArray().Select(WorkOrder)];
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

    private static FieldServicePart Part(JsonElement part) => new(
        FieldServiceJson.Text(part, "id"),
        FieldServiceJson.Text(part, "product_id"),
        FieldServiceJson.Text(part, "description"),
        FieldServiceJson.Amount(part, "quantity"),
        FieldServiceJson.Amount(part, "unit_price"),
        FieldServiceJson.Flag(part, "covered"),
        IsPending: false);

    private static FieldServiceReport Report(JsonElement report) => new(
        FieldServiceJson.Text(report, "id"),
        FieldServiceJson.Text(report, "work_order_id"),
        FieldServiceJson.Text(report, "report_number"),
        FieldServiceJson.Text(report, "work_performed_summary"),
        [.. FieldServiceJson.Items(report, "checklist").Select(ChecklistItem)],
        FieldServiceJson.Amount(report, "labor_hours"),
        FieldServiceJson.Amount(report, "travel_hours"),
        FieldServiceJson.Amount(report, "travel_km"),
        FieldServiceJson.Flag(report, "apply_callout_fee"),
        FieldServiceJson.NullableAmount(report, "meter_reading_hours"),
        FieldServiceJson.NullableWhole(report, "meter_reading_cycles"),
        FieldServiceJson.Text(report, "customer_signer_name"),
        FieldServiceJson.NullableText(report, "customer_signer_role"),
        FieldServiceJson.Flag(report, "signature_intact"),
        FieldServiceJson.Moment(report, "signed_at") ?? DateTimeOffset.MinValue,
        FieldServiceJson.Text(report, "locale"),
        FieldServiceJson.Flag(report, "has_pdf"),
        FieldServiceJson.NullableText(report, "invoice_id"),
        Billing(FieldServiceJson.Child(report, "billing") ?? default),
        IsPending: false);

    private static FieldServiceChecklistItem ChecklistItem(JsonElement item) => new(
        FieldServiceJson.Text(item, "item"),
        FieldServiceJson.Text(item, "result"),
        FieldServiceJson.NullableText(item, "notes"));

    private static FieldServiceBilling Billing(JsonElement billing) => new(
        [.. FieldServiceJson.Items(billing, "lines").Select(BillingLine)],
        FieldServiceJson.Amount(billing, "billable_total"),
        FieldServiceJson.Amount(billing, "covered_total"),
        FieldServiceJson.Text(billing, "currency"));

    private static FieldServiceBillingLine BillingLine(JsonElement line) => new(
        FieldServiceJson.Text(line, "kind"),
        FieldServiceJson.Text(line, "description"),
        FieldServiceJson.Amount(line, "quantity"),
        FieldServiceJson.Amount(line, "unit_price"),
        FieldServiceJson.Amount(line, "list_amount"),
        FieldServiceJson.Amount(line, "charged_amount"),
        FieldServiceJson.Flag(line, "covered"));
}
