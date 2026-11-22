// <copyright file="FieldServiceBillingRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A line of the billing breakdown of a service report, formatted for display.
/// </summary>
/// <param name="KindText">The localized kind (labor, travel, call-out fee, part).</param>
/// <param name="Description">The description.</param>
/// <param name="QuantityText">The quantity.</param>
/// <param name="ListAmountText">The list amount.</param>
/// <param name="ChargedAmountText">The amount charged.</param>
/// <param name="CoverageText">Covered or chargeable.</param>
/// <param name="IsCovered">Whether the line is covered.</param>
internal sealed record FieldServiceBillingRow(string KindText, string Description, string QuantityText, string ListAmountText, string ChargedAmountText, string CoverageText, bool IsCovered);
