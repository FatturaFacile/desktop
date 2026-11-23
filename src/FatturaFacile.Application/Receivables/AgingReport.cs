// <copyright file="AgingReport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The aging of the receivables, in the buckets the server reports.
/// </summary>
/// <param name="AsOf">The day of the aging.</param>
/// <param name="Currency">The ISO currency of every figure.</param>
/// <param name="AvailableCurrencies">The currencies the company has receivables in.</param>
/// <param name="Buckets">The buckets in aging order; always render from this list.</param>
/// <param name="Totals">The open amount per bucket key.</param>
/// <param name="TotalOpen">The total open amount.</param>
/// <param name="TotalOverdue">The overdue amount.</param>
/// <param name="Over90">The amount more than 90 days past due.</param>
/// <param name="Customers">The aging per customer.</param>
public sealed record AgingReport(
    DateOnly AsOf,
    string Currency,
    IReadOnlyList<string> AvailableCurrencies,
    IReadOnlyList<AgingBucket> Buckets,
    IReadOnlyDictionary<string, decimal> Totals,
    decimal TotalOpen,
    decimal TotalOverdue,
    decimal Over90,
    IReadOnlyList<AgingCustomerRow> Customers);
