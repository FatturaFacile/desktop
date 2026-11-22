// <copyright file="FieldServiceContractParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Reads the maintenance contract, equipment and equipment service history payloads of `/api/v1/fsm`.
/// </summary>
public static class FieldServiceContractParser
{
    /// <summary>
    /// Parses the list of maintenance contracts.
    /// </summary>
    /// <param name="json">The `data` array of the answer.</param>
    /// <returns>The contracts.</returns>
    /// <exception cref="JsonException">The payload is not a list.</exception>
    public static IReadOnlyList<MaintenanceContract> ParseContracts(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Array);

        return [.. FieldServiceJson.Elements(document.RootElement).Select(Contract)];
    }

    /// <summary>
    /// Parses one maintenance contract.
    /// </summary>
    /// <param name="json">The `data` object of the answer.</param>
    /// <returns>The contract.</returns>
    /// <exception cref="JsonException">The payload is not a contract.</exception>
    public static MaintenanceContract ParseContract(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Object);

        return Contract(document.RootElement);
    }

    /// <summary>
    /// Parses the list of customer equipment.
    /// </summary>
    /// <param name="json">The `data` array of the answer.</param>
    /// <returns>The equipment.</returns>
    /// <exception cref="JsonException">The payload is not a list.</exception>
    public static IReadOnlyList<FieldServiceEquipment> ParseEquipment(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Array);

        return [.. FieldServiceJson.Elements(document.RootElement).Select(Equipment)];
    }

    /// <summary>
    /// Parses the service history of an asset.
    /// </summary>
    /// <param name="json">The `data` object of the answer.</param>
    /// <returns>The history.</returns>
    /// <exception cref="JsonException">The payload is not a history.</exception>
    public static EquipmentHistory ParseHistory(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Object);
        var root = document.RootElement;
        var equipment = FieldServiceJson.Child(root, "equipment") ?? default;

        return new EquipmentHistory(
            FieldServiceJson.Text(equipment, "id"),
            FieldServiceJson.Text(equipment, "serial_number"),
            FieldServiceJson.Amount(equipment, "current_operating_hours"),
            FieldServiceJson.Whole(equipment, "current_cycle_count"),
            FieldServiceJson.Day(equipment, "last_service_date"),
            FieldServiceJson.Day(equipment, "next_scheduled_service_date"),
            [.. FieldServiceJson.Items(root, "history").Select(HistoryEntry)]);
    }

    private static MaintenanceContract Contract(JsonElement item)
    {
        var rule = FieldServiceJson.Child(item, "preventive_frequency_rule");

        return new MaintenanceContract(
            FieldServiceJson.Text(item, "id"),
            FieldServiceJson.Text(item, "customer_id"),
            FieldServiceJson.Text(item, "contract_number"),
            FieldServiceJson.Text(item, "title"),
            FieldServiceJson.Day(item, "start_date") ?? DateOnly.MinValue,
            FieldServiceJson.Day(item, "end_date"),
            FieldServiceJson.Text(item, "status"),
            MaintenanceCoverageWire.FromWire(FieldServiceJson.NullableText(item, "coverage_type")),
            rule is { ValueKind: JsonValueKind.Object } value ? Frequency(value) : null,
            FieldServiceJson.NullableWhole(item, "included_visits_per_year"),
            FieldServiceJson.Whole(item, "used_visits_current_year"),
            FieldServiceJson.Amount(item, "callout_fee_amount"),
            FieldServiceJson.NullableAmount(item, "hourly_labor_rate"),
            FieldServiceJson.Text(item, "currency"),
            FieldServiceJson.NullableText(item, "notes"),
            FieldServiceJson.Strings(item, "equipment_passport_ids"));
    }

    private static PreventiveFrequency Frequency(JsonElement rule)
    {
        var calendar = FieldServiceJson.Child(rule, "calendar") ?? default;
        var meter = FieldServiceJson.Child(rule, "meter") ?? default;

        return new PreventiveFrequency(
            FieldServiceJson.NullableWhole(calendar, "every"),
            FieldServiceJson.NullableText(calendar, "unit"),
            FieldServiceJson.NullableAmount(meter, "operating_hours"),
            FieldServiceJson.NullableWhole(meter, "cycles"),
            FieldServiceJson.NullableAmount(meter, "lead_percent"),
            FieldServiceJson.NullableWhole(rule, "lead_days"));
    }

    private static FieldServiceEquipment Equipment(JsonElement item) => new(
        FieldServiceJson.Text(item, "id"),
        FieldServiceJson.Text(item, "serial_number"),
        FieldServiceJson.NullableText(item, "customer_id"),
        FieldServiceJson.NullableText(item, "service_contract_id"),
        FieldServiceJson.NullableText(item, "site_address"),
        FieldServiceJson.Amount(item, "current_operating_hours"),
        FieldServiceJson.Whole(item, "current_cycle_count"),
        FieldServiceJson.Day(item, "last_service_date"),
        FieldServiceJson.Day(item, "next_scheduled_service_date"));

    private static EquipmentHistoryEntry HistoryEntry(JsonElement entry) => new(
        FieldServiceJson.Text(entry, "report_id"),
        FieldServiceJson.Text(entry, "report_number"),
        FieldServiceJson.Text(entry, "work_order_id"),
        FieldServiceJson.Text(entry, "work_type"),
        FieldServiceJson.Moment(entry, "signed_at") ?? DateTimeOffset.MinValue,
        FieldServiceJson.Text(entry, "summary"),
        FieldServiceJson.Amount(entry, "labor_hours"),
        FieldServiceJson.NullableAmount(entry, "meter_reading_hours"),
        FieldServiceJson.NullableWhole(entry, "meter_reading_cycles"),
        [.. FieldServiceJson.Items(entry, "parts").Select(HistoryPart)]);

    private static EquipmentHistoryPart HistoryPart(JsonElement part) => new(
        FieldServiceJson.Text(part, "product_id"),
        FieldServiceJson.Amount(part, "quantity"),
        FieldServiceJson.Flag(part, "covered"));
}
