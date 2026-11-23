// <copyright file="RebateContribution.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A marketing, co-marketing or listing contribution planned on a rebate agreement.
/// </summary>
/// <param name="Id">The contribution identifier.</param>
/// <param name="Kind">The kind: marketing, co_marketing or listing.</param>
/// <param name="Description">The description.</param>
/// <param name="Amount">The amount.</param>
/// <param name="DueOn">The due date.</param>
/// <param name="Status">The status: planned, accrued, settled or cancelled.</param>
public sealed record RebateContribution(string Id, string Kind, string Description, decimal Amount, DateOnly? DueOn, string Status);
