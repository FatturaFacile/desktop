// <copyright file="FieldServiceRequests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Builds the JSON bodies of the Field Service write endpoints exactly as the web-api validates them. Decimals travel as
/// invariant strings, instants as ISO 8601 with their offset and dates as `yyyy-MM-dd`.
/// </summary>
public static class FieldServiceRequests
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string InstantFormat = "o";

    /// <summary>
    /// Builds the body of `POST work-orders/{id}/dispatch`.
    /// </summary>
    /// <param name="technicianId">The technician.</param>
    /// <param name="start">The scheduled start.</param>
    /// <param name="end">The scheduled end.</param>
    /// <returns>The JSON body.</returns>
    public static string Dispatch(string technicianId, DateTimeOffset start, DateTimeOffset end) =>
        JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["technician_id"] = technicianId,
            ["scheduled_start"] = Instant(start),
            ["scheduled_end"] = Instant(end),
        });

    /// <summary>
    /// Builds the body of `POST service-reports/{id}/invoice`.
    /// </summary>
    /// <param name="taxRate">The VAT rate in percent.</param>
    /// <param name="dueDate">The due date, or null.</param>
    /// <returns>The JSON body.</returns>
    public static string Invoice(decimal taxRate, DateOnly? dueDate) =>
        JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["tax_rate"] = Number(taxRate),
            ["due_date"] = dueDate?.ToString(DateFormat, CultureInfo.InvariantCulture),
        });

    /// <summary>
    /// Builds the body of `POST maintenance-contracts` (new) or `PATCH maintenance-contracts/{id}` (edit). The contract number is
    /// only sent for a new contract: it never changes afterwards.
    /// </summary>
    /// <param name="contract">The contract fields.</param>
    /// <returns>The JSON body.</returns>
    public static string Contract(FieldServiceContractInput contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        Dictionary<string, object?> body = new()
        {
            ["customer_id"] = contract.CustomerId,
            ["title"] = contract.Title,
            ["start_date"] = contract.StartDate.ToString(DateFormat, CultureInfo.InvariantCulture),
            ["end_date"] = contract.EndDate?.ToString(DateFormat, CultureInfo.InvariantCulture),
            ["status"] = contract.Status,
            ["coverage_type"] = MaintenanceCoverageWire.ToWire(contract.Coverage),
            ["preventive_frequency_rule"] = Frequency(contract.Frequency),
            ["included_visits_per_year"] = contract.IncludedVisits,
            ["callout_fee_amount"] = Number(contract.CalloutFee),
            ["hourly_labor_rate"] = contract.HourlyRate is { } rate ? Number(rate) : null,
            ["currency"] = contract.Currency,
            ["notes"] = contract.Notes,
            ["equipment_passport_ids"] = contract.EquipmentIds,
        };

        if (contract.Id is null)
        {
            body["contract_number"] = contract.Number;
        }

        return JsonSerializer.Serialize(body);
    }

    /// <summary>
    /// Builds the `payload` of a `status_transition` operation.
    /// </summary>
    /// <param name="status">The new status (en_route or on_site).</param>
    /// <param name="at">When the technician moved.</param>
    /// <returns>The JSON payload.</returns>
    public static string StatusPayload(string status, DateTime at) =>
        JsonSerializer.Serialize(new Dictionary<string, object?> { ["status"] = status, ["at"] = Instant(at) });

    /// <summary>
    /// Builds the `payload` of a `part_consumption` operation.
    /// </summary>
    /// <param name="input">The part.</param>
    /// <returns>The JSON payload.</returns>
    public static string PartPayload(FieldServicePartInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["product_id"] = input.ProductId.ToString("D", CultureInfo.InvariantCulture),
            ["quantity"] = Number(input.Quantity),
            ["unit_price"] = Number(input.UnitPrice),
            ["lot_number"] = string.IsNullOrWhiteSpace(input.LotNumber) ? null : input.LotNumber,
            ["description"] = input.Description,
        });
    }

    /// <summary>
    /// Builds the `payload` of a `service_report` operation.
    /// </summary>
    /// <param name="input">The report.</param>
    /// <param name="signedAt">When the customer signed.</param>
    /// <returns>The JSON payload.</returns>
    public static string ReportPayload(FieldServiceReportInput input, DateTime signedAt)
    {
        ArgumentNullException.ThrowIfNull(input);

        return JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["work_performed_summary"] = input.Summary,
            ["checklist"] = input.Checklist.Select(line => new Dictionary<string, object?> { ["item"] = line.Item, ["result"] = line.Result, ["notes"] = line.Notes }).ToList(),
            ["labor_hours"] = Number(input.LaborHours),
            ["travel_hours"] = Number(input.TravelHours),
            ["travel_km"] = Number(input.TravelKm),
            ["apply_callout_fee"] = input.ApplyCalloutFee,
            ["meter_reading_hours"] = input.MeterHours is { } hours ? Number(hours) : null,
            ["meter_reading_cycles"] = input.MeterCycles,
            ["customer_signer_name"] = input.SignerName,
            ["customer_signer_role"] = input.SignerRole,
            ["signature_svg"] = input.SignatureSvg,
            ["signed_at"] = Instant(signedAt),
            ["locale"] = input.Locale,
        });
    }

    private static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Instant(DateTimeOffset value) => value.ToString(InstantFormat, CultureInfo.InvariantCulture);

    private static string Instant(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString(InstantFormat, CultureInfo.InvariantCulture);

    private static Dictionary<string, object?>? Frequency(PreventiveFrequency? rule)
    {
        if (rule is null)
        {
            return null;
        }

        Dictionary<string, object?> body = [];
        if (rule.CalendarEvery is { } every && !string.IsNullOrWhiteSpace(rule.CalendarUnit))
        {
            body["calendar"] = new Dictionary<string, object?> { ["every"] = every, ["unit"] = rule.CalendarUnit };
        }

        if (rule.OperatingHours is not null || rule.Cycles is not null)
        {
            body["meter"] = new Dictionary<string, object?>
            {
                ["operating_hours"] = rule.OperatingHours is { } hours ? Number(hours) : null,
                ["cycles"] = rule.Cycles,
                ["lead_percent"] = rule.LeadPercent is { } lead ? Number(lead) : null,
            };
        }

        if (rule.LeadDays is { } days)
        {
            body["lead_days"] = days;
        }

        return body;
    }
}
