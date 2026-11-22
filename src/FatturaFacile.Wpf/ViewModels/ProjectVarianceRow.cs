// <copyright file="ProjectVarianceRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One node of the baseline comparison.
/// </summary>
/// <param name="Code">The WBS code.</param>
/// <param name="Title">The node title.</param>
/// <param name="StatusText">The localized status against the baseline.</param>
/// <param name="Status">The status name, for the style triggers (Unchanged, Changed, Added, Removed).</param>
/// <param name="StartText">The start slip in days.</param>
/// <param name="FinishText">The finish slip in days.</param>
/// <param name="HoursText">The planned hours variance.</param>
/// <param name="CostText">The planned cost variance.</param>
/// <param name="RevenueText">The planned revenue variance.</param>
internal sealed record ProjectVarianceRow(string Code, string Title, string StatusText, string Status, string StartText, string FinishText, string HoursText, string CostText, string RevenueText);
