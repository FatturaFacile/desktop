// <copyright file="SupplierRebateParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Reads the `data` payloads of the supplier rebate and blanket agreement endpoints into read models. Decimals arrive as strings
/// (or numbers) and are parsed with the invariant culture. Missing or null fields are tolerated, so a server that adds or drops a
/// field does not break a cached payload.
/// </summary>
public static class SupplierRebateParser
{
    private const int DateLength = 10;
    private const string DefaultCurrency = "EUR";

    /// <summary>
    /// Reads the agreements of a list.
    /// </summary>
    /// <param name="json">The `data` array, or null.</param>
    /// <returns>The agreements (none for a null payload).</returns>
    public static IReadOnlyList<RebateAgreement> ParseAgreements(string? json) => ParseList(json, ParseAgreementElement);

    /// <summary>
    /// Reads one agreement.
    /// </summary>
    /// <param name="json">The `data` object.</param>
    /// <returns>The agreement.</returns>
    public static RebateAgreement ParseAgreement(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);

        return ParseAgreementElement(document.RootElement);
    }

    /// <summary>
    /// Reads the live progress of an agreement.
    /// </summary>
    /// <param name="json">The `data` object of /rebates/agreements/{id}/progress.</param>
    /// <returns>The progress.</returns>
    public static RebateProgress ParseProgress(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var tier = Object(root, "tier");
        var bonus = Object(root, "bonus");
        var contributions = Object(root, "contributions");

        return new RebateProgress(
            Text(root, "agreement_id") ?? string.Empty,
            Text(root, "currency") ?? DefaultCurrency,
            Date(root, "as_of"),
            Number(root, "qualifying_volume") ?? 0m,
            Number(root, "earned_rebate") ?? 0m,
            Number(root, "accrued_rebate") ?? 0m,
            Number(root, "settled_rebate") ?? 0m,
            tier.ValueKind == JsonValueKind.Object ? ParseTierProgress(tier) : null,
            bonus.ValueKind == JsonValueKind.Object ? ParseBonus(bonus) : null,
            new RebateContributionSummary(Number(contributions, "planned") ?? 0m, Number(contributions, "accrued") ?? 0m, Number(contributions, "settled") ?? 0m),
            [.. Items(root, "tiers").Select(item => new RebateScaleBand(Number(item, "threshold_amount") ?? 0m, Number(item, "max_threshold"), Number(item, "rate_percent") ?? 0m, Number(item, "flat_amount")))]);
    }

    /// <summary>
    /// Reads the accrual runs of an agreement.
    /// </summary>
    /// <param name="json">The `data` array, or null.</param>
    /// <returns>The runs (none for a null payload).</returns>
    public static IReadOnlyList<RebateAccrualRun> ParseRuns(string? json) => ParseList(json, ParseRun);

    /// <summary>
    /// Reads the settlement proposals.
    /// </summary>
    /// <param name="json">The `data` array, or null.</param>
    /// <returns>The proposals (none for a null payload).</returns>
    public static IReadOnlyList<RebateSettlementProposal> ParseProposals(string? json) => ParseList(json, ParseProposalElement);

    /// <summary>
    /// Reads one settlement proposal.
    /// </summary>
    /// <param name="json">The `data` object.</param>
    /// <returns>The proposal.</returns>
    public static RebateSettlementProposal ParseProposal(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);

        return ParseProposalElement(document.RootElement);
    }

    /// <summary>
    /// Reads the balance of a blanket agreement.
    /// </summary>
    /// <param name="json">The `data` object of /procurement/contracts/{id}/balance.</param>
    /// <returns>The balance.</returns>
    public static BlanketBalance ParseBlanketBalance(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new BlanketBalance(
            Text(root, "contract_id") ?? string.Empty,
            Text(root, "currency") ?? DefaultCurrency,
            Number(root, "ceiling_amount") ?? 0m,
            Number(root, "minimum_spend_amount"),
            Number(root, "ordered_amount") ?? 0m,
            Number(root, "received_amount") ?? 0m,
            Number(root, "invoiced_amount") ?? 0m,
            Number(root, "remaining_ceiling"),
            Number(root, "ceiling_used_percent"),
            Number(root, "minimum_shortfall"),
            Number(root, "minimum_reached_percent"),
            [.. Items(root, "lines").Select(item => new BlanketBalanceLine(Text(item, "line_id") ?? string.Empty, Text(item, "description") ?? string.Empty, Number(item, "unit_price") ?? 0m, Number(item, "committed_quantity"), Number(item, "released_quantity") ?? 0m, Number(item, "remaining_quantity"), Number(item, "ordered_amount") ?? 0m))],
            [.. Items(root, "alerts").Select(item => new BlanketAlert(Text(item, "code") ?? string.Empty, Text(item, "severity") ?? string.Empty))]);
    }

    /// <summary>
    /// Reads the blanket agreements of the picker.
    /// </summary>
    /// <param name="json">The `data` array, or null.</param>
    /// <returns>The agreements (none for a null payload).</returns>
    public static IReadOnlyList<BlanketContract> ParseBlanketContracts(string? json) =>
        ParseList(json, item => new BlanketContract(Text(item, "id") ?? string.Empty, Text(item, "contract_number") ?? string.Empty, Text(item, "title") ?? string.Empty, Text(item, "supplier_id"), Text(item, "status") ?? string.Empty));

    /// <summary>
    /// Reads the supplier invoices of a supplier that still have an open balance: credit notes and settled invoices are left out.
    /// </summary>
    /// <param name="json">The invoice list: a `data` array, or an object whose `data` is the array.</param>
    /// <param name="supplierId">The supplier identifier.</param>
    /// <returns>The open payables, oldest first.</returns>
    public static IReadOnlyList<RebatePayableInvoice> ParsePayables(string? json, string supplierId)
    {
        ArgumentNullException.ThrowIfNull(supplierId);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        using var document = JsonDocument.Parse(json);
        var rows = document.RootElement;

        // The list is wrapped once ({"data": [...]}) or twice when the server paginates ({"data": {"data": [...]}}).
        while (rows.ValueKind == JsonValueKind.Object && rows.TryGetProperty("data", out var inner))
        {
            rows = inner;
        }

        List<RebatePayableInvoice> payables = [];

        foreach (var item in ArrayItems(rows))
        {
            var invoice = new RebatePayableInvoice(
                Text(item, "id") ?? string.Empty,
                Text(item, "invoice_number") ?? string.Empty,
                Date(item, "date"),
                Number(item, "total_amount") ?? 0m,
                Number(item, "total_paid") ?? 0m,
                Text(item, "currency") ?? DefaultCurrency);
            var sameSupplier = string.Equals(Text(item, "supplier_id"), supplierId, StringComparison.OrdinalIgnoreCase);
            var isCreditNote = string.Equals(Text(item, "document_type"), RebateCodes.CreditNoteDocumentType, StringComparison.OrdinalIgnoreCase);

            if (sameSupplier && !isCreditNote && invoice.OpenBalance > 0m)
            {
                payables.Add(invoice);
            }
        }

        return [.. payables.OrderBy(invoice => invoice.InvoiceDate)];
    }

    private static RebateAgreement ParseAgreementElement(JsonElement e) =>
        new(
            Text(e, "id") ?? string.Empty,
            Text(e, "code") ?? string.Empty,
            Text(e, "name") ?? string.Empty,
            Text(e, "direction") ?? RebateCodes.DirectionPurchase,
            Text(e, "contact_id") ?? string.Empty,
            Text(e, "contact_name"),
            RebateWire.ParseTierMode(Text(e, "tier_mode")),
            Date(e, "starts_on") ?? DateOnly.MinValue,
            Date(e, "ends_on") ?? DateOnly.MinValue,
            Text(e, "currency") ?? DefaultCurrency,
            Text(e, "status") ?? RebateCodes.StatusDraft,
            Text(e, "notes"),
            Text(e, "rebate_type") ?? RebateCodes.TypeTieredVolume,
            Text(e, "supplier_contract_id"),
            Number(e, "fixed_bonus_target"),
            Number(e, "fixed_bonus_amount"),
            Text(e, "accrual_frequency") ?? RebateCodes.FrequencyMonthly,
            Text(e, "accrual_account_id"),
            Text(e, "income_account_id"),
            Integer(e, "version") ?? 1,
            Number(e, "open_rebate"),
            [.. Items(e, "tiers").Select(ParseTier)],
            [.. Items(e, "contributions").Select(item => new RebateContribution(Text(item, "id") ?? string.Empty, Text(item, "kind") ?? RebateCodes.KindMarketing, Text(item, "description") ?? string.Empty, Number(item, "amount") ?? 0m, Date(item, "due_on"), Text(item, "status") ?? "planned"))]);

    private static RebateTier ParseTier(JsonElement e) => new(Number(e, "threshold_amount") ?? 0m, Number(e, "rate_percent") ?? 0m, Number(e, "flat_amount"));

    private static RebateTierProgress ParseTierProgress(JsonElement e)
    {
        var current = Object(e, "current_tier");
        var next = Object(e, "next_tier");

        return new RebateTierProgress(
            current.ValueKind == JsonValueKind.Object ? ParseTier(current) : null,
            next.ValueKind == JsonValueKind.Object ? ParseTier(next) : null,
            Number(e, "accrued_rebate") ?? 0m,
            Number(e, "distance_to_next"),
            Number(e, "progress_percent") ?? 0m,
            Number(e, "projected_rebate_at_next"),
            Number(e, "projected_gain"));
    }

    private static RebateBonusProgress ParseBonus(JsonElement e) =>
        new(Number(e, "target") ?? 0m, Number(e, "bonus") ?? 0m, Flag(e, "reached"), Number(e, "distance_to_target") ?? 0m, Number(e, "progress_percent") ?? 0m);

    private static RebateAccrualRun ParseRun(JsonElement e) =>
        new(
            Text(e, "id") ?? string.Empty,
            Date(e, "period_start"),
            Date(e, "period_end"),
            Text(e, "status") ?? RebateCodes.RunPosted,
            Number(e, "base_amount") ?? 0m,
            Number(e, "cumulative_rebate") ?? 0m,
            Number(e, "delta_amount") ?? 0m,
            Text(e, "journal_entry_id"),
            Text(e, "reversal_reason"),
            [.. Items(e, "lines").Select(item => new RebateAccrualLine(Text(item, "passive_invoice_id") ?? string.Empty, Text(item, "invoice_number") ?? string.Empty, Text(item, "document_type") ?? string.Empty, Date(item, "invoice_date"), Number(item, "net_amount") ?? 0m))]);

    private static RebateSettlementProposal ParseProposalElement(JsonElement e) =>
        new(
            Text(e, "id") ?? string.Empty,
            Text(e, "agreement_id") ?? string.Empty,
            Number(e, "amount") ?? 0m,
            Number(e, "gross_amount") ?? 0m,
            Number(e, "tax_rate"),
            Number(e, "tax_amount") ?? 0m,
            Text(e, "currency") ?? DefaultCurrency,
            RebateWire.ParseStatus(Text(e, "status")),
            RebateWire.ParseMode(Text(e, "execution_mode")),
            Text(e, "passive_invoice_id"),
            Number(e, "set_off_amount"),
            Integer(e, "version") ?? 1,
            [.. Items(e, "lines").Select(item => new RebateSettlementLine(Text(item, "line_type") ?? string.Empty, Text(item, "reference") ?? string.Empty, Date(item, "reference_date"), Number(item, "net_amount") ?? 0m, Number(item, "rebate_amount") ?? 0m))]);

    private static List<T> ParseList<T>(string? json, Func<JsonElement, T> read)
    {
        List<T> result = [];
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        using var document = JsonDocument.Parse(json);
        foreach (var item in ArrayItems(document.RootElement))
        {
            result.Add(read(item));
        }

        return result;
    }

    private static List<JsonElement> ArrayItems(JsonElement array)
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

    private static List<JsonElement> Items(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) ? ArrayItems(value) : [];

    private static JsonElement Object(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : default;

    private static string? Text(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool Flag(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    private static int? Integer(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed)
            ? parsed
            : null;

    private static decimal? Number(JsonElement parent, string name)
    {
        if (parent.ValueKind != JsonValueKind.Object || !parent.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null,
        };
    }

    private static DateOnly? Date(JsonElement parent, string name)
    {
        var text = Text(parent, name);
        if (text is null)
        {
            return null;
        }

        var day = text.Length > DateLength ? text[..DateLength] : text;

        return DateOnly.TryParseExact(day, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }
}
