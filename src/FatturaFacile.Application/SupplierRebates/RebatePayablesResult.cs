// <copyright file="RebatePayablesResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The open supplier invoices a settlement can be set off against. They are read live: an open balance that was cached could be stale.
/// </summary>
/// <param name="Status">How the read ended.</param>
/// <param name="Invoices">The open payables of the supplier (none unless the server answered).</param>
public sealed record RebatePayablesResult(SupplierRebatesStatus Status, IReadOnlyList<RebatePayableInvoice> Invoices);
