// <copyright file="WeighingStep.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Gelato;

/// <summary>
/// One ingredient of a guided weighing: its target weight in grams and, once weighed, the measured weight and the lot.
/// </summary>
/// <param name="IngredientId">The ingredient identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="TargetGrams">The target weight, scaled to the batch.</param>
/// <param name="MeasuredGrams">The weight read from the scale or typed in, when weighed.</param>
/// <param name="LotCode">The raw material lot used, when chosen.</param>
public sealed record WeighingStep(string IngredientId, string Name, decimal TargetGrams, decimal? MeasuredGrams = null, string? LotCode = null)
{
    /// <summary>
    /// Gets the difference measured minus target in grams (zero while pending).
    /// </summary>
    public decimal DeltaGrams => MeasuredGrams is { } measured ? measured - TargetGrams : 0m;
}
