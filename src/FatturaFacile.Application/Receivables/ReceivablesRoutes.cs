// <copyright file="ReceivablesRoutes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The routes of the receivables endpoints, relative to the API root. A route with its query is also the key of the local cache.
/// </summary>
public static class ReceivablesRoutes
{
    /// <summary>
    /// The base of every route.
    /// </summary>
    public const string Base = "invoicing/receivables";

    /// <summary>
    /// The route that applies allocations.
    /// </summary>
    public const string Allocations = Base + "/allocations";

    /// <summary>
    /// The route that creates a credit limit.
    /// </summary>
    public const string CreditLimitsRoot = Base + "/credit-limits";

    /// <summary>
    /// The route that changes the provision rates.
    /// </summary>
    public const string ProvisionRatesRoute = Base + "/provision/rates";

    /// <summary>
    /// The cache key prefix of the open items pages.
    /// </summary>
    public const string ItemsPrefix = Base + "/items?";

    private const string DateFormat = "yyyy-MM-dd";
    private const string AmountFormat = "0.00";

    /// <summary>
    /// Gets the dashboard route.
    /// </summary>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string Dashboard(string? currency) => Base + "/dashboard" + Query(("currency", currency));

    /// <summary>
    /// Gets the aging route.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string Aging(Guid? customerId, string? currency) => Base + "/aging" + Query(("customer_id", Id(customerId)), ("currency", currency));

    /// <summary>
    /// Gets the open items route.
    /// </summary>
    /// <param name="filter">The filters.</param>
    /// <returns>The route with its query.</returns>
    public static string Items(ReceivableItemFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return Base + "/items" + Query(
            ("customer_id", Id(filter.CustomerId)),
            ("status", filter.Status?.ToWire()),
            ("document_type", filter.DocumentType),
            ("currency", filter.Currency),
            ("due_from", Day(filter.DueFrom)),
            ("due_to", Day(filter.DueTo)),
            ("issued_from", Day(filter.IssuedFrom)),
            ("issued_to", Day(filter.IssuedTo)),
            ("value_from", Amount(filter.ValueFrom)),
            ("value_to", Amount(filter.ValueTo)),
            ("bucket", filter.Bucket),
            ("open_only", filter.OpenOnly ? "1" : "0"),
            ("per_page", Number(Math.Max(1, filter.PerPage))),
            ("page", Number(Math.Max(1, filter.Page))));
    }

    /// <summary>
    /// Gets the route of an item.
    /// </summary>
    /// <param name="id">The item.</param>
    /// <returns>The route.</returns>
    public static string Item(Guid id) => Base + "/items/" + Id(id);

    /// <summary>
    /// Gets the dispute route of an item.
    /// </summary>
    /// <param name="id">The item.</param>
    /// <returns>The route.</returns>
    public static string ItemDispute(Guid id) => Item(id) + "/dispute";

    /// <summary>
    /// Gets the route that reverses an allocation.
    /// </summary>
    /// <param name="id">The allocation.</param>
    /// <returns>The route.</returns>
    public static string ReverseAllocation(Guid id) => Allocations + "/" + Id(id) + "/reverse";

    /// <summary>
    /// Gets the ledger route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string Ledger(Guid customerId, DateOnly? from, DateOnly? to, string? currency) =>
        Customer(customerId) + "/ledger" + Query(("from", Day(from)), ("to", Day(to)), ("currency", currency));

    /// <summary>
    /// Gets the unreconciled payments route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <returns>The route.</returns>
    public static string UnreconciledPayments(Guid customerId) => Customer(customerId) + "/unreconciled-payments";

    /// <summary>
    /// Gets the credit position route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <returns>The route.</returns>
    public static string Credit(Guid customerId) => Customer(customerId) + "/credit";

    /// <summary>
    /// Gets the risk route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <returns>The route.</returns>
    public static string Risk(Guid customerId) => Customer(customerId) + "/risk";

    /// <summary>
    /// Gets the credit limit history route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <returns>The route.</returns>
    public static string CreditLimits(Guid customerId) => Customer(customerId) + "/credit-limits";

    /// <summary>
    /// Gets the route of one credit limit.
    /// </summary>
    /// <param name="id">The limit.</param>
    /// <returns>The route.</returns>
    public static string CreditLimit(Guid id) => CreditLimitsRoot + "/" + Id(id);

    /// <summary>
    /// Gets the route that grants an override to a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <returns>The route.</returns>
    public static string Overrides(Guid customerId) => Customer(customerId) + "/overrides";

    /// <summary>
    /// Gets the route that revokes an override.
    /// </summary>
    /// <param name="id">The override.</param>
    /// <returns>The route.</returns>
    public static string RevokeOverride(Guid id) => Base + "/overrides/" + Id(id) + "/revoke";

    /// <summary>
    /// Gets the credit check route.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="scope">The operation.</param>
    /// <param name="amount">What the operation adds to the exposure.</param>
    /// <returns>The route.</returns>
    public static string CreditCheck(Guid customerId, CreditScope scope, decimal amount) =>
        Base + "/credit-check" + Query(("customer_id", Id(customerId)), ("scope", scope == CreditScope.All ? "order" : scope.ToWire()), ("amount", Amount(amount)));

    /// <summary>
    /// Gets the DSO route.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="periodDays">The days of the period.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string Dso(Guid? customerId, int periodDays, string? currency) =>
        Base + "/dso" + Query(("customer_id", Id(customerId)), ("period_days", Number(periodDays)), ("currency", currency));

    /// <summary>
    /// Gets the provision route.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string Provision(Guid? customerId, string? currency) => Base + "/provision" + Query(("customer_id", Id(customerId)), ("currency", currency));

    /// <summary>
    /// Gets the statement route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string Statement(Guid customerId, DateOnly from, DateOnly to, string? currency) =>
        Customer(customerId) + "/statement" + Query(("from", Day(from)), ("to", Day(to)), ("currency", currency));

    /// <summary>
    /// Gets the statement file route of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="format">pdf or xlsx.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The route.</returns>
    public static string StatementFile(Guid customerId, string format, DateOnly from, DateOnly to, string? currency) =>
        Customer(customerId) + "/statement/" + format + Query(("from", Day(from)), ("to", Day(to)), ("currency", currency));

    /// <summary>
    /// Gets the route that emails the statement of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <returns>The route.</returns>
    public static string StatementSend(Guid customerId) => Customer(customerId) + "/statement/send";

    private static string Customer(Guid customerId) => Base + "/customers/" + Id(customerId);

    private static string Id(Guid guid) => guid.ToString("D", CultureInfo.InvariantCulture);

    private static string? Id(Guid? guid) => guid is { } value ? Id(value) : null;

    private static string? Day(DateOnly? date) => date?.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string Day(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string Amount(decimal value) => value.ToString(AmountFormat, CultureInfo.InvariantCulture);

    private static string? Amount(decimal? value) => value?.ToString(AmountFormat, CultureInfo.InvariantCulture);

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Query(params (string Name, string? Value)[] pairs)
    {
        var parts = pairs
            .Where(pair => !string.IsNullOrEmpty(pair.Value))
            .Select(pair => pair.Name + "=" + Uri.EscapeDataString(pair.Value!));

        var query = string.Join('&', parts);

        return query.Length == 0 ? string.Empty : "?" + query;
    }
}
