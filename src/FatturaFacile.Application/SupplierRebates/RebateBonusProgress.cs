// <copyright file="RebateBonusProgress.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Progress of a fixed target bonus.
/// </summary>
/// <param name="Target">The spend target.</param>
/// <param name="Bonus">The bonus earned at the target.</param>
/// <param name="Reached">Whether the target is reached.</param>
/// <param name="DistanceToTarget">The spend missing to the target (zero once reached).</param>
/// <param name="ProgressPercent">The progress to the target, between 0 and 100.</param>
public sealed record RebateBonusProgress(decimal Target, decimal Bonus, bool Reached, decimal DistanceToTarget, decimal ProgressPercent);
