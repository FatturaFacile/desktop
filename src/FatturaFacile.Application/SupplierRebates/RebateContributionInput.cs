// <copyright file="RebateContributionInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The data of a contribution to plan on an agreement.
/// </summary>
/// <param name="Kind">The kind: marketing, co_marketing or listing.</param>
/// <param name="Description">The description.</param>
/// <param name="Amount">The amount (greater than zero).</param>
/// <param name="DueOn">The due date.</param>
public sealed record RebateContributionInput(string Kind, string Description, decimal Amount, DateOnly DueOn);
