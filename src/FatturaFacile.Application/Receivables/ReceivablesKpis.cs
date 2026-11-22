// <copyright file="ReceivablesKpis.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The headline figures of the receivables dashboard.
/// </summary>
/// <param name="TotalExposure">The total credit exposure.</param>
/// <param name="TotalOpen">The total open amount.</param>
/// <param name="OverdueAmount">The overdue amount.</param>
/// <param name="Over90">The amount more than 90 days past due.</param>
/// <param name="Dso">The days sales outstanding, null when undefined.</param>
/// <param name="BadDebtProvision">The prudential bad-debt provision.</param>
/// <param name="NetRealizableValue">The open amount minus the provision.</param>
/// <param name="BlockedCustomers">The customers blocked for credit.</param>
/// <param name="ActiveOverrides">The credit overrides in force.</param>
public sealed record ReceivablesKpis(
    decimal TotalExposure,
    decimal TotalOpen,
    decimal OverdueAmount,
    decimal Over90,
    decimal? Dso,
    decimal BadDebtProvision,
    decimal NetRealizableValue,
    int BlockedCustomers,
    int ActiveOverrides);
