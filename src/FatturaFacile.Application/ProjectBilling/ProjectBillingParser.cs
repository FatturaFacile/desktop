// <copyright file="ProjectBillingParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Reads the `data` objects of the project billing endpoints into read models. Amounts arrive as decimal strings. Missing or null
/// fields are tolerated, so a server that adds or drops a field does not break a cached snapshot.
/// </summary>
public static class ProjectBillingParser
{
    private const string DefaultCurrency = "EUR";
    private const string DefaultModel = "time_and_materials";
    private const string DefaultStatus = "draft";
    private const string DefaultJurisdiction = "IT";
    private const string DefaultRule = "manual";

    /// <summary>
    /// Builds the snapshot of a project from the stored payloads.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <returns>The snapshot.</returns>
    /// <exception cref="JsonException">The summary payload is not an object.</exception>
    public static ProjectBillingSnapshot ParseSnapshot(string projectId, ProjectBillingPayload payload, DateTime syncedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(payload);

        using var summary = JsonDocument.Parse(payload.SummaryJson);
        using var contract = JsonDocument.Parse(payload.ContractJson);
        using var certificates = JsonDocument.Parse(payload.CertificatesJson);
        using var retention = JsonDocument.Parse(payload.RetentionJson);
        using var retainer = JsonDocument.Parse(payload.RetainerJson);

        if (summary.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The project billing summary is not an object.");
        }

        List<ProjectBillingSalCertificate> sals = [];
        foreach (var item in Items(certificates.RootElement))
        {
            sals.Add(ParseCertificate(item));
        }

        var balance = Object(retainer.RootElement, "balance");

        return new ProjectBillingSnapshot(
            projectId,
            ParseSummary(summary.RootElement),
            contract.RootElement.ValueKind == JsonValueKind.Object ? ParseContract(contract.RootElement) : null,
            sals,
            balance.ValueKind == JsonValueKind.Object ? ParseRetainer(balance) : null,
            retention.RootElement.ValueKind == JsonValueKind.Object ? ParseRetention(retention.RootElement) : null,
            syncedAtUtc);
    }

    private static ProjectBillingSummary ParseSummary(JsonElement e)
    {
        var retention = Object(e, "retention");
        var wip = Object(e, "latest_wip_run");
        var latestWip = wip.ValueKind == JsonValueKind.Object ? ParseWip(wip) : null;

        return new ProjectBillingSummary(
            Text(e, "currency") ?? DefaultCurrency,
            Amount(e, "contract_value"),
            Amount(e, "billed_to_date"),
            Amount(e, "draft_billing"),
            Amount(e, "unbilled_time_and_materials"),
            Count(e, "unbilled_milestones"),
            Count(e, "open_sal_certificates"),
            Amount(e, "open_sal_invoice_amount"),
            retention.ValueKind == JsonValueKind.Object ? Amount(retention, "balance") : null,
            latestWip);
    }

    private static ProjectBillingWip ParseWip(JsonElement e) =>
        new(Date(e, "period_end_date"), Amount(e, "recognized_revenue_to_date"), Amount(e, "billed_to_date"), Amount(e, "wip_asset_or_deferred_revenue_diff"));

    private static ProjectBillingContractTerms ParseContract(JsonElement e) =>
        new(
            Text(e, "billing_model") ?? DefaultModel,
            Text(e, "billing_status") ?? DefaultStatus,
            Text(e, "fiscal_jurisdiction") ?? DefaultJurisdiction,
            Text(e, "currency_code") ?? DefaultCurrency,
            Amount(e, "contract_total_value"),
            Amount(e, "advance_payment_percent"),
            Amount(e, "advance_payment_amount"),
            Amount(e, "advance_recouped_amount"),
            Amount(e, "retention_guarantee_percent"),
            Text(e, "cig"));

    private static ProjectBillingSalCertificate ParseCertificate(JsonElement e) =>
        new(
            Text(e, "id") ?? string.Empty,
            Count(e, "sal_number"),
            Amount(e, "previous_completion_percent"),
            Amount(e, "cumulative_completion_percent"),
            Amount(e, "cumulative_gross_earned_value"),
            Amount(e, "current_period_gross_sal"),
            Amount(e, "advance_deduction_amount"),
            Amount(e, "guarantee_retention_withheld"),
            Amount(e, "invoice_amount"),
            Amount(e, "net_billable_amount"),
            Flag(e, "is_final"),
            Text(e, "status") ?? DefaultStatus,
            Date(e, "period_start_date"),
            Date(e, "period_end_date"));

    private static ProjectBillingRetainer ParseRetainer(JsonElement e) =>
        new(
            !string.Equals(Text(e, "unit"), "amount", StringComparison.Ordinal),
            Amount(e, "remaining_hours"),
            Amount(e, "remaining_amount"),
            Amount(e, "remaining_percent"),
            Amount(e, "consumed_hours"),
            Amount(e, "low_balance_threshold_percent"),
            Flag(e, "is_low_balance"));

    private static ProjectBillingRetention ParseRetention(JsonElement e) =>
        new(Amount(e, "withheld"), Amount(e, "released"), Amount(e, "balance"), Text(e, "release_rule") ?? DefaultRule);

    private static JsonElement Object(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : default;

    private static List<JsonElement> Items(JsonElement array)
    {
        List<JsonElement> items = [];
        if (array.ValueKind != JsonValueKind.Array)
        {
            return items;
        }

        for (var index = 0; index < array.GetArrayLength(); index++)
        {
            if (array[index].ValueKind == JsonValueKind.Object)
            {
                items.Add(array[index]);
            }
        }

        return items;
    }

    private static string? Text(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool Flag(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    private static decimal Amount(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : FromText(Text(parent, name));

    private static int Count(JsonElement parent, string name) => (int)Math.Truncate(Amount(parent, name));

    private static decimal FromText(string? text) =>
        text is not null && decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;

    private static DateOnly? Date(JsonElement parent, string name) =>
        Text(parent, name) is { Length: >= 10 } text && DateOnly.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
}
