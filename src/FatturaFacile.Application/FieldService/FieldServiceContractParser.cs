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

        return [.. document.RootElement.EnumerateArray().Select(Contract)];
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

        return [.. document.RootElement.EnumerateArray().Select(Equipment)];
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

    private static MaintenanceContract Contract(JsonElement contract)
    {
        var rule = FieldServiceJson.Child(contract, "preventive_frequency_rule");

        return new MaintenanceContract(
            FieldServiceJson.Text(contract, "id"),
            FieldServiceJson.Text(contract, "customer_id"),
            FieldServiceJson.Text(contract, "contract_number"),
            FieldServiceJson.Text(contract, "title"),
            FieldServiceJson.Day(contract, "start_date") ?? DateOnly.MinValue,
            FieldServiceJson.Day(contract, "end_date"),
            FieldServiceJson.Text(contract, "status"),
            MaintenanceCoverageWire.FromWire(FieldServiceJson.NullableText(contract, "coverage_type")),
            rule is { ValueKind: JsonValueKind.Object } value ? Frequency(value) : null,
            FieldServiceJson.NullableWhole(contract, "included_visits_per_year"),
            FieldServiceJson.Whole(contract, "used_visits_current_year"),
            FieldServiceJson.Amount(contract, "callout_fee_amount"),
            FieldServiceJson.NullableAmount(contract, "hourly_labor_rate"),
            FieldServiceJson.Text(contract, "currency"),
            FieldServiceJson.NullableText(contract, "notes"),
            FieldServiceJson.Strings(contract, "equipment_passport_ids"));
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

    private static FieldServiceEquipment Equipment(JsonElement equipment) => new(
        FieldServiceJson.Text(equipment, "id"),
        FieldServiceJson.Text(equipment, "serial_number"),
        FieldServiceJson.NullableText(equipment, "customer_id"),
        FieldServiceJson.NullableText(equipment, "service_contract_id"),
        FieldServiceJson.NullableText(equipment, "site_address"),
        FieldServiceJson.Amount(equipment, "current_operating_hours"),
        FieldServiceJson.Whole(equipment, "current_cycle_count"),
        FieldServiceJson.Day(equipment, "last_service_date"),
        FieldServiceJson.Day(equipment, "next_scheduled_service_date"));

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
