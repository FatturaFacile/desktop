// <copyright file="DsoResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The days sales outstanding of the company or of a customer.
/// </summary>
/// <param name="AsOf">The day of the figure.</param>
/// <param name="PeriodDays">The days of the period.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="Receivables">The open receivables.</param>
/// <param name="CreditSales">The credit sales of the period.</param>
/// <param name="Dso">The DSO; null when there are receivables but no sales.</param>
public sealed record DsoResult(
    DateOnly AsOf,
    int PeriodDays,
    string Currency,
    decimal Receivables,
    decimal CreditSales,
    decimal? Dso);
