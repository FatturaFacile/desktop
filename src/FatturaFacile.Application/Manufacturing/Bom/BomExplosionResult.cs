// <copyright file="BomExplosionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Bom;

/// <summary>
/// A complete BOM explosion.
/// </summary>
/// <param name="Rows">The rows in depth-first order.</param>
/// <param name="TotalCost">The total material cost.</param>
/// <param name="MaxLevel">The deepest level.</param>
public sealed record BomExplosionResult(IReadOnlyList<BomExplosionRow> Rows, decimal TotalCost, int MaxLevel);
