// <copyright file="BlanketBalanceLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// An item of a blanket agreement with its committed and released quantity.
/// </summary>
/// <param name="LineId">The line identifier.</param>
/// <param name="Description">The description.</param>
/// <param name="UnitPrice">The agreed unit price.</param>
/// <param name="CommittedQuantity">The committed quantity, when there is one.</param>
/// <param name="ReleasedQuantity">The quantity called off.</param>
/// <param name="RemainingQuantity">The quantity left, when a quantity is committed.</param>
/// <param name="OrderedAmount">The value ordered on the line.</param>
public sealed record BlanketBalanceLine(string LineId, string Description, decimal UnitPrice, decimal? CommittedQuantity, decimal ReleasedQuantity, decimal? RemainingQuantity, decimal OrderedAmount);
