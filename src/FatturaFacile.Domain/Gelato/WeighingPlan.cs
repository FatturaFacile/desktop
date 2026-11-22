// <copyright file="WeighingPlan.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Domain.Gelato;

/// <summary>
/// Outcome of a weighed ingredient against its target.
/// </summary>
public enum WeighingStatus
{
    /// <summary>The ingredient is not weighed yet.</summary>
    Pending,

    /// <summary>The weight is inside the tolerance of the target.</summary>
    InTolerance,

    /// <summary>The weight is below the target by more than the tolerance.</summary>
    Under,

    /// <summary>The weight is above the target by more than the tolerance.</summary>
    Over,
}

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

/// <summary>
/// Guided weighing station plan: the recipe lines scaled to the batch weight, weighed one ingredient at a time. Decimal
/// arithmetic only, so the targets add up exactly to the batch weight (the rounding remainder goes to the heaviest line).
/// </summary>
public sealed class WeighingPlan
{
    private readonly List<WeighingStep> _steps;

    private WeighingPlan(List<WeighingStep> steps, decimal tolerancePercent)
    {
        _steps = steps;
        TolerancePercent = tolerancePercent;
    }

    /// <summary>
    /// Gets the steps in weighing order.
    /// </summary>
    public IReadOnlyList<WeighingStep> Steps => _steps;

    /// <summary>
    /// Gets the accepted deviation from the target, in percent of the target.
    /// </summary>
    public decimal TolerancePercent { get; }

    /// <summary>
    /// Gets a value indicating whether every ingredient is weighed.
    /// </summary>
    public bool IsComplete => _steps.TrueForAll(static s => s.MeasuredGrams is not null);

    /// <summary>
    /// Gets the index of the first ingredient that is not weighed, or -1 when the plan is complete.
    /// </summary>
    public int CurrentIndex => _steps.FindIndex(static s => s.MeasuredGrams is null);

    /// <summary>
    /// Builds a plan scaling the recipe lines to the batch weight.
    /// </summary>
    /// <param name="lines">The recipe lines: ingredient id, name, grams.</param>
    /// <param name="batchWeightGrams">The batch weight to weigh.</param>
    /// <param name="tolerancePercent">The accepted deviation in percent of each target.</param>
    /// <returns>The plan.</returns>
    public static WeighingPlan Create(IReadOnlyList<(string IngredientId, string Name, decimal Grams)> lines, decimal batchWeightGrams, decimal tolerancePercent = 1m)
    {
        ArgumentNullException.ThrowIfNull(lines);

        if (batchWeightGrams <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(batchWeightGrams), "The batch weight must be positive.");
        }

        var total = lines.Sum(static l => l.Grams);

        if (lines.Count == 0 || total <= 0m)
        {
            throw new ArgumentException("A recipe needs at least one ingredient with a positive weight.", nameof(lines));
        }

        var targets = lines.Select(l => Math.Round(l.Grams * batchWeightGrams / total, 2, MidpointRounding.AwayFromZero)).ToArray();
        var heaviest = Array.IndexOf(targets, targets.Max());
        targets[heaviest] += batchWeightGrams - targets.Sum();

        var steps = lines.Select((l, i) => new WeighingStep(l.IngredientId, l.Name, targets[i])).ToList();

        return new WeighingPlan(steps, tolerancePercent);
    }

    /// <summary>
    /// Records the weight of an ingredient.
    /// </summary>
    /// <param name="index">The step index.</param>
    /// <param name="measuredGrams">The weight read from the scale or typed in.</param>
    /// <param name="lotCode">The raw material lot used.</param>
    /// <returns>The status of the step against its target.</returns>
    public WeighingStatus Record(int index, decimal measuredGrams, string? lotCode = null)
    {
        if (index < 0 || index >= _steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (measuredGrams < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(measuredGrams), "A weight cannot be negative.");
        }

        _steps[index] = _steps[index] with { MeasuredGrams = measuredGrams, LotCode = lotCode };

        return StatusOf(_steps[index]);
    }

    /// <summary>
    /// Gets the status of a step against its target and the tolerance.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>The status.</returns>
    public WeighingStatus StatusOf(WeighingStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (step.MeasuredGrams is null)
        {
            return WeighingStatus.Pending;
        }

        var allowed = step.TargetGrams * TolerancePercent / 100m;
        var delta = step.DeltaGrams;

        return Math.Abs(delta) <= allowed ? WeighingStatus.InTolerance : delta < 0m ? WeighingStatus.Under : WeighingStatus.Over;
    }

    /// <summary>
    /// Formats a weight for display with one decimal.
    /// </summary>
    /// <param name="grams">The weight.</param>
    /// <returns>The text.</returns>
    public static string Format(decimal grams) => grams.ToString("0.0", CultureInfo.CurrentCulture);
}
