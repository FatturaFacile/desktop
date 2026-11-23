// <copyright file="BlanketLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One item of a blanket agreement, formatted for display.
/// </summary>
/// <param name="Description">The description.</param>
/// <param name="UnitPrice">The agreed unit price.</param>
/// <param name="Committed">The committed quantity, or a dash.</param>
/// <param name="Released">The quantity called off.</param>
/// <param name="Remaining">The quantity left, or a dash.</param>
/// <param name="Ordered">The value ordered on the item.</param>
internal sealed record BlanketLineRow(string Description, string UnitPrice, string Committed, string Released, string Remaining, string Ordered);
