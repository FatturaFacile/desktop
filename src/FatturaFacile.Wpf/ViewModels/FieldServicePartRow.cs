// <copyright file="FieldServicePartRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A consumed part of a work order, formatted for display.
/// </summary>
/// <param name="Description">The description.</param>
/// <param name="QuantityText">The quantity.</param>
/// <param name="UnitPriceText">The unit price.</param>
/// <param name="CoverageText">Covered, chargeable or pending.</param>
/// <param name="IsCovered">Whether the contract or the warranty covers it.</param>
/// <param name="IsPending">Whether it is only in the outbox.</param>
internal sealed record FieldServicePartRow(string Description, string QuantityText, string UnitPriceText, string CoverageText, bool IsCovered, bool IsPending);
