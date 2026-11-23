// <copyright file="RebateContributionRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One contribution of an agreement, formatted for display.
/// </summary>
/// <param name="Id">The contribution identifier.</param>
/// <param name="Kind">The localized kind.</param>
/// <param name="Description">The description.</param>
/// <param name="Amount">The formatted amount.</param>
/// <param name="DueOn">The formatted due date.</param>
/// <param name="Status">The status code (planned, accrued, settled or cancelled).</param>
/// <param name="Contribution">The contribution behind the row.</param>
internal sealed record RebateContributionRow(string Id, string Kind, string Description, string Amount, string DueOn, string Status, RebateContribution Contribution);
