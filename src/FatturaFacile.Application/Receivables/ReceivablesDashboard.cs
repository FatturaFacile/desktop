// <copyright file="ReceivablesDashboard.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The receivables dashboard: KPIs and the blocked customers.
/// </summary>
/// <param name="AsOf">The day of the figures.</param>
/// <param name="Currency">The ISO currency of every figure.</param>
/// <param name="Kpis">The headline figures.</param>
/// <param name="Blocked">The blocked customers.</param>
public sealed record ReceivablesDashboard(
    DateOnly AsOf,
    string Currency,
    ReceivablesKpis Kpis,
    IReadOnlyList<BlockedCustomer> Blocked);
