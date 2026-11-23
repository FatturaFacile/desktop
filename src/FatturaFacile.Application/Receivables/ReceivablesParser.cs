// <copyright file="ReceivablesParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Parses the answers of the receivables endpoints into the domain models. Amounts arrive as strings and are read as
/// <see cref="decimal"/> with the invariant culture; the buckets are taken from the answer, never assumed.
/// </summary>
[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The array and object enumerators are structs whose Dispose is a no-op; they are enumerated to completion.")]
[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP005:Return type should indicate that the value should be disposed", Justification = "The array enumerator is a struct whose Dispose is a no-op.")]
public static class ReceivablesParser
{
    private static readonly string[] NameProperties = ["name", "company_name", "business_name", "display_name", "legal_name"];

    /// <summary>
    /// Parses the dashboard.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The dashboard.</returns>
    public static ReceivablesDashboard ParseDashboard(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        var kpis = data.TryGet("kpis", out var found) ? found : default;

        return new ReceivablesDashboard(
            data.Date("as_of"),
            data.Text("currency"),
            new ReceivablesKpis(
                kpis.Dec("total_exposure"),
                kpis.Dec("total_open"),
                kpis.Dec("overdue_amount"),
                kpis.Dec("over_90"),
                kpis.DecimalOrNull("dso"),
                kpis.Dec("bad_debt_provision"),
                kpis.Dec("net_realizable_value"),
                kpis.Int("blocked_customers"),
                kpis.Int("active_overrides")),
            [.. data.Elements("blocked").Select(row => new BlockedCustomer(row.GuidOrEmpty("customer_id"), row.Text("reason"), row.Dec("exposure"), row.Dec("limit")))]);
    }

    /// <summary>
    /// Parses a page of receivable items (the resource pagination: data and meta).
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The page.</returns>
    public static ReceivableItemPage ParseItemPage(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var root = Root(json);
        var rows = root.Data().AsArray().Select(ToItem).ToList();
        var meta = root.TryGet("meta", out var found) ? found : default;

        return new ReceivableItemPage(rows, meta.Int("current_page", 1), meta.Int("last_page", 1), meta.Int("total", rows.Count));
    }

    /// <summary>
    /// Parses one receivable item with its allocations.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The item.</returns>
    public static ReceivableItem ParseItem(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return ToItem(Root(json).Data());
    }

    /// <summary>
    /// Parses the aging.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The aging with the buckets of the answer.</returns>
    public static AgingReport ParseAging(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        List<AgingBucket> buckets = [.. data.Elements("buckets").Select(ToBucket)];
        if (buckets.Count == 0)
        {
            buckets = [.. new AgingBucketEngine().Buckets];
        }

        var totals = Complete(AmountMap(data, "totals"), buckets);

        return new AgingReport(
            data.Date("as_of"),
            data.Text("currency"),
            [.. data.Strings("available_currencies")],
            buckets,
            totals,
            data.Dec("total_open"),
            data.Dec("total_overdue"),
            data.Dec("over_90"),
            [
                .. data.Elements("customers").Select(row => new AgingCustomerRow(
                    row.GuidOrEmpty("customer_id"),
                    row.Text("customer_name"),
                    Complete(AmountMap(row, "buckets"), buckets),
                    row.Dec("total_open"),
                    row.Dec("total_overdue"),
                    row.Int("items"))),
            ]);
    }

    /// <summary>
    /// Parses the customer ledger; the running balance of the server is kept, or computed when the server sent none.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The ledger.</returns>
    public static CustomerLedger ParseLedger(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        var rows = ToMovements(data);
        var closing = data.DecimalOrNull("closing_balance");

        if (rows.Count == 0)
        {
            return new CustomerLedger(rows, closing ?? 0m);
        }

        var serverBalances = data.Elements("movements").All(row => row.TryGet("balance", out _));
        if (!serverBalances)
        {
            var computed = CustomerLedger.WithRunningBalance(data.DecimalOrNull("opening_balance") ?? 0m, rows);

            return closing is { } value ? computed with { ClosingBalance = value } : computed;
        }

        return new CustomerLedger(rows, closing ?? rows[^1].Balance);
    }

    /// <summary>
    /// Parses the unreconciled payments of a customer.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The payments.</returns>
    public static IReadOnlyList<UnreconciledPayment> ParseUnreconciledPayments(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return
        [
            .. Root(json).Data().AsArray().Select(row => new UnreconciledPayment(
                row.GuidOrEmpty("ledger_entry_id"),
                row.Text("reference"),
                row.Date("date"),
                row.Text("currency"),
                row.Dec("amount"),
                row.DecimalOrNull("unallocated") ?? row.Dec("amount"))),
        ];
    }

    /// <summary>
    /// Parses the credit position of a customer.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The credit position.</returns>
    public static CustomerCreditStatus ParseCredit(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        var limit = data.TryGet("limit", out var limitElement) && limitElement.ValueKind == JsonValueKind.Object ? ToLimit(limitElement) : null;
        var exposure = data.TryGet("exposure", out var exposureElement) ? ToExposure(exposureElement) : CreditExposure.Empty;

        return new CustomerCreditStatus(
            data.GuidOrEmpty("customer_id"),
            data.Date("as_of"),
            limit,
            exposure,
            data.DecimalOrNull("utilization_percent"),
            data.DecimalOrNull("available_credit"),
            data.Dec("insured_cover"),
            data.Dec("uninsured_exposure"),
            data.Flag("is_credit_hold"),
            data.Flag("is_blocked"),
            data.TryGet("decision", out var decision) ? ToDecision(decision) : null,
            [.. data.Elements("overrides").Select(ToOverride)]);
    }

    /// <summary>
    /// Parses the composite risk of a customer.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The risk.</returns>
    public static CreditRiskScore ParseRisk(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();

        return new CreditRiskScore(
            data.Int("score"),
            ReceivablesWire.ParseRating(data.TextOrNull("rating")),
            [.. data.Elements("factors").Select(factor => new RiskFactor(factor.Text("key"), factor.TextOrNull("value"), factor.Dec("points")))],
            data.Flag("blocked"));
    }

    /// <summary>
    /// Parses the credit limit history of a customer.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The limits, in the order of the server.</returns>
    public static IReadOnlyList<CustomerCreditLimit> ParseLimits(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return [.. Root(json).Data().AsArray().Select(ToLimit)];
    }

    /// <summary>
    /// Parses a credit check.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The decision.</returns>
    /// <exception cref="JsonException">The answer is empty.</exception>
    public static CreditDecision ParseDecision(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return ToDecision(Root(json).Data()) ?? throw new JsonException("The credit check answer is empty.");
    }

    /// <summary>
    /// Parses the DSO; an answer without the dso member is computed from the receivables and the sales.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The DSO.</returns>
    public static DsoResult ParseDso(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        var receivables = data.Dec("receivables");
        var sales = data.Dec("credit_sales");
        var days = data.Int("period_days", DsoCalculator.DefaultPeriodDays);
        var dso = data.ValueKind == JsonValueKind.Object && data.TryGetProperty("dso", out _)
            ? data.DecimalOrNull("dso")
            : DsoCalculator.Calculate(receivables, sales, days);

        return new DsoResult(data.Date("as_of"), days, data.Text("currency"), receivables, sales, dso);
    }

    /// <summary>
    /// Parses the bad-debt provision.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The provision per bucket with the totals.</returns>
    public static ProvisionSummary ParseProvision(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        List<ProvisionLine> lines =
        [
            .. data.Elements("lines").Select(line => new ProvisionLine(
                line.Text("key"),
                line.Int("from"),
                line.IntOrNull("to"),
                line.Dec("open_amount"),
                line.Dec("rate_percent"),
                line.Dec("provision"),
                line.Flag("is_default_rate", fallback: true))),
        ];
        var totalOpen = data.DecimalOrNull("total_open") ?? lines.Sum(line => line.OpenAmount);
        var totalProvision = data.DecimalOrNull("total_provision") ?? lines.Sum(line => line.Provision);

        return new ProvisionSummary(lines, totalOpen, totalProvision, data.DecimalOrNull("net_realizable_value") ?? (totalOpen - totalProvision));
    }

    /// <summary>
    /// Parses the statement of a customer.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The statement.</returns>
    public static CustomerStatement ParseStatement(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var data = Root(json).Data();
        var movements = ToMovements(data);
        var period = data.TryGet("period", out var found) ? found : default;
        var asOf = data.Date("as_of");
        var aging = data.TryGet("aging", out var agingElement) ? agingElement : default;
        var agingTotals = aging.TryGet("totals", out var totalsElement) ? AmountMap(totalsElement) : AmountMap(aging);

        return new CustomerStatement(
            NameOf(data, "customer"),
            NameOf(data, "tenant"),
            data.Text("currency"),
            period.DateOrNull("from") ?? data.DateOrNull("from") ?? (movements.Count > 0 ? movements.Min(row => row.Date) : asOf),
            period.DateOrNull("to") ?? data.DateOrNull("to") ?? asOf,
            asOf,
            data.Dec("opening_balance"),
            movements,
            data.DecimalOrNull("closing_balance") ?? (movements.Count > 0 ? movements[^1].Balance : data.Dec("opening_balance")),
            [.. data.Elements("open_items").Select(ToItem)],
            data.Dec("total_open"),
            data.Dec("total_overdue"),
            agingTotals);
    }

    private static AgingBucket ToBucket(JsonElement element) => new(element.Text("key"), element.Int("from"), element.IntOrNull("to"));

    private static Dictionary<string, decimal> AmountMap(JsonElement element, string name) =>
        element.TryGet(name, out var found) ? AmountMap(found) : [with(StringComparer.Ordinal)];

    private static Dictionary<string, decimal> AmountMap(JsonElement element)
    {
        Dictionary<string, decimal> map = [with(StringComparer.Ordinal)];

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (Number(property.Value) is { } amount)
                {
                    map[property.Name] = amount;
                }
            }
        }

        return map;
    }

    private static Dictionary<string, decimal> Complete(Dictionary<string, decimal> map, IEnumerable<AgingBucket> buckets)
    {
        foreach (var bucket in buckets)
        {
            _ = map.TryAdd(bucket.Key, 0m);
        }

        return map;
    }

    private static string NameOf(JsonElement element, string property)
    {
        if (!element.TryGet(property, out var value))
        {
            return string.Empty;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? string.Empty;
        }

        foreach (var candidate in NameProperties)
        {
            var text = value.TextOrNull(candidate);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return (value.Text("first_name") + " " + value.Text("last_name")).Trim();
    }

    private static List<LedgerMovement> ToMovements(JsonElement data) =>
        [.. data.Elements("movements").Select(ToMovement)];

    private static LedgerMovement ToMovement(JsonElement row) => new(
        row.Date("date"),
        row.DateOrNull("value_date"),
        row.Text("kind"),
        row.Text("document_type"),
        row.Text("reference"),
        row.GuidOrNull("receivable_item_id"),
        row.GuidOrNull("allocation_id"),
        row.Text("currency"),
        row.Dec("debit"),
        row.Dec("credit"),
        row.DateOrNull("due_date"),
        row.TextOrNull("status"),
        row.Dec("balance"));

    private static ReceivableItem ToItem(JsonElement row) => new(
        row.GuidOrEmpty("id"),
        row.GuidOrEmpty("customer_id"),
        row.GuidOrNull("invoice_id"),
        row.Int("sequence"),
        row.Text("document_type"),
        row.Text("document_number"),
        row.Text("currency"),
        row.Dec("original_amount"),
        row.Dec("open_amount"),
        row.Date("issue_date"),
        row.Date("due_date"),
        row.DateOrNull("value_date"),
        ReceivablesWire.ParseStatus(row.TextOrNull("status")),
        row.Int("days_past_due"),
        row.TextOrNull("dispute_reason"),
        [.. row.Elements("allocations").Select(ToAllocation)]);

    private static ReceivableAllocation ToAllocation(JsonElement row) => new(
        row.GuidOrEmpty("id"),
        row.GuidOrEmpty("receivable_item_id"),
        ReceivablesWire.ParseAllocationType(row.TextOrNull("source_type") ?? row.TextOrNull("type")),
        row.Text("origin"),
        row.TextOrNull("source_id"),
        row.TextOrNull("source_reference"),
        row.Dec("amount"),
        row.Date("value_date"),
        row.TextOrNull("notes"),
        row.InstantOrNull("reversed_at"));

    private static CustomerCreditLimit ToLimit(JsonElement row) => new(
        row.GuidOrEmpty("id"),
        row.GuidOrEmpty("customer_id"),
        row.Text("currency"),
        row.Dec("granted_limit"),
        row.Dec("insured_limit"),
        row.Dec("insured_cover"),
        row.TextOrNull("insurer_name"),
        row.TextOrNull("policy_number"),
        row.Dec("coverage_percentage"),
        row.IntOrNull("max_overdue_days"),
        row.Dec("overdue_tolerance_amount"),
        ReceivablesWire.ParsePolicy(row.TextOrNull("reaction_policy")),
        row.Date("valid_from"),
        row.DateOrNull("valid_to"),
        row.Flag("is_active", fallback: true),
        row.TextOrNull("notes"));

    private static CreditOverride ToOverride(JsonElement row) => new(
        row.GuidOrEmpty("id"),
        row.GuidOrEmpty("customer_id"),
        ReceivablesWire.ParseScope(row.TextOrNull("scope")),
        row.Text("reason"),
        row.TextOrNull("authorized_by"),
        row.InstantOrNull("expires_at"),
        row.InstantOrNull("revoked_at"),
        row.TextOrNull("revoke_reason"),
        row.InstantOrNull("created_at"));

    private static CreditExposure ToExposure(JsonElement row) => new(
        row.Dec("open_invoices"),
        row.Dec("unbilled_delivery_notes"),
        row.Dec("confirmed_orders"),
        row.Dec("unreconciled_payments"),
        row.Dec("total_exposure"),
        row.Dec("overdue_amount"),
        row.Int("overdue_items"),
        row.Int("max_days_past_due"));

    private static CreditDecision? ToDecision(JsonElement row)
    {
        if (row.ValueKind == JsonValueKind.String)
        {
            return new CreditDecision(ReceivablesWire.ParseOutcome(row.GetString()), [], Reason: null, 0m, 0m, Overridden: false, ReactionPolicy: null);
        }

        if (row.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var policy = row.TextOrNull("reaction_policy");

        return new CreditDecision(
            ReceivablesWire.ParseOutcome(row.TextOrNull("outcome")),
            [.. row.Strings("breaches")],
            row.TextOrNull("reason"),
            row.Dec("exposure"),
            row.Dec("limit"),
            row.Flag("overridden"),
            policy is null ? null : ReceivablesWire.ParsePolicy(policy));
    }

    private static JsonElement Root(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }

    private static JsonElement Data(this JsonElement root) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) ? data : root;

    private static bool TryGet(this JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null)
        {
            return true;
        }

        value = default;

        return false;
    }

    private static IEnumerable<JsonElement> Elements(this JsonElement element, string name) =>
        element.TryGet(name, out var value) ? value.AsArray() : [];

    private static IEnumerable<JsonElement> AsArray(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Array ? element.EnumerateArray() : [];

    private static string Text(this JsonElement element, string name) => element.TextOrNull(name) ?? string.Empty;

    private static string? TextOrNull(this JsonElement element, string name) => element.TryGet(name, out var value) ? AsText(value) : null;

    private static string? AsText(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();

    private static IEnumerable<string> Strings(this JsonElement element, string name) =>
        element.Elements(name).Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? string.Empty);

    private static decimal? Number(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Number => value.TryGetDecimal(out var number) ? number : null,
        JsonValueKind.String => decimal.TryParse(value.GetString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
        JsonValueKind.Undefined => null,
        JsonValueKind.Object => null,
        JsonValueKind.Array => null,
        JsonValueKind.True => null,
        JsonValueKind.False => null,
        JsonValueKind.Null => null,
        _ => null,
    };

    private static decimal? DecimalOrNull(this JsonElement element, string name) => element.TryGet(name, out var value) ? Number(value) : null;

    private static decimal Dec(this JsonElement element, string name) => element.DecimalOrNull(name) ?? 0m;

    private static int? IntOrNull(this JsonElement element, string name) => element.DecimalOrNull(name) is { } number ? (int)number : null;

    private static int Int(this JsonElement element, string name, int fallback = 0) => element.IntOrNull(name) ?? fallback;

    private static bool Flag(this JsonElement element, string name, bool fallback = false) => element.TryGet(name, out var value) ? FlagValue(value, fallback) : fallback;

    private static bool FlagValue(JsonElement value, bool fallback) => value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number => value.TryGetDecimal(out var number) && number != 0m,
        JsonValueKind.String => string.Equals(value.GetString(), "1", StringComparison.Ordinal) || string.Equals(value.GetString(), "true", StringComparison.OrdinalIgnoreCase),
        JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array or JsonValueKind.Null => fallback,
        _ => fallback,
    };

    private static DateOnly? DateOrNull(this JsonElement element, string name)
    {
        var text = element.TextOrNull(name);

        return text is { Length: >= 10 } && DateOnly.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) ? day : null;
    }

    private static DateOnly Date(this JsonElement element, string name) => element.DateOrNull(name) ?? default;

    private static DateTime? InstantOrNull(this JsonElement element, string name)
    {
        var text = element.TextOrNull(name);

        return !string.IsNullOrWhiteSpace(text) && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var instant)
            ? DateTime.SpecifyKind(instant, DateTimeKind.Utc)
            : null;
    }

    private static Guid? GuidOrNull(this JsonElement element, string name) =>
        Guid.TryParse(element.TextOrNull(name), out var id) ? id : null;

    private static Guid GuidOrEmpty(this JsonElement element, string name) => element.GuidOrNull(name) ?? Guid.Empty;
}
